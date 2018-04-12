using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;

public class TileMapTestManager : MonoBehaviour{


    [SerializeField]
    Grid mainMapGrid;
    [SerializeField]
    List<TileBase> tileRefs;
    [SerializeField]
    Tilemap tileMapPrefab;
    [SerializeField]
    float chanceToCell=45;
    [SerializeField]
    int reqToBirth = 3;
    [SerializeField]
    int reqToKill = 2;


    int mapWidth, mapHeight;
    [SerializeField]
    int iterations = 5;

    private Tilemap baseLayer, wallLayer;

    // Use this for initialization
    void Start () {
        mapWidth = 50;
        mapHeight = 50;

        baseLayer = Tilemap.Instantiate(tileMapPrefab);
        baseLayer.GetComponent<TilemapRenderer>().sortingLayerName = "Map Tiles";
        wallLayer = Tilemap.Instantiate(tileMapPrefab);
        wallLayer.GetComponent<TilemapRenderer>().sortingLayerName = "Walls";
        //Tilemap
        baseLayer.transform.SetParent(mainMapGrid.transform);
        wallLayer.transform.SetParent(mainMapGrid.transform);

        configureSize();
        //setRandomWalls();
        generateCaves(wallLayer);
	}

    void Update()
    {
        
    }

    private void generateCaves(Tilemap inputLayer)
    {
        bool[,] tempMap = new bool[mapWidth,mapHeight];
        //First, set some random Walls
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < mapWidth; temp.x++)
            {
                if (UnityEngine.Random.Range(0, 100) <= chanceToCell)
                    tempMap[temp.x, temp.y] = true;
            }
        }

        //Iterate a few times
        for (int i=0;i< iterations; i++)
            tempMap = iterate(tempMap);

        for (temp.y = 0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < mapWidth; temp.x++)
            {
                if (!tempMap[temp.x, temp.y])
                    inputLayer.SetTile(temp,tileRefs[2]);
            }
        }
    }

    private bool[,] iterate(bool[,] originalMap)
    {
        bool[, ] newLayer = new bool[mapWidth, mapHeight];
        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y = 0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x = 0; temp.x < mapWidth; temp.x++)
            {
                int count = countAliveNeighbours(originalMap, temp);
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
    
    private void configureSize()
    {
        //int height = GameManager.Instance.mapHeight;
        //int width = GameManager.Instance.mapWidth;

        Vector3Int temp = new Vector3Int(0, 0, 0);
        for (temp.y=0; temp.y < mapHeight; temp.y++)
        {
            for (temp.x=0;temp.x<mapWidth;temp.x++)
            {
                baseLayer.SetTile(temp, tileRefs[0]);
            }
        }
    }
    
    public int countAliveNeighbours(bool[,] map, Vector3Int location)
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
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= mapWidth || neighbour_y >= mapHeight)
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
}
