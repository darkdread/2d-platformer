using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;

public class DialogueScript : MonoBehaviour {

    [SerializeField]
    private TextAsset dialogueDataXMLFile;
    public DialogueContainer dialogueData;
    public static Text dialogueText;
    public static Text dialogueSpeaker;
    public static string dialogueCurrentText;
    public static bool isDialogueAnimating;
    public static Coroutine animateCoroutine;

    public static string speakerName;
    public static int level = 1;
    public static int dialogueNumber = 1;
    public static int textNumber = 0;

    public float letterPerSecond = 10f;

    private static bool justOpened = true;
    private static DialogueContainer.NewDialogue currentDialogue;

    private void Awake () {
        dialogueData = DialogueContainer.LoadDialogueXML();
        dialogueData.Test();

        dialogueText = GameObject.Find("Script").GetComponent<Text>();
        dialogueSpeaker = GameObject.Find("Speaker").GetComponent<Text>();
    }

    private void Update() {
        if (LevelController.isDialogueOpen) {
            if (justOpened) {
                NextDialogueText();
                currentDialogue = dialogueData.GetSpeakerDialogue(level - 1, speakerName, dialogueNumber - 1);
                justOpened = false;
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isDialogueAnimating) {
                    SkipToEndOfDialogue();
                    return;
                }

                if (!NextDialogueText()) {
                    if (NextDialogueSpeaker()) {
                        NextDialogueText();
                    } else {
                        justOpened = true;
                        LevelController.HideDialogue();
                    }
                }
            }
        }
    }

    IEnumerator AnimateText() {
        int i = 0;
        isDialogueAnimating = true;

        while (isDialogueAnimating) {
            i += 1;
            SetDialogueText(dialogueCurrentText.Substring(0, i));

            if (GetDialogueText().Length == dialogueCurrentText.Length) {
                isDialogueAnimating = false;
            }

            yield return new WaitForSeconds(1 / letterPerSecond);
        }
    }

    public static void ChangeDialogueSpeaker(string name) {
        speakerName = name;
        SetDialogueSpeaker(name);
    }

    public static void ChangeDialogueText(int newDialogueNumber, int newTextNumber) {
        textNumber = newTextNumber;
        dialogueNumber = newDialogueNumber;
    }

    public void SkipToEndOfDialogue() {
        isDialogueAnimating = false;
        SetDialogueText(dialogueCurrentText);
    }

    public bool NextDialogueSpeaker() {
        string newSpeaker = dialogueData.GetNextSpeaker(currentDialogue);

        if (newSpeaker != null) {
            ChangeDialogueSpeaker(currentDialogue.nextDialogueSpeaker);
            ChangeDialogueText(currentDialogue.nextDialogueNumber, 0);
            currentDialogue = dialogueData.GetSpeakerDialogue(level - 1, speakerName, dialogueNumber - 1);

            return true;
        }

        return false;
    }

    public bool NextDialogueText() {
        string nextText = dialogueData.GetDialogueText(level - 1, speakerName, dialogueNumber - 1, ++textNumber - 1);

        // Next text exist
        if (nextText != "") {
            dialogueCurrentText = nextText;
            animateCoroutine = StartCoroutine(AnimateText());

            return true;
        } else {
            --textNumber;
        }

        return false;
    }

    public bool NextDialogue() {
        DialogueContainer.NewDialogue nextDialogue = dialogueData.GetSpeakerDialogue(level - 1, speakerName, ++dialogueNumber - 1);

        // Next dialogue exist
        if (nextDialogue != null) {
            currentDialogue = nextDialogue;

            textNumber = 0;
            return true;
        } else {
            --dialogueNumber;
        }

        return false;
    }

    public static string GetDialogueText() {
        return dialogueText.text;
    }

    private static void SetDialogueSpeaker(string name) {
        dialogueSpeaker.text = name;
    }

    private static void SetDialogueText(string text) {
        dialogueText.text = text;
    }
}
