﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
	public TutorialBox tutorialBox;

	[SerializeField] // makes private attribute appear in the editor
	[TextArea(5, 5)] // makes the text box in the editor bigger (5 lines high)
	private string[] messages;
	private int currentMessageIndex = 0;

	public void Initialize()
	{
		if (!PlayerPrefs.HasKey("TutorialEnabled"))
		{
			// if the key has not been initialized, initialize it to true
			// (Enable tutorial by default since this is the first launch of the game)
			PlayerPrefs.SetInt("TutorialEnabled", 1);
		}

		// tutorial is enabled
		if (PlayerPrefs.GetInt("TutorialEnabled") == 1)
		{
			tutorialBox.gameObject.SetActive(true);
			NextBox();
		}
		// tutorial is disabled
		else
		{
			// disable this script
			gameObject.SetActive(false);
		}
	}

	public void NextBox()
	{
		// if we finished the list of messages, close the tutorial box with an animation
		if (currentMessageIndex >= messages.Length)
		{
			tutorialBox.TriggerClosingAnimation();
			Debug.Log("End of tutorial boxes reached.");
			return;
		}

		if (currentMessageIndex == messages.Length - 1)
		{
			tutorialBox.ShowDots(false);
		}

		// display current message
		tutorialBox.SetText(messages[currentMessageIndex]);

		// set next message index
		currentMessageIndex++;
	}
}
