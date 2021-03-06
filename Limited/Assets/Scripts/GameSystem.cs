﻿using UnityEngine;
using System.Collections.Generic;

public class GameSystem : MonoBehaviour
{
	/* Object that permits use of different non specific "utility" functions 
       that can be used in any other part of the code */

	public static void DumpToConsole(object obj)
	{
		/* debug function: displays an object to the console */
		var output = JsonUtility.ToJson(obj, true);
		Debug.Log(output);
	}

	public static bool xor(bool a, bool b)
	{
		/* exclusive or (xor) logical operator. */
		return (a && !b) || (!a && b);
	}

	public static int ManhattanDistance(Vector3Int a, Vector3Int b)
	{
		checked
		{
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}
	}

	public static float EuclideanDistance(Vector3 a, Vector3 b)
	{
		/* returns the euclidean distance over x and y axis between two vectors */
		checked
		{
			return (a.x - b.x)*(a.x - b.x) + (a.y - b.y)*(a.y - b.y);
		}
	}

	public static List<Vector3Int> FindInRangeManhattan(Vector3Int originalPosition, int range)
	{
		/* Get all positions within manhattan distance of an origin position */

		List<Vector3Int> tilesInRange = new List<Vector3Int>();

		int minX = originalPosition.x - range;
		int maxX = originalPosition.x + range;
		int minY = originalPosition.y - range;
		int maxY = originalPosition.y + range;

		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				Vector3Int thisPos = new Vector3Int(x, y, 0);
				if (ManhattanDistance(originalPosition, thisPos) <= range)
				{
					tilesInRange.Add(thisPos);
				}
			}
		}

		return tilesInRange;
	}

	public static Color SignColor(float value)
	{
		Color color;

		if (value < 0)
		{
			color = GameController.instance.negativeColor;
		}
		else
		{
			color = GameController.instance.positiveColor;
		}

		return color;
	}

	public static Vector3Int RandomInsideBoundsInt(BoundsInt bounds)
	{
		return new Vector3Int(
			Random.Range(bounds.x, bounds.xMax),
			Random.Range(bounds.y, bounds.yMax),
			Random.Range(bounds.z, bounds.zMax)
		);
	}
}
