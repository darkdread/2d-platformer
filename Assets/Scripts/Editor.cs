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
    public Image background;

    //tiles in editor, not game object
    private GameObject[] tiles;
    private GameObject selectedTile;
    public Button tilePrefabBtn;
    public Transform tileHolder;

    //filter out things I do not want the players to touch
    private static string[] filterTiles = new string[] { "outerWall" };

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
            button.onClick.AddListener(SelectTile);
            tileCount += 1;
        }

        //default selected tile
        selectedTile = Resources.Load<GameObject>("Tiles/grid");
        backBtn.onClick.AddListener(BackToMenu);

        //auto hide menu on startup
        HideMenu();
	}

    void SelectTile() {
        GameObject clickedTile = EventSystem.current.currentSelectedGameObject;

        currentTileBtn.GetComponent<Image>().sprite = clickedTile.GetComponent<Image>().sprite;
        selectedTile = Resources.Load<GameObject>("Tiles/" + clickedTile.name);
        //print(string.Format("{0}", clickedTile.name));
    }
	
	// Update is called once per frame
	void Update () {
        if (!Main.EditorMode) return;

        
        if (Input.GetMouseButton(0) && selectedTile != null) {

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
                tile.GetComponent<SpriteRenderer>().sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
                tile.name = selectedTile.name;
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
        SaveLoad.SaveMap();
        GameController.ClearMap();

        HideMenu();
        MainController.GetComponent<Main>().ShowMenu();
    }

}
