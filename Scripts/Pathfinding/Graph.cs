using UnityEngine;
using System.Collections;

public class Graph {

	public int rows;
    public int cols;
	public Node[] nodes;

	public Graph(int[] grid, int width, int height){
        rows = height;
        cols = width;
		nodes = new Node[rows*cols];
        int currentRow = -1;
        int currentCol = 0;
		for (var i = 0; i < nodes.Length; i++) {
            currentCol = i % cols;
            if (currentCol == 0)
                currentRow++;
			var node = new Node(i, new Vector2(currentCol, currentRow));
			node.label = i.ToString();
			nodes[i] = node;
		}

		for (var r = 0; r < rows; r++) {
			for(var c = 0; c < cols; c++){
				var node = nodes[cols*r + c];

				if(grid[(r*cols)+c] == 1){
                    //if not walkable, go to the next one
					continue;
				}

				// Up
				if(r < rows-1){
					node.adjecent.Add(nodes[((r+1)*cols)+c]);
				}

				// Right
				if(c < cols-1){
					node.adjecent.Add(nodes[(cols*r)+c+1]);
				}

				// Down
				if(r > 0){
					node.adjecent.Add(nodes[(cols*(r-1))+c]);
				}

				// Left
				if(c > 0){
					node.adjecent.Add(nodes[cols*r+c-1]);
				}

			}
		}
	}

    public void update(int[] grid)
    {
        int rows = GameManager.Instance.mapHeight;
        int cols = GameManager.Instance.mapWidth;

        nodes = new Node[rows * cols];
        int currentRow = -1;
        int currentCol = 0;
        for (var i = 0; i < nodes.Length; i++)
        {
            currentCol = i % cols;
            if (currentCol == 0)
                currentRow++;
            var node = new Node(i, new Vector2(currentCol, currentRow));
            node.label = i.ToString();
            nodes[i] = node;
        }

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var node = nodes[cols * r + c];

                if (grid[(r * cols) + c] == 1)
                {
                    //if not walkable, go to the next one
                    continue;
                }

                // Up
                if (r < rows - 1)
                {
                    node.adjecent.Add(nodes[((r + 1) * cols) + c]);
                }

                // Right
                if (c < cols - 1)
                {
                    node.adjecent.Add(nodes[(cols * r) + c + 1]);
                }

                // Down
                if (r > 0)
                {
                    node.adjecent.Add(nodes[(cols * (r - 1)) + c]);
                }

                // Left
                if (c > 0)
                {
                    node.adjecent.Add(nodes[cols * r + c - 1]);
                }

            }
        }
    }

    public Node getNodeAt(Vector3 vec)
    {

        try
        {
            //this is somehow getting a abnormally large vec.x value.  Possibly not accounting for the underground layres offset
            float index = vec.y * cols + vec.x;
            int dex = Mathf.RoundToInt(index);
            return nodes[dex];
        }
        catch
        {
            return null;
        }
    }

    public Vector2Int getGridOfNodeAtIndex(int index)
    {
        int x = index % cols;
        int y = (index - x) / cols;

        return new Vector2Int(x, y);
    }

}
