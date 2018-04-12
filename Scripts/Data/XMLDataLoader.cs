using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Linq;


public class ItemDataShell
{

    public bool carriable;
    public string myType;
	public bool walkable;
    public ItemDataShell()
    {

    }

	public ItemDataShell(bool ty, bool w)
    {
        carriable = ty;
		walkable = w;
        myType = "Test Object";
    }
    
}

public class HarvestableItemDataShell : ItemDataShell
{
    public Resource contains;
    public float harvestTime;
    public HarvestableItemDataShell()
    {
    }

	public HarvestableItemDataShell(int i, bool walkable) : base(true, walkable) {
        contains = new Resource("Cheese", 5);
        harvestTime = 1.0f;
    }
}

public class ContainerItemDataShell : ItemDataShell {
    public int maxResources;

    public ContainerItemDataShell()
    {

    }
}

public class HouseItemDataShell : ItemDataShell
{
    public int SizeX;
    public int SizeY;
    public bool[] WalkableArray;

}

public class BeingData
{
    public Resource[] contentsOnDeath;
    public string name;
}

#region Item Data XML serializations
[XmlRoot("Items")]
public class ItemDataContainer
{
    [XmlArray("Items")]
    [XmlArrayItem("ItemDataShell")]
    public List<ItemDataShell> theItems = new List<ItemDataShell>();
    [XmlArray("HarvestableItems")]
    [XmlArrayItem("HarvestableItemDataShell")]
    public List<HarvestableItemDataShell> theHarItems = new List<HarvestableItemDataShell>();
    [XmlArray("ContainerItems")]
    [XmlArrayItem("ContainerItemDataShell")]
    public List<ContainerItemDataShell> theContItems = new List<ContainerItemDataShell>();
    [XmlArray("HouseItems")]
    [XmlArrayItem("HouseItemDataShell")]
    public List<HouseItemDataShell> theHouseItems = new List<HouseItemDataShell>();
    [XmlArray("ItemCosts")]
    [XmlArrayItem("ItemCostGroup")]
	public List<ItemCostGroup> theItemCosts = new List<ItemCostGroup>();
    [XmlArray("BeingDatas")]
    [XmlArrayItem("BeingData")]
    public List<BeingData> theBeingData = new List<BeingData>();


    public ItemDataContainer()
    {

    }

    public void save(string filepath)
    {
        var serializer = new XmlSerializer(typeof(ItemDataContainer));
        using (var stream = new FileStream(filepath, FileMode.Create))
        {
            serializer.Serialize(stream, this);
            stream.Close();
        }

    }

    public static ItemDataContainer load(string filepath)
    {
        var serializer = new XmlSerializer(typeof(ItemDataContainer));
        var text = Resources.Load<TextAsset>(filepath);
        //using (var stream = new FileStream(filepath, FileMode.Open))
        using (var stream = new StringReader(text.text))
        {
            ItemDataContainer d = serializer.Deserialize(stream) as ItemDataContainer;
            stream.Close();
            return d;
        }
    }

    public void addItem(ItemDataShell theItem)
    {
        theItems.Add(theItem);
    }

    public void addItem(HarvestableItemDataShell theItem)
    {
        theHarItems.Add(theItem);
    }

    public void addItem(ContainerItemDataShell theItem)
    {
        theContItems.Add(theItem);
    }

    public void addItem(HouseItemDataShell theItem)
    {
        theHouseItems.Add(theItem);
    }

    public void addItemCostGroup(ItemCostGroup g)
    {
        theItemCosts.Add(g);
    }

    public ItemDataShell getDataShellForItem(string identifier)
    {
        foreach (ItemDataShell i in theItems)
        {
            if (i.myType == identifier)
            {
                return i;
            }
        }
        foreach(HarvestableItemDataShell h in theHarItems)
        {
            if (h.myType == identifier)
                return h;
        }

        foreach (ContainerItemDataShell cd in theContItems)
        {
            if (cd.myType == identifier)
                return cd;
        }

        foreach (HouseItemDataShell hi in theHouseItems)
        {
            if (hi.myType == identifier)
                return hi;
        }

        Debug.LogAssertion("Trying to pull data for: "+identifier+ ". It doesn't exist");
        return null;
    }

    public ItemCostGroup getCostForItem(string theItem)
    {
        foreach (var i in theItemCosts)
        {
            if (i.theItemName == theItem)
                return i;
        }
        return null;
    }

    public BeingData getBeingDataFor(string v)
    {
        foreach (var b in theBeingData)
        {
            if (b.name == v)
                return b;
        }
        Debug.LogAssertion("Trying to pull being data for: " + v + ". It doesn't exist");
        return null;
    }

}

#endregion