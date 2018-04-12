using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineShaft {
    GameObject myGO;
    //private Vector3 _myGrid;
    public Vector3Int myGrid;

    public MineShaft(Vector3Int g)
    {
        myGO = GameObject.Instantiate(GameManager.Instance.mineshaftPrefab);
        myGO.name = g.z.ToString() + "'s Mineshaft";

        var sr = myGO.GetComponent<SpriteRenderer>();
        sr.sprite = SpriteLibrary.Instance.getSprite("Mine Shaft");
        myGrid = g;
        Vector3 screenPositionGrid = new Vector3(myGrid.x, myGrid.y, 0);
        if (myGrid.z != 0)
            screenPositionGrid.x += UndergroundLevel.getNumberOfTilesToOffsetForLayer(Mathf.RoundToInt(myGrid.z));
        var t = new Vector3(screenPositionGrid.x * GameManager.Instance.tileSize.x,
            screenPositionGrid.y * GameManager.Instance.tileSize.y,
            0);
        myGO.transform.position = t;
    }

  
}
