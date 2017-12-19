using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public static class SaveLoad {

    public static List<Game> savedGames = new List<Game>();
    public static GameObject[,] savedMap;

    //it's static so we can call it from anywhere
    public static void Save() {
        SaveLoad.savedGames.Add(Game.current);
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd"); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }

    public static void SaveMap() {
        SaveLoad.savedMap = Game.tiles;

        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/savedMap.gd"); //you can call it anything you want

        GameObjectInScene[] goList = new GameObjectInScene[Game.gridHeight * Game.gridWidth];
        int index = 0;

        foreach (var t in Game.tiles) {
            GameObjectInScene go = new GameObjectInScene(t.name, t.transform.localScale, t.transform.position, t.transform.rotation);

            goList[index] = go;
            index++;
        }

        string json = JsonHelper.ToJson(goList);
        Debug.Log(json);

        bf.Serialize(file, json);
        file.Close();
    }

    public static string LoadMap() {
        if (File.Exists(Application.persistentDataPath + "/savedMap.gd")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedMap.gd", FileMode.Open);
            string json = (string) bf.Deserialize(file);
            file.Close();

            return json;
        }
        return null;
    }

    public static void Load() {
        if (File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            SaveLoad.savedGames = (List<Game>)bf.Deserialize(file);
            file.Close();
        }
    }
}