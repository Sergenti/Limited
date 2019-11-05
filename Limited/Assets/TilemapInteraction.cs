﻿using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TilemapInteraction : MonoBehaviour
{
	public Tilemap environmentTilemap;
	public Tilemap facilitiesTilemap;
	public BuildDialogBoxAPI dialogBox;

	// registry of highlighted tiles
	private List<Vector3Int> highlightedPositions = new List<Vector3Int>();

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// prevents clicking through UI
			if (EventSystem.current.IsPointerOverGameObject())
			{
				Debug.Log("Clicked on the UI");
			}
			else
			{
				EnvironmentTile eTile;
				FacilityTile fTile;

				// get the position of the tile being clicked on
				Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector3Int tilePos = environmentTilemap.WorldToCell(point);

				var environmentTiles = GameTiles.instance.environmentTiles; // This is our Dictionary of tiles
				var facilitiesTiles = GameTiles.instance.facilitiesTiles;

				bool environmentTileHere = false;
				bool facilityTileHere = false;

				if (facilitiesTiles.TryGetValue(tilePos, out fTile))
				{
					// debug
					Debug.Log(createFacilityLog(fTile));

					facilityTileHere = true;
				}
				// if we have a tile registered at this position
				if (environmentTiles.TryGetValue(tilePos, out eTile))
				{
					// Debug
					Debug.Log(createEnvironmentLog(eTile));



					environmentTileHere = true;
				}

				if (environmentTileHere)
				{
					// check if tile is highlighted
					if (eTile.TilemapMember.GetColor(eTile.LocalPlace) == Color.green)
					{
						// build new city
						GameTiles.instance.BuildFacility(GameTiles.instance.GetFacilitiesTypes().FindType("Tileset_facilities_0"), eTile.LocalPlace);
						removeHighlights();
					}
					else if (eTile.Name != "Water")
					{
						if (!facilityTileHere)
						{
							doBuildDialogBox(tilePos);
						}
					}


				}
				else
				{
					if (dialogBox.IsOpen())
					{
						dialogBox.Enabled(false);
					}
				}


			}

		}
	}

	public void HighlightTile(EnvironmentTile tile)
	{
		// turn tile green
		tile.TilemapMember.SetTileFlags(tile.LocalPlace, TileFlags.None);
		tile.TilemapMember.SetColor(tile.LocalPlace, Color.green);
		Debug.Log(tile.TilemapMember.GetColor(tile.LocalPlace));

		highlightedPositions.Add(tile.LocalPlace);
	}

	public void removeHighlights()
	{
		foreach (Vector3Int pos in highlightedPositions)
		{
			// to remove the tint, just set the color to white
			environmentTilemap.SetColor(pos, Color.white);
		}

		// reset registry
		highlightedPositions = new List<Vector3Int>();
	}

	private void doBuildDialogBox(Vector3Int pos)
	{
		if (dialogBox.IsOpen())
		{
			dialogBox.Enabled(false);
		}
		else
		{
			dialogBox.Enabled(true);
			dialogBox.MoveTo(pos);
			dialogBox.UpdateButtons(pos);
		}
	}

	private string createFacilityLog(FacilityTile ft)
	{
		string str = "";
		str += "Name: " + ft.Name;
		str += "\nSprite: " + ft.TileBase.ToString();
		foreach (string resourceName in GameTiles.instance.FacilitiesResourceNames)
		{
			str += "\n" + resourceName + ": " + ft.Resources[resourceName];
		}
		str += "\nPollution radius: " + ft.PollutionRadius;

		return str;
	}

	private string createEnvironmentLog(EnvironmentTile et)
	{
		string str = "";
		str += "Name: " + et.Name;
		str += "\nSprite: " + et.TileBase.ToString();
		foreach (string resourceName in GameTiles.instance.EnvironmentResourceNames)
		{
			str += "\n" + resourceName + ": " + et.Resources[resourceName];
		}

		return str;
	}
}
