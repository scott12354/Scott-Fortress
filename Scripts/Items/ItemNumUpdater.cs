using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemNumUpdater : MonoBehaviour {
    ResourceItem myItem;
    TextMesh theText;

	// Use this for initialization
	void Start () {
        theText = GetComponent<TextMesh>();
        var mr = GetComponent<MeshRenderer>();
        mr.sortingLayerName = "UI";
    }
	
	// Update is called once per frame
	void Update () {
		if (myItem != null)
        {
            theText.text = myItem.contains.amount.ToString();
        }
	}

    public void setItem(ResourceItem i)
    {
        myItem = i;
    }
}
