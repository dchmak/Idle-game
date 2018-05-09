/*
* Created by Daniel Mak
*/

using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

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

    [Header("Dialogs")]
    public GameObject dialogBox;
    public Dialog[] dialogs;
    [Range(0f, 1f)] public float typeWaitTime;

    [Header("Score Text")]
    public TextMeshProUGUI totalTimeText;

    [Header("Pause Screen")]
    public GameObject pauseUI;

    private Dialog currentDialog;
    private Animator dialogBoxAnimator;

    private bool isPaused;

	private void Start() {
        //Debug.Log(TimePassedAfterLastQuit());

        int playedIntro = PlayerPrefs.GetInt("PlayedIntro", 0);
        if (playedIntro == 0) {
            PlayerPrefs.SetFloat("Currency", 0);
            StartConversation("Introduction");
            PlayerPrefs.SetInt("PlayedIntro", 1);
        }
	}

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (isPaused) Unpause();
            else Pause();
        } 

        pauseUI.SetActive(isPaused);
        if (isPaused) Time.timeScale = 0f;
        else Time.timeScale = 1f;

        float income = Time.deltaTime * PlayerPrefs.GetFloat("CurrencyMultiplier", 1);
        PlayerPrefs.SetFloat("Currency", PlayerPrefs.GetFloat("Currency", 0) + income);
        totalTimeText.text = "Tick: " + PlayerPrefs.GetFloat("Currency", 0).ToString("F0");
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

    // type each characters in sentence one by one
    private IEnumerator TypeConversation(string sentence) {
        TextMeshProUGUI conversation = dialogBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        conversation.text = "";

        foreach (char letter in sentence.ToCharArray()) {
            conversation.text += letter;
            yield return new WaitForSeconds(typeWaitTime);
        }
    }

    // set the currency multiplier
    public void SetCurrencyMultiplier() {
        PlayerPrefs.SetFloat("CurrencyMultiplier", PlayerPrefs.GetFloat("CurrencyMultiplier", 1) + 0.5f);
    }
}