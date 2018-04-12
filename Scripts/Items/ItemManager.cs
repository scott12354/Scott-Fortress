using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{

    private Dictionary<Vector3Int, Item> mainItemDictionary;

    List<ResourceItem> resourceItems = new List<ResourceItem>();
    List<HarvestableGrid> activeHarvestableGrids = new List<HarvestableGrid>();

	List<ItemCostGroup> itemCosts = new List<ItemCostGroup> ();
	static ItemDataContainer dataContainer;

    public List<Vector3Int> tappedItemGrid = new List<Vector3Int>();


	public static ItemDataContainer itemData {
		get {
            if (_instance == null)
            {
                _instance = new ItemManager();
            }
            return dataContainer;
		}
		private set { 
			Debug.LogAssertion ("Why are you here???");
			dataContainer = value;
		}
	}

	//TODO - optimization
	//split theItems into arrays of type to speed up the loops


    static ItemManager _instance;
    public static ItemManager Instance
    {
        get
        {
			if (_instance == null) {
				_instance = new ItemManager ();
			}
				
            return _instance;
        }
        private set { }
    }

    public void gameManagerUpdate()
    {
        var untapped = resourceItems.Where(x => x.tapped == false).ToList();
        untapped = untapped.Where(x => x.isInContainer == false).ToList();
        foreach (var i in untapped)
        {
            var cont = getClosestContainerThatFits(i.myGrid, i);
            if (cont != null)
            {
                JobManager.Instance.newDepositResourcesJob(i);
            }
        }
    }

	public ItemManager() {
		dataContainer = Item.initializeItemDataBase ();
		itemCosts = dataContainer.theItemCosts;
        mainItemDictionary = new Dictionary<Vector3Int, Item>();

    }
    
    private List<ResourceItem> getResourceItems()
    {
        return resourceItems;
    }
    
	public List<Resource> getItemCost(string name) {
		foreach (var item in itemCosts) {
			if (item.theItemName == name)
				return item.resources;
		}
		Debug.LogAssertion ("Couldn't find item cost data for " + name + ".");
		return null;
	}

	public string getTypeForItemString(string v) {
		var dataShell = itemData.getDataShellForItem (v);
		string stringg = dataShell.GetType().ToString();
		stringg = stringg.Replace ("DataShell", "");
		return stringg;
	}

    public ContainerItem addNewContainerItem(string v, Vector3Int newItemGrid)
    {
        var ret = new ContainerItem(newItemGrid, v);
        return ret;
    }

	public Item addNewItem(string v, Vector3Int theGrid) {
		if (mainItemDictionary[theGrid] != null) {
			Debug.Log ("Creating an item on top of another Item, old item returned: "+v+".");
			return mainItemDictionary[theGrid];
		}
		switch (getTypeForItemString (v).ToString()) {
		case "ContainerItem":
			return addNewContainerItem (v, theGrid);
		case "ResourceItem":
			Debug.LogAssertion ("Cannot add a resource item in this manner, use newResourceIteminstead.");
			return null;
		//case "HarvestableItem":
		//	return AddNewHarItem (v, theGrid);
		case "Item":
            var item = new Item(theGrid, v);
            return item;
		default:
			Debug.LogAssertion ("Attempting to add an item who's type (" + getTypeForItemString(v).ToString() + ") doesnt exist, or this function doesnt work");
			return null;
		}
	}

	public ResourceItem addNewResourceItem(string res, int quan, Vector3Int vec) {
		ResourceItem r = new ResourceItem (res, quan, vec);
		return r;
	}

    //public HarvestableItem AddNewHarItem(string v, Vector3Int newItemGrid)
    //{
    //    var ret = new HarvestableItem(newItemGrid, v);
    //    return ret;
    //}

    public void removeItem(Item i)
    {
        if (i is ResourceItem)
        {
            resourceItems.Remove(i as ResourceItem);
        } else
        {
            mainItemDictionary.Remove(i.myGrid);
        }

        GameManager.Instance.flagForDestruction(i.myGO);
		//GameObject.Destroy (i.myGO);
        //Lean.LeanPool.Despawn(ii.myGO);
    }

    public ContainerItem getClosestContainerThatFits(Minion m, ResourceItem item)
    {
        return getClosestContainerThatFits(m.myGrid, item);
    }

    public ContainerItem getClosestContainerThatFits(Vector3Int vec, ResourceItem item)
    {
        List<ContainerItem> activeContainerItems = new List<ContainerItem>();
        foreach (Item i in mainItemDictionary.Values.ToList())
        {
            if (i is ContainerItem)
                activeContainerItems.Add(i as ContainerItem);
        }
        if (activeContainerItems.Count == 0)
            return null;
        ContainerItem winner = null;
        foreach (var w in activeContainerItems)
        {
            if (w.willFit(item))
                winner = w;
        }
        if (winner == null)
        {
            return null;
        }
        float curDist = Vector3Int.Distance(vec, winner.myGrid);

        foreach (ContainerItem c in activeContainerItems)
        {
            if ((Vector3Int.Distance(vec, c.myGrid) < curDist))
            {
                var test = false;
                if (c.willFit(item))
                {
                    test = true;
                }
                if (test)
                {
                    winner = c;
                    curDist = Vector3Int.Distance(vec, winner.myGrid);
                }
            }
        }
        if (winner.willFit(item))
            return winner;
        return null; //wont fit

    }

    //returns null if it cant find them all
    public List<ResourceItem> searchForAndGetResources(List<Resource> toSearchFor) {
		int toRemove = toSearchFor.Count;
		var returnList = new List<ResourceItem> ();
        List<ResourceItem> itemsRequiringSplitting = new List<ResourceItem>();
        List<int> amountsToTake = new List<int>();
		for (int i=0;i< toSearchFor.Count;i++) {
			int rAmount = toSearchFor[i].amount;
			foreach (var item in resourceItems) {
				if (item.myType == toSearchFor[i].name && item.tapped == false) {
					if (item.contains.amount == rAmount) {
						//it has everything we need
						returnList.Add (item);
						toRemove--;
						break; // go to the next item on the shopping list
					} else if (item.contains.amount < rAmount) {
						//doesn't contain enough, keep looking
						returnList.Add (item);
						//dont break, keep looking
						rAmount -= item.contains.amount;
					} else {
						//contains more than enough
						//TODO - temporary, split occurs her einstead of when minion arrives
						//returnList.Add(item.takeAmount(rAmount));

                        //Dont actually split hte item until you know you'll have everything you need
                        itemsRequiringSplitting.Add(item);
                        amountsToTake.Add(rAmount);
						toRemove--;
						break;
					}
				}
			}
		}

		if (toRemove > 0) {
				Debug.Log ("Couldn't find enough Resources");
				return null;
			}
        for (int i=0;i<itemsRequiringSplitting.Count;i++)
        {
            returnList.Add(itemsRequiringSplitting[i].takeAmount(amountsToTake[i]));
        }
		return returnList;

	}

    public List<ItemCostGroup> getBuilableItemList()
    {
        return itemCosts;
    }

    public void newItemInstantiated(Item i)
    {
        
        if (i is ResourceItem)
        {
            resourceItems.Add(i as ResourceItem);
            return;
        } else
        {
            if (mainItemDictionary.ContainsKey(i.myGrid)) 
            {
                Debug.LogAssertion("Creating a non-resource Item on top of another one!1! " + i.myType + " not created");
            }
            mainItemDictionary.Add(i.myGrid, i);
        }
    }

    public Item getItemAtGrid(Vector3Int vec)
    {
        if (mainItemDictionary.ContainsKey(vec))
        {
            return mainItemDictionary[vec];
        } else
        {
            return null;
        }
    }

    public bool canWalkHere(Vector3Int grid)
    {
        Item i = getItemAtGrid(grid);
        if (i != null && i.walkable == false)
        {
            if (i is ResourceItem)
                Debug.LogAssertion("res Item cought");
            return false;
        }

        return true;
    }

    public List<Resource> sumAvailableResources()
    {
        var poolItems = resourceItems.Where(x => x.tapped == false).ToList();
        if (poolItems.Count == 0)
            return new List<Resource>();
        Dictionary<string, int> resDictionary = new Dictionary<string, int>();
        
        foreach (var r in poolItems)
        {
            if (resDictionary.ContainsKey(r.contains.name))
            {
                resDictionary[r.contains.name] += r.contains.amount;
            } else
            {
                resDictionary.Add(r.contains.name, r.contains.amount);
            }
        }
        List<Resource> toReturn = new List<Resource>();
        foreach (var entry in resDictionary)
        {
            toReturn.Add(new Resource(entry.Key, entry.Value));
        }

        toReturn.OrderBy(x => x.name).ToList();
        return toReturn;
    }

    public void tapItem(Vector3Int location)
    {
        tappedItemGrid.Add(location);
    }

    public bool isItemTapped(Vector3Int location)
    {
        if (tappedItemGrid.Contains(location))
            return true;
        return false;
    }

    public void untapItem(Vector3Int location)
    {
        if (tappedItemGrid.Contains(location))
        {
            tappedItemGrid.Remove(location);
        }
    }

    public void newHarvestableGrid(HarvestableGrid thevec)
    {
        //tapItem(thevec.myGrid);
        activeHarvestableGrids.Add(thevec);
    }

    public void removeHarvestableGrid(HarvestableGrid har)
    {
        untapItem(har.myGrid);
        activeHarvestableGrids.Remove(har);
    }

    public HarvestableGrid addNewHarvestableGrid(Vector3Int grid, string name)
    {
        foreach (HarvestableGrid g in activeHarvestableGrids)
        {
            if (g.myGrid == grid)
                return g;
        }
        //calls new har grid, taps it
        return new HarvestableGrid(grid, name);
    }
}

