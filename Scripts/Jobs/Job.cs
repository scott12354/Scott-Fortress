
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Job
{

    public enum JobType { BUILD = 0, CARRY, HARVEST, DEPOSIT, HUNT, DIG};
    public JobType myType;

    private static int startingSerial = 0;
    protected int mySerial;


    public Job(JobType jt)
    {
        mySerial = startingSerial++;
        myType = jt;
    }

    //public Job(JobSaveData j, JobType myTypeIn)
    //{
    //    myType = myTypeIn;
    //    mySerial = j.mySerial;
    //    if (mySerial >= startingSerial)
    //    {
    //        startingSerial = mySerial + 1;
    //    }
    //}

	public int getSerial() {
		return mySerial;
	}

    public virtual Vector3Int getJobLocation()
    {
        Debug.LogAssertion("CALLED THE BASE JOB CLASS GET JOB LOCATION FUNCTION, THIS SHOULD NOT HAPPEN");
        return new Vector3Int(0, 0,0);
    }

    new public abstract string ToString();
    
    ~Job()
    {
		//string s = this.GetType ().ToString ();
		//Debug.Log ("Job of type: " + s + " was just completed.  Serial# " + mySerial + ".");
        JobManager.Instance.completedJob(mySerial);
    }
}

public class CarryJob : Job {

    public Vector3Int destinationGrid, pickupGrid;
    public Item theItem;
    int amountToCarry;


    public CarryJob(Item i, Vector3Int destination, int amountToCarryIn) : base(JobType.CARRY)
    {
        destinationGrid = destination;
        theItem = i;
        i.tapped = true;
        amountToCarry = amountToCarryIn;
        pickupGrid = theItem.myGrid;
        theItem.tapped = true;
    }

    //public CarryJob(CarryJobSaveData sav, Item theItemIn) : base(sav, JobType.CARRY)
    //{
    //    destinationGrid = sav.destinationGrid;
    //    pickupGrid = sav.pickupGrid;
    //    amountToCarry = sav.amountToCarry;
    //    theItem = theItemIn;
    //}

    //public CarryJobSaveData getSaveData()
    //{
    //    var sav = new CarryJobSaveData();
    //    sav.destinationGrid = destinationGrid;
    //    sav.pickupGrid = pickupGrid;
    //    sav.mySerial = mySerial;
    //    sav.itemSerial = theItem.mySerial;
    //    sav.amountToCarry = amountToCarry;

    //    return sav;
    //}

    public int getAmount()
    {
        return amountToCarry;
    }

    ~CarryJob()
    {
        theItem.tapped = false;
    }

    public override string ToString()
    {
        return "Carrying " + amountToCarry.ToString() + " " + theItem.myType + " to grid: " + destinationGrid.ToString();
    }
}

public class DepositResourceJob : Job
{
    public ContainerItem destinationItem;
    public ResourceItem itemToDeposit;

    public DepositResourceJob(ContainerItem i, ResourceItem it) : base(JobType.DEPOSIT)
    {
        it.tapped = true;
        destinationItem = i;
        itemToDeposit = it;
    }

    //public DepositResourceJob(DespositResourceJobSaveData data, ContainerItem cont, ResourceItem res) : base(data, JobType.DEPOSIT)
    //{
    //    destinationItem = cont;
    //    itemToDeposit = res;
    //}

    //public DespositResourceJobSaveData getSaveData()
    //{
    //    var sav = new DespositResourceJobSaveData();
    //    sav.mySerial = mySerial;
    //    sav.destinationItemSerial = destinationItem.mySerial;
    //    sav.itemToDepositSerial = itemToDeposit.mySerial;

    //    return sav;
    //}

    public override Vector3Int getJobLocation()
    {
        return destinationItem.myGrid;
    }

    ~DepositResourceJob()
    {
        itemToDeposit.tapped = false;
    }

