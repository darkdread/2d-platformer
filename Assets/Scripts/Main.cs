using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public CameraController cameraController;
    public Game GameController;
    public Scroller scroller;
    public Editor EditorController;

    public GameObject[] backgrounds;
    public CanvasGroup mainMenu;
    public Canvas levelSelection;
    public Button chapter1;
    public Button chapter2;
    public Button chapter3;

    public Button playBtn;
    public Button editBtn;
    public Button settingsBtn;
    public Button exitBtn;

    private int playerProgress;

    private static bool editorMode;

    public static bool EditorMode {
        get {
            return editorMode;
        }
    }

    // Use this for initialization
    private void Awake () {
        playBtn.onClick.AddListener(ShowLevelSelectionScreen);
        editBtn.onClick.AddListener(EditMode);
        settingsBtn.onClick.AddListener(GoToSettings);
        exitBtn.onClick.AddListener(ExitToDesktop);

        chapter1.onClick.AddListener(delegate () {
            StartGame("level01");
        });
        chapter2.onClick.AddListener(delegate () {
            StartGame("level02");
        });
        chapter3.onClick.AddListener(delegate () {
            StartGame("level03");
        });
        SaveProgress();
        LoadProgress();

        ShowMenu();
    }

    public void SaveProgress() {
        SaveLoad.SaveProgress("test");
    }

    public void LoadProgress() {
        string json = SaveLoad.LoadProgress("test");
        Progress progress = JsonUtility.FromJson<Progress>(json);

        playerProgress = progress.level;
        print(playerProgress);
    }

    public void HideBackground() {
        foreach (GameObject background in backgrounds) {
            background.SetActive(false);
        }
    }

    public void ShowBackground() {
        foreach (GameObject background in backgrounds) {
            background.SetActive(true);
        }
    }

    //hide main menu
    public void HideMenu() {
        mainMenu.gameObject.SetActive(false);
    }

    //show main menu
    public void ShowMenu() {
        editorMode = false;

        mainMenu.gameObject.SetActive(true);
        HideBackground();
    }

    private void EditMode() {
        editorMode = true;
        string fileName = "level01";

        if (fileName == null) {
            GameController.GetComponent<Game>().GenerateMap();
        } else {
            EditorController.GetComponent<Editor>().LoadMap(fileName);
        }

        HideMenu();

        cameraController.targetOrtho = 16.875f;
        EditorController.GetComponent<Editor>().ShowMenu();
    }
    
    private void ShowLevelSelectionScreen() {
        HideMenu();
        levelSelection.gameObject.SetActive(true);

        List<FileInfo> mapList = new List<FileInfo>();

        // Look through the folder and search for maps with extension OMEGALUL.
        DirectoryInfo info = new DirectoryInfo(SaveLoad.folderName);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo) {
            if (file.Extension == ".OMEGALUL") {
                if (!mapList.Contains(file))
                    mapList.Add(file);
            }
        }
    }

    private void HideLevelSelectionScreen() {
        levelSelection.gameObject.SetActive(false);
    }

    private void StartGame(string fileName) {
        string json = SaveLoad.LoadMap(fileName);

        if (json == null) {
            return;
        }

        HideMenu();
        HideLevelSelectionScreen();
        ShowBackground();

        cameraController.targetOrtho = 5f;
        GameController.GetComponent<Game>().GenerateMapFromJson(json);
        Scroller.SetPlayerTransform(FindObjectOfType<Player>().transform);
        //GameController.PauseGame();
    }

    void GoToSettings() {

    }

    //exit game
    void ExitToDesktop() {
        Application.Quit();
    }
}
