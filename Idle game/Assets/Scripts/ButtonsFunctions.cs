/*
* Created by Daniel Mak
*/

using UnityEngine;

public class ButtonsFunctions : MonoBehaviour {

    public void ResetAllPrefs() {
        PlayerPrefs.DeleteAll();
    }
}