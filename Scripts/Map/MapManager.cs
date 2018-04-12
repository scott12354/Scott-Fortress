using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.EventSystems;

public enum TILE_MAP_LAYERS {BASE=0, WALLS, OBJECTS};

public class MapManager
{
    #region Variables
    //Singleton framework
    private static MapManager _instance;
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MapManager(GameManager.Instance.mapWidth, GameManager.Instance.mapHeight);
            }
            return _instance;
        }
        private set { }

    }

    //Existing Objects and Prefabs
    private Grid mainMapGrid;

    //Generation Variables
    private float chanceToCell;
    private int reqToBirth;
    private int reqToKill;
    private int iterations;
    private int mapWidth, mapHeight;

    //Core Data Variables
    public List<Tilemap> tileMapLayers;
    public List<TileBase> tileRefs; //TODO: Replace with better way of storing tile prefabs
    private List<UndergroundLevel> undergroundLayers = new List<UndergroundLevel>();
    public MineShaft myShaft;

    //Pathfinding Variables
    private int[] _runtimeGrid;
    private int[] runtimeGrid
    {
        get
        {
            if (_runtimeGrid == null)
                generateGrid();
            return _runtimeGrid;
        }
        set
        {
            _runtimeGrid = value;
        }
    }
    private Graph _runtimeGraph;
    private Graph runtimeGraph
    {
        get {
            if (_runtimeGraph == null)
                _runtimeGraph = new Graph(runtimeGrid, mapWidth, mapHeight);
            return _runtimeGraph;
        }
    }
    #endregion

    //Main Constructor functions
    public MapManager(int width, int height) {
		mapWidth = width;
		mapHeight = height;
    }

    public void initializeNewMap()
    {
        //TODO: doesnt delete hte old one yet
        //Pull variables from Game Manager
        tileRefs = GameManager.Instance.tileRefs;
        chanceToCell = GameManager.Instance.chanceToCell;
        reqToBirth = GameManager.Instance.reqToBirth;
        reqToKill = GameManager.Instance.reqToKill;
        iterations = GameManager.Instance.iterations;


        mainMapGrid = GameObject.Instantiate(GameManager.Instance.tileGridOriginal).GetComponent<Grid>();

        tileMapLayers = new List<Tilemap>();
        for (int i = 0; i < mainMapGrid.transform.childCount; i++)
        {
            tileMapLayers.Add(mainMapGrid.transform.GetChild(i).GetComponent<Tilemap>());
        }

        //Configure base layer
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < mapWidth; temp.x++)
            {
                tileMapLayers[0].SetTile(temp, tileRefs[0]);
            }
        }

        Tilemap tempTilemap = GameObject.Instantiate(tileMapLayers[0]);
        tempTilemap.ClearAllTiles();
        generateCaves(tileMapLayers[1], mapHeight, mapWidth, tileRefs[1]);
        generateCaves(tempTilemap, mapHeight, mapWidth, tileRefs[2]);
        mergeTileLayers(tileMapLayers[1], tempTilemap, mapHeight, mapWidth);
        GameObject.Destroy(tempTilemap.gameObject);


        //creating layers here:
        //its important i starts at 1, layer 0 is the main map
        for (int i = 1; i <= GameManager.Instance.numUndergroundLayers; i++)
        {
            undergroundLayers.Add(new UndergroundLevel(i));
        }

    }

    #region Cellular Generation Functions

    public void mergeTileLayers(Tilemap baseMap, Tilemap subMap, int mapHeight, int mapWidth)
    {
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < mapWidth; temp.x++)
            {
                if (baseMap.GetTile(temp)==null)
                {
                    baseMap.SetTile(temp, subMap.GetTile(temp));
                }

            }
        }
    }

    public void generateCaves(Tilemap inputLayer, int height, int width, TileBase cellTile)
    {
        bool[,] tempMap = new bool[width, height];
        //First, set some random Walls
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < height; temp.y++)
        {
            for (temp.x = 0; temp.x < width; temp.x++)
            {
                if (UnityEngine.Random.Range(0, 100) <= chanceToCell)
                    tempMap[temp.x, temp.y] = true;
            }
        }

        //Iterate a few times
        for (int i = 0; i < iterations; i++)
            tempMap = iterate(tempMap, height, width);

        for (temp.y = 0; temp.y < height; temp.y++)
        {
            for (temp.x = 0; temp.x < width; temp.x++)
            {
                if (!tempMap[temp.x, temp.y])
                    inputLayer.SetTile(temp, cellTile);
            }
        }
    }

    private bool[,] iterate(bool[,] originalMap, int height, int width)
    {
        bool[,] newLayer = new bool[width, height];
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < height; temp.y++)
        {
            for (temp.x = 0; temp.x < width; temp.x++)
            {
                int count = countAliveNeighbours(originalMap, temp, width, height);
                //If alive
                if (originalMap[temp.x, temp.y])
                {
                    if (count < reqToKill)
                    {
                        newLayer[temp.x, temp.y] = false;
                    }
                    else
                    {
                        newLayer[temp.x, temp.y] = true;
                    }
                }
                //If dead
                else
                {
                    if (count > reqToBirth)
                    {
                        newLayer[temp.x, temp.y] = true;
                    }
                    else
                    {
                        newLayer[temp.x, temp.y] = false;
                    }
                }
            }
        }
        return newLayer;
    }

    public int countAliveNeighbours(bool[,] map, Vector3Int location, int mapWidthIn, int mapHeightIn)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = location.x + i;
                int neighbour_y = location.y + j;
                //If we're looking at the middle point
                if (i == 0 && j == 0)
                {
                    //Do nothing, we don't want to add ourselves in!
                }
                //In case the index we're looking at it off the edge of the map
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= mapWidthIn || neighbour_y >= mapHeightIn)
                {
                    count = count + 1;
                }
                //Otherwise, a normal check of the neighbour
                else if (map[neighbour_x, neighbour_y])
                {
                    count = count + 1;
                }
            }
        }
        return count;
    }

    public bool tileWalkable(Vector3Int gridPos)
    {
        var tilemaps = getProperLayerList(gridPos);
        foreach (Tilemap t in tilemaps)
        {
            if (t.GetTile(gridPos) == null)
                continue;
            if (t.GetTile(gridPos) is RuleTile)
            {
                if (t.GetTile<RuleTile>(gridPos).walkable == false)
                {
                    return false;
                }
            } else if (t.GetTile(gridPos) is RandomTile)
            {
                if (t.GetTile<RandomTile>(gridPos).walkable == false)
                {
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    public UndergroundLevel getUGLevel(int i)
    {
        if (i <= 0 || i > GameManager.Instance.numUndergroundLayers)
            return null;
        //if (i > GameManager.Instance.numUndergroundLayers)
        //    return null;
        return undergroundLayers.First(x => x.level == i);
    }

    public void harvested(Vector3Int myGrid)
    {
        if (myGrid.z==0)
        {
            tileMapLayers[1].SetTile(myGrid, null);
            return;
        }
        var ug = getUGLevel(myGrid.z);
        myGrid.z = 0;
        ug.getTilemapLayers()[1].SetTile(myGrid, null);
    }

    public List<Vector3Int> getMapTilesWithinDistance(Vector3Int location, int distance)
    {
        int mapHeightIn, mapWidthIn;
        if (location.z == 0)
        {
            mapHeightIn = mapHeight;
            mapWidthIn = mapWidth;

        }
        else
        {
            mapHeightIn = GameManager.Instance.undergroundHeight;
            mapWidthIn = GameManager.Instance.undergroundWidth;
        }

        List<Vector3Int> toReturn = new List<Vector3Int>();
        for (int x=location.x-distance;x <= location.x+distance;x++)
        {
            for (int y = location.y-distance;y <= location.y+distance;y++)
            {
                if ((x < 0) || (y < 0) || (y >= mapHeightIn) || (x >= mapWidthIn)) {
                    continue;
                }
                if (Vector3Int.Distance(location,new Vector3Int(x,y,location.z)) > distance) {
                    continue;
                }
                toReturn.Add(new Vector3Int(x, y, location.z));
            }
        }
        return toReturn;
    }

    public List<Vector3Int> getClearMapTilesWithinDistance(Vector3Int location, int distance)
	{
        List<Tilemap> layers = tileMapLayers;
        int mapHeightIn, mapWidthIn;
        if (location.z == 0)
        {
            mapHeightIn = mapHeight;
            mapWidthIn = mapWidth;

        }
        else
        {
            layers = getUGLevel(location.z).getTilemapLayers();
            mapHeightIn = GameManager.Instance.undergroundHeight;
            mapWidthIn = GameManager.Instance.undergroundWidth;
        }

        var tempList = getMapTilesWithinDistance(location, distance);
        var toReturn = new List<Vector3Int>();
        foreach (Vector3Int v in tempList)
        {
            if (isLocationClear(v))
            {
                toReturn.Add(new Vector3Int(v.x, v.y, v.z));
            }
        }
        return toReturn;
	}

    public bool isLocationClear(Vector3Int location)
    {
        var tileList = getProperLayerList(location);
        location.z = 0;
        for (int i=1;i< tileList.Count;i++)
        {
            if (tileList[i].GetTile(location) != null)
            {
                return false;
            }
        }
        return true;
    }

    public Vector3Int convertToGameGrid(Vector3 input)
    {
        input.z = 0;
        //Only works if the sublayers are shorter than the main map layer
        if (input.y > mapHeight*GameManager.Instance.tileSize.y)
            return new Vector3Int(-1, -1, -1);
        if (input.x < mapWidth * GameManager.Instance.tileSize.x)
        {
            var test = mainMapGrid.WorldToLocal(input);
            var test2 = mainMapGrid.WorldToCell(input);
            return test2;
        }
        if (input.x > getUGLevel(GameManager.Instance.numUndergroundLayers).myGrid.transform.position.x)
        {
            Vector3Int toReturn = getUGLevel(GameManager.Instance.numUndergroundLayers).myGrid.WorldToCell(input);
            toReturn.z= GameManager.Instance.numUndergroundLayers;
            return toReturn;
        }
        for (int i =1;i< GameManager.Instance.numUndergroundLayers;i++)
        {
            if ((input.x > getUGLevel(i).myGrid.transform.position.x) &&
                input.x < getUGLevel(i+1).myGrid.transform.position.x)
            {
                Vector3Int toReturn = getUGLevel(i).myGrid.WorldToCell(input);
                toReturn.z = i;
                return toReturn;
            }
        }

        return new Vector3Int(-1, -1, -1);
        //if (mainMapGrid)
        //if (level ==0)
        //    return mainMapGrid.WorldToCell(input);

        ////else
        //var tempVec = getUGLevel(level).myGrid.WorldToCell(input);
        //tempVec.z = level;
        //return tempVec;
    }

    public Vector3 convertToWorldGrid(Vector3Int input)
    {
        if (input.z==0)
        {
            return mainMapGrid.CellToWorld(input);
        } else
        {
            var ug = getUGLevel(input.z);
            return ug.myGrid.CellToWorld(input);
        }
    }

    public void clearTileAtGrid(Vector3Int grid, List<Tilemap> tilemapLayersInput)
    {
        grid.z = 0;
        for (int x = 1; x < tilemapLayersInput.Count; x++)
        {
            tilemapLayersInput[x].SetTile(grid, null);
        }
    }

    public void clearMapTilesWithinDist(Vector3Int Gridin, int distance)
    {
        var temp = getMapTilesWithinDistance(Gridin, distance);
        var ug = getUGLevel(Gridin.z);
        List<Tilemap> tilelayers = null;
        if (ug != null)
            tilelayers = ug.getTilemapLayers();
        foreach (Vector3Int v in temp)
        {
            var tempvec = v;
            tempvec.z = 0;
            if (Gridin.z == 0)
            {
                clearTileAtGrid(tempvec, tileMapLayers);
            } else
            {
                clearTileAtGrid(tempvec, tilelayers);
            }
            
        }
    }

    public void clearMapTilesWithinDist(Vector3Int Gridin, int distance, List<Tilemap> tilelayers)
    {
        var temp = getMapTilesWithinDistance(Gridin, distance);
        Gridin.z = 0;
        foreach (Vector3Int v in temp)
        {
            clearTileAtGrid(v, tilelayers);

        }
    }

    public Vector3Int getClosestOpenTile(Vector3Int vec)
    {
        var list = getProperLayerList(vec);
        Vector3Int returnVec = new Vector3Int(-1,-1,-1);
        List<Vector3Int> listOfTiles = new List<Vector3Int>();
        bool finished = false;
        int x = 0;
        int count = 0;
        while (!finished)
        {
            if (count++ >= 200)
            {
                Debug.LogAssertion("Cannot find open tile");
                finished = true;
            }
            listOfTiles = getClearMapTilesWithinDistance(vec, 2 + x);
            int randInt = Mathf.RoundToInt(UnityEngine.Random.Range(0, listOfTiles.Count - 1));
            while (listOfTiles.Count > 0)
            {
                if (count++ >= 200)
                {
                    Debug.LogAssertion("Cannot find open tile");
                    finished = true;
                    break;
                }
                if (isLocationClear(listOfTiles[randInt])) {
                    returnVec = listOfTiles[randInt];
                    finished = true;
                    break;
                } else
                {
                    listOfTiles.RemoveAt(randInt);
                }
                randInt = Mathf.RoundToInt(UnityEngine.Random.Range(0, listOfTiles.Count - 1));
            }
        }
        return returnVec;
    }


    private List<Tilemap> getProperLayerList(Vector3Int vec)
    {
        List<Tilemap> tileList = null;
        if (vec.z == 0)
        {
            tileList = tileMapLayers;
        }
        else
        {
            tileList = getUGLevel(vec.z).tileMapLayers;
        }
        return tileList;
    }

    #region Pathfinding Functions

    public Vector3Int[] getPathLayerChange(Vector3Int start, Vector3Int stop)
    {
        if (Vector3Int.Distance(start, stop) <= 1.5 && start.z== stop.z)
        {
            List<Vector3Int> returning = new List<Vector3Int>();
            returning.Add(stop);
            return returning.ToArray();
        }
        if (start.z == stop.z)
        {
            //This returns null if its layer 0
            var startLayer2 = MapManager.Instance.getUGLevel(start.z);

            Graph theGraph = null;
            if (startLayer2 == null)
            { theGraph = runtimeGraph; }
            else
            { theGraph = startLayer2.myGraph; }

            Search theSearch2 = new Search(theGraph);
            theSearch2.Start(theGraph.getNodeAt(start), theGraph.getNodeAt(stop));

            while (!theSearch2.finished)
            {
                theSearch2.Step();
            }
            List<Vector3Int> otherReturnThis = new List<Vector3Int>();
            foreach (Node vec in theSearch2.path)
            {
                Vector3Int temp = new Vector3Int(0, 0, start.z);
                temp.x = theGraph.getGridOfNodeAtIndex(vec.index).x;
                temp.y = theGraph.getGridOfNodeAtIndex(vec.index).y;
                otherReturnThis.Add(temp);
            }
            if (theSearch2.path.Count == 0)
            {
                //otherReturnThis = new List<Vector3>();
                //otherReturnThis.Add(stop);
                //return otherReturnThis.ToArray();

                //My plan didn't work, I think it actually can't find a path;
                return null;
            }
            return otherReturnThis.ToArray();
        }


        var startLayer = MapManager.Instance.getUGLevel(start.z);
        Graph startGraph = null;
        if (startLayer == null)
        { startGraph = runtimeGraph; }
        else
        { startGraph = startLayer.myGraph; }

        Graph stopGraph = null;
        var stopLayer = MapManager.Instance.getUGLevel(stop.z);
        if (stopLayer == null)
        { stopGraph = runtimeGraph; }
        else
        { stopGraph = stopLayer.myGraph; }

        //TODO: Ive removed checking for a destination/origin that is not walkable.
        //May need to implement getclosest grid for UGLs

        //Starting layer
        Search theSearch = new Search(startGraph);
        Vector3Int endOnLayer = getMineShaftAtLayer(start.z).myGrid;
        theSearch.Start(startGraph.getNodeAt(start),startGraph.getNodeAt(endOnLayer));
        
        while (!theSearch.finished)
        {
            theSearch.Step();
        }
        List<Vector3Int> returnThis = new List<Vector3Int>();
        foreach(Node vec in theSearch.path)
        {
            Vector3Int temp = new Vector3Int(0, 0, start.z);
            temp.x = startGraph.getGridOfNodeAtIndex(vec.index).x;
            temp.y = startGraph.getGridOfNodeAtIndex(vec.index).y;
            returnThis.Add(temp);
        }
        if (theSearch.path.Count == 0)
            return null;
        //COmpleted the first level, add the transition movement step
        returnThis.Add(getMineShaftAtLayer(stop.z).myGrid);
        //Now pathfind to the destination in the final layer
        theSearch = new Search(stopGraph);
        Vector3 startOnLayer = getMineShaftAtLayer(stop.z).myGrid;
        theSearch.Start(stopGraph.getNodeAt(startOnLayer), stopGraph.getNodeAt(stop));

        while (!theSearch.finished)
        {
            theSearch.Step();
        }
        foreach (Node vec in theSearch.path)
        {
            Vector3Int temp = new Vector3Int(0, 0, stop.z);
            temp.x = stopGraph.getGridOfNodeAtIndex(vec.index).x;
            temp.y = stopGraph.getGridOfNodeAtIndex(vec.index).y;
            returnThis.Add(temp);
        }


        if (theSearch.path.Count != 0 && Vector3Int.Distance(returnThis[theSearch.path.Count - 1], stop) > 1.5f)
        {
            Debug.Log("Pathfinding could not find a path");
            //return null;
        }
        return returnThis.ToArray();
    }

    public string getTileNameAt(Vector3Int tile, int v)
    {
        if (tile.z == 0)
        {
            if (v >= tileMapLayers.Count)
                return "";
            if (tileMapLayers[v].GetTile(tile) == null)
                return "";
            return tileMapLayers[v].GetTile(tile).name;
        } else
        {
            List<Tilemap> layers = getUGLevel(tile.z).getTilemapLayers();
            tile.z = 0;
            if (v >= layers.Count)
                return "";
            if (layers[v].GetTile(tile) == null)
                return "";
            return layers[v].GetTile(tile).name;
        }
    }

    private void generateGrid()
    {
        int[] toReturn = new int[mapHeight * mapWidth];
        for (int i = 0; i < toReturn.Length; i++)
            toReturn[i] = 0;
        Vector3Int v = new Vector3Int(0, 0, 0);
        for (v.y = 0; v.y < mapHeight; v.y++)
        {
            for (v.x = 0; v.x < mapWidth; v.x++)
            {
                if (tileWalkable(v))
                {
                    toReturn[v.x + (mapWidth * v.y)] = 0;

                } 
                else
                {
                    toReturn[v.x + (mapWidth * v.y)] = 1;
                }
            }
        }
        runtimeGrid = toReturn;
    }

    public void generateNewPathfindingGrid()
    {
        generateGrid();
        runtimeGraph.update(runtimeGrid);
    }

    public Vector3Int getRandomClearGrid(int Level)
    {
        List<Tilemap> tilelayers = null;
        if (Level ==0)
        {
            tilelayers = tileMapLayers;
        } else
        {
            var ug = getUGLevel(Level);
            tilelayers = ug.getTilemapLayers();
        }
        int x = UnityEngine.Random.Range(0, mapWidth - 1);
        int y = UnityEngine.Random.Range(0, mapHeight - 1);
        while (!tileWalkable(new Vector3Int(x, y, 0)))
        {
            x = UnityEngine.Random.Range(0, mapWidth - 1);
            y = UnityEngine.Random.Range(0, mapHeight - 1);
        }
        return new Vector3Int(x, y, 0);
    }

    public MineShaft getMineShaftAtLayer(int z)
    {
        int level = Mathf.RoundToInt(z);
        if (level == 0)
        {
            return myShaft;
        } else
        {
            var ugl = getUGLevel(level);
            return ugl.layerShaft;
        }
    }

    //public static Vector3 convertToGameGrid(Vector3 input)
    //{
    //    int groundLevelWidthInPixels = (int)(GameManager.Instance.tileSize.x * MapManager.Instance.mapWidth);
    //    //int UGLayerWidthInPixels = (int)(GameManager.Instance.undergroundWidth * GameManager.Instance.tileSize.x);
    //    if (input.x <= groundLevelWidthInPixels)
    //    {
    //        int x = Mathf.RoundToInt(input.x / GameManager.Instance.tileSize.x);
    //        int y = Mathf.RoundToInt(input.y / GameManager.Instance.tileSize.y);
    //        return new Vector3(x, y, input.z);
    //    } else
    //    {
    //        int x = (int)input.x;
    //        int z = 0;
    //        while (x > groundLevelWidthInPixels)
    //        {
    //            x -= (int)(GameManager.Instance.undergroundWidth * GameManager.Instance.tileSize.x);
    //            z++;
    //        }
    //        x = (int)(x/GameManager.Instance.tileSize.x);

    //        float newx = input.x / GameManager.Instance.tileSize.x;
    //        newx -= GameManager.Instance.mapWidth;
    //        newx -= ((z-1) * GameManager.Instance.undergroundWidth);
    //        int y = Mathf.RoundToInt(input.y / GameManager.Instance.tileSize.y);
    //        return new Vector3(newx, y, z);
    //    }
    //}


    #endregion
}
