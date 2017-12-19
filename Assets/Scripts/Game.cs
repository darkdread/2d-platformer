using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Game : SerializedMonoBehaviour {

    public static Game current;
    public Main main;
    public MyDictionary myDictionary;
    public Dictionary<string, GameObject> TileDictionary;

    //size is +2 because of outer walls [left/right, top/btm]
    public static int gridWidth = 60+2;
    public static int gridHeight = 32+2;
    public GameObject player;
    public GameObject gameHolder;
    public CameraController cameraController;

    //public static Dictionary<Vector2, GameObject> tiles;
    public static GameObject[,] tiles;

    public GameObject[,] Tiles {
        get {
            return tiles;
        }

        set {
            tiles = value;
        }
    }

    // Use this for initialization
    void Start () {
        TileDictionary = myDictionary.TileDictionary;
        tiles = new GameObject[gridWidth, gridHeight];
	}
	
	// Update is called once per frame
	void Update () {
		if (!Main.EditorMode && Input.GetKeyDown(KeyCode.Escape)) {
            BackToMenu();
        }
	}

    private void BackToMenu() {
        ClearMap();
        main.ShowMenu();
    }

    public GameObject AddTile(string type, Vector3 position) {
        GameObject tile = Instantiate(Resources.Load<GameObject>("Tiles/" + type));
        tile.name = type;
        tile.transform.position = position;
        tile.transform.SetParent(gameHolder.transform);
        
        //pause the tile from moving
        if (Main.EditorMode) {
            if (!Editor.FilterTile(tile.name)) {
                tile.AddComponent<EditableTile>();
            }
            Rigidbody2D rb = tile.GetComponent<Rigidbody2D>();
            if (rb) {
                rb.isKinematic = true;
            }
        }

        tiles[(int)tile.transform.position.x, (int)tile.transform.position.y] = tile;
        return tile;
    }

    public void ClearMap() {
        if (tiles[0, 0] == null) return;
        foreach(var t in tiles) {
            Destroy(t.gameObject);
        }
    }

    public void GenerateMapFromJson(string json) {
        GameObjectInScene[] goList = JsonHelper.FromJson<GameObjectInScene>(json);

        foreach(var t in goList) {
            GameObject tile = AddTile(t.name, t.position);
            int xPos = (int)t.position.x;
            int yPos = (int)t.position.y;

            //playing the game
            if (!Main.EditorMode) {
                switch (t.name) {
                    case "player":
                        cameraController.player = tile;
                        break;
                    case "grid":
                        Destroy(tile);
                        break;
                }
            }
        }
    }

    //generate blank map for editor
    public void GenerateMap() {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                GameObject tile;
                if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1) {
                    tile = Instantiate(TileDictionary["outerWall"]);
                    tile.name = "outerWall";
                } else {
                    tile = Instantiate(TileDictionary["grid"]);
                    tile.name = "grid";
                    tile.AddComponent<EditableTile>();
                }
                tile.transform.SetParent(gameHolder.transform);
                tile.transform.position = new Vector3(x, y, 0);
                tiles[x, y] = tile;
            }
        }

        cameraController.transform.position = new Vector3(30, 15, cameraController.transform.position.z);
    }


}
