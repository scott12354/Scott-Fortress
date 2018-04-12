using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class SpriteLibrary {

    private List<Sprite> mySprites;
	private Color[][] baseTileColorArrays;

    private static SpriteLibrary _instance;
    public static SpriteLibrary Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SpriteLibrary(GameManager.Instance.mapTextures);
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public SpriteLibrary(Texture2D[] textures)
    {
        mySprites = new List<Sprite>();
		Texture2D baseTileTexture = textures [0];
        foreach (Texture2D t in textures)
        {
            Sprite[] temparray = Resources.LoadAll<Sprite>(t.name);
            foreach (Sprite s in temparray)
            {
                mySprites.Add(s);
            }
        }
		setupColorArrays (baseTileTexture);
    }

    public Sprite getMinionSprite()
    {
        return mySprites.First(x => x.name == "Minion Main");
    }

	public Color[] getMapTileColours(int autotileID)
    {
        if (autotileID == -1)
            return baseTileColorArrays[16];
        return baseTileColorArrays[autotileID];
    }

    public Sprite getSprite(string name)
    {
		var temp = mySprites.FirstOrDefault(x => x.name == name);

		if (temp != null) {
			return temp;
		} else {
			Debug.Log ("Sprite doesn't exist for: " + name + ".");
			return null;
		}
    }

	private void setupColorArrays(Texture2D theTexture ) {
		baseTileColorArrays = new Color[17][]; //16 base types and one wall texture

		for (int i=0;i < 17;i++) {
			Sprite s = mySprites [i];
			Rect pullRectangle = s.textureRect;
			int x = Mathf.RoundToInt (pullRectangle.x);
			int y = Mathf.RoundToInt (pullRectangle.y);
			int width = Mathf.RoundToInt (pullRectangle.width);
			int height = Mathf.RoundToInt (pullRectangle.height);
			baseTileColorArrays[i] = s.texture.GetPixels (x, y, width, height);
		}
	}
		
}
