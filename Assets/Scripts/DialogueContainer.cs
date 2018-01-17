using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Reflection;

[XmlRoot("Dialogue")]
public class DialogueContainer {

    public class Level {

        [XmlAttribute("name")]
        public string name;

        [XmlElement("Speaker")]
        public Speaker[] speaker;
        //public List<Speaker> Speaker = new List<Speaker>();
    }

    public class Speaker {

        [XmlAttribute("name")]
        public string name;

        [XmlElement("NewDialogue")]
        public NewDialogue[] Dialogue;
    }

    public class NewDialogue {

        [XmlAttribute("id")]
        public int id;

        [XmlAttribute("nextDialogueSpeaker")]
        public string nextDialogueSpeaker;

        [XmlAttribute("nextDialogueNumber")]
        public int nextDialogueNumber;

        [XmlElement("Text")]
        public string[] Text;
    }

    // For each level, there are multiple dialogues and texts in each of these dialogues.

    [XmlArray("LevelContainer")]
    [XmlArrayItem("Level")]
    public List<Level> levels = new List<Level>();

    public static DialogueContainer LoadDialogueXML() {

        var serializer = new XmlSerializer(typeof(DialogueContainer));
        using (var stream = new FileStream("dialogue.xml", FileMode.Open)) {
            return serializer.Deserialize(stream) as DialogueContainer;
        }
    }

    public string GetDialogueText(int level, string dialogueSpeaker, int dialogue, int index) {

        if (this.levels.Count - 1 < level) return "";

        Level l = levels[level];
        foreach (Speaker speaker in l.speaker) {
                
            if (speaker.name == dialogueSpeaker) {
                if (speaker.Dialogue.Length - 1 >= dialogue) {
                    if (speaker.Dialogue[dialogue].Text.Length - 1 >= index) {

                        return speaker.Dialogue[dialogue].Text[index];
                    }
                }
            }
        }

        return "";
    }

    public string GetNextSpeaker(NewDialogue dialogue) {
        if (dialogue.nextDialogueSpeaker == "")
            return null;
        return dialogue.nextDialogueSpeaker;
    }

    public NewDialogue GetNextSpeakerDialogue(NewDialogue dialogue) {
        if (dialogue.nextDialogueSpeaker == null)
            return null;

        return GetSpeakerDialogue(DialogueScript.level, dialogue.nextDialogueSpeaker, dialogue.nextDialogueNumber);
    }

    public NewDialogue GetSpeakerDialogue(int level, string dialogueSpeaker, int dialogue) {

        if (this.levels.Count - 1 < level) return null;

        Level l = levels[level];
        foreach (Speaker speaker in l.speaker) {

            if (speaker.name == dialogueSpeaker) {
                if (speaker.Dialogue.Length - 1 >= dialogue) {
                    return speaker.Dialogue[dialogue];
                }
            }
        }

        return null;
    }

    public void Test() {
        int dialogueCount = 0;
        foreach(Level level in levels) {
            foreach (Speaker speaker in level.speaker) {
                dialogueCount += speaker.Dialogue.Length;
            }
        }

        Debug.Log(string.Format("Level: {0}, Speaker name: {1}, Dialogue number: {2}, Dialogue text: {3}, Next speaker name: {4}, id: {5}", 1, levels[0].speaker[0].name, 1, levels[0].speaker[0].Dialogue[0].Text, levels[0].speaker[0].Dialogue[0].nextDialogueSpeaker, levels[0].speaker[0].Dialogue[0].id));
        Debug.Log(string.Format("There are {0} dialogues overall.", dialogueCount));
    }

}
