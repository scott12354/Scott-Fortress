using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public struct Resource
{
    public string name;
    public int amount;
    public Resource(string n, int q)
    {
        name = n;
        amount = q;
    }

	public Resource(Resource r) {
		name = r.name;
		amount = r.amount;
	}
}

public class ItemCostGroup
{
    public List<Resource> resources;
    public float buildTime;
    public string theItemName;

    public ItemCostGroup(List<Resource> rIn, string theItemIn, float timeIn)
    {
        resources = rIn;
        theItemName = theItemIn;
        buildTime = timeIn;
    }
	public ItemCostGroup() {
	}
}

public class Item
{
    private static int nextSerial = 0;
    public int mySerial;
    private bool _tapped = false;
    public bool tapped {
        get
        {
            return _tapped;
        }
        set
        {
            if (value == false && JobManager.Instance.keepItemTapped(this))
            {
                _tapped = true;
            }
            else
            {
                _tapped = value;
            }
        }
    }


    public bool carriable=true;
	public bool walkable=true;

    public GameObject myGO;

    public string myType;

    private Vector3Int _myGrid;
    public Vector3Int myGrid
    {
        get
        {
            return _myGrid;
            //return GameManager.Instance.actualToGrid(myGO.transform.position);
        }
        set
        {
            if (myGO == null)
            {
                Debug.Log("Null game object");
            }
            else
            {
                _myGrid = value;
                Vector3 t = MapManager.Instance.convertToWorldGrid(value);
                myGO.transform.position = t;
            }

        }
    }

	public Item(Vector3Int grid, string type) : 
        this(grid,type,ItemManager.itemData.getDataShellForItem(type)) {}

    protected Item(Vector3Int grid, string type, ItemDataShell idc)
    {
        mySerial = nextSerial++;
        var obj = GameObject.Instantiate(GameManager.Instance.itemPrefab);
        myGO = obj;
		myGO.name = type + " #" + mySerial;
        myGO.transform.SetParent(GameManager.Instance.itemFolder.transform);

        myType = type;
        var sr = myGO.GetComponent<SpriteRenderer>();
        if (myType == "Tree")
        {
            sr.sprite = SpriteLibrary.Instance.getSprite("Trees_" +Mathf.RoundToInt(Random.Range(0,100)).ToString());
        }
        else
        {
            sr.sprite = SpriteLibrary.Instance.getSprite(type);
        }

        carriable = idc.carriable;
		walkable = idc.walkable;

        myGrid = grid;

        ItemManager.Instance.newItemInstantiated(this);
        if (!(this is ResourceItem))
        {
            GameManager.Instance.updatePathfindingGraph();
        }
    }

    //public Item(ItemSaveData sav)
    //{
    //    mySerial = sav.mySerial;
    //    if (mySerial >= nextSerial)
    //    {
    //        nextSerial = mySerial + 1;
    //    }
    //    tapped = sav.tapped;
    //    carriable = sav.carriable;
    //    walkable = sav.walkable;
    //    myType = sav.myType;
    //    _myGrid = sav.myGrid;


    //    var obj = GameObject.Instantiate(GameManager.Instance.itemPrefab);
    //    myGO = obj;
    //    myGO.name = myType + " #" + mySerial;
    //    myGO.transform.SetParent(GameManager.Instance.itemFolder.transform);

    //    var sr = myGO.GetComponent<SpriteRenderer>();
    //    if (myType == "Tree")
    //    {
    //        sr.sprite = SpriteLibrary.Instance.getSprite("Trees_" + Mathf.RoundToInt(Random.Range(0, 100)).ToString());
    //    }
    //    else
    //    {
    //        sr.sprite = SpriteLibrary.Instance.getSprite(myType);
    //    }
        
    //    var t = new Vector3(_myGrid.x * GameManager.Instance.tileSize.x,
    //        _myGrid.y * GameManager.Instance.tileSize.y,
    //        0);
    //    myGO.transform.position = t;
    //    //ItemManager.Instance.newItemInstantiated(this);
    //    //if (!(this is ResourceItem))
    //    //{
    //    //    GameManager.Instance.updatePathfindingGraph();
    //    //}
    //}

    //public ItemSaveData getSaveData()
    //{
    //    var sav = new ItemSaveData();
    //    sav.mySerial = mySerial;
    //    sav.tapped = _tapped;
    //    sav.carriable = carriable;
    //    sav.walkable = walkable;
    //    sav.myType = myType;
    //    sav.myGrid = myGrid;
    //    return sav;
    //}

    public void carriedTo(Vector3Int destination)
    {
        myGrid = destination;
        var temp = MapManager.Instance.convertToWorldGrid(destination);
        myGO.transform.position = new Vector3(temp.x, temp.y, myGO.transform.position.y);
    }
    public static ItemDataContainer initializeItemDataBase()
    {
         return ItemDataContainer.load("Data\\Item Data");
    }
    public void pickUp()
    {
        if (!(this is ResourceItem))
            Debug.LogAssertion("Trying to carry a non resource Item");
        (this as ResourceItem).removeFromContainer();
        tapped = true;
    }
    public void putDown()
    {
        tapped = false;
    }
	~Item()
	{
        ItemManager.Instance.removeItem(this);
        GameManager.Instance.updatePathfindingGraph();
	}
}

//public class HarvestableItem : Item
//{
//    public Resource contains;
//    public float harvestTime;
//    public float harvestProgress = 0;

//    public HarvestableItem(Vector3Int gridIn, string typeIn) : 
//        base(gridIn, typeIn, ItemManager.itemData.getDataShellForItem(typeIn))
//    {
//        HarvestableItemDataShell g = (HarvestableItemDataShell)(ItemManager.itemData.getDataShellForItem(typeIn));
//        var res = Mathf.RoundToInt(g.contains.amount * Random.Range(50, 151) / 100);
//		var harvest = Mathf.RoundToInt(g.harvestTime * Random.Range(50, 151) / 100);
//        this.contains = g.contains;
//        contains.amount = res;
//        harvestTime = harvest;
//    }

//    //public HarvestableItem(HarvestableItemSaveData sav) :base(sav)
//    //{
//    //    contains = sav.contains;
//    //    harvestTime = sav.harvestTime;
//    //}

//    //new public HarvestableItemSaveData getSaveData()
//    //{
//    //    var sav = new HarvestableItemSaveData();
//    //    //From the base class
//    //    sav.mySerial = mySerial;
//    //    sav.tapped = tapped;
//    //    sav.carriable = carriable;
//    //    sav.walkable = walkable;
//    //    sav.myType = myType;
//    //    sav.myGrid = myGrid;

//    //    sav.contains = contains;
//    //    sav.harvestTime = harvestTime;

//    //    return sav;
//    //}

//	public ResourceItem harvestMe() {
//        ResourceItem r = ItemManager.Instance.addNewResourceItem(contains.name, contains.amount, myGrid);
//		ItemManager.Instance.removeItem (this);
//		return r;
//	}
//}

public class HarvestableGrid
{
    private static int nextSerial = 0;
    public int mySerial;
    public Vector3Int myGrid;
    public string name;
    public Resource contains;
    public float harvestTime;
    public float harvestProgress = 0;

    public HarvestableGrid(Vector3Int myGridIn, string nameIn)
    {
        mySerial = nextSerial++;
        myGrid = myGridIn;
        name = nameIn;
        ItemManager.Instance.tapItem(myGridIn);
        

        HarvestableItemDataShell g = (HarvestableItemDataShell)(ItemManager.itemData.getDataShellForItem(nameIn));
        var res = Mathf.RoundToInt(g.contains.amount * Random.Range(50, 151) / 100);
        var harvest = Mathf.RoundToInt(g.harvestTime * Random.Range(50, 151) / 100);
        this.contains = g.contains;
        contains.amount = res;
        harvestTime = harvest;

        ItemManager.Instance.newHarvestableGrid(this);
    }

    public ResourceItem harvestMe()
    {
        ResourceItem r = ItemManager.Instance.addNewResourceItem(contains.name, contains.amount, myGrid);
        ItemManager.Instance.removeHarvestableGrid(this);
        MapManager.Instance.harvested(myGrid);
        return r;
    }
}

public class ResourceItem : Item {

