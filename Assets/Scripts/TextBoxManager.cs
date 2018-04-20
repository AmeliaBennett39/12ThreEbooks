using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextBoxManager : MonoBehaviour {

    public GameObject textBox;

    public Text theText;

    public int currentLine;
    public int endAtLine;
    
    public TextAsset textfile;
    public string[] textlines;

    
    // Use this for initialization
    void Start()
    {

        if (textfile != null)
        {
            textlines = (textfile.text.Split('\n'));
        }

        if(endAtLine == 0)
        {
            endAtLine = textlines.Length - 1;
        }
   
    }



}
