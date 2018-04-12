using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalState {
    IDLE,
    IDLECOOLDOWN,
    WANDERING,
    RUNNING,
}

public class Animal : Being{
    AnimalState myState = AnimalState.IDLE;

    public Animal(Vector3Int vecIn, string typeIn) : base(typeIn, vecIn)
    {
 
    }

    public Animal(string typeIn) : this(Vector3Int.zero, typeIn) { }

    //public Animal(AnimalSaveData data) : base(data)
    //{
    //    myState = data.myState;
    //}

    public override void gameManagerUpdate(float secondsElapsed)
    {
        if (myStats.isDead)
        {
            //killing code here
        }
        myStats.update(secondsElapsed);
        secondsAlive += secondsElapsed;

        switch (myState)
        {
            case AnimalState.IDLE:
                List<Vector3Int> tiles = MapManager.Instance.getClearMapTilesWithinDistance(myGrid, 3);
                tiles = GameManager.shuffleList(tiles);
                MoveMe(tiles[0]);
                myState = AnimalState.WANDERING;
                break;
            case AnimalState.WANDERING:
                if (movementSteps.Count==0)
                {
                    myState = AnimalState.IDLECOOLDOWN;
                    GameManager.Instance.delayThenCall(() => idleDoneCoolingDown(),
                        GameManager.Instance.minionCooldownDelay);
                }
                break;
            case AnimalState.RUNNING:
                Minion m = GameManager.Instance.whoIsHuntimeMe(this);
                if (movementSteps.Count == 0 && m != null)
                {
                    if (m == null)
                    {
                        Debug.LogAssertion("who is hunting me function failed");
                    }
                    else
                    {
                        var goingTiles = MapManager.Instance.getMapTilesWithinDistance(myGrid, 5);
                        goingTiles = goingTiles.OrderByDescending(
                            x => Vector3Int.Distance(m.myGrid, x)).ToList();
                        MoveMe(goingTiles[0]);
                    }
                } else if( m == null)
                {
                    myState = AnimalState.IDLE;
                }
                break;
            default:
                break;
        }

    }

    //public AnimalSaveData getSaveData()
    //{
    //    var sav = new AnimalSaveData();
    //    sav.mySerial = mySerial;
    //    sav.myGrid = myGrid;
    //    sav.worldGrid = worldGrid;
    //    sav.movementSteps = movementSteps.ToArray();
    //    sav.type = myType;
    //    sav.secondsAlive = secondsAlive;
    //    sav.myStats = myStats.getSaveData();
    //    sav.myState = myState;
    //    return sav;

    //}

    private void idleDoneCoolingDown()
    {
        myState = AnimalState.IDLE;
    }

    public override bool strikeMe()
    {


        //also start running here!!
        if (myState != AnimalState.RUNNING)
            movementSteps.Clear();
        myState = AnimalState.RUNNING;
        return base.strikeMe();
        //Kill me is run by the base caller
    }

    public override void killMe()
    {
        base.killMe();
    }

}
