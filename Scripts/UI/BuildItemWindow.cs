using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildItemWindow : MonoBehaviour {
    [SerializeField]
    GameObject listItemPrefab;

    List<GameObject> listOfItems=new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        var buildableItems = ItemManager.Instance.getBuilableItemList();

        foreach (ItemCostGroup i in buildableItems)
        {
            var go = GameObject.Instantiate(listItemPrefab);
            go.SendMessage("receiveCG", i);
            listOfItems.Add(go);
            go.transform.SetParent(transform);
        }
    }

    public void closeWindow()
    {
        GameManager.Instance.setBuildMode("");
        Destroy(gameObject);
    }

    public void clickedButtonToBuild(string name)
    {
        GameManager.Instance.setBuildMode(name);
        Destroy(gameObject);
    }
}