	public Resource contains;
	private bool _isInContainer = false;
	public bool isInContainer {
		get {
			return _isInContainer;
		}
		private set {
            if (myGO != null)
            {
                if (value == true)
                {
                    myGO.GetComponent<SpriteRenderer>().enabled = false;
                    myGO.transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    myGO.GetComponent<SpriteRenderer>().enabled = true;
                    myGO.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            _isInContainer = value;
        }

	}
	public ResourceItem (string name, int amount, Vector3Int grid) : base(grid, name)
	{
		contains = new Resource (name, amount);
        var childGO = myGO.transform.GetChild(0).gameObject;

        ItemNumUpdater iud = childGO.transform.GetChild(0).gameObject.GetComponent<ItemNumUpdater>();
        childGO.SetActive(true);
        iud.SendMessage("setItem", this);
    }
	public ResourceItem (Resource r, Vector3Int grid) : this(r.name, r.amount,grid) {}

	public ResourceItem takeAmount(int amountToTake) {
		if (amountToTake > contains.amount) {
			Debug.LogAssertion ("TRIED TO TAKE TOO MANY RESOURCES AWAY FROM A Resource object");
			return null;
		} else if (amountToTake == contains.amount) {
			//take everything, dont split
			return this;
		} else {
			//split the resource
			ResourceItem newItem = new ResourceItem (contains, myGrid);
			newItem.contains.amount = amountToTake;
			this.contains.amount -= amountToTake;
			return newItem;
		}
	}

    //public ResourceItem(ResourceItemSaveData r) : base(r)
    //{
    //    contains = r.contains;
    //    _isInContainer = r.inContainer;

    //    var childGO = myGO.transform.GetChild(0).gameObject;
    //    ItemNumUpdater iud = childGO.transform.GetChild(0).gameObject.GetComponent<ItemNumUpdater>();
    //    childGO.SetActive(true);
    //    iud.SendMessage("setItem", this);
    //}

    //Deletes the passed in resource item
	public ResourceItem combine(ResourceItem toCombine) {
        if (toCombine.myType != this.myType)
        {
            Debug.LogAssertion("Trying to combine two unlike items");
            return null;
        }
        else if (toCombine.mySerial == this.mySerial)
        {
            Debug.LogAssertion("Combining a resource with itself");
            return null;
        }
        else
        {
            this.contains.amount += toCombine.contains.amount;
            ItemManager.Instance.removeItem(toCombine);
            return this;
        }
	}

    //new public ResourceItemSaveData getSaveData()
    //{
    //    var sav = new ResourceItemSaveData();
    //    //From the base class
    //    sav.mySerial = mySerial;
    //    sav.tapped = tapped;
    //    sav.carriable = carriable;
    //    sav.walkable = walkable;
    //    sav.myType = myType;
    //    sav.myGrid = myGrid;

    //    sav.contains = contains;
    //    sav.inContainer = isInContainer;


    //    return sav;
    //}

    public void removeFromContainer()
    {
        isInContainer = false;
    }

    public void Deposit(ContainerItem i)
    {
        foreach(ResourceItem r in i.containedResources)
        {
            if (r.myType == this.myType)
            {
                r.combine(this);
                return;
            }
        }
        //cant combine anything, just add it
        i.containedResources.Add(this);
        isInContainer = true;
    }
}

public class ContainerItem : Item
{
    int maxResources;
    public List<ResourceItem> containedResources = new List<ResourceItem>();
    public ContainerItem(Vector3Int gridIn, string typeIn) :base(gridIn, typeIn, ItemManager.itemData.getDataShellForItem(typeIn))
    {
        ContainerItemDataShell cd = 
            (ContainerItemDataShell)(ItemManager.itemData.getDataShellForItem(typeIn));
        maxResources = cd.maxResources;
        carriable = false;
    }

    //public new ContainerItemSaveData getSaveData()
    //{
    //    var sav = new ContainerItemSaveData();
    //    //From the base class
    //    sav.mySerial = mySerial;
    //    sav.tapped = tapped;
    //    sav.carriable = carriable;
    //    sav.walkable = walkable;
    //    sav.myType = myType;
    //    sav.myGrid = myGrid;

    //    sav.maxResources = maxResources;
    //    sav.containedResourceSerials = new int[containedResources.Count];
    //    for (int i = 0; i < containedResources.Count; i++)
    //    {
    //        sav.containedResourceSerials[i] = containedResources[i].mySerial;
    //    }

    //    return sav;

    //}

    //public ContainerItem(ContainerItemSaveData r, ResourceItem[] contained) : base(r)
    //{
    //    maxResources = r.maxResources;
    //    containedResources = contained.ToList();
    //}

    private int sumResources()
    {
        int x = 0;
        foreach (var i in containedResources)
        {
            x += i.contains.amount;
        }
        return x;
    }

	public ResourceItem removeResource(Resource res) {
		foreach (var item in containedResources) {
			if (res.name == item.myType) {
				if (res.amount > item.contains.amount) {
					Debug.LogAssertion ("taking too many resources out of this container");
					return null;
				} else if (res.amount < item.contains.amount) {
					//the resource is being split, we'll leave some behind in the container
					//by default the returned item below will have isincontainer = false
					var toReturn = item.takeAmount (res.amount);
                    toReturn.pickUp();
                    toReturn.removeFromContainer();
					return toReturn;
				} else {
					//Taking everything, remove it from the container
					var ret = item;
					ret.pickUp();
					containedResources.Remove(item);
                    ret.removeFromContainer();
					return ret; //returning ret just in case item gets deleted in this scope before the return statement
				}
			}
		}
		Debug.LogAssertion ("trying to remove a resource from a container that doesn't contain that resource");
		return null;
	}

    public bool addResource(ResourceItem r)
    {
        if ((sumResources() + r.contains.amount) > maxResources)
        {
            return false;
        } else
        {
            r.Deposit(this);
            return true;
        }
    }

    public bool willFit(ResourceItem r)
    {
        if ((sumResources() + r.contains.amount) > maxResources)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}






