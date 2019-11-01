﻿using System;
using UnityEngine.Tilemaps;
using UnityEngine;

[Serializable]
public class EnvironmentTile
{
	public Vector3Int LocalPlace { get; set; }

	public TileBase TileBase { get; set; }

	public Tilemap TilemapMember { get; set; }

	public string Name { get; set; }

	public int Oil { get; set; }

	public int Coal { get; set; }
	public int Wood { get; set; }
	public int Metal { get; set; }
	public bool Polluted { get; set; }
}

// Classes used to retrieve tile data from JSON file
[Serializable]
public class EnvironmentTileType
{
	public string Name;
	public string[] SpriteNames;

	public int Oil;

	public int Coal;
	public int Wood;
	public int Metal;
}

// Root class used because the jsonUtility needs the JSON to represent 
// a class (can't parse an array of objects)
[Serializable]
public class EnvironmentTileTypeRoot
{
	public EnvironmentTileType[] tileTypes;

	public EnvironmentTileType FindType(string spriteName)
	{
		/* finds the type of a certain tile based on its sprite name
		   returns null if there is no type corresponding to the given name */

		EnvironmentTileType returnType = null;
		foreach (EnvironmentTileType type in tileTypes)
		{
			foreach (string name in type.SpriteNames)
			{
				if (spriteName == name)
				{
					returnType = type;
					break;
				}
			}
		}
		return returnType;
	}
}
