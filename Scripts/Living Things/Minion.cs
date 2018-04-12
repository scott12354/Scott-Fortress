using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Minion : Being {

    public enum MinionState
    {
        IDLE,
        TRUEIDLE,
        IDLECOOLDOWN,
        CARRYJOBGOTO,
        CARRYJOBDELIVER,
        HARVESTJOBGOTO,
        HARVESTJOBHARVEST,
		DEPOSITJOBGOTOPICKUP,
        DEPOSITRESOURCESGOTO,
        BUILDJOBGOTO,
        BUILDJOBBUILD,
        HUNTCHASE,
        HUNTSTRIKE,
        DIGGOTO,
        DIGWAIT,
        DIGCOMPLETE,

    }
    private MinionState _myState;
    public MinionState myState
    {
        get
        {
            return _myState;
        }
        set
        {
            if (value == MinionState.IDLECOOLDOWN)
			{
				myJob = null;
                Action signalAction= () => signalDoneWaiting();
                GameManager.Instance.delayThenCall(signalAction, GameManager.Instance.minionCooldownDelay);
                //GameManager.Instance.delayForMinion(this, GameManager.Instance.minionCooldownDelay);
            }
            _myState = value;
        }
    }

    private List<ResourceItem> myCarriedItems = new List<ResourceItem>();
    public Job myJob;
    private bool strikeCooledDown = true;

    // Use this for initialization
    public Minion (Vector3Int vecIn) :base("Minion", vecIn) {
        var spgo = GameManager.Instance.MinionStatsPanelMain;
        spgo.SendMessage("newPanel", this);
	}

    //public Minion(MinionSaveData m, List<ResourceItem> carriedItems) : base (m)
    //{
    //    var spgo = GameManager.Instance.MinionStatsPanelMain;
    //    spgo.SendMessage("newPanel", this);

    //    //myJob = SaveGameLoader.getJobFromDataContainer(m.myJob);
    //    //Jobs are set by the data loader funtins
    //    myState = m.myState;
    //    myCarriedItems = carriedItems;
    //}

    public override void gameManagerUpdate(float secondsElapsed)
    {
        secondsAlive += secondsElapsed;
        myStats.update(secondsElapsed);
        
        switch (myState)
        {
            case MinionState.IDLE:
                if (myCarriedItems.Count > 0)
                {
					myJob = JobManager.Instance.pickedUpResources(this);
					if (myJob != null) {
						if (myJob is DepositResourceJob) {
							myState = MinionState.DEPOSITRESOURCESGOTO;
							MoveMe (((DepositResourceJob)myJob).getJobLocation ());
						} else if (myJob is CarryJob) {
							myState = MinionState.CARRYJOBGOTO;
							MoveMe((myJob as CarryJob).pickupGrid);
						} else {
							Debug.LogAssertion ("Job type error. Didn't expect Jobtype: " + myJob.GetType ().ToString () + ".");
							return;
						}
					}
            	}
			if (myJob == null)
                {
                    checkForJob();
                }
                break;

            #region BuildJob
            case MinionState.BUILDJOBGOTO:
                if (movementSteps.Count == 0)
                {
                    //Found area, harvest item
                    (myJob as BuildJob).startBuilding(); //Comsumes Objects
                    myState = MinionState.BUILDJOBBUILD;
                }
                break;
            case MinionState.BUILDJOBBUILD:

                float buildTime = ((BuildJob)myJob).buildTime;
                ((BuildJob)myJob).progressTime += secondsElapsed;

                if (((BuildJob)myJob).progressTime >= buildTime)
                {
                    var j = myJob as BuildJob;
                    ItemManager.Instance.addNewItem(j.itemToBuildString(), j.getJobLocation());
                    //need to consume the ingredients;
                    foreach (var i in j.constructionItems)
                    {
                        myCarriedItems.Remove(i);
                        ItemManager.Instance.removeItem(i);
                    }
                    myState = MinionState.IDLECOOLDOWN;
                    //GameManager.Instance.updatePathfindingGraph();
                }
                break;

            #endregion
            #region Carry Job
            case MinionState.CARRYJOBGOTO:
                if(movementSteps.Count==0)
                {
                    //Found area, pickup item
                    var cj = myJob as CarryJob;
                    if (cj.theItem is ResourceItem)
                    {
                        myCarriedItems.Add((cj.theItem as ResourceItem).takeAmount(cj.getAmount()));
                        JobManager.Instance.splitResourceCB(cj.theItem as ResourceItem, myCarriedItems[myCarriedItems.Count - 1] as ResourceItem);
                    }
                    else
                    {
                        if (((CarryJob)myJob).theItem is ResourceItem)
                        {
                            myCarriedItems.Add(((CarryJob)myJob).theItem as ResourceItem);
                        } else
                        {
                            Debug.LogAssertion("Carrying non resource Item");
                        }
                        
                    }
                    myState = MinionState.CARRYJOBDELIVER;
                    MoveMe(((CarryJob)myJob).destinationGrid);
                }
                break;
            case MinionState.CARRYJOBDELIVER:
                if (movementSteps.Count == 0)
                {
                    if (((CarryJob)myJob).theItem is ResourceItem)
                    {
                        myCarriedItems.Remove(((CarryJob)myJob).theItem as ResourceItem);
                        ((CarryJob)myJob).theItem.putDown();
                        myState = MinionState.IDLECOOLDOWN;
                    } else
                    {
                        Debug.LogAssertion("Carrying non resource Item");
                    }
                }
                break;
            #endregion
            #region HarvestJob
            case MinionState.HARVESTJOBGOTO:
                if (movementSteps.Count ==0)
                {
                    HarvestableGrid g = ((HarvestJob)myJob).theItem;
                    if (Vector3Int.Distance(g.myGrid, myGrid) > 1.5)
                    {
                        JobManager.Instance.returnJob(myJob);
                        myState = MinionState.IDLECOOLDOWN;
                    }
                    else
                    {
                        myState = MinionState.HARVESTJOBHARVEST;
                    }
                }
                break;
            case MinionState.HARVESTJOBHARVEST:
                ((HarvestJob)myJob).theItem.harvestProgress += secondsElapsed;
                float temptime = ((HarvestJob)myJob).theItem.harvestProgress;
                if (temptime >= ((HarvestJob)myJob).theItem.harvestTime)
                {
                    //GameManager.Instance.updatePathfindingGraph();
                    HarvestableGrid res = ((HarvestJob)myJob).theItem;
                    var resourceItemToBePickedUp = res.harvestMe();
                    if (resourceItemToBePickedUp != null)
                    {
                        myCarriedItems.Add(resourceItemToBePickedUp); //minion will automatically carry the item to a container when it is left in the inventory
                        resourceItemToBePickedUp.tapped = true;
                    }
                    myState = MinionState.IDLECOOLDOWN;
                    break;
                }
                break;

            #endregion
            #region DepositJob
		case MinionState.DEPOSITJOBGOTOPICKUP:
			if (movementSteps.Count ==0) {
				myCarriedItems.Add((myJob as DepositResourceJob).itemToDeposit);
				(myJob as DepositResourceJob).itemToDeposit.pickUp();
				MoveMe((myJob as DepositResourceJob).destinationItem.myGrid);
				myState = MinionState.DEPOSITRESOURCESGOTO;
			}
			break;
            case MinionState.DEPOSITRESOURCESGOTO:
                if (movementSteps.Count == 0)
                {
                    DepositResourceJob j = (DepositResourceJob)myJob;
                    List<ResourceItem> toRemove = new List<ResourceItem>();
                    foreach (ResourceItem r in myCarriedItems)
                    {
                        //Weird error work around
                        //if(j==null && myCarriedItems.Count >0)
                        //{
                        //    myJob = new DepositResourceJob(ItemManager.Instance.getClosestContainerThatFits(this, myCarriedItems[0]), myCarriedItems[0]);
                        //    j = myJob as DepositResourceJob;
                        //}
                        if(j.destinationItem.addResource(r))
                        {
                            toRemove.Add(r);
                        }
                    }
                    foreach (var r in toRemove)
                    {
                        myCarriedItems.Remove(r);
                        //dont need to call drop item here, container items "Carry" the tiems
                    }
                    myState = MinionState.IDLECOOLDOWN;
                }
                break;
            #endregion
            #region Hunt Job
            case MinionState.HUNTCHASE:
                var jo = (myJob as HuntJob);
                if (jo.beingHunted.withinHuntingRange(worldGrid))
                {
                    myState = MinionState.HUNTSTRIKE;
                } else if (movementSteps.Count > 0 && // still moving
                    !jo.beingHunted.withinHuntingRange( //computes if end position is out of range
                        MapManager.Instance.convertToWorldGrid(
                            movementSteps[movementSteps.Count - 1])))
                {
                    //destination is too far away, recycle movement stesp
                    movementSteps.Clear();
                    MoveMe((myJob as HuntJob).beingHunted.myGrid);
                } else if (movementSteps.Count == 0)
                {
                    //you wont get to this point in the loop unless out of range
                    //so just move closer again;
                    MoveMe(jo.beingHunted.myGrid);
                } else
                {
                    //keep moving
                }
                break;
            case MinionState.HUNTSTRIKE:
                if (!strikeCooledDown)
                    break;
                if ((myJob as HuntJob).beingHunted.withinHuntingRange(worldGrid))
                {
                    bool targetDied = false;
                    //strike code
                    if ((myJob as HuntJob).beingHunted.strikeMe())
                    {
                        targetDied = true;
                    }
                    strikeCooledDown = false;
                    Action act = () => strikeCooledDown = true;
                    GameManager.Instance.delayThenCall(act, GameManager.Instance.strikeCooldownTime);
                    if (targetDied)
                    {
                        myState = MinionState.IDLECOOLDOWN;
                    }
                } else
                {
                    myState = MinionState.HUNTCHASE;
                }
                break;
            #endregion
            #region Dig Job
            case MinionState.DIGGOTO:
                if (movementSteps.Count == 0)
                {
                    //arrived
                    Action stopDigging = () => myState = MinionState.DIGCOMPLETE;
                    myState = MinionState.DIGWAIT;
                    GameManager.Instance.delayThenCall(stopDigging, 1.0f);
                }
                break;
            case MinionState.DIGWAIT:
                //just wait here
                break;
            case MinionState.DIGCOMPLETE:
                var dj = myJob as DigJob;
                var ugl = MapManager.Instance.getUGLevel(dj.getJobLocation().z);
                ugl.harvestGrid(dj.getJobLocation());
                myState = MinionState.IDLECOOLDOWN;
                break;
            #endregion
            case MinionState.IDLECOOLDOWN:
                break;
            default:
                break;
        }
    }

	public List<ResourceItem> getCarriedItems() {
		return myCarriedItems;
	}
	
    public void checkForJob()
    {
        //this statement checks for a job, if received, returns true and starts job
        if (JobManager.Instance.getJobFor(this))
        {
            switch (myJob.myType)
            {
                case Job.JobType.BUILD:
                    myState = MinionState.BUILDJOBGOTO;
                    MoveMe((myJob as BuildJob).getJobLocation());
                    break;
                case Job.JobType.HUNT:
                    var j = myJob as HuntJob;
                    MoveMe(j.beingHunted.myGrid);
                    myState = MinionState.HUNTCHASE;
                    break;
				case Job.JobType.DEPOSIT:
					DepositResourceJob dj = myJob as DepositResourceJob;
					if (myCarriedItems.Count ==0 || myCarriedItems [myCarriedItems.Count - 1].mySerial == dj.itemToDeposit.mySerial) {
						MoveMe (dj.itemToDeposit.myGrid);

					myState = MinionState.DEPOSITJOBGOTOPICKUP;
					} else {
						MoveMe (dj.itemToDeposit.myGrid);
					myState = MinionState.DEPOSITRESOURCESGOTO;
					}
					break;
                case Job.JobType.CARRY:
                    myState = MinionState.CARRYJOBGOTO;
                    MoveMe(((CarryJob)myJob).pickupGrid);
                    break;
                case Job.JobType.HARVEST:
                    myState = MinionState.HARVESTJOBGOTO;
                    var jjj = ((HarvestJob)myJob);
                    MoveMe(jjj.getJobLocation());
                    break;
                case Job.JobType.DIG:
                    myState = MinionState.DIGGOTO;
                    MoveMe((myJob as DigJob).getJobLocation());
                    if (movementSteps.Count == 0)
                    {
                        myState = MinionState.IDLE;
                        myJob = null;
                    }
                    
                    break;
                default:
                    Debug.Assert(true, "JOB TYPE DOESNT EXIST IN THE MINIONS CHECK FOR JOB SWITCH SCRIPT");
                    break;
            }
        }


    }

    public void signalDoneWaiting()
    {
        switch (myState)
        {
            case MinionState.IDLECOOLDOWN:
                myState = MinionState.IDLE;
                break;
            default:
                Debug.Assert(true, "Minion State Error in SignalDoneWaiting function");
                break;
        }
    }

    public override void movementTick(float distanceToGo)
    {
        var oldGrid = myGrid;
        base.movementTick(distanceToGo);
        if (oldGrid != myGrid)
        {
            myCarriedItems.ForEach(x => x.myGrid = myGrid);
            myCarriedItems.ForEach(x => x.pickUp());
        }
    }

    //public MinionSaveData getSaveData()
    //{
    //    var sav = new MinionSaveData();
    //    sav.mySerial = mySerial;
    //    sav.myState = myState;
    //    sav.myGrid = myGrid;
    //    sav.worldGrid = worldGrid;
    //    sav.movementSteps = movementSteps.ToArray();
    //    sav.type = myType;
    //    sav.secondsAlive = secondsAlive;
    //    sav.myStats = myStats.getSaveData();

    //    sav.heldItems = new int[myCarriedItems.Count];
    //    for (int i=0;i<myCarriedItems.Count;i++)
    //    {
    //        sav.heldItems[i] = myCarriedItems[i].mySerial;
    //    }
    //    if (myJob != null)
    //    {
    //        switch (myJob.GetType().ToString())
    //        {
    //            case "CarryJob":
    //                sav.myJob = (myJob as CarryJob).getSaveData();
    //                break;
    //            case "DepositResourceJob":
    //                sav.myJob = (myJob as DepositResourceJob).getSaveData();
    //                break;
    //            case "HarvestJob":
    //                sav.myJob = (myJob as HarvestJob).getSaveData();
    //                break;
    //            case "BuildJob":
    //                sav.myJob = (myJob as BuildJob).getSaveData();
    //                break;
    //            case "HuntJob":
    //                sav.myJob = (myJob as HuntJob).getSaveData();
    //                break;
    //            default:
    //                Debug.LogAssertion("Job Type serialization error");
    //                break;
    //        }
    //    }

    //    return sav;
    //}
}