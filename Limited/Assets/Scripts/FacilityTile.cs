﻿using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class FacilityTile
{
	public Vector3Int LocalPlace { get; set; }
	public Tilemap TilemapMember { get; set; }

	public TileBase TileBase { get; set; }

	public string Name { get; set; }

	public Dictionary<string, int> Resources { get; set; }

	public int PollutionRadius { get; set; }
	public bool Extractor { get; set; }

	public HealthBar HealthBar;

	public void Extract()
	{
		if (Extractor)
		{
			EnvironmentTile eTile = GameTiles.instance.environmentTiles[LocalPlace];
			List<string> extractResources = GameTiles.instance.EnvironmentResourceNames;

			// if there are still resources in the ground
			if (GroundHasResources())
			{
				// extract it
				foreach (string resourceName in extractResources)
				{
					eTile.Resources[resourceName] -= Resources[resourceName];
					if (eTile.Resources[resourceName] < 0) eTile.Resources[resourceName] = 0;
				}

				// update health bar
				float ratio = GetResourcesRatio();
				HealthBar.SetValue(ratio);
			}
		}
		else
		{
			Debug.LogError(Name + " can't extract resources.");
		}
	}

	public bool GroundHasResources()
	{
		if (Extractor)
		{
			EnvironmentTile eTile = GameTiles.instance.environmentTiles[LocalPlace];
			List<string> extractResources = GameTiles.instance.EnvironmentResourceNames;

			foreach (string resourceName in extractResources)
			{
				if (!(eTile.Resources[resourceName] >= Resources[resourceName]))
				{
					return false;
				}
			}

			return true;
		}
		else
		{
			Debug.LogError(Name + " can't extract resources.");
			return true;
		}


	}

	public float GetResourcesRatio()
	{
		EnvironmentTile ground = GameTiles.instance.environmentTiles[LocalPlace];
		var groundTypeResources = GameTiles.instance.GetEnvironmentTypes().FindTypeByName(ground.Name).GenerateResourcesDictionary();

		Dictionary<string, int> extractedResources = new Dictionary<string, int>();

		// get ratio in function of ground resources

		// list the resources consumed by the facility and record their amount in the ground tile
		foreach (string resourceName in GameTiles.instance.EnvironmentResourceNames)
		{
			if (Resources[resourceName] > 0)
			{
				extractedResources[resourceName] = ground.Resources[resourceName];
			}
		}

		// total in the ground among all resources extracted
		float groundTotal = 0;
		// total in the ground before any mining was performed
		float groundMax = 0;

		foreach (KeyValuePair<string, int> entry in extractedResources)
		{
			groundTotal += entry.Value;
			groundMax += groundTypeResources[entry.Key];
		}

		return groundTotal / groundMax;
	}
}

// Classes used to retrieve tile data from JSON file
[Serializable]
public class FacilitiesTileType
{
	public string Name;
	public string Description;
	public string SpriteName;

	// consumption of resources is represented by a negative value
	// production by positive value
	public int Oil;
	public int Coal;
	public int Wood;
	public int Metal;
	public int Power;
	public int Goods;
	public int Food;

	public int PollutionRadius;
	public bool Extractor;

	public Dictionary<string, int> GenerateResourcesDictionary()
	{
		return new Dictionary<string, int>{
			{"Oil", Oil},
			{"Coal", Coal},
			{"Wood", Wood},
			{"Metal", Metal},
			{"Power", Power},
			{"Goods", Goods},
			{"Food", Food}
		};
	}

