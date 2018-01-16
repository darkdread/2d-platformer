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

    public int level;
    public int dialogueNumber;
    public int textNumber;

    public float letterPerSecond = 10f;
    
    private void Awake () {
        dialogueData = DialogueContainer.LoadDialogueXML();
        dialogueData.Test();
        print(dialogueData.lmaos[0].Dialogue.Length);

        dialogueText = GameObject.Find("Dialogue Box").GetComponentInChildren<Text>();
    }
    public static bool isDialogueRunning;

    private void Update() {
        if (LevelController.isDialogueOpen) {
            if (Input.GetKeyDown(KeyCode.C)) {
                if (isDialogueRunning) return;

                if (NextDialogueText()) {
                    
                } else if (NextDialogue()) {
                    NextDialogueText();
                }
            }
        }
    }

    IEnumerator AnimateText() {

        for (int i = 0; i < dialogueCurrentText.Length + 1; i++) {
            SetDialogueText(dialogueCurrentText.Substring(0, i));

            if (i == dialogueCurrentText.Length) {
                isDialogueRunning = false;
            }
            yield return new WaitForSeconds(1/letterPerSecond);
        }
    }

    public bool NextDialogueText() {
        // Next text exist
        string nextText = dialogueData.GetDialogueText(level - 1, dialogueNumber - 1, ++textNumber - 1);
        if (nextText != "") {
            dialogueCurrentText = nextText;
            isDialogueRunning = true;
            StartCoroutine(AnimateText());

            return true;
        } else {
            --textNumber;
        }

        return false;
    }

    public bool NextDialogue() {
        bool nextDialogue = dialogueData.lmaos[level - 1].Dialogue.Length >= ++dialogueNumber;
        if (nextDialogue) {
            // Next dialogue exist
            textNumber = 0;
        } else {
            --dialogueNumber;
        }

        return nextDialogue;
    }

    public static void SetDialogueText(string text) {
        dialogueText.text = text;
    }
}
