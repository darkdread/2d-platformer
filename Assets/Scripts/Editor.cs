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
    public InputField saveInputField, loadInputField;
    public CanvasGroup saveMenu, loadMenu;

    //tiles in editor, not game object
    private GameObject[] tiles;
    private GameObject selectedTile;
    public Button tilePrefabBtn;
    public Transform tileHolder;

    public static bool allowCameraMovement = true;
    private static GameObject gridTile;
    private static string folderName = "Objects";
    private static GameObject lastHoveredTile;
    private static float selectedTileRotation = 0f;

    //filter out things I do not want the players to touch/edit/place/whatever
    public static Dictionary<string, Tile> EditableTiles = new Dictionary<string, Tile>();
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
	void Start () {
        tiles = Resources.LoadAll<GameObject>(string.Format("{0}/", folderName));
        gridTile = Resources.Load<GameObject>(string.Format("{0}/Grid", folderName));
        Tile[] allTiles = Resources.LoadAll<Tile>("Objects");
        foreach (Tile tile in allTiles) {
            string tileName = tile.name;
            EditableTiles.Add(tileName, tile);
        }

        int tileCount = 1;
        foreach(var t in tiles) {
            // Things I do not want players to place/edit. Grid is an exceptional case as I want them to be editable BUT not show in the menu.
            if (FilterTile(t.name) || t.name == "Grid") {
                continue;
            }
            Button button = Instantiate<Button>(tilePrefabBtn);
            button.transform.SetParent(tileHolder);
            button.GetComponent<Image>().sprite = t.GetComponent<SpriteRenderer>().sprite;
            button.transform.localScale = Vector3.one;
            button.name = t.name;

            Vector3 position;
            float yPos = 12;
            if (tileCount <= 2) {
                yPos = 12;
            } else if (tileCount <= 4) {
                yPos = 10;
            }

            yPos = 12 - (((tileCount-1) / 2) * 6);

            //even tile
            if (tileCount % 2 == 0) {
                position = new Vector3(27, yPos, 1);
            } else {
                position = new Vector3(23, yPos, 1);
            }

            button.transform.position = position;
            button.onClick.AddListener(OnClickTile);
            tileCount += 1;
        }

        // Default selected tile.
        SelectTile(tiles[0]);
        saveMenuBtn.onClick.AddListener(ShowSaveMenu);
        loadMenuBtn.onClick.AddListener(ShowLoadMenu);
        saveBtn.onClick.AddListener(SaveMap);
        loadBtn.onClick.AddListener(LoadMap);
        backBtn.onClick.AddListener(BackToMenu);


        // Auto hide menu on startup.
        HideMenu();
	}

    private void SaveMap() {
        string fileName = saveInputField.text;
        SaveLoad.SaveMap(fileName);

        allowCameraMovement = true;
        ShowSaveMenu();
    }

    private void LoadMap() {
        string fileName = loadInputField.text;
        string json = SaveLoad.LoadMap(fileName);

        // Generate map and close menu
        if (json != null) {
            GameController.ClearMap();
            GameController.GenerateMapFromJson(json);
            ShowLoadMenu();
        }

        allowCameraMovement = true;
    }

    private void ShowSaveMenu() {
        // Show save menu
        if (saveMenu.alpha == 0f) {
            saveMenu.alpha = 1f;
            saveMenu.blocksRaycasts = true;
            loadMenu.alpha = 0f;
            loadMenu.blocksRaycasts = false;
            allowCameraMovement = false;
        } else {
            saveMenu.alpha = 0f;
            saveMenu.blocksRaycasts = false;
            allowCameraMovement = true;
        }
    }

    private void ShowLoadMenu() {
        // Show menu
        if (loadMenu.alpha == 0f) {
            saveMenu.alpha = 0f;
            saveMenu.blocksRaycasts = false;
            loadMenu.alpha = 1f;
            loadMenu.blocksRaycasts = true;
            allowCameraMovement = false;
        } else {
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
        if (tile.GetComponent<Image>())
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<Image>().sprite;
        else
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<SpriteRenderer>().sprite;

        selectedTile = Resources.Load<GameObject>(string.Format("{0}/{1}", folderName, tile.name));
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
                Tile newTile = EditableTiles[selectedTile.name];
                Vector2 newTileSize = newTile.size;
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                lastHoveredTile = tile;

                if (leftClick) {
                    tile.GetComponent<EditableTile>().tile = EditableTiles[selectedTile.name];
                    spriteRenderer.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
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
        MainController.GetComponent<Main>().ShowMenu();
    }

}