    public override string ToString()
    {
        return "Depositing " + itemToDeposit.contains.amount + " " + itemToDeposit.myType + " " + " to the " + destinationItem.myType + " at grid: " + destinationItem.myGrid.ToString();
    }
}

public class HarvestJob : Job
{
    public HarvestableGrid theItem;

    public HarvestJob(HarvestableGrid harGrid) : base(JobType.HARVEST)
    {
        theItem = harGrid;
    }

    //public HarvestJob(HarvestJobSaveData data, HarvestableItem itemIn) : base(data, JobType.HARVEST)
    //{
    //    theHarItem = itemIn;
    //}

    //public HarvestJobSaveData getSaveData()
    //{
    //    var sav = new HarvestJobSaveData();

    //    sav.mySerial = mySerial;
    //    sav.theHarItemSerial = theHarItem.mySerial;
    //    return sav;
    //}

    public override Vector3Int getJobLocation()
    {
        return theItem.myGrid;
    }

    public override string ToString()
    {
        return "Harvesting " + theItem.name + " at grid: " + theItem.myGrid.ToString();
    }
}

public class BuildJob : Job
{
    string itemToBuild;
    Vector3Int theLocation;
    public float buildTime;
    public float progressTime=0;
	//Just the list of resources, not the actual items
    public List<ResourceItem> constructionItems;

    public BuildJob(Vector3Int location, string item, 
        float buildTimeIn, List<ResourceItem> conIn) : base(JobType.BUILD)
    {
        itemToBuild = item;
        theLocation = location;
        buildTime = buildTimeIn;
        constructionItems = conIn;
        conIn.ForEach(x => x.tapped = true);
    }

    //public BuildJob(BuildJobSaveData data, List<ResourceItem> resitems) : base(data, JobType.BUILD)
    //{
    //    itemToBuild = data.itemToBuild;
    //    theLocation = data.theLocation;
    //    buildTime = data.buildTime;
    //    constructionItems = resitems;
    //}

    //public BuildJobSaveData getSaveData()
    //{
    //    var sav = new BuildJobSaveData();
    //    sav.itemToBuild = itemToBuild;
    //    sav.theLocation = theLocation;
    //    sav.buildTime = buildTime;
    //    sav.mySerial = mySerial;
    //    sav.constructionItemSerials = new int[constructionItems.Count];

    //    for (int i=0;i< constructionItems.Count;i++)
    //    {
    //        sav.constructionItemSerials[i] = constructionItems[i].mySerial;
    //    }
    //    return sav;
    //}

    public override Vector3Int getJobLocation()
    {
        return theLocation;
    }

    public string itemToBuildString()
    {
        return itemToBuild;
    }

    public void startBuilding()
    {
        foreach (var item in constructionItems)
        {
            //Destroy items immediately, building has started
            ItemManager.Instance.removeItem(item);
        }
    }

    public override string ToString()
    {
        return "Building a " + itemToBuild + " at grid: " + theLocation.ToString();
    }
}

public class HuntJob : Job
{
    public Being beingHunted;

    public HuntJob(Being b) : base(JobType.HUNT)
    {
        beingHunted = b;
    }

    //public HuntJob(HuntJobSaveData data, Animal a) : base(data, JobType.HUNT)
    //{
    //    beingHunted = a;
    //}

    //public HuntJobSaveData getSaveData()
    //{
    //    var sav = new HuntJobSaveData();
    //    sav.mySerial = mySerial;
    //    sav.beingHunted = beingHunted.getSerial();
    //    return sav;
    //}

    public override string ToString()
    {
        return "Hunting: " + beingHunted.myType;
    }
}

public class DigJob : Job
{
    Vector3Int gridToDig;

    public DigJob(Vector3Int grid) : base(JobType.DIG)
    {
        gridToDig = grid;
    }

    public override string ToString()
    {
        return "Digging at: " + gridToDig.ToString();
    }

    public override Vector3Int getJobLocation()
    {
        return gridToDig;
    }
}


