/*
* Created by Daniel Mak
*/

using System;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private void Start () {
        String timeStr = PlayerPrefs.GetString("LastTimeQuit", "");

        if (timeStr != "") {
            // Time passed after last quitting the game.
            Debug.Log(System.DateTime.Now.Subtract(System.DateTime.Parse(timeStr)).TotalSeconds);
        }
	}

    private void OnApplicationQuit() {
        PlayerPrefs.SetString("LastTimeQuit", System.DateTime.Now.ToString());
    }
}