using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailResourcePanelScript : MonoBehaviour {
    RectTransform myTrans;
    [SerializeField]
    float resourceItemPanelHeight = 30f;
    [SerializeField]
    float resourcePanelSpacing = 5f;
    [SerializeField]
    GameObject ResourcePanelPrefab;
    
    List<GameObject> resPanels = new List<GameObject>();
    List<Resource> resItems = new List<Resource>();
    // Use this for initialization
    void Awake () {
		myTrans = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        var newList = ItemManager.Instance.sumAvailableResources();
        if (newList == null || newList.Count==0)
        {
            foreach (var panel in resPanels)
                Destroy(panel);
            resPanels.Clear();
            resItems.Clear();
        }else if (newList.Count != resItems.Count)
        {
            foreach (var panel in resPanels)
                Destroy(panel);
            resPanels.Clear();
            foreach (var resource in newList)
            {
                var go = Instantiate(ResourcePanelPrefab);
                go.SendMessage("receiveResource", resource);
                go.transform.SetParent(this.transform);
                resPanels.Add(go);
            }
            resItems = newList;
        } else
        {
            resItems = newList;
            for (int i = 0; i < resPanels.Count; i++)
            {
                var script = resPanels[i].GetComponent<ResourceItemPanel>();
                script.receiveResource(resItems[i]);
            }
        }
        if (resPanels.Count != resItems.Count)
        {
            Debug.LogAssertion("ERROR");
        }
        myTrans.sizeDelta = new Vector2(180, 10 +(resItems.Count * resourcePanelSpacing) + (1 +resItems.Count) * resourceItemPanelHeight);
    }
}
