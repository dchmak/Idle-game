/*
* Created by Daniel Mak
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

    #region classes

    [System.Serializable]
    public class Dialog {
        public string tag;
        public string speaker;
        [TextArea()]
        public string[] sentences;

        private int currentSentenceIndex = 0;

        public string FirstSentence() {
            currentSentenceIndex = 0;
            return sentences[currentSentenceIndex];
        }

        public string NextSentence() {
            currentSentenceIndex++;
            if (currentSentenceIndex < sentences.Length) return sentences[currentSentenceIndex];
            else {
                //Debug.LogWarning(speaker + " is speechless!");
                return "";
            }
        }
    }

    [System.Serializable]
    public class Upgradable{
        public string tag;
        public float baseCost;
        public float baseMultiplier;
        public float baseOfflineMultiplier;
        public int maxLevel;
        public Sprite sprite;
        [TextArea()] public string description;

        public bool Upgrade() {
            if (PlayerPrefs.GetInt(tag, 0) < maxLevel) {
                PlayerPrefs.SetInt(tag, PlayerPrefs.GetInt(tag, 0) + 1);
                return true;
            }
            return false;
        }

        public float GetMultiplier() {
            return baseMultiplier * PlayerPrefs.GetInt(tag, 0);
        }

        public float GetOfflineMultiplier() {
            return baseMultiplier * PlayerPrefs.GetInt(tag, 0);
        }
    }

    #endregion

    #region public variables

    [Header("Dialogs")]
    public GameObject dialogBox;
    [Range(0f, 1f)] public float typeWaitTime;
    public Dialog[] dialogs;

    [Header("Score Text")]
    public TextMeshProUGUI currencyText;

    [Header("Pause Screen")]
    public GameObject pauseUI;

    [Header("Upgradables")]
    public GameObject upgradeUI;
    public float itemIconWidth;
    public GameObject itemPrefab;
    public Upgradable[] upgradables;

    #endregion

    #region private variables

    private Dialog currentDialog;
    private Animator dialogBoxAnimator;

    private bool isPaused;

    #endregion

    #region methods

    // starts the conversation with tag
    public void StartConversation(string tag) {
        dialogBoxAnimator = dialogBox.GetComponent<Animator>();
        if (dialogBoxAnimator != null) {
            dialogBoxAnimator.SetBool("isOpen", true);
        }

        currentDialog = Array.Find(dialogs, dialog => dialog.tag == tag);
        //Debug.Log("Start conversation with " + d.speaker);

        dialogBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentDialog.speaker + ":";
        StopAllCoroutines();
        StartCoroutine(TypeConversation(currentDialog.FirstSentence()));
    }

    // continues the started conversation
    public void ContinueConversation() {
        string next = currentDialog.NextSentence();

        if (next != "") {
            StopAllCoroutines();
            StartCoroutine(TypeConversation(next));
        } else {
            dialogBoxAnimator.SetBool("isOpen", false);
        }
    }

    // pause the game
    public void Pause() {
        isPaused = true;
    }

    // unpause the game
    public void Unpause() {
        isPaused = false;
    }

    // open upgrade UI
    public void OpenUpgradeUI() {
        upgradeUI.SetActive(true);
    }

    // open upgrade UI
    public void CloseUpgradeUI() {
        upgradeUI.SetActive(false);
    }

    public Upgradable[] AllUpgradables() {
        return upgradables;
    }

    #endregion

    #region private functions

    private void Start() {
        int playedIntro = PlayerPrefs.GetInt("PlayedIntro", 0);
        if (playedIntro == 0) {
            PlayerPrefs.SetFloat("Currency", 0);
            StartConversation("Introduction");
            PlayerPrefs.SetInt("PlayedIntro", 1);
        } else {
            float offlineIncome = TimePassedAfterLastQuit() * GetOfflineMultiplier();
            PlayerPrefs.SetFloat("Currency", PlayerPrefs.GetFloat("Currency", 0) + offlineIncome);
        }

        InitializeUpgradeUI();
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (isPaused) Unpause();
            else Pause();
        }

        pauseUI.SetActive(isPaused);
        if (isPaused) Time.timeScale = 0f;
        else Time.timeScale = 1f;

        float income = Time.deltaTime * GetMultiplier();
        PlayerPrefs.SetFloat("Currency", PlayerPrefs.GetFloat("Currency", 0) + income);
        currencyText.text = "Tick: " + PlayerPrefs.GetFloat("Currency", 0).ToString("F0");

        currencyText.text += " | Multiplier: " + GetMultiplier().ToString() + "X";
    }

    // returns the time passed after last quitting the game.
    private int TimePassedAfterLastQuit() {
        String timeStr = PlayerPrefs.GetString("LastTimeQuit", "");

        if (timeStr != "") {
            return (int)System.DateTime.Now.Subtract(System.DateTime.Parse(timeStr)).TotalSeconds;
        }

        return 0;
    }

    // stores when the game is closed.
    private void OnApplicationQuit() {
        PlayerPrefs.SetString("LastTimeQuit", System.DateTime.Now.ToString());
    }

    // type each characters in sentence one by one
    private IEnumerator TypeConversation(string sentence) {
        TextMeshProUGUI conversation = dialogBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        conversation.text = "";

        foreach (char letter in sentence.ToCharArray()) {
            conversation.text += letter;
            yield return new WaitForSeconds(typeWaitTime);
        }
    }

    // calculate the offline multiplier
    private float GetOfflineMultiplier() {
        float offlineMultiplier = 1f;

        foreach (Upgradable upgrade in upgradables) {
            offlineMultiplier += upgrade.GetOfflineMultiplier();
        }

        return offlineMultiplier;
    }

    // calculate the  multiplier
    private float GetMultiplier() {
        float multiplier = 1f;

        foreach (Upgradable upgrade in upgradables) {
            multiplier += upgrade.GetMultiplier();
        }

        return multiplier;
    }

    // initialize upgrade UI
    private void InitializeUpgradeUI() {
        Transform upgradableItems = upgradeUI.transform.GetChild(1).GetChild(0);

        HorizontalLayoutGroup horizontalLayoutGroup = upgradableItems.GetComponent<HorizontalLayoutGroup>();
        float leftPadding = horizontalLayoutGroup.padding.left;
        float rightPadding = horizontalLayoutGroup.padding.right;
        float spacing = horizontalLayoutGroup.spacing;

        float totalWidth = leftPadding + rightPadding + itemIconWidth * upgradables.Length + spacing * (upgradables.Length - 1);

        RectTransform rt = upgradableItems.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(totalWidth, rt.sizeDelta.y);

        for (int i = 0; i < upgradables.Length; i++) {
            GameObject item = Instantiate(itemPrefab);
            item.transform.SetParent(upgradableItems, false);
            item.GetComponent<Item>().SetIndex(i);

            item.GetComponentInChildren<Image>().sprite = upgradables[i].sprite;
        }
    }

    #endregion
}