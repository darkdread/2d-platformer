using System.Collections;
using System.Collections.Generic;
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

    //filter out things I do not want the players to touch
    private static string[] filterTiles = new string[] { "outerWall", "grid" };

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
        tiles = Resources.LoadAll<GameObject>("Tiles/");
        gridTile = Resources.Load<GameObject>("Tiles/grid");

        int tileCount = 1;
        foreach(var t in tiles) {
            //things i do not want players to place/edit
            if (FilterTile(t.name)) {
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
                position = new Vector3(28, yPos, 1);
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

    private void OnClickTile() {
        GameObject clickedTile = EventSystem.current.currentSelectedGameObject;

        SelectTile(clickedTile);
    }

    private void SelectTile(GameObject tile) {
        if (tile.GetComponent<Image>())
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<Image>().sprite;
        else
            currentTileBtn.GetComponent<Image>().sprite = tile.GetComponent<SpriteRenderer>().sprite;

        selectedTile = Resources.Load<GameObject>("Tiles/" + tile.name);
        //print(string.Format("{0}", clickedTile.name));
    }
	
	// Update is called once per frame
	void Update () {
        if (!Main.EditorMode) return;

        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);
        if (leftClick || rightClick && selectedTile != null) {

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
                        return;
                    }
                }
            }

            Ray ray = GameController.cameraController.worldCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null && hit.collider.GetComponent<EditableTile>()) {
                GameObject tile = hit.collider.gameObject;

                if (leftClick) {
                    tile.GetComponent<SpriteRenderer>().sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
                    tile.name = selectedTile.name;
                } else if (rightClick) {
                    tile.GetComponent<SpriteRenderer>().sprite = gridTile.GetComponent<SpriteRenderer>().sprite;
                    tile.name = gridTile.name;
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
        SaveLoad.SaveMap("test");
        GameController.ClearMap();

        HideMenu();
        MainController.GetComponent<Main>().ShowMenu();
    }

}