	public bool IsBuildable(EnvironmentTile tile)
	{
		PlayerInventory playerInventory = GameController.instance.playerInventory;
		Dictionary<string, int> inventory = playerInventory.getCount();
		Dictionary<string, int> typeResourcesDictionary = GenerateResourcesDictionary();

		bool canBuild = true;

		if (Name == "City")
		{
			// if the tile is water or the tile is polluted or there is already a building on this tile
			if (tile.Name == "Water" || tile.Polluted || GameTiles.instance.facilitiesTilemap.HasTile(tile.LocalPlace))
			{
				canBuild = false;
			}
		}
		else
		{
			// check if the player has the resources that are needed to build this facility
			foreach (KeyValuePair<string, int> resource in inventory)
			{
				string resourceName = resource.Key;
				int availableAmount = resource.Value;

				int typeAmountForThisRessource = typeResourcesDictionary[resourceName];

				// if extractor
				if (Extractor && GameTiles.instance.EnvironmentResourceNames.Contains(resourceName))
				{
					// get the amount of this ressource in the tile we want to build on
					int tileAmountOfThisRessource = tile.Resources[resourceName];

					// if the amount is negative, means that the facility is consuming from the player inventory
					if (typeAmountForThisRessource < 0)
					{
						// if amount in player inventory is insufficient
						if (inventory[resourceName] < typeAmountForThisRessource)
						{
							canBuild = false;
							break;
						}
					}

					// if it is positive, it means the facility extracts that amount of resources from the ground 
					// and produces that amount for the player
					if (typeAmountForThisRessource > 0)
					{
						// if the amount of this resource in the ground is insufficient
						if (tileAmountOfThisRessource < typeAmountForThisRessource)
						{
							canBuild = false;
							break;
						}
					}


				}
				// if the resource is extracted from the tile by the facility and the resource can be extracted from the ground
				// for example, a coal mine needs power, and coal in the ground, but power can't be extracted from the ground, but
				// taken from the player's inventory.
				// TL;DR - we only apply this "ground" check for the resources that can be in the ground.

				// if the resource is taken from the inventory by the facility
				// only for the numbers indicating a consumption (negative numbers)
				else if (typeAmountForThisRessource < 0)
				{
					// if the amount in the inventory of the player is insufficient
					if (availableAmount < Mathf.Abs(typeAmountForThisRessource))
					{
						canBuild = false;
						break;
					}
				}
			}

			// can't build if there is a city or a farm in the pollution range of this tile
			if (canBuild && PollutionRadius > 0)
			{
				List<Vector3Int> tilesInRange = GameSystem.FindInRange(tile.LocalPlace, PollutionRadius);

				// iterate through all tiles in range
				foreach (Vector3Int tilePos in tilesInRange)
				{
					// if there is a facility at these coordinates
					FacilityTile facilityAtTheseCoordinates;
					if (GameTiles.instance.facilitiesTiles.TryGetValue(tilePos, out facilityAtTheseCoordinates))
					{
						if (facilityAtTheseCoordinates.Name == "City" || facilityAtTheseCoordinates.Name == "Farm")
						{
							canBuild = false;
							break;
						}
					}
				}
			}

			if (canBuild)
			{
				// custom rules
				if (Name == "Farm" && tile.Polluted == true) canBuild = false;
			}
		}

		return canBuild;
	}

}

[Serializable]
public class FacilitiesTileTypeRoot
{
	/* Root class used because the jsonUtility needs the JSON to represent 
       a class (can't parse an array of objects) */
	public FacilitiesTileType[] tileTypes;

	public FacilitiesTileType FindType(string spriteName)
	{
		/* finds the type of a certain tile based on its sprite name
		   returns null if there is no type corresponding to the given name */

		FacilitiesTileType returnType = null;
		for (int i = 0; i < tileTypes.Length; i++)
		{
			var type = tileTypes[i];

			if (spriteName == type.SpriteName)
			{
				returnType = type;
				break;
			}
		}
		return returnType;
	}

	public FacilitiesTileType FindTypeByName(string name)
	{
		FacilitiesTileType returnType = null;
		foreach (FacilitiesTileType type in tileTypes)
		{
			if (type.Name == name)
			{
				returnType = type;
				break;
			}
		}
		return returnType;
	}
}