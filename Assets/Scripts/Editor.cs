using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Editor : MonoBehaviour {

    public Main MainController;
    public Game GameController;

    public CanvasGroup editorMenu;
    public Button currentTileBtn;
    public Button backBtn;
    public Button saveMenuBtn, loadMenuBtn;
    public Button saveBtn, loadBtn;
    public Button showTerrainBtn, showObjectBtn, showTrapBtn;
    public Button tileMenuBtn;
    public InputField saveInputField, loadInputField;
    public CanvasGroup saveMenu, loadMenu;
    public Text textPosition;

    //tiles in editor, not game object
    private GameObject[] tiles;
    private GameObject selectedTile;
    public GameObject tilePrefabBtn;
    public Transform tileContentHolder;
    public GameObject tileMenu;
    private Dictionary<string, Transform> tileHolders = new Dictionary<string, Transform>();

    public static Material LineMaterial;

    public static bool allowCameraMovement = true;
    private static string folderName = "Tiles";
    private static GameObject gridTile;
    private static GameObject lastHoveredTile;
    private static float selectedTileRotation = 0f;
    public static bool EnableGrid = true;
    public static GameObject currentTileType;

    //filter out things I do not want the players to touch/edit/place/whatever
    public static Dictionary<string, Tile> EditableTiles = new Dictionary<string, Tile>();
    public static string DefaultTile = "Default";
    private static string[] filterTiles = new string[] { "OuterWall" };

    public static bool FilterTile(string tile) {
        foreach (var t in filterTiles) {
            //name match
            if (tile == t) {
                return true;
            }
        }
        return false;
    }

	// Use this for initialization
	private void Start () {
        tiles = Resources.LoadAll<GameObject>(string.Format("{0}", folderName));
        gridTile = Game.TileDictionary["Grid"];
        Tile[] allTileTypes = Resources.LoadAll<Tile>(string.Format("{0}/TileType", folderName));

        foreach (Tile tile in allTileTypes) {
            string tileType = tile.name;
            EditableTiles.Add(tileType, tile);

            // Create an empty game object to hold the tile types. Hide the game object.
            GameObject tileHolder = new GameObject(tileType);
            tileHolder.transform.SetParent(tileContentHolder);
            tileHolder.transform.localScale = Vector3.one;
            tileHolder.transform.localPosition = Vector3.zero;
            tileHolder.SetActive(false);

            // Add the tile type to a list 
            tileHolders.Add(tileType, tileHolder.transform);
        }

        int[] tileCount = new int[allTileTypes.Length];

        foreach(var t in tiles) {
            // Things I do not want players to place/edit. Grid is an exceptional case as I want them to be editable BUT not show in the menu.
            if (FilterTile(t.name) || t.name == "Grid") {
                continue;
            }

            // Creating the button
            GameObject button = Instantiate<GameObject>(tilePrefabBtn);
            Tile tile = button.AddComponent<TileType>().GetCopyOf(t.GetComponent<TileType>()).tile;
            button.transform.SetParent(tileHolders[tile.name]);

            float xPos = 0, yPos = -100;

            // Compare the tile types between the one stored in allTiles and the current tile.
            // If it is the same, perform the following calculation.
            for (int i = 0; i < allTileTypes.Length; i++) {
                if (allTileTypes[i] == tile) {
                    xPos = 200 + (tileCount[i] * (100 + 30));
                    tileCount[i] += 1;
                    break;
                }
            }

            button.name = t.name;
            button.transform.localScale = Vector3.one;
            button.GetComponent<Image>().sprite = t.GetComponent<SpriteRenderer>().sprite;
            button.GetComponent<Image>().color = t.GetComponent<SpriteRenderer>().color;
            button.GetComponent<RectTransform>().localPosition = new Vector3(xPos, yPos, 0);
            button.GetComponent<Button>().onClick.AddListener(OnClickTile);
        }

        // Set the container height to contain all the tiles
        // 100 is the button height, 60 is for the border between each row
        //tileHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(250, ((tileCount/2) * (100 + 60)));

        // Default selected tile.
        SelectTile(tiles[0]);

        showTerrainBtn.onClick.AddListener(delegate {
            ShowTileType("Terrain");
        });

        showObjectBtn.onClick.AddListener(delegate {
            ShowTileType("Object");
        });

        showTrapBtn.onClick.AddListener(delegate {
            ShowTileType("Trap");
        });

        saveMenuBtn.onClick.AddListener(ShowSaveMenu);
        loadMenuBtn.onClick.AddListener(ShowLoadMenu);

        saveBtn.onClick.AddListener(delegate {
            SaveMap(saveInputField.text);
            HideSaveMenu();
        });
        loadBtn.onClick.AddListener(delegate {
            LoadMap(loadInputField.text);
            HideLoadMenu();
        });

        tileMenuBtn.onClick.AddListener(delegate {
            if (tileMenu.activeSelf) {
                HideTileMenu();
            } else {
                ShowTileMenu();
            }
        });
        backBtn.onClick.AddListener(BackToMenu);


        // Auto hide menu on startup.
        //HideMenu();
	}

    public void HideTileType() {
        if (currentTileType != null)
            currentTileType.SetActive(false);
    }

    public void ShowTileType(string type) {
        if (tileHolders.ContainsKey(type)) {
            HideTileType();
            currentTileType = tileHolders[type].gameObject;
            currentTileType.SetActive(true);
        }
    }

    public void HideTileMenu() {
        tileMenu.SetActive(false);
    }
    public void ShowTileMenu() {
        tileMenu.SetActive(true);
    }

    public void SaveMap(string fileName = "") {
        SaveLoad.SaveMap(fileName);
    }

    public bool LoadMap(string fileName = "") {
        string json = SaveLoad.LoadMap(fileName);

        // Generate map
        if (json != null) {
            GameController.ClearMap();
            GameController.GenerateMapFromJson(json);

            return true;
        }

        return false;
    }

    private void ShowSaveMenu() {
        if (saveMenu.alpha == 0f) {
            saveMenu.alpha = 1f;
            saveMenu.blocksRaycasts = true;
            loadMenu.alpha = 0f;
            loadMenu.blocksRaycasts = false;
            allowCameraMovement = false;
        }
    }

    private void HideSaveMenu() {
        if (saveMenu.alpha == 1f) {
            saveMenu.alpha = 0f;
            saveMenu.blocksRaycasts = false;
            allowCameraMovement = true;
        }
    }

    private void ShowLoadMenu() {
        if (loadMenu.alpha == 0f) {
            saveMenu.alpha = 0f;
            saveMenu.blocksRaycasts = false;
            loadMenu.alpha = 1f;
            loadMenu.blocksRaycasts = true;
            allowCameraMovement = false;
        }
    }

    private void HideLoadMenu() {
         if (loadMenu.alpha == 1f) {
            loadMenu.alpha = 0f;
            loadMenu.blocksRaycasts = false;
            allowCameraMovement = true;
        }
    }

    // Clicking on Menu Tiles
    private void OnClickTile() {
        GameObject clickedTile = EventSystem.current.currentSelectedGameObject;

        SelectTile(clickedTile);
    }

    // Change selected tile
    private void SelectTile(GameObject tile) {
        if (tile.GetComponent<Image>()) {
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<Image>().sprite;
            currentTileBtn.GetComponent<Image>().color = tile.GetComponent<Image>().color;
        } else {
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<SpriteRenderer>().sprite;
            currentTileBtn.GetComponent<Image>().color = tile.GetComponent<SpriteRenderer>().color;
        }

        selectedTile = Game.TileDictionary[tile.name];
    }

    private bool IsPointerOnUI() {
        //check if mouse is on UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0) {
            foreach (var result in results) {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI")) {
                    //print("is ui! stop! " + result.gameObject.name);
                    return true;
                }
            }
        }
        return false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!Main.EditorMode) return;

        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);
        if (lastHoveredTile != null) {

            // If mouse is hovering over a grid, we change the tile's sprite back to grid's sprite.
            if (lastHoveredTile.name == gridTile.name) {
                lastHoveredTile.GetComponent<SpriteRenderer>().sprite = gridTile.GetComponent<SpriteRenderer>().sprite;
                //lastHoveredTile.transform.rotation = Quaternion.Euler(lastHoveredTile.transform.eulerAngles + new Vector3(0, 0, selectedTileRotation));
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                // If mouse is hovering over a grid, we change the selected tile's rotation.
                if (lastHoveredTile.name == gridTile.name) {
                    selectedTileRotation = selectedTileRotation + 90f >= 360 ? 0f : selectedTileRotation + 90f;
                } else {
                    // If mouse is hovering over a tile, we change the tile's rotation.
                    lastHoveredTile.transform.rotation = Quaternion.Euler(lastHoveredTile.transform.eulerAngles + new Vector3(0, 0, 90f));
                }
            }
        }

        if (selectedTile != null) {

            if (IsPointerOnUI())
                return;
            
            Ray ray = GameController.cameraController.worldCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null && hit.collider.GetComponent<EditableTile>()) {
                GameObject tile = hit.collider.gameObject;

                // The x and y axis of the clicked tile, in world units.
                int tileX = (int)tile.transform.position.x;
                int tileY = (int)tile.transform.position.y;

                Tile newTile = EditableTiles[DefaultTile];
                Tile type = selectedTile.GetComponent<TileType>().tile;
                if (EditableTiles.ContainsKey(type.name)) {
                    newTile = EditableTiles[type.name];
                }
                Vector2 newTileSize = newTile.size;
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                lastHoveredTile = tile;
                textPosition.text = string.Format("X: <b>{0}</b>, Y: <b>{1}</b>", tileX, tileY);

                if (leftClick) {
                    tile.GetComponent<EditableTile>().tile = newTile;
                    spriteRenderer.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
                    spriteRenderer.color = selectedTile.GetComponent<SpriteRenderer>().color;
                    tile.GetComponent<EditableTile>().defaultColor = spriteRenderer.color;
                    tile.name = selectedTile.name;
                    tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selectedTileRotation));

                    // Loop through the grids to see if there's any space to place the new tile.
                    for (int i = 0; i < newTileSize.x; i++) {
                        for (int j = 0; j < newTileSize.y; j++) {
                            if (true) {

                            }
                        }
                    }
                } else if (rightClick) {
                    tile.GetComponent<EditableTile>().tile = EditableTiles[gridTile.name];
                    spriteRenderer.sprite = gridTile.GetComponent<SpriteRenderer>().sprite;
                    tile.name = gridTile.name;
                    tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                } else if (tile.name == gridTile.name) {
                    // If mouse is hovering over a grid, we change the tile's sprite to the selected tile's sprite.
                    spriteRenderer.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
                    // We also rotate the grid's rotation to the current selection rotation.
                    tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selectedTileRotation));
                }
            }
        }
        
    }

    public void ShowMenu() {
        editorMenu.gameObject.SetActive(true);
    }

    public void HideMenu() {
        editorMenu.gameObject.SetActive(false);
    }

    public void BackToMenu() {
        SaveLoad.SaveMap("backupMap");
        GameController.ClearMap();

        HideMenu();
        Main.ShowMenu();
    }

}
