using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public static class SaveLoad {

    public static List<Game> savedGames = new List<Game>();
    public static GameObject[,] savedMap;
    public static string folderName = "Maps";

    public static void SaveMap(string fileName) {
        SaveLoad.savedMap = Game.tiles;

        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(string.Format(
            "{0}/{1}.OMEGALUL",
            folderName,
            fileName
        ));

        GameObjectInScene[] goList = new GameObjectInScene[Game.gridHeight * Game.gridWidth];
        int index = 0;

        foreach (var t in Game.tiles) {
            GameObjectInScene go = new GameObjectInScene(t.name, t.transform.localScale, t.transform.position, t.transform.rotation);

            goList[index] = go;
            index++;
        }

        string json = JsonHelper.ToJson(goList);
        //Debug.Log(json);

        bf.Serialize(file, json);
        file.Close();
    }

    public static string LoadMap(string fileName) {
        string filePath = string.Format(
            "{0}/{1}.OMEGALUL",
            folderName,
            fileName
            );
        if (File.Exists(filePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            string json = (string) bf.Deserialize(file);
            file.Close();

            return json;
        }
        return null;
    }
}