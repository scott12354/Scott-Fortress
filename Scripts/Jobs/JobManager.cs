using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct StandByJobReqs {
	public List<int> serials;
	public Job theJob;

	public StandByJobReqs(List<int> serialsIn, Job theJobIn) {
		serials = serialsIn;
		theJob = theJobIn;
	}

    //current support for build Job only
    public bool containsItem(Item i)
    {
        if (!(theJob is BuildJob)) {
            Debug.LogAssertion("Not yet implemented, trying to check if a jobque job contains an item but the queued job isnt a build job");
            return false;
        }
        foreach (Item ii in (theJob as BuildJob).constructionItems)
        {
            if (ii.mySerial == i.mySerial)
                return true;
        }
        return false;
    }

    public void swapItem(ResourceItem oldItem, ResourceItem newItem)
    {
        if (!(theJob is BuildJob))
        {
            Debug.LogAssertion("Not yet implemented, trying to swap items in a non-build job");
            return;
        }
        for (int z=0;z < (theJob as BuildJob).constructionItems.Count;z++)
        {
            if ((theJob as BuildJob).constructionItems[z].mySerial == oldItem.mySerial)
            {
                (theJob as BuildJob).constructionItems[z] = newItem;
                return;
            }
        }
        Debug.LogAssertion("Couldnt find item to swap");
    }
}

public class JobManager {

    public List<Job> jobQue;
	public List<StandByJobReqs> standByJobQue;



    public GameObject jobIndicatorPrefab;

