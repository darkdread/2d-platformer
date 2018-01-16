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
    public static string dialogueCurrentText;
    public static bool isDialogueAnimating;
    public static Coroutine animateCoroutine;

    public string speakerName;
    public int level;
    public int dialogueNumber;
    public int textNumber;

    public float letterPerSecond = 10f;
    
    private void Awake () {
        dialogueData = DialogueContainer.LoadDialogueXML();
        dialogueData.Test();

        dialogueText = GameObject.Find("Dialogue Box").GetComponentInChildren<Text>();
    }
    

    private void Update() {
        if (LevelController.isDialogueOpen) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isDialogueAnimating) {
                    SkipToEndOfDialogue();
                    return;
                }

                if (NextDialogueText()) {
                    
                } else if (NextDialogue()) {
                    NextDialogueText();
                } else {
                    ChangeDialogueSpeaker("notMain");
                    ChangeDialogueText(1, 0);
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

    public void ChangeDialogueSpeaker(string name) {
        speakerName = name;
    }

    public void ChangeDialogueText(int newDialogueNumber, int newTextNumber) {
        textNumber = newTextNumber;
        dialogueNumber = newDialogueNumber;
    }

    public void SkipToEndOfDialogue() {
        isDialogueAnimating = false;
        SetDialogueText(dialogueCurrentText);
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

    public static void SetDialogueText(string text) {
        dialogueText.text = text;
    }
}
