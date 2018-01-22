using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData {

    public int level;
    public Main.Settings settings;

    public GameData(int level) {
        this.level = level;
        this.settings = Main.settings;
    }
}
