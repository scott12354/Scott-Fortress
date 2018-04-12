using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeUpdater : MonoBehaviour {

    Text theText;
	// Use this for initialization
	void Start () {
		theText = gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        theText.text = TimeOfDay.Instance.ToString();
        //int seconds = Mathf.RoundToInt(Time.realtimeSinceStartup);
        //string temp = "Time: " + Mathf.RoundToInt(seconds / 60) + ":" +
        //    seconds % 60;
        //theText.text = temp;
	}
}
