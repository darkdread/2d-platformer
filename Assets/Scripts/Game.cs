using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;

[System.Serializable]
public class Game : SerializedMonoBehaviour {

    public static Game current;
    public static bool paused;
    public static bool started;

    public Main main;

    public CanvasGroup pauseMenuGroup;
    public Button mainMenuBtn, resumeGameBtn, settingsBtn;

    public LevelController levelController;
    public MyDictionary myDictionary;
    public static Dictionary<string, GameObject> TileDictionary = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> ProjectileDictionary;

    //size is +2 because of outer walls [left/right, top/btm]
    public static int gridWidth = 128+2;
    public static int gridHeight = 64+2;
    public static Player player;
    public static Transform gameHolder;
    public CameraController cameraController;

    private bool combined;
    public static GameObject[,] tiles;

    private static string folderName = "Tiles";
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
    private void Awake () {
        current = this;
        //TileDictionary = myDictionary.TileDictionary;
        ProjectileDictionary = myDictionary.ProjectileDictionary;
        tiles = new GameObject[gridWidth, gridHeight];
        gameHolder = GameObject.Find("GameHolder").transform;

        GameObject[] allTiles = Resources.LoadAll<GameObject>(string.Format("{0}", folderName));

        foreach(GameObject tile in allTiles) {
            TileDictionary[tile.name] = tile;
        }
        //string.Format("{0}/{1}/{2}", folderName, tileType.name, tile.name);

        mainMenuBtn.onClick.AddListener(BackToMenu);
        resumeGameBtn.onClick.AddListener(current.HidePauseMenu);
        settingsBtn.onClick.AddListener(Main.ShowSettings);
    }
	
	// Update is called once per frame
	private void Update () {
		if (Input.GetKeyDown(KeyCode.Escape) && started && !LevelController.isDialogueOpen) {
            if (pauseMenuGroup.gameObject.activeSelf) {
                HidePauseMenu();
            } else {
                ShowPauseMenu();
            }
        }
	}

    public static void WinGame() {
        started = false;

        // Progress logic heres
        if (Main.mapLevel + 1 > Main.playerProgress.level) {
            Main.playerProgress.level += 1;
            Main.mapLevel = Main.playerProgress.level;
            Main.SaveProgress();
        }

        current.BackToMenu();
    }

    public static void PauseGame() {
        paused = true;

        foreach (GameObject enemy in Enemy.list) {
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            Animator anim = enemy.GetComponent<Animator>();

            if (rb) {
                PausableRigidbody2D.PauseRigidbody(rb);
            }

            if (anim) {
                anim.enabled = false;
            }
        }

        foreach (GameObject projectile in Projectile.list) {
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            
            if (rb) {
                PausableRigidbody2D.PauseRigidbody(rb);
            }
            
        }

        player.particleSys.Pause();
        PausableRigidbody2D.PauseRigidbody(player.GetComponent<Rigidbody2D>());
        player.GetComponent<Animator>().enabled = false;
    }

    public static void ResumeGame() {
        paused = false;

        foreach (GameObject enemy in Enemy.list) {
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            Animator anim = enemy.GetComponent<Animator>();

            if (rb) {
                PausableRigidbody2D.ResumeRigidbody(rb);
            }

            if (anim) {
                anim.enabled = true;
            }
        }

        foreach (GameObject projectile in Projectile.list) {
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb) {
                PausableRigidbody2D.ResumeRigidbody(rb);
            }
        }
        
        player.particleSys.Play();
        PausableRigidbody2D.ResumeRigidbody(player.GetComponent<Rigidbody2D>());
        player.GetComponent<Animator>().enabled = true;
    }

    private void ShowPauseMenu() {
        pauseMenuGroup.gameObject.SetActive(true);
        PauseGame();
    }

    private void HidePauseMenu() {
        pauseMenuGroup.gameObject.SetActive(false);
        ResumeGame();
    }

    private void BackToMenu() {
        ClearMap();
        HidePauseMenu();
        LevelController.HideDialogue();
        Main.ShowMenu();
    }

    public GameObject AddTile(string tileName, Vector3 position, float rotation = 0f) {
        GameObject tile = Instantiate(TileDictionary[tileName]);

        tile.name = tileName;
        tile.transform.position = position;
        tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        tile.transform.SetParent(gameHolder.transform);

        if (tile.GetComponent<Player>()) {
            player = tile.GetComponent<Player>();
        }
        
        // Disable the tile from moving & make tile editable.
        if (Main.EditorMode) {
            if (!Editor.FilterTile(tile.name)) {
                EditableTile editableTile = tile.AddComponent<EditableTile>();
                string tileType = tile.GetComponent<TileType>().tile.name;
                if (Editor.EditableTiles.ContainsKey(tileType)) {
                    editableTile.tile = Editor.EditableTiles[tileType];
                } else {
                    editableTile.tile = Editor.EditableTiles[Editor.DefaultTile];
                }
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
        /*if (tiles[0, 0] == null) return;
        foreach(var t in tiles) {
            Destroy(t.gameObject);
        }*/
        started = false;

        foreach(Transform child in gameHolder.transform) {
            if (child) {
                Destroy(child.gameObject);
            }
        }
        Array.Clear(tiles, 0, tiles.Length);
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
            BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();

            // Combine the box collider and remove its component
            foreach (var neighbor in neighbors) {
                if (neighbor.CompareTag("Ground")) {

                    // Remove neighbor tiles' colliders and add them to current tile
                    int x = (int)tile.transform.position.x;
                    int y = (int)tile.transform.position.y;
                    float xOffset = x - neighbor.transform.position.x;
                    float yOffset = y - neighbor.transform.position.y;

                    Vector2 colliderSize = new Vector2(collider.size.x + xOffset, collider.size.y + yOffset);
                    collider.size = colliderSize;

                    Vector2 colliderOffset = new Vector2(collider.offset.x - xOffset/2, collider.offset.y - yOffset/2);
                    collider.offset = colliderOffset;
                    
                    //print(string.Format("x: {0}, y: {1}, neighborX: {2}, neighborY: {3}", x, y, (int)neighbor.transform.position.x, (int)neighbor.transform.position.y));
                    
                    Destroy(neighbor.GetComponent<BoxCollider2D>());
                }
            }
            
            //tiles[x, y];
        }
    }

    // Generate blank map for editor.
    // Outer Wall objects start from x, y [0, 0] to [width-1, height-1]
    // So the contents actually start from [1, 1] to [width-2, height-2]

    public void GenerateMap() {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                GameObject tile;
                Vector3 pos = new Vector3(x, y, 0);
                
                // Creating the tile
                if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1) {
                    tile = AddTile("OuterWall", pos);
                } else {
                    tile = AddTile("Grid", pos);
                }

                tile.transform.SetParent(gameHolder.transform);
                tiles[x, y] = tile;
            }
        }

        cameraController.transform.position = new Vector3(30, 15, cameraController.transform.position.z);
    }

    public GameObject GetTile(int x, int y) {
        return tiles[x, y];
    }

    public void GenerateMapFromJson(string json) {
        MapData gameData = JsonUtility.FromJson<MapData>(json);
        GameObjectInScene[] tileArray = new GameObjectInScene[gameData.mapWidth * gameData.mapHeight];
        GameObjectInScene[] tileData = gameData.tiles;

        // What it does
        // ============================
        // First, it loads the map's width and height from the Json file.
        // Then, it will search for GameObjectInScenes stored in the Json file.
        // Create tiles using the position, rotation, scale and name from the GameObjectInScene.
        // After creating tiles FROM the stored data, we will then create Grids.
        // Grids are created by searching for a tile's position. If the tile is empty, create at its position.

        // Loops from y to mapHeight then x to mapWidth.
        Vector3 tilePos = new Vector3();
        int count = 0;

        foreach(GameObjectInScene t in tileData) { 
            GameObject tile = AddTile(t.name, t.position, t.rotation.eulerAngles.z);
            tileArray[count++] = t;

            //playing the game
            if (!Main.EditorMode) {
                switch (t.name) {
                    case "Main Character":
                        cameraController.player = tile;
                        break;
                }
            }
        }

        // Creating the Grids
        if (Main.EditorMode) {
            // Creating the Lines

            for (int i = 0; i < tileArray.Length; i++) {
                GameObject tile = GetTile((int)tilePos.x, (int)tilePos.y);

                if (tile == null) {
                    tile = AddTile("Grid", tilePos);
                }

                // Loops from y to mapHeight then x to mapWidth.
                if (++tilePos.y >= gameData.mapHeight) {
                    tilePos.y = 0;
                    tilePos.x += 1;
                }
            }
        }

        if (Main.EditorMode) {
            return;
        } else {
            // Game started
            started = true;
        }

        // How it works
        // --------------------------
        // Get tiles on the map (World Space)
        // Convert the colliders from PolyCollider2D to a List of List of Vector2s
        // The List contains the Path<List> and the Points<Vector2>
        // The Path contains the Points<Vector2>
        // Increase each Points by the offset from 0,0 in World Space
        

        // Generating the list of polygons
        List<List<Vector2>> polygons = new List<List<Vector2>>();
        int polygonPointsCount = 0;

        foreach (var tile in tiles) {
            // Tile is empty (Probably because of the map width/height)
            if (tile == null)
                continue;

            Vector3 worldSpace = tile.transform.position;
            PolygonCollider2D polygonCollider = tile.GetComponent<PolygonCollider2D>();
            TileType tileType = tile.GetComponent<TileType>();

            if (polygonCollider && tileType.tile.name == "Terrain") {

                // If the rotation of the tile is not 0, do not combine.
                // There is a problem with rotations and colliders. The position of the points do not change, but the bounds change.
                // With Clipper library, it is not possible to change the bounds. The only solution is to reposition each rotated tile, but the algorithm is unclear.

                if (tile.transform.rotation.eulerAngles.z != 0) {
                    continue;
                }

                Vector2[] polyPoints = polygonCollider.points;
                
                for(int i = 0; i < polyPoints.Length; i++) {
                    polyPoints[i] = new Vector2(polyPoints[i].x + worldSpace.x, polyPoints[i].y + worldSpace.y);
                }
                
                List<Vector2> vectorList = polyPoints.ToList<Vector2>();
                polygonPointsCount += vectorList.Count;

                polygons.Add(vectorList);
                Destroy(polygonCollider);
            }
        }

        print(string.Format("Polygon Count: {0}, Polygon Points Count: {1}", polygons.Count, polygonPointsCount));

        // Combining the list of polygons into huge chunks of polygons
        List<List<Vector2>> unitedPolygon = new List<List<Vector2>>();

        // Creating the game collider which holds all the united polygons
        GameObject gameCollider = new GameObject("Polygon Collider");
        gameCollider.layer = LayerMask.NameToLayer("Ground");
        gameCollider.transform.SetParent(gameHolder.transform);

        PolygonCollider2D collider = gameCollider.AddComponent<PolygonCollider2D>();

        // The method to unite all the polygons
        unitedPolygon = UniteCollisionPolygons(polygons);
        int unitedPolygonPointCount = 0;

        // Debug printing the point positions
        foreach (List<Vector2> polygonPath in unitedPolygon) {
            foreach (Vector2 polygonPoint in polygonPath) {
                //print(string.Format("x: {0}, y: {1}", polygonPoint.x, polygonPoint.y));
            }
            unitedPolygonPointCount += polygonPath.Count;
        }

        print(string.Format("UnitedPolygon Count: {0}, UnitedPolygon Points Count: {1}", unitedPolygon.Count, unitedPolygonPointCount));

        // Set the number of polygons on the new collider to the amount of polygons we have
        collider.pathCount = unitedPolygon.Count;

        // Adding all the points to the new polygons
        for (int i = 0; i < unitedPolygon.Count; i++) {
            Vector2[] points = unitedPolygon[i].ToArray<Vector2>();
            
            collider.SetPath(i, points);
        }
    }

    //this function takes a list of polygons as a parameter, this list of polygons represent all the polygons that constitute collision in your level.
    public List<List<Vector2>> UniteCollisionPolygons(List<List<Vector2>> polygons) {
        //this is going to be the result of the method
        List<List<Vector2>> unitedPolygons = new List<List<Vector2>>();
        Clipper clipper = new Clipper();

        //clipper only works with ints, so if we're working with floats, we need to multiply all our floats by
        //a scaling factor, and when we're done, divide by the same scaling factor again
        int scalingFactor = 10000;

        //this loop will convert our List<List<Vector2>> to what Clipper works with, which is "Path" and "IntPoint"
        //and then add all the Paths to the clipper object so we can process them
        for (int i = 0; i < polygons.Count; i++) {
            Path allPolygonsPath = new Path(polygons[i].Count);

            for (int j = 0; j < polygons[i].Count; j++) {
                allPolygonsPath.Add(new IntPoint(Mathf.Floor(polygons[i][j].x * scalingFactor), Mathf.Floor(polygons[i][j].y * scalingFactor)));
                //print(new IntPoint(Mathf.Floor(polygons[i][j].x * scalingFactor), Mathf.Floor(polygons[i][j].y * scalingFactor)).X + ", " + new IntPoint(Mathf.Floor(polygons[i][j].x * scalingFactor), Mathf.Floor(polygons[i][j].y * scalingFactor)).Y);
            }
            clipper.AddPath(allPolygonsPath, PolyType.ptSubject, true);
            
        }
        
        //this will be the result
        Paths solution = new Paths();

        //having added all the Paths added to the clipper object, we tell clipper to execute an union
        clipper.Execute(ClipType.ctUnion, solution);

        
        //the union may not end perfectly, so we're gonna do an offset in our polygons, that is, expand them outside a little bit
        ClipperOffset offset = new ClipperOffset();
        offset.AddPaths(solution, JoinType.jtMiter, EndType.etClosedPolygon);
        //5 is the amount of offset
        offset.Execute(ref solution, 0.1f);
        

        //now we just need to convert it into a List<List<Vector2>> while removing the scaling
        foreach (Path path in solution) {
            List<Vector2> unitedPolygon = new List<Vector2>();
            foreach (IntPoint point in path) {
                unitedPolygon.Add(new Vector2(point.X / (float)scalingFactor, point.Y / (float)scalingFactor));
            }
            unitedPolygons.Add(unitedPolygon);
        }


        //this removes some redundant vertices in the polygons when they are too close from each other
        //may be useful to clean things up a little if your initial collisions don't match perfectly from tile to tile
        unitedPolygons = RemoveClosePointsInPolygons(unitedPolygons);

        //everything done
        return unitedPolygons;
    }

    //create the collider in unity from the list of polygons
    public void CreateLevelCollider(List<List<Vector2>> polygons) {
        GameObject colliderObj = new GameObject("LevelCollision");
        //colliderObj.layer = GR.inst.GetLayerID(Layer.PLATFORM);
        //colliderObj.transform.SetParent(level.levelObj.transform);

        PolygonCollider2D collider = colliderObj.AddComponent<PolygonCollider2D>();

        collider.pathCount = polygons.Count;

        for (int i = 0; i < polygons.Count; i++) {
            Vector2[] points = polygons[i].ToArray();

            collider.SetPath(i, points);
        }
    }

    public List<List<Vector2>> RemoveClosePointsInPolygons(List<List<Vector2>> polygons) {
        float proximityLimit = 0.1f;

        List<List<Vector2>> resultPolygons = new List<List<Vector2>>();

        foreach (List<Vector2> polygon in polygons) {
            List<Vector2> pointsToTest = polygon;
            List<Vector2> pointsToRemove = new List<Vector2>();

            foreach (Vector2 pointToTest in pointsToTest) {
                foreach (Vector2 point in polygon) {
                    if (point == pointToTest || pointsToRemove.Contains(point)) continue;

                    bool closeInX = Mathf.Abs(point.x - pointToTest.x) < proximityLimit;
                    bool closeInY = Mathf.Abs(point.y - pointToTest.y) < proximityLimit;

                    if (closeInX && closeInY) {
                        pointsToRemove.Add(pointToTest);
                        break;
                    }
                }
            }
            polygon.RemoveAll(x => pointsToRemove.Contains(x));

            if (polygon.Count > 0) {
                resultPolygons.Add(polygon);
            }
        }

        return resultPolygons;
    }


}
