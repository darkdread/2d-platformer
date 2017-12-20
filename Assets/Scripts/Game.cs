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

    private bool combined;
    public static GameObject[,] tiles;
    private static int[,] NEIGHBOURS = {
    {-1, -1}, {0, -1}, {+1, -1},
    {-1, 0},           {+1, 0},
    {-1, +1}, {0, +1}, {+1, +1}};

    // [the offset array, the offset array's element index]
    // to get +1, do [5, 1]
    // {-1, +1}

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
        print(NEIGHBOURS[5, 1]);
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
        
        // Disable the tile from moving & make tile editable
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

    private List<GameObject> GetNeighborTiles(GameObject tile) {
        int x = (int)tile.transform.position.x;
        int y = (int)tile.transform.position.y;
        List<GameObject> neighbourTiles = new List<GameObject>();
        int count = 0;

        for (int i = 0; i < NEIGHBOURS.GetLength(0); i++) {
            int xPos = x + NEIGHBOURS[i, 0];
            int yPos = y + NEIGHBOURS[i, 1];

            // If the position is not out of bounds
            if ((xPos >= 0 && xPos < tiles.GetLength(0)) && (yPos >= 0 && yPos < tiles.GetLength(1))){
                GameObject selectedTile = tiles[xPos, yPos];

                // Why check if tile is alive? Because we are getting tiles while we are creating it. Will return 4 results (excluding self) when creating and searching
                /* {tile}    {tile}   {not created}
                 * {tile}    {self}   {not created}
                   {tile}{not created}{not created}
                */
                if (selectedTile != null) {
                    //print(string.Format("x: {0}, y: {1}, name: {2}", xPos, yPos, selectedTile.gameObject.name));
                    //neighbourTiles[count] = selectedTile;
                    neighbourTiles.Add(selectedTile);
                    count++;
                }
            }
        }

        return neighbourTiles;
    }

    private void CombineCollider(GameObject tile) {
        if (tile.CompareTag("Ground") && tile.GetComponent<BoxCollider2D>()) {

            List<GameObject> neighbors = GetNeighborTiles(tile);

            foreach(var neighbor in neighbors) {
                if (neighbor.CompareTag("Ground")) {
                    // Combine the box collider and remove its component
                }
            }
            
            //tiles[x, y];
        }
    }

    public void GenerateMapFromJson(string json) {
        GameObjectInScene[] goList = JsonHelper.FromJson<GameObjectInScene>(json);

        foreach(var t in goList) {
            GameObject tile = AddTile(t.name, t.position);
            CombineCollider(tile);

            //playing the game
            if (!Main.EditorMode) {
                switch (t.name) {
                    case "Main Character":
                    case "playerr":
                        cameraController.player = tile;
                        break;
                    case "grid":
                        // It doesn't remove the tile from the array
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