    private static JobManager _instance;
    public static JobManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new JobManager();
            return _instance;
        }
        set
        {
            Debug.LogAssertion("Calling the setter for a singleton!!!");
        }

    }

    public JobManager()
    {
        jobQue = new List<Job>();
		standByJobQue = new List<StandByJobReqs> ();

        //Dont create any jobs here, it breaks everything.
    }

	public void newStandbyJob(List<int> serials, Job j) {
		var sbjr = new StandByJobReqs (serials, j);
		standByJobQue.Add (sbjr);
	}

    public bool getJobFor(Minion g)
    {
        if (jobQue.Count == 0)
        {
            g.myJob = null;
            return false;
        }
        else
        {
            g.myJob = jobQue[0];
            jobQue.RemoveAt(0);
            return true;
        }
    }

    public void removeJobFromQue(Job j)
    {
        jobQue.Remove(j);
    }

    public void completedJob(int mySerial)
    {
		foreach (var item in standByJobQue) {
			foreach (var i in item.serials) {
				if (mySerial == i) {
					item.serials.Remove (i);
					if (item.serials.Count == 0) {
						jobQue.Add (item.theJob);
					}
					return;
				}
			}
		}
    }

    public Job newCarryJob(Vector3Int destination, Item i, int amount)
    {
        var j = new CarryJob(i, destination, amount);
        jobQue.Add(j);
        return j;
    }

    public Job newHarvestJob(HarvestableGrid i)
    {
		foreach (var job in jobQue) {
			HarvestJob hj = null;
			if (job is HarvestJob) {
				hj = job as HarvestJob;
			} else {
				continue;
			}
			if (hj.theItem.mySerial == i.mySerial)
				return null;
		}
        var j = new HarvestJob(i);
        jobQue.Add(j);
        return j;
    }

	public Job newBuildJob(string theItemString, Vector3Int vec) {

        //Have to create a gatherResources Job too
        var itemCostData = ItemManager.itemData.getCostForItem(theItemString);
        var theItemsToGet = ItemManager.Instance.searchForAndGetResources(ItemManager.itemData.getCostForItem(theItemString).resources);
        if (theItemsToGet == null)
        {
            return null; //all the required resources couldnt be found
        }

		//Check to see if theres something on top of it already:
		Job preReqJob = null;
        Item itemCheck = ItemManager.Instance.getItemAtGrid(vec);

        string tempName = MapManager.Instance.getTileNameAt(vec, 1);
        if (tempName != "") {
            preReqJob = newHarvestJob(new HarvestableGrid(vec, tempName));
		}

		List<int> reqSerials = new List<int> ();
		if (preReqJob != null)
			reqSerials.Add (preReqJob.getSerial());
        foreach (var i in theItemsToGet)
        {
			var cj = new CarryJob (i, vec, i.contains.amount);
			reqSerials.Add (cj.getSerial ());
            jobQue.Add(cj);
        }
		var j = new BuildJob (vec, theItemString, itemCostData.buildTime, theItemsToGet);
		newStandbyJob (reqSerials, j);
		return j;
	}

	public Job pickedUpResources(Minion minion)
	{
		var carrieditems = minion.getCarriedItems();
		var item = carrieditems [carrieditems.Count - 1];
		ContainerItem i = ItemManager.Instance.getClosestContainerThatFits(minion, item as ResourceItem);
        if (i == null) {
			Vector3Int t = MapManager.Instance.getClosestOpenTile (minion.myGrid);
			//drop it like its hot
			carrieditems.Remove(item);
			return new CarryJob(item, t, (item as ResourceItem).contains.amount);
        }
        else
        {
            var j = new DepositResourceJob(i,carrieditems[carrieditems.Count-1] as ResourceItem);
            return j; 
        }
    }

    public Job newDepositResourcesJob(ResourceItem res)
    {

        //TODO all this crazy code could probably be removed by simply
        //checking if the item is tapped...
		var joblist = new List<Job> ();
		jobQue.ForEach (x => joblist.Add (x));
		foreach (var m in GameManager.Instance.activeMinions) {
			if (m.myJob != null) {
				joblist.Add (m.myJob);
			}
		}
		foreach (Job j in joblist) {
			string tempstring = j.GetType ().ToString ();
			switch (tempstring) {
				case "DepositResourceJob":
					var jj = j as DepositResourceJob;
					if (jj.itemToDeposit.mySerial == res.mySerial)
						return null;
					break;
				case "CarryJob":
					var jc = j as CarryJob;
					if (jc.theItem.mySerial == res.mySerial)
						return null;
					break;
				default:
					//Other type of Job that doesnt have an item.
					break;
			}

		}
        var container = ItemManager.Instance.getClosestContainerThatFits(res.myGrid, res);
		if (container == null) {
			return null; 
		} else {
			DepositResourceJob j = new DepositResourceJob (container, res);
			jobQue.Add (j);
			return j;
		}
    }

    public void returnJob(Job myJob)
    {
        Debug.Log("I haven't properly implemented this yet, job is just being thrown out now");
    }

    public Job getJobWithSerial(int serial)
    {
        return jobQue.FirstOrDefault(x => x.getSerial() == serial);
    }

    public void splitResourceCB(ResourceItem originalItem, ResourceItem newItemCarried)
    {
        if (originalItem.mySerial == newItemCarried.mySerial)
            return;
        foreach (var stbyj in standByJobQue)
        {
            if (stbyj.containsItem(originalItem))  //BREAKPOINT HERE FOR CHECKING IF THIS EVER HAPPENS
            {
                stbyj.swapItem(originalItem, newItemCarried);
            }
        }
        originalItem.tapped = false;
        newItemCarried.tapped = true;
    }

    public bool keepItemTapped(Item i)
    {
        foreach (var item in standByJobQue)
        {
            if (item.containsItem(i))
                return true;
        }
        return false;
    }

    public Job newHuntJob(Animal a)
    {
        foreach(Job i in jobQue)
        {
            if (i is HuntJob && ((i as HuntJob).beingHunted.myGO == a.myGO))
                return null;
        }
        var j = new HuntJob(a);
        jobQue.Add(j);
        return j;
    }

    public Job newDigJob(Vector3Int theGrid)
    {
        if (!MapManager.Instance.isLocationClear(theGrid)) 
        {
            DigJob j = new DigJob(theGrid);
            jobQue.Add(j);
            return j;
        } else
        {
            //already harvested
            return null;
        }
    }

}
