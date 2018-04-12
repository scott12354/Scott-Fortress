using UnityEngine;
using System.Collections.Generic;

public class Node
{

    public List<Node> adjecent = new List<Node>();
    public Node previous = null;
    public string label = "";
    public int index;
    public Vector2 myGrid;

    public Node(int input, Vector2 myGridIn)
    {
        index = input;
        myGrid = myGridIn;
    }

    public void Clear()
    {
        previous = null;
    }

}