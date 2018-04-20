using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;
//using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Define
{
    class definition
    {

        public static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex("[^a-zA-Z0-9 ’'’]", RegexOptions.IgnoreCase);// | RegexOptions.CultureInvariant);
            return r.Replace(input, String.Empty);
        }

        public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }
        public List<string> getWebsterDef(string arg, int place, string wordarg)
        {
            //fix network errors
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            //List of definitions containing defs for all PoS
            List<string> defs = new List<string>();
            // Create new instance of webclient 
            System.Net.WebClient wc = new System.Net.WebClient();

            string taggedtext;
            // Request part of speech data
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["text"] = arg;

                var response = wb.UploadValues("http://text-processing.com/api/tag/", "POST", data);
                taggedtext = Encoding.UTF8.GetString(response);
                // Debug.Log(taggedtext);
                //https://cs.nyu.edu/grishman/jet/guide/PennPOS.html
                //describes each Part of Speech label
            }

            //Part of speech using complex Penn labeling
            string complexpos;
            //Debug.Log(taggedtext);
            try
            {
                var matches = Regex.Match(taggedtext, "\\W" + wordarg + "/\\S+ ");
                Debug.Log(matches);
                string match = matches.ToString();
                complexpos = match.Split('/')[1].Split('\\')[0].Split(' ')[0];
                Debug.Log(complexpos);
            }

            catch (Exception e)
            {
                complexpos = "GO";
            }
            string simplepos = SimplePartOfSpeechWebster(complexpos);

            string word;
            word = arg.Split(' ')[place];
            // Take punctuation off the word for the Webster definition
            //word = word.Replace(",", "").Replace("!", "").Replace(".", "").Replace("?", "").Replace("\n", "").Replace("\"", "").Replace("“", "").Replace("”", "").Replace("“", "");
            //Debug.Log(word);
            //Debug.Log("this is the right code");

            char chr = (char)8217;
            string str = "";
            str += chr;
            // Debug.Log(str + ":asdfsdaf");
            word = word.Replace(str, "'");
            //Debug.Log(word);
            word = RemoveSpecialCharacters(word);
            //Debug.Log(word);
            // Use the API to get definitions of the word passed
            string url = "https://www.dictionaryapi.com/api/v1/references/sd4/xml/" + word + "?key=6c1bfb6c-266f-41c8-a52a-3ac78bc616d8";
            // Return a XML using the webclient instance
            Debug.Log(url);
            string webData = wc.DownloadString(url);

            //parse XML
            XElement definitions = XElement.Parse(webData);
            IEnumerable<XElement> a = definitions.Descendants("entry");
            bool changeWord = false;
            bool matchedPoS = false;
            for (int i = 0; i < a.Count(); i++)
            {
                XElement mainXElement = (XElement)a.ElementAt(i);
                IEnumerable<XElement> nodeContainer;

                string defWord = (mainXElement.Attribute((XName)"id").Value);
                //Debug.Log(defWord);
                defWord = defWord.Split('[')[0];
                defWord = RemoveSpecialCharacters(defWord);
                //Debug.Log(defWord);
                if (changeWord)
                {
                    word = defWord;
                    changeWord = false;
                }
                //Debug.Log((defWord.Equals(word)));
                if (!defWord.ToUpper().Equals(word.ToUpper()))
                {
                    if (i == 0)
                        word = defWord;
                    else
                        continue;
                }
                //check for pos and word here;
                string defpos;
                //checks if the definition has a listed part of speech and switches to next definition if not
                nodeContainer = mainXElement.Descendants((XName)"fl");
                bool iscx = false;
                if (nodeContainer.Count() == 0)
                {
                    iscx = mainXElement.Descendants((XName)"cx").Count() > 0;
                    if (iscx)
                    {
                        defs.Add("Unknown part of speech");
                        XElement cxelement = mainXElement.Descendants((XName)"cx").First();
                        XElement ctelement = cxelement.Descendants((XName)"ct").First();
                        ctelement.SetValue(" " + (String)ctelement);
                        defs.Add(cxelement.Value);
                        matchedPoS = true;
                        continue;
                    }
                    else if (word.Contains("'"))
                    {
                        defpos = "GO";
                    }
                    else if (i == 0)
                    {
                        Debug.Log("changed word");
                        changeWord = true;
                        continue;
                    }
                    else
                        continue;
                }
                else
                {
                    defpos = ((XElement)nodeContainer.First()).Value;
                }

                if (simplepos.Equals("definite article"))
                    simplepos = "indefinite article";
                if (!defpos.Equals("GO") && !defpos.ToUpper().Equals(simplepos.ToUpper()) && !simplepos.ToUpper().Equals("GO"))
                    continue;
                matchedPoS = true;
                // Part of speech converted to simple labeling used in dictionaries

                if (!defpos.Equals("GO"))
                    defs.Add(((XElement)nodeContainer.First()).Value);
                else
                    defs.Add("Unknown Part of Speech");
                nodeContainer = mainXElement.Descendants((XName)"def"); //get all the definitions under this version of the word
                if (nodeContainer.Count() == 0)
                {
                    Debug.Log("aa");
                    if (i == 0)
                    {
                        Debug.Log("changed word");
                        changeWord = true;
                    }
                    continue;
                }

                XElement Xdefinitions = (XElement)nodeContainer.First();
                nodeContainer = Xdefinitions.Descendants((XName)"dt");

                int definitionCount = 0; //definition count for this particular word or part of speech
                int maxDefinitionsPerSpecificWord = 5; //most definitions allowed to be given for a specific part of speech
                for (int j = 0; j < nodeContainer.Count() && definitionCount < maxDefinitionsPerSpecificWord; j++)
                {
                    XElement Xdefinition = (XElement)nodeContainer.ElementAt(j);
                    IEnumerable<XElement> nodeContainer2 = Xdefinition.Descendants((XName)"vi");
                    for (int k = nodeContainer2.Count(); k > 0; k--)
                    {
                        nodeContainer2.ElementAt(k - 1).Remove();
                    }
                    nodeContainer2 = Xdefinition.Descendants((XName)"sx");
                    for (int k = nodeContainer2.Count(); k > 0; k--)
                    {
                        nodeContainer2.ElementAt(k - 1).Remove();
                    }
                    nodeContainer2 = Xdefinition.Descendants((XName)"dx");
                    for (int k = nodeContainer2.Count(); k > 0; k--)
                    {
                        nodeContainer2.ElementAt(k - 1).Remove();
                    }

                    defs.Add(Xdefinition.Value);
                    definitionCount++;

                }
            }

            if (!matchedPoS)
            {
                try
                {
                    XElement mainXElement = (XElement)a.ElementAt(0);
                    IEnumerable<XElement> nodeContainer;
                    nodeContainer = mainXElement.Descendants((XName)"def");
                    XElement Xdefinitions = (XElement)nodeContainer.First();
                    nodeContainer = Xdefinitions.Descendants((XName)"dt");

                    int definitionCount = 0; //definition count for this particular word or part of speech
                    int maxDefinitionsPerSpecificWord = 5; //most definitions allowed to be given for a specific part of speech
                    for (int j = 0; j < nodeContainer.Count() && definitionCount < maxDefinitionsPerSpecificWord; j++)
                    {
                        XElement Xdefinition = (XElement)nodeContainer.ElementAt(j);
                        IEnumerable<XElement> nodeContainer2 = Xdefinition.Descendants((XName)"vi");
                        for (int k = nodeContainer2.Count(); k > 0; k--)
                        {
                            nodeContainer2.ElementAt(k - 1).Remove();
                        }
                        nodeContainer2 = Xdefinition.Descendants((XName)"sx");
                        for (int k = nodeContainer2.Count(); k > 0; k--)
                        {
                            nodeContainer2.ElementAt(k - 1).Remove();
                        }
                        nodeContainer2 = Xdefinition.Descendants((XName)"dx");
                        for (int k = nodeContainer2.Count(); k > 0; k--)
                        {
                            nodeContainer2.ElementAt(k - 1).Remove();
                        }

                        defs.Add(Xdefinition.Value);
                        definitionCount++;

                    }
                }
                catch (Exception e)
                {
                    defs.Add("No definitions found");
                }
            }
            return defs;



        }
        /*public List<string> getDef(string arg, int place)
        {
            //List of definitions containing defs for all PoS
            List<string> stringdefs = new List<string>();
            //List of definitions containing defs for only the identified PoS
            List<string> newdefs = new List<string>();

            // Create new instance of webclient 
            System.Net.WebClient wc = new System.Net.WebClient();

            string word;

            word = arg.Split(' ')[place];
            Debug.Log(word);
            // Use the API to get definitions of the word passed
            string url = "http://api.datamuse.com/words?sp=" + word + "&qe=sp&md=d&max=1";
            // Return a JSON using the webclient instance
            string webData = wc.DownloadString(url);

            // Parse the JSON
            var res = JsonConvert.DeserializeObject<dynamic>(webData);

            // Text to be returned from text-processing.com showing parts of speech for the sentence
            string taggedtext;
            // Request part of speech data
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["text"] = arg;

                var response = wb.UploadValues("http://text-processing.com/api/tag/", "POST", data);
                taggedtext = Encoding.UTF8.GetString(response);

                //https://cs.nyu.edu/grishman/jet/guide/PennPOS.html
                //describes each Part of Speech label
            }

            //Part of speech using complex Penn labeling
            string complexpos;
            try
            {
                complexpos = taggedtext.Split(' ')[place + 2].Split('/')[1].Split(')')[0];
            } catch (Exception e)
            {
                complexpos = "GO";
            }

            // Part of speech converted to simple labeling used in dictionaries
            string simplepos = SimplePartOfSpeech(complexpos);

           

            // Return an object of the definitions
            JArray def = res[0].defs;

            

            if (simplepos.Equals("XX") || def == null || def.Count() == 0)
            {
                if (complexpos.Equals("FW"))
                    stringdefs.Add("The word is foreign");
                else
                    stringdefs.Add("No definitions could be found");
                Debug.Log(complexpos);
                return stringdefs;
            }

            //Load JSON text into string list
            for (int i = 0; i < def.Count(); i++)
            {
                stringdefs.Add(def[i].ToString());
            }

            //For some identified PoS we will display all definitions
            if (simplepos.Equals("GO")){ //ignore part of speech
                return stringdefs;
            }

            //Load any definitions that share the PoS with the word in context
            //into a new list to be displayed
            for (int i = 0; i < stringdefs.Count(); i++)
            {

                string curdef = stringdefs[i];
                string curpos = curdef.Split('\t')[0];
                if (curpos.Equals(simplepos))
                {
                    newdefs.Add(curdef);
                }

            }

            //If no definitions are found for this PoS, display all definitions
            if (newdefs.Count>0)
                return newdefs;
            return stringdefs;
           //return def;
        }
		*/
        public string SimplePartOfSpeechWebster(string pos)
        {
            string simplepos;
            switch (pos)
            {
                case "CD":
                case "NN":
                case "NNP":
                case "NNPS":
                case "PDT":
                    simplepos = "noun";
                    break;
                case "MD":
                case "VB":
                case "VBG":
                case "VBD":
                case "VBP":
                case "VBZ":
                    simplepos = "verb";
                    break;
                case "JJ":
                case "JJR":
                case "JJS":
                case "WDT":
                    simplepos = "adjective";
                    break;
                case "RB":
                case "RBR":
                case "RBS":
                case "TO":
                case "WRB":
                    simplepos = "adverb";
                    break;
                case "CC":
                    simplepos = "conjunction";
                    break;
                case "DT":
                    simplepos = "definite article";
                    break;
                case "UH":
                    simplepos = "interjection";
                    break;
                case "WP":
                case "WP$":
                case "PRP$":
                case "PRP":
                    simplepos = "pronoun";
                    break;
                case "FW":
                case "SYM":
                    simplepos = "XX";
                    break;
                case "RP":
                default:
                    simplepos = "GO";
                    break;
            }

            return simplepos;
        }
        public string SimplePartOfSpeech(string pos)
        {
            string simplepos;
            switch (pos)
            {
                case "CD":
                case "NN":
                case "NNP":
                case "NNPS":
                case "PRP":
                case "PDT":
                case "PRP$":
                    simplepos = "n";
                    break;
                case "MD":
                case "VB":
                case "VBG":
                case "VBD":
                case "VBP":
                case "VBZ":

                    simplepos = "v";
                    break;
                case "JJ":
                case "JJR":
                case "JJS":
                    simplepos = "adj";
                    break;
                case "RB":
                case "RBR":
                case "RBS":
                    simplepos = "adv";
                    break;
                case "CC":
                case "FW":
                case "DT":
                case "TO":
                case "SYM":
                case "UH":
                case "WP":
                case "WDT":
                case "WP$":
                    simplepos = "XX";
                    break;
                case "RP":
                case "WRB":
                default:
                    simplepos = "GO";
                    break;



            }
            return simplepos;
        }
    }
}
