using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionInfoPanelScript : MonoBehaviour {
    [SerializeField]
    private GameObject minionInfoGroupPrefab;
    [SerializeField]
    private float minionInfoPanelHeight = 40.0f;


    RectTransform rectT;


    Dictionary<Minion, GameObject> infoPanelDictionary = new Dictionary<Minion, GameObject>();

    public void newPanel(Minion m)
    {
        if (rectT == null)
        {
            rectT = GetComponent<RectTransform>();
        }

        var go = Instantiate(minionInfoGroupPrefab);
        go.transform.SetParent(transform);
        go.SendMessage("setMinion", m);
        infoPanelDictionary.Add(m, go);
        if (infoPanelDictionary.Count > 1)
        {
            rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, rectT.sizeDelta.y + minionInfoPanelHeight);
            rectT.localPosition = new Vector3(rectT.localPosition.x, rectT.localPosition.y - minionInfoPanelHeight);
        }

    }

    public void removePanel(Minion m)
    {
        GameObject go = infoPanelDictionary[m];
        Destroy(go);
        infoPanelDictionary.Remove(m);

        if (infoPanelDictionary.Count > 0)
        {
            rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, rectT.sizeDelta.y - minionInfoPanelHeight);
            rectT.localPosition = new Vector3(rectT.localPosition.x, rectT.localPosition.y + minionInfoPanelHeight);
        }
    }
}
