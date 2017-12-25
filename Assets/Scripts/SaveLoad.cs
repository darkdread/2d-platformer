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

        // What it does
        // ============================
        // First, it takes the map's width and height and stores it into the class GameData.
        // Then, it will search the entire map for tiles that aren't Grids. Add them into a List of type GameObjectInScene.
        // The GameObjectInScene will store its position, rotation, name and scale.
        // After storing them into GameData, we will serialize GameData into a Json string. Stream the Json string into a folder.

        GameData gameData = new GameData() {
            mapWidth = Game.gridWidth,
            mapHeight = Game.gridHeight
        };

        List<GameObjectInScene> tileList = new List<GameObjectInScene>();

        int count = 0, index = 0;

        foreach (var t in Game.tiles) {
            if (t.name == "Grid") {
                continue;
            }

            tileList.Add(new GameObjectInScene(t.name, t.transform.localScale, t.transform.position, t.transform.rotation));

            count++;
        }
        
        gameData.tiles = new GameObjectInScene[count];

        foreach(GameObjectInScene tile in tileList) { 
            GameObjectInScene go = new GameObjectInScene(tile.name, tile.scale, tile.position, tile.rotation);
            
            gameData.tiles[index] = go;
            index++;
        }

        string json = JsonUtility.ToJson(gameData);

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