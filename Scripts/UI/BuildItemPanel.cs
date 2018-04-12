using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildItemPanel : MonoBehaviour {
    [SerializeField]
    GameObject resourcePanel;
    [SerializeField]
    Text itemName;
    [SerializeField]
    Image itemImage;

    ItemCostGroup theItem;
    List<GameObject> itemCostResourcePanels=new List<GameObject>();
	//// Use this for initialization
	//void Start () {
		
	//}
	
	//// Update is called once per frame
	//void Update () {
		
	//}
    
    public void receiveCG(ItemCostGroup i)
    {
        theItem = i;
        itemName.text = i.theItemName;
        itemImage.sprite = SpriteLibrary.Instance.getSprite(i.theItemName);
        foreach (var q in i.resources)
        {
            var go = Instantiate(resourcePanel);
            go.SendMessage("receiveResource", q);
            itemCostResourcePanels.Add(go);
            go.transform.SetParent(transform.Find("Costs Panel").transform);
        }
    }

    public void clickedBuildMe()
    {
        transform.parent.gameObject.SendMessage("clickedButtonToBuild", theItem.theItemName);
    }
}
