using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("Dialogue")]
public class DialogueContainer {

    public class Lmao {

        [XmlElement("NewDialogue")]
        public NewDialogue[] Dialogue;
    }

    public class NewDialogue {
        
        [XmlElement("Text")]
        public string[] Text;
    }

    // For each level, there are multiple dialogues and texts in each of these dialogues.

    [XmlArray("Level")]
    [XmlArrayItem("Lmao")]
    public List<Lmao> lmaos = new List<Lmao>();

    public static DialogueContainer LoadDialogueXML() {

        var serializer = new XmlSerializer(typeof(DialogueContainer));
        using (var stream = new FileStream(Path.Combine(Application.dataPath, "dialogue.xml"), FileMode.Open)) {
            //Debug.Log(stream);
            return serializer.Deserialize(stream) as DialogueContainer;
        }
    }

    public string GetDialogueText(int level, int dialogue, int index) {
        if (lmaos.Count - 1 < level) return "";
        else if (lmaos[level].Dialogue.Length - 1 < dialogue) return "";
        else if (lmaos[level].Dialogue[dialogue].Text.Length - 1 < index) return "";
        return lmaos[level].Dialogue[dialogue].Text[index];
    }

    public void Test() {
        int dialogueCount = 0;
        foreach(Lmao lmao in lmaos) {
            dialogueCount += lmao.Dialogue.Length;
        }

        Debug.Log(string.Format("There are {0} dialogues overall.", dialogueCount));
    }

}
