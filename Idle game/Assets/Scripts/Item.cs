/*
* Created by Daniel Mak
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item : MonoBehaviour {

    private int index;
    private Slider slider;
    private GameManager gameManager;
    private string itemTag;

    private void Start() {
        slider = GetComponentInChildren<Slider>();
        gameManager = FindObjectOfType<GameManager>();

        slider.minValue = 0;
        slider.maxValue = gameManager.AllUpgradables()[index].maxLevel;

        itemTag = gameManager.AllUpgradables()[index].tag;
    }

    private void Update() {
        slider.value = PlayerPrefs.GetInt(itemTag, 0);

        TextMeshProUGUI textGUI = GetComponentInChildren<TextMeshProUGUI>();
        textGUI.text = "LV: ";

        int lv = PlayerPrefs.GetInt(itemTag, 0);
        if (lv == gameManager.AllUpgradables()[index].maxLevel) {
            textGUI.text += "MAX | Cost: -";
        } else {
            textGUI.text += lv.ToString() + " | Cost: ";
            textGUI.text += (gameManager.AllUpgradables()[index].baseCost * (PlayerPrefs.GetInt(itemTag, 0) + 1)).ToString();
        }        
    }

    public void SetIndex(int i) {
        index = i;
    }

    public void Upgrade() {
        float cost = gameManager.AllUpgradables()[index].baseCost * (PlayerPrefs.GetInt(itemTag, 0) + 1);

        if (PlayerPrefs.GetFloat("Currency", 0) > cost) {
            PlayerPrefs.SetFloat("Currency", PlayerPrefs.GetFloat("Currency", 0) - cost);
            gameManager.AllUpgradables()[index].Upgrade();
        }
    }
}