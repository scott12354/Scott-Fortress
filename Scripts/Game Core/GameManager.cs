/*
TODO:
need to save the in progress wait then call functions...?
house items currently sit in the item array, but the loader doesnt load it like that
include standbyjobs in savedata
Change Itemmanager.new____item functions to use the item constructor instead
combat
hunger
fix deposit resource job
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


public enum MouseMode {
	NULL,
	BUILD,
	GATHER,
    HUNT,
};

public enum GameSpeedState
{
    NORMAL,
    PAUSE,
    TWOTIMES
}

public class GameManager : MonoBehaviour {//Singleton<GameManager> {

    //public delegate void testDelagate(int num);

    public static GameManager Instance;
	string buildMode = "";
	public MouseMode currentMouseMode = MouseMode.NULL;
    public GameSpeedState theGamesState = GameSpeedState.NORMAL;

    public List<Minion> activeMinions = new List<Minion>();
    public List<Animal> activeAnimals = new List<Animal>();

    private List<GameObject> flaggedForDestruction = new List<GameObject>();

    private bool updatePathFinding = true;

    private bool loadComplete = false;

    public Vector3Int activeTile;

    #region Editor Variables
    [Header("Prefabs")]
    [SerializeField]
    public GameObject tilePrefab;
    [SerializeField]
    public GameObject minionPrefab;
    [SerializeField]
    public GameObject itemPrefab;
    [SerializeField]
    public GameObject mineshaftPrefab;
    [SerializeField]
    public GameObject buildIconPrefab;
    [Space]
    [Header("Tile Files")]
    public Texture2D[] mapTextures;
    [Space]
    [Header("Save Game File")]
    public string saveGamefilePath;
    public bool loadSaveFile = false;
    [Header("Minion Properties")]
    [SerializeField]
    public float minionCooldownDelay;
    public float minionMoveDelay;
    public float minionMovementStepDistance=2f;
    public float statDegredationPerSecond = 1.0f;
    public float strikeCooldownTime = 1.0f;
    [Header("Animal Properties")]
    [SerializeField]
    public float animalMovementStepDistance = 1f;
    [SerializeField]
    public Color damageTintColour;
    [Space]
    [Header("Map Size")]
    public int mapWidth;
    public int mapHeight;
    public int numUndergroundLayers = 3;
    [Space]
    [Header("TileMap Properties")]
    [SerializeField]
    public List<TileBase> tileRefs;
    [SerializeField]
    public GameObject tileGridOriginal;
    [SerializeField]
    public float chanceToCell = 45;
    [SerializeField]
    public int reqToBirth = 3;
    [SerializeField]
    public int reqToKill = 2;
    [SerializeField]
    public int iterations = 5;
    [Space]
    [Header("Tile Size")]
    public Vector2 tileSize;
    public int pixelsPerUnitx;
    public int pixelsPerUnity;
    [Space]
    [Header("Map Generation Properties")]
    [Range(0, .9f)]
    public float erodePercent = .5f;
    public int erodeIterations = 2;
	[Range(0,.9f)]
	public float bushPercent = 0.4f;
	[Range(0,.9f)]
	public float treePercent = 0.4f;
    [Range(0, .9f)]
    public float lakePercent = .05f;
	public int numWallIslands = 5;
	public float sizeWallIslands = 5.0f;
	[Space]
	[Header("Path Finding")]
	public int pathFindingIterationsMax = 100000;
    [Header("Folders")]
    [SerializeField]
    public GameObject itemFolder;
    public GameObject beingFolder;
    [Space]
    [Header("Game Object Links")]
    [SerializeField]
    public GameObject MinionStatsPanelMain;

    [Space]
    [Header("Underground Variables")]
    public int undergroundWidth = 50;
    public int undergroundHeight = 50;
    #endregion

    // Use this for initialization

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }
    
    void Start () {
        if (!loadSaveFile)
        {
            Initialization();
        }
        else
        {
            //loadGame();
            //Vector3Int tempPos = activeMinions[0].myGrid;
            //Camera.main.transform.position = new Vector3(tempPos.x * tileSize.x,
            //    tempPos.y * tileSize.y, Camera.main.transform.position.z);
        }
        loadComplete = true;
    }

    int updatenum = 0;
    float oldUpdateTime=0;
    int currentViewLayer = 0;

    public void Update()
    {
        if (loadComplete == true)
        {
            var lightlevel = TimeOfDay.Instance.getLightPercent();
            RenderSettings.ambientLight = new Color(lightlevel, lightlevel, lightlevel);

            foreach (var i in flaggedForDestruction)
            {
                Destroy(i);
            }
            flaggedForDestruction.Clear();

            if (updatePathFinding)
            {
                StartCoroutine(generateNewMapGrid());
                updatePathFinding = false;
            }
            //TODO possibly only run this once every 8 times or something
            if (updatenum++ >= 3)
            {
                //Active tile
                activeTile = MapManager.Instance.convertToGameGrid(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (currentMouseMode == MouseMode.BUILD && buildMode != "" && !dragging)
                {
                    GameObject buildIcon = gameObject.transform.Find("Build Icon").gameObject;

                    Vector3Int intVecTemp = MapManager.Instance.convertToGameGrid(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    
                    var mousesTileGrid = MapManager.Instance.convertToWorldGrid(intVecTemp);
                    if (mousesTileGrid != null)
                    {
                        buildIcon.transform.position = mousesTileGrid;
                    }
                    if (buildIcon.GetComponent<SpriteRenderer>().sprite.name != buildMode)
                    {
                        buildIcon.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.Instance.getSprite(buildMode);
                    }

                }
                else if (gameObject.transform.Find("Build Icon") != null && currentMouseMode != MouseMode.BUILD)
                {
                    removeBuildIcon(gameObject.transform.Find("Build Icon").gameObject);
                }
                updatenum = 0;
                foreach (Minion m in activeMinions)
                {
                    m.gameManagerUpdate(Time.realtimeSinceStartup - oldUpdateTime);
                }

                activeAnimals.ForEach(x => x.gameManagerUpdate(Time.realtimeSinceStartup - oldUpdateTime));
                ItemManager.Instance.gameManagerUpdate();

                oldUpdateTime = Time.realtimeSinceStartup;
            }

            //Mouse Handling
            if (dragging)
            {
                continueDrag();
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                buildMode = "";
                flagForDestruction(gameObject.transform.Find("Build Icon").gameObject);
            }
            if (Input.GetKeyUp(KeyCode.E)) {
                currentViewLayer += 1;
                if (currentViewLayer > numUndergroundLayers)
                {
                    currentViewLayer = 0;
                }
                setCameraOnNewLayer(currentViewLayer);
            } else if(Input.GetKeyUp(KeyCode.Q))
            {
                currentViewLayer -= 1;
                if (currentViewLayer < 0)
                {
                    currentViewLayer = numUndergroundLayers;
                }
                setCameraOnNewLayer(currentViewLayer);
            }
        }
    }

    private void setCameraOnNewLayer(int layer)
    {
        Vector3 vec = Camera.main.transform.position;
        var newGrid = MapManager.Instance.convertToWorldGrid(
            MapManager.Instance.getMineShaftAtLayer(currentViewLayer).myGrid);

        vec.x = newGrid.x;
        vec.y = newGrid.y;
        Camera.main.transform.position = vec;
    }

    public void Initialization()
    {
        MapManager.Instance.initializeNewMap();

        loadStartGameDefaults();

        StartCoroutine(minionMovingFunction());
    }
    
    public void loadStartGameDefaults()
    {
        //TODO: better starting minion implementation?
        var newMinionPosition = MapManager.Instance.getRandomClearGrid(0);
        MapManager.Instance.clearMapTilesWithinDist(newMinionPosition, 8);
        var clearGrids = MapManager.Instance.getClearMapTilesWithinDistance(newMinionPosition, 8);

        //spawn animals
        var tempTiles = MapManager.Instance.getClearMapTilesWithinDistance(
            new Vector3Int((int)mapWidth/2, (int)mapHeight/2,0),mapHeight+mapWidth);


        tempTiles = shuffleList(tempTiles);
        //for (int i = 0; i < 20; i = i + 2)
        //{
        //    activeAnimals.Add(new Animal(tempTiles[i], "Sheep"));
        //    activeAnimals.Add(new Animal(tempTiles[i + 1], "Cow"));
        //}
        tempTiles = MapManager.Instance.getClearMapTilesWithinDistance(tempTiles[10], 10);
        int index = 0;
        

        activeMinions.Add(new Minion(tempTiles[index]));
        MapManager.Instance.clearMapTilesWithinDist(tempTiles[index], 10);

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        MapManager.Instance.myShaft = new MineShaft(tempTiles[index]);
        MapManager.Instance.clearMapTilesWithinDist(tempTiles[index], 5, MapManager.Instance.tileMapLayers);

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        activeMinions.Add(new Minion(tempTiles[index]));

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        activeMinions.Add(new Minion(tempTiles[index]));

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        ItemManager.Instance.addNewResourceItem("Wood", 100, tempTiles[index]);

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        ItemManager.Instance.addNewResourceItem("Stone", 50, tempTiles[index]);

        tempTiles.RemoveAt(index);
        index = UnityEngine.Random.Range(0, tempTiles.Count);
        ItemManager.Instance.addNewContainerItem("Basket", tempTiles[index]);

        setCameraOnNewLayer(0);

    }

    public void flagForDestruction(GameObject go)
    {
        flaggedForDestruction.Add(go);
    }

    public Vector2 actualToGrid(Vector2 i)
    {
        return new Vector2(pixelsPerUnitx * i.x / tileSize.x, pixelsPerUnity *  i.y / tileSize.y);
    }

    public void updatePathfindingGraph()
    {
        updatePathFinding = true;
    }

    private IEnumerator generateNewMapGrid()
    {
        MapManager.Instance.generateNewPathfindingGrid();
        yield return null;
    }

    //public void saveGame()
    //{
    //    var save = new GameDataGroup();
    //    save.convertAndStoreMinionData(activeMinions);
    //    save.convertAndStoreAnimalData(activeAnimals);
    //    save.convertAndStoreItemData(ItemManager.Instance.getItemArray(),ItemManager.Instance.getResourceItems());
    //    save.convertAndStoreJobs(JobManager.Instance.jobQue);
    //    save.convertAndStoreTiles(MapManager.Instance.getTiles());
    //    save.storeMapTexture(MapManager.Instance.getMapTexture());
    //    SaveGameLoader.save("Assets\\Resources\\SavedGames\\", "Test", save);
    //}

    //public void loadGame()
    //{
    //    var load = SaveGameLoader.Load();
    //    JobManager.Instance.jobQue = load.theJobQue;
    //    MapManager.Instance.loadMap(load.theMapTexture, load.theTiles);
    //    ItemManager.Instance.setItems(load.theItems, load.theResItems, load.theHouses);
    //    updatePathfindingGraph();
    //    activeAnimals = load.theAnimals;
    //    activeMinions = load.theMinions;
    //    StartCoroutine(minionMovingFunction());
    //}
    
    #region Minion Interactions

    IEnumerator minionMovingFunction()
    {
        while (true /*TODO change this? */)
        {
            activeMinions.ForEach(x => x.movementTick(minionMovementStepDistance));
            activeAnimals.ForEach(x => x.movementTick(animalMovementStepDistance));
            yield return new WaitForSeconds(minionMoveDelay);

        }
    }

    public void delayThenCall(Action theFunc, float time)
    {
        StartCoroutine(delayCoroutine(theFunc, time));
    }

    IEnumerator delayCoroutine(Action theFunc, float time)
    {
        yield return new WaitForSeconds(time);
        theFunc();
    }

    public Minion whoIsHuntimeMe(Being b)
    {
        foreach (Minion m in activeMinions)
        {
            if (m.myJob != null && 
                m.myJob is HuntJob && 
                (m.myJob as HuntJob).beingHunted.myGO == b.myGO)
            {
                return m;
            }
        }
        return null;
    }

    public void beingDied(Being b)
    {
        if (b is Animal)
        {
            activeAnimals.Remove(b as Animal);
        } else if (b is Minion)
        {
            activeMinions.Remove(b as Minion);
        } else
        {
            //other types of beings??

        }
    }
    #endregion

    private void createItemDataBaseData()
	{
		ItemDataContainer itemData = new ItemDataContainer();

		HarvestableItemDataShell wal = new HarvestableItemDataShell ();
		wal.contains = new Resource ("Stone", 20);
		wal.harvestTime = 1.0f;
		wal.carriable = false;
		wal.myType = "Wall";
		wal.walkable = false;
		itemData.addItem (wal);

		ItemDataShell berry = new ItemDataShell ();
		berry.carriable = true;
		berry.myType = "Berry";
		berry.walkable = true;
		itemData.addItem (berry);


		ItemDataShell dat = new ItemDataShell ();
		dat.carriable = true;
		dat.myType = "Wood";
		dat.walkable = true;
		itemData.addItem (dat);

        dat = new ItemDataShell();
        dat.carriable = false;
        dat.myType = "Bed";
        dat.walkable = true;
        itemData.addItem(dat);

        dat = new ItemDataShell();
        dat.carriable = false;
        dat.myType = "Chair";
        dat.walkable = true;
        itemData.addItem(dat);

        ItemDataShell stone = new ItemDataShell ();
		stone.carriable = true;
		stone.myType = "Stone";
		stone.walkable = true;
		itemData.addItem (stone);

        HarvestableItemDataShell h = new HarvestableItemDataShell();
        h.carriable = false;
        h.contains = new Resource("Cheese", 5);
        h.harvestTime = 5.0f;
        h.myType = "Cheese Wheel";
		h.walkable = true;
        itemData.addItem(h);

		var ha = new HarvestableItemDataShell ();
		ha.carriable = false;
		ha.contains = new Resource ("Wood", 30);
		ha.harvestTime = 2.5f;
		ha.myType = "Tree";
		ha.walkable = false;
		itemData.addItem (ha);

        HarvestableItemDataShell i = new HarvestableItemDataShell();
        i.carriable = true;
        i.contains = new Resource("Berry", 10);
        i.harvestTime = 3.0f;
        i.myType = "Small Bush";
		i.walkable = true;
        itemData.addItem(i);

        ContainerItemDataShell c = new ContainerItemDataShell();
        c.myType = "Basket";
        c.carriable = false;
        c.maxResources = 100;
		c.walkable = true;
        itemData.addItem(c);

        var resl = new List<Resource>();
        resl.Add(new Resource("Wood", 5));

        ItemCostGroup cg = new ItemCostGroup(resl, "Wall", 2.0f);
        itemData.addItemCostGroup(cg);

        resl.Clear();
        resl.Add(new Resource("Wood", 10));
        resl.Add(new Resource("Stone", 5));
        cg = new ItemCostGroup(resl, "Bed", 2.0f);
        itemData.addItemCostGroup(cg);

        resl.Clear();
        resl.Add(new Resource("Wood", 10));
        cg = new ItemCostGroup(resl, "Chair", 1.0f);
        itemData.addItemCostGroup(cg);

        HouseItemDataShell hd = new HouseItemDataShell();
        hd.carriable = false;
        hd.SizeX = 1;
        hd.SizeY = 1;
        hd.walkable = true;
        bool[] temp = { true, false, true, false };
        hd.WalkableArray = temp;
        itemData.addItem(hd);

        itemData.save("Assets\\Resources\\Data\\New Item Data.xml");
    }

	#region UI interface
	public void setBuildMode(string mode) {
		buildMode = mode;
        //Create a summy sprite to follow the mouse around.
        var buildIcon = newBuildIcon(
            MapManager.Instance.convertToGameGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition)),
            buildMode);

        buildIcon.name = "Build Icon";
        currentMouseMode = MouseMode.BUILD;
        originalBuildIcon = buildIcon;
	}

	public void setGatherMode() {
		currentMouseMode = MouseMode.GATHER;
	}

    public void setHuntMode()
    {
        currentMouseMode = MouseMode.HUNT;
    }

    #endregion

    #region Clicks and Drags

    public bool dragging = false;
    private Vector3Int startDragGrid;
    private Vector3Int lastDragGrid;
    private GameObject originalBuildIcon;
    List<GameObject> dragIcons = new List<GameObject>();
    List<Vector3Int> dragGrids = new List<Vector3Int>();

    public void startDrag(Vector3Int mousePosition)
    {
        dragging = true;
        if (currentMouseMode == MouseMode.BUILD)
        {
            lastDragGrid = mousePosition;
            startDragGrid = lastDragGrid;
            dragIcons.Add(newBuildIcon(startDragGrid, buildMode));
            dragGrids.Add(startDragGrid);
        }
    }

    private void continueDrag()
    {
        Vector3Int currentMouseGameGrid = MapManager.Instance.convertToGameGrid(
            Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (currentMouseGameGrid.z != startDragGrid.z)
            Debug.LogAssertion("You dragged across layers...");
        if (currentMouseGameGrid != lastDragGrid)
        {
            int count = dragIcons.Count;
            for (int i = 0; i < count; i++)
            {
                removeBuildIcon(dragIcons[i]);
            }
            dragIcons.Clear();
            dragGrids.Clear();
            //Dragging has happened
            int deltaX = currentMouseGameGrid.x - startDragGrid.x;
            int deltaY = currentMouseGameGrid.y - startDragGrid.y;
            if (deltaX >= 0)
            {
                if (deltaY >= 0)
                {
                    for (int y=0;y <= deltaY; y++)
                    {
                        for (int x=0;x <=deltaX;x++)
                        {
                            Vector3Int tempVec = startDragGrid;
                            tempVec.x += x;
                            tempVec.y += y;
                            var newIcon = newBuildIcon(tempVec, buildMode);
                            dragGrids.Add(tempVec);
                            dragIcons.Add(newIcon);
                        }
                        //create dx*2 + dy*2 - 1 buildIcons
                    }
                } else if (deltaY < 0)
                {
                    for (int y = 0; y >= deltaY; y--)
                    {
                        for (int x = 0; x <= deltaX; x++)
                        {
                            Vector3Int tempVec = startDragGrid;
                            tempVec.x += x;
                            tempVec.y += y;
                            var newIcon = newBuildIcon(tempVec, buildMode);
                            dragGrids.Add(tempVec);
                            dragIcons.Add(newIcon);
                        }
                        //create dx*2 + dy*2 - 1 buildIcons
                    }
                }
            } else if (deltaX < 0)
            {
                //work starts here
                if (deltaY >= 0)
                {
                    for (int y = 0; y <= deltaY; y++)
                    {
                        for (int x = 0; x >= deltaX; x--)
                        {
                            Vector3Int tempVec = startDragGrid;
                            tempVec.x += x;
                            tempVec.y += y;
                            var newIcon = newBuildIcon(tempVec, buildMode);
                            dragGrids.Add(tempVec);
                            dragIcons.Add(newIcon);
                        }
                        //create dx*2 + dy*2 - 1 buildIcons
                    }
                }
                else if (deltaY < 0)
                {
                    for (int y = 0; y >= deltaY; y--)
                    {
                        for (int x = 0; x >= deltaX; x--)
                        {
                            Vector3Int tempVec = startDragGrid;
                            tempVec.x += x;
                            tempVec.y += y;
                            var newIcon = newBuildIcon(tempVec, buildMode);
                            dragGrids.Add(tempVec);
                            dragIcons.Add(newIcon);
                        }
                        //create dx*2 + dy*2 - 1 buildIcons
                    }
                }
            }








            //Ends with
            lastDragGrid = currentMouseGameGrid;
        }
    }

    private List<GameObject> deadBuildIcons = new List<GameObject>();
    private GameObject newBuildIcon(Vector3Int grid, string name)
    {
        if (deadBuildIcons.Count == 0)
        {
            GameObject buildIcon = GameObject.Instantiate(buildIconPrefab);
            buildIcon.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.Instance.getSprite(name);
            buildIcon.transform.position = MapManager.Instance.convertToWorldGrid(grid);
            buildIcon.transform.SetParent(this.transform);
            return buildIcon;
        } else
        {
            GameObject newBuildIcon = deadBuildIcons[0];
            deadBuildIcons.RemoveAt(0);
            newBuildIcon.SetActive(true);
            newBuildIcon.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.Instance.getSprite(name);
            newBuildIcon.transform.position = MapManager.Instance.convertToWorldGrid(grid);
            return newBuildIcon;
        }
    }

    private void removeBuildIcon(GameObject theIcon)
    {
        deadBuildIcons.Add(theIcon);
        theIcon.SetActive(false);
    }

    public void endDrag(Vector3 mousePosition)
    {
        var count = dragIcons.Count;
        for (int i=0;i<count;i++)
        {
            //Add job creation here
            clickedMap(dragGrids[0]);


            removeBuildIcon(dragIcons[0]);
            dragGrids.RemoveAt(0);
            dragIcons.RemoveAt(0);
        }
        dragging = false;
    }

    public void clickedMap(Vector3Int tile)
    {
        int level = tile.z;
        Item i = ItemManager.Instance.getItemAtGrid(tile);
        switch (currentMouseMode)
        {

            case MouseMode.BUILD:
                var clickedGrid = tile;
                if (i != null)
                {
                    //Theres already an object there

                }
                else if (buildMode == "")
                {
                    //Do nothing
                }
                else
                {
                    //Do nothing for now
                    //JobManager.Instance.newBuildJob(buildMode, clickedGrid);
                }
                break;
            case MouseMode.GATHER:
                //if (i is HarvestableItem)
                //{
                //    JobManager.Instance.newHarvestJob(i as HarvestableItem); // automatically doesn't add the job if harvest already exists
                //} else 
                if (MapManager.Instance.getTileNameAt(tile, 1) != "")
                {
                    string name = MapManager.Instance.getTileNameAt(tile, 1);
                    HarvestableGrid newHarGrid = ItemManager.Instance.addNewHarvestableGrid(tile, name);
                    JobManager.Instance.newHarvestJob(newHarGrid);
                }
                break;
            case MouseMode.HUNT:
                foreach (var anim in activeAnimals)
                {
                    //if (anim.myGrid == t.myGridPosition)
                    if (anim.withinHuntingRange(MapManager.Instance.convertToWorldGrid(tile)))
                    {
                        JobManager.Instance.newHuntJob(anim);
                        break;
                    }
                }
                break;
            default:
                break;
        }


    }
    #endregion

    public static List<E> shuffleList<E>(List<E> inputList)
    {
        List<E> randomList = new List<E>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }

        return randomList;

    }

}
