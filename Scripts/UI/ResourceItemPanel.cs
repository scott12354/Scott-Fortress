using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceItemPanel : MonoBehaviour {
    [SerializeField]
    Text resName;
    [SerializeField]
    Text resQuan;
    [SerializeField]
    Image resImage;

    public int resourceAmount;

    public void Update()
    {
        resQuan.text = resourceAmount.ToString();
    }

    public void updateValue(Resource r)
    {
        resQuan.text = r.amount.ToString();
    }

    public void receiveResource(Resource i)
    {
        resourceAmount = i.amount;
        resName.text = i.name;
        resQuan.text = i.amount.ToString();
        resImage.sprite = SpriteLibrary.Instance.getSprite(i.name);
    }
}
