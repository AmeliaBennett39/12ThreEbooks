using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Define;
using System.Text.RegularExpressions;


public class TextInput : MonoBehaviour {

    //Input field 
    public InputField inputField;

    //Panel, Dialog
    public GameObject Panel;

    //Game object
    public GameObject textBox;

    //Search Button
    public Button button;

    string words;

    Text thetext;

    public Text theWord;

    InputField theInput;
    
    void Awake()
    {
        //On start inputfield is deactivated
        inputField.image.enabled = false;
        inputField.placeholder.enabled = false;
        inputField.enabled = false;
        Panel.SetActive(false);
       
        //After user enters word and exits input box or presses enter, 
        //function AcceptStringInput is executed
        button.onClick.AddListener(TaskOnClick);
        
    }

    void TaskOnClick()
    {
        //If inputfield is activated, disable it
        if(inputField.enabled == true)
        {
            inputField.enabled = false;
            inputField.image.enabled = false;
            inputField.placeholder.enabled = false;
			theWord.text = "";
            theWord.enabled = false;
            Panel.SetActive(false);
        }
        //If Inputfield is not enabled, enable it for user to input words
        else
        {
            inputField.enabled = true;
            inputField.image.enabled = true;
            inputField.placeholder.enabled = true;
            theWord.enabled = true;
            Panel.SetActive(true);
            inputField.onEndEdit.AddListener(AcceptStringInput);

        }

    }

    void AcceptStringInput(string userInput)
    {
        
        //Grabs the text from the game object
        thetext = textBox.GetComponent<Text>();

        //Grabs the word to search from user input
        string word = userInput.ToLower();

        //Retrieve the textbox for the Webster API
        string words = thetext.text.ToLower().Replace("\n", " ");

        //Split input into seperate words (split by single space)
        //Using StringSplitOptions.RemoveEmptyEntries helps get rid of extra space so it does not search for space
        //string[] strings = inputField.text.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


        //Split text box into seperate words (split by single space)
        string[] strings = words.Split(' ');

        int i = 0;
        //If the text contains my input from the input field return found to unity console, else return not found
        Debug.Log("Length: " + strings.Length);


        while (i < strings.Length)
        {
            if (definition.RemoveSpecialCharacters(strings[i].Replace("’", "'")).Equals(definition.RemoveSpecialCharacters(word)))
            {
                Debug.Log("FOUND: " + word);
               
                definition def = new definition();
                int wordPos = i;
                Debug.Log("Position: " + wordPos);
                List<string> strResults = def.getWebsterDef(words, wordPos, word);
                if (strResults.Count >= 2)
                {
                    while (strResults[1].Replace(":", "").Replace(" ", "").Length < 1)
                    {
                        strResults[1] = strResults[2];
                        strResults.RemoveAt(2);
                    }
                    theWord.text = (word + "\n-----------\n" + strResults[0] + "\n\n" + strResults[1]).Replace(":", ""); //Amelia - added .Replace(":","")
                }
                    //theWord.text = (word + "\n_______\n" + strResults[0] + "\n" + strResults[1]);
                else if (strResults.Count == 1)
                    theWord.text = (word + "\n-----------\n" + strResults[0]).Replace(":", ""); //Amelia - added .Replace(":","")
                else if (word.Length == 1 && strResults.Count == 0)
                    theWord.text = word + "\n-----------\n" + "No definition found.";
                //call function for finding definition with inputField.text.ToLower(); //lowercase ver of string
                
                break;
            }
            else
            {
                theWord.text = word + "\nWord not in text.\n";
                Debug.Log("NOT FOUND: " + word);
            }
            i++;
        }

        //After word is found, input field is cleared and has focus
        InputComplete();
    }

    void InputComplete()
    {
        //inputField.ActivateInputField();    //Puts focus on input field
        inputField.text = null;             //Clears input field
    }


}
