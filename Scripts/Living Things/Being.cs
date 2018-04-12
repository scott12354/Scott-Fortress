using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Being {
    private static int theSerial = 0;
    protected int mySerial;
    public Vector3Int myGrid;
    public GameObject myGO;
    public List<Vector3Int> movementSteps;
    public string myType;
    protected float secondsAlive;
    protected Vector3 worldGrid;
    bool animationActive = false;
    Animator anim;
    protected List<Resource> droppedResourcesOnDeath;

    public StatsGroup myStats = new StatsGroup();

    public Being(string typeIn, Vector3Int gridIn)
    {
        mySerial = theSerial++;
        myType = typeIn;
        myGO = GameObject.Instantiate(GameManager.Instance.minionPrefab);
        myGO.transform.SetParent(GameManager.Instance.beingFolder.transform);

        //Animation Stuff
        RuntimeAnimatorController r = Resources.Load<RuntimeAnimatorController>("Animations\\"+ typeIn + "\\" + typeIn + " Animation Controller");
        if (r==null)
        {
            //disabled by default
            
        } else
        {
            animationActive = true;
            anim = myGO.GetComponent<Animator>();
            anim.runtimeAnimatorController = r;
            anim.enabled = true;
        }

        quickMoveToGrid(gridIn);
        movementSteps = new List<Vector3Int>();
        var sr = myGO.GetComponent<SpriteRenderer>();
        sr.sprite = SpriteLibrary.Instance.getSprite(myType);
        worldGrid = new Vector2(GameManager.Instance.tileSize.x * myGrid.x,
            GameManager.Instance.tileSize.y * myGrid.y);
        var bd = ItemManager.itemData.getBeingDataFor(typeIn);
        if (bd != null)
            droppedResourcesOnDeath = bd.contentsOnDeath.ToList();
    }

    //public Being(BeingSaveData data)
    //{
    //    mySerial = data.mySerial;
    //    if (mySerial >= theSerial)
    //    {
    //        theSerial = mySerial + 1;
    //    }
    //    myType = data.type;
    //    myGO = GameObject.Instantiate(GameManager.Instance.minionPrefab);
    //    myGO.transform.SetParent(GameManager.Instance.beingFolder.transform);

    //    //Animation Stuff
    //    RuntimeAnimatorController r = Resources.Load<RuntimeAnimatorController>("Animations\\" + myType + "\\" + myType + " Animation Controller");
    //    if (r == null)
    //    {
    //        //disabled by default

    //    }
    //    else
    //    {
    //        animationActive = true;
    //        anim = myGO.GetComponent<Animator>();
    //        anim.runtimeAnimatorController = r;
    //        anim.enabled = true;
    //    }

    //    quickMoveToGrid(data.myGrid);
    //    movementSteps = new List<Vector3>();
    //    movementSteps.AddRange(data.movementSteps);
    //    var sr = myGO.GetComponent<SpriteRenderer>();
    //    sr.sprite = SpriteLibrary.Instance.getSprite(myType);
    //    worldGrid = data.worldGrid;
    //    var bd = ItemManager.itemData.getBeingDataFor(myType);
    //    if (bd != null)
    //        droppedResourcesOnDeath = bd.contentsOnDeath.ToList();
    //    secondsAlive = data.secondsAlive;
    //    myStats = new StatsGroup(data.myStats);
    //}

    public void assignMovementArray(Vector3Int[] additions)
    {
        movementSteps.AddRange(additions);
    }

    public virtual void MoveMe(Vector3Int destination)
    {
        if (myGrid == destination)
        {
            return;
        }
        if (myGrid.z == destination.z)
        {
            //For same level movements
            if (Vector3.Distance(myGrid, destination) < 1.1)
            {
                movementSteps.Add(destination);
            }
            else
            {
                var steps = MapManager.Instance.getPathLayerChange(myGrid, destination);
                if (steps == null)
                {
                    Debug.Log(myType + " tried to move to an inacessable location");
                }
                else
                {
                    movementSteps.AddRange(steps);
                }
            }
        }
        else
        {
            //Movement through mineshaft is required
            var steps = MapManager.Instance.getPathLayerChange(myGrid, destination);
            if (steps == null)
            {
                Debug.Log(myType + " tried to move to an inacessable location");
            }
            else
            {
                movementSteps.AddRange(steps);
            }
        }
    }

    public virtual void quickMoveToGrid(Vector3Int moveMeTo)
    {
        quickMoveToWorldGrid(MapManager.Instance.convertToWorldGrid(moveMeTo));
    }

    protected void quickMoveToWorldGrid(Vector3 newWorldGridVector)
    {
        myGrid = MapManager.Instance.convertToGameGrid(newWorldGridVector);
        worldGrid = newWorldGridVector;
        //if (newWorldGridVector.z != 0)
        //    newWorldGridVector.x += MapManager.Instance.getUGLevel(newWorldGridVector.z).offset*GameManager.Instance.pixelsPerUnitx;
        myGO.transform.position = newWorldGridVector;
    }

    public abstract void gameManagerUpdate(float secondsElapsed);

    public virtual void movementTick(float distanceToGo)
    {
        if (movementSteps.Count > 0)
        {
            if (movementSteps[0].z != myGrid.z)
            {
                quickMoveToGrid(movementSteps[0]);
                movementSteps.RemoveAt(0);
                return;
            }
            var nextStep = MapManager.Instance.convertToWorldGrid(movementSteps[0]);
            var newWorldGrid = Vector3.MoveTowards(worldGrid, nextStep,
                distanceToGo);
            //ANimation
            if (animationActive)
            {
                var deltaVector = newWorldGrid - worldGrid;
                anim.SetFloat("InputX", deltaVector.x);
                anim.SetFloat("InputY", deltaVector.y);
                anim.SetBool("Moving", true);
            }

            quickMoveToWorldGrid(newWorldGrid);

            var myGridAsWorldGrid = MapManager.Instance.convertToWorldGrid(myGrid);
            if (myGridAsWorldGrid == nextStep)
            {
                movementSteps.RemoveAt(0);
            }

        } else
        {
            if (animationActive)
            {
                anim.SetBool("Moving", false);

            }
            //Not moving, freeze animator
        }
    }

    public bool withinHuntingRange(Vector3 worldGridIn)
    {
        float rangeMax = GameManager.Instance.tileSize.x * 1.5f;
        var distance = Vector2.Distance(worldGridIn, worldGrid);
        if (distance > rangeMax) return false;
        return true;
    }

    public virtual bool strikeMe()
    {
        if (myStats.isDead)
            return true;
        //For now we'll use 20 damage each.
        myStats.damage(20f);
        if (myStats.isDead)
        {
            killMe();
            return true;
        }
        else
        {
            //possibly change the sprite here?
            var sr = myGO.GetComponent<SpriteRenderer>();
            sr.color = GameManager.Instance.damageTintColour;
            Action toCall = () => sr.color = Color.white;
            GameManager.Instance.delayThenCall(toCall, .5f);
            return false;
        }
    }

    public virtual void killMe()
    {
        GameManager.Instance.beingDied(this);
        GameManager.Instance.flagForDestruction(myGO);
        if (droppedResourcesOnDeath == null)
            return;
        foreach( var i in droppedResourcesOnDeath)
        {
            ItemManager.Instance.addNewResourceItem(i.name, i.amount, myGrid);
        }
    }

    public int getSerial()
    {
        return mySerial;
    }
}
