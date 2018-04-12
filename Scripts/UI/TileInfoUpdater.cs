using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfoUpdater : MonoBehaviour {

    Text theText;
    // Use this for initialization
    void Start()
    {
        theText = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update () {
        Vector3Int activeTile = GameManager.Instance.activeTile;
        if (activeTile == null)
            return;
        string temp = "Item On Top: ";
        if (ItemManager.Instance.getItemAtGrid(activeTile) == null)
        {
            temp += "None.\n";
        }
        else
        {
            temp += ItemManager.Instance.getItemAtGrid(activeTile).myType.ToString() + "\n";
        }
        if (activeTile.z != -1)
        {
            temp += "Walkable: " + MapManager.Instance.tileWalkable(activeTile).ToString();
        } else
        {
            temp += "Walkable: False";
        }
        theText.text = temp;
	}
}
