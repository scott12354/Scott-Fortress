using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UndergroundLevel {
    //New TIlemap variables
    public Grid myGrid;
    public List<Tilemap> tileMapLayers;





    private int layerHeight = GameManager.Instance.undergroundHeight;
    private int layerWidth = GameManager.Instance.undergroundWidth;
    public int level;

    public Graph myGraph; //used in pathfinding for the layer
    private bool updatePathfinding = true;


    public Vector3Int shaftLocation;
    public MineShaft layerShaft;

    public int offset
    {
        get
        {
            return Mathf.RoundToInt((GameManager.Instance.mapWidth + (level - 1) * layerWidth) * GameManager.Instance.tileSize.x);
        }
    }

    public UndergroundLevel(int levelin)
    {
        myGrid = GameObject.Instantiate(GameManager.Instance.tileGridOriginal).GetComponent<Grid>();
        //myGrid.SendMessage("setLevel", levelin);
        tileMapLayers = new List<Tilemap>();
        for (int i = 0; i < myGrid.transform.childCount; i++)
        {
            tileMapLayers.Add(myGrid.transform.GetChild(i).GetComponent<Tilemap>());
        }
        
        level = levelin;
        
        //Generate map here
        MapManager.Instance.generateCaves(tileMapLayers[1], 
            layerHeight, layerWidth, MapManager.Instance.tileRefs[4]);

        myGrid.transform.position = new Vector3(offset, 0, level);


        int x = Random.Range(0, layerWidth - 1);
        int y = Random.Range(0, layerHeight - 1);

        bool walkableTile = false;

        while (!walkableTile)
        {
            x = Random.Range(0, layerWidth - 1);
            y = Random.Range(0, layerHeight - 1);
            if (tileMapLayers[1].GetTile(new Vector3Int(x,y,0)) == null)
            {
                continue;
            }
            walkableTile = true;
        }
        var newShaftLocaton = new Vector3Int(x, y, level);

        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < layerHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < layerWidth; temp.x++)
            {
                tileMapLayers[0].SetTile(temp, MapManager.Instance.tileRefs[3]);
            }
        }

        harvestGridNoRubble(newShaftLocaton);

        MapManager.Instance.clearMapTilesWithinDist(newShaftLocaton, 5, tileMapLayers);
        layerShaft = new MineShaft(newShaftLocaton);
        generateNewPathfindingGrid();
    }

    public void gameManagerUpdate(float deltaT)
    {
        if (updatePathfinding)
        {
            generateNewPathfindingGrid();
        }
    }
    
    public Vector2 getWorldGridForLevelGrid(Vector2 levelGrid)
    {
        int tileSize = Mathf.RoundToInt(GameManager.Instance.tileSize.x);
        return new Vector2(levelGrid.x * tileSize + offset, levelGrid.y * tileSize);
    }

    //Does the actual harvesting and spawns rubble, the click receiver will do the job set up
    public void harvestGrid(Vector3Int grid)
    {
        //var index = Mathf.RoundToInt(layerWidth * grid.y + grid.x);
        clearTextureTileAt(grid);
        ItemManager.Instance.addNewResourceItem("Rubble", 5, new Vector3Int(grid.x, grid.y, level));
        generateNewPathfindingGrid();
    }

    public void harvestGridNoRubble(Vector3Int grid)
    {
        clearTextureTileAt(grid);
    }

    private void clearTextureTileAt(Vector3Int grid)
    {
        tileMapLayers[(int)TILE_MAP_LAYERS.WALLS].SetTile(grid, null);
        generateNewPathfindingGrid();
    }

    public bool canwalk(Vector3Int grid, List<Tilemap> tilemaps)
    {
        grid.z = 0;
        foreach (Tilemap t in tilemaps)
        {
            if (t.GetTile(grid) == null)
            {
                continue;
            }
            if (t.GetTile<RuleTile>(grid).walkable == false)
            {
                return false;
            }
        }
        return true;
    }

    private int[] generateGrid()
    {
        int[] toReturn = new int[layerHeight * layerWidth];
        int i = 0;
        for (int y=0;y<layerHeight;y++)
        {
            for (int x=0;x<layerWidth;x++)
            {
                if (canwalk(new Vector3Int(x,y,level), tileMapLayers))
                {
                    toReturn[i] = 0; //0 is walkable
                } else
                {
                    toReturn[i] = 1;
                }
                i++;
            }
        }
        return toReturn;
    }

    public void generateNewPathfindingGrid()
    {
        myGraph = new Graph(generateGrid(), layerWidth, layerHeight);
    }

    public static int getNumberOfTilesToOffsetForLayer(int l)
    {
        return Mathf.RoundToInt((GameManager.Instance.mapWidth + (l - 1) * GameManager.Instance.undergroundWidth));// * GameManager.Instance.tileSize.x);
    }

    ///////////////////////////////////////////////
    // NEW STUFF

    public Vector3 convertToWorldGrid(Vector3Int input)
    {
        if (input.z != level)
            Debug.LogAssertion("trying to convert in the wrong layer");
        return myGrid.CellToWorld(input);
    }

    public List<Tilemap> getTilemapLayers()
    {
        return tileMapLayers;
    }
}
