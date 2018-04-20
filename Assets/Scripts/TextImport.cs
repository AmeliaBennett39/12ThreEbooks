using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TextImport : MonoBehaviour {
    public TextAsset textfile;
    public string[] textlines;
   

	// Use this for initialization
	void Start () {

		if(textfile != null)
        {
            textlines = (textfile.text.Split('\n'));
        }
	}

}
