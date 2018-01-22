using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public static Main current;

    public CameraController cameraController;
    public Game GameController;
    public Scroller scroller;
    public Editor EditorController;

    public GameObject[] backgrounds;
    public Transform cooldownHolder;
    public CanvasGroup mainMenu, settingsMenu;
    public Canvas levelSelection;
    public Button chapter0, chapter1, chapter2, chapter3;

    public Button playBtn;
    public Button editBtn;
    public Button settingsBtn, settingsBackBtn;
    public Button exitBtn;

    public Button vSyncLeftBtn, vSyncRightBtn;
    public Text vSyncValue;

    public Slider frameSlider;
    public Text frameText;

    public static int mapLevel;
    public static GameData playerProgress;
    public CanvasGroup[] chapterGroup;

    private static bool editorMode;
    public static Settings settings;

    public static bool EditorMode {
        get {
            return editorMode;
        }
    }

    public static void SetVSync(int value) {
        value = (value < 0) ? 0 : (value > 1) ? 1 : value;
        // No sync
        if (value == 0) {
            QualitySettings.vSyncCount = 0;
            current.vSyncValue.text = "OFF";
        } else if (value >= 1) {
            QualitySettings.vSyncCount = value;
            current.vSyncValue.text = "ON";
        }

        settings.vSync = value;
    }

    public static void SetFramerate(int value) {
        value = (value < 10) ? 10 : (value > 180) ? 180 : value;

        current.frameText.text = value.ToString();
        Application.targetFrameRate = value;
        settings.frameRate = value;
    }

    // Use this for initialization
    private void Awake() {
        current = this;

        playBtn.onClick.AddListener(ShowLevelSelectionScreen);
        editBtn.onClick.AddListener(EditMode);
        settingsBtn.onClick.AddListener(ShowSettings);
        settingsBackBtn.onClick.AddListener(HideSettings);
        exitBtn.onClick.AddListener(ExitToDesktop);

        frameSlider.onValueChanged.AddListener(delegate {
            SetFramerate((int)frameSlider.value);
        });

        vSyncLeftBtn.onClick.AddListener(delegate{
            SetVSync(settings.vSync - 1);
        });

        vSyncRightBtn.onClick.AddListener(delegate {
            SetVSync(settings.vSync + 1);
        });

        chapter0.onClick.AddListener(delegate () {
            StartGame("level00");
        });
        chapter1.onClick.AddListener(delegate () {
            if (playerProgress.level >= 1)
                StartGame("level01");
        });

        /*
        chapter2.onClick.AddListener(delegate () {
            if (playerProgress >= 2)
                StartGame("level02");
        });
        chapter3.onClick.AddListener(delegate () {

            StartGame("level03");
        });
        */

        //SaveProgress();
        if (!LoadProgress()) {
            settings = new Settings();
            playerProgress = new GameData(0);
        }

        UpdateLevelScreen();
        
        ShowMenu();
        HideSettings();
    }

    public static void SaveProgress() {
        SaveLoad.SaveProgress("test");
    }

    public static bool LoadProgress() {
        string json = SaveLoad.LoadProgress("test");
        if (json == null) return false;

        GameData progress = JsonUtility.FromJson<GameData>(json);
        
        playerProgress = progress;
        settings = progress.settings;
        SetFramerate(progress.settings.frameRate);
        SetVSync(progress.settings.vSync);

        return true;
    }

    public static void HideBackground() {
        foreach (GameObject background in current.backgrounds) {
            background.SetActive(false);
        }
    }

    public static void ShowBackground() {
        foreach (GameObject background in current.backgrounds) {
            background.SetActive(true);
        }
    }

    //hide main menu
    public static void HideMenu() {
        current.mainMenu.gameObject.SetActive(false);
    }

    public static void UpdateLevelScreen() {

        for (int i = 0; i < current.chapterGroup.Length; i++) {
            if (playerProgress.level >= i) {
                current.chapterGroup[i].alpha = 1;
            } else {
                current.chapterGroup[i].alpha = 0.3f;
            }
        }
    }

    //show main menu
    public static void ShowMenu() {
        editorMode = false;

        UpdateLevelScreen();
        current.cooldownHolder.gameObject.SetActive(false);
        current.mainMenu.gameObject.SetActive(true);
        HideBackground();
    }

    private void EditMode() {
        editorMode = true;
        string fileName = "level01";

        if (!EditorController.GetComponent<Editor>().LoadMap(fileName)) {
            GameController.GetComponent<Game>().GenerateMap();
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
        cooldownHolder.gameObject.SetActive(true);

        Vector2 tilePos, boxSize;

        switch (fileName) {
            default:
            case "level00":
                mapLevel = 0;
                break;
            case "level01":
                mapLevel = 1;
                tilePos = new Vector2(5, 1);
                boxSize = new Vector2(5, 5);
                DialogueSpeaker speaker = CreateSpeakerAtPos(tilePos, boxSize, "Narrator", 1);
                speaker.spoken = true;

                tilePos = new Vector2(15, 1);
                boxSize = new Vector2(5, 5);
                DialogueSpeaker speaker2 = CreateSpeakerAtPos(tilePos, boxSize, "Narrator", 2);
                //speaker2.spoken = true;
                break;
        }
    }

    private DialogueSpeaker CreateSpeakerAtPos(Vector2 tilePos, Vector2 boxSize, string name, int dialogueNumber) {
        GameObject tile = new GameObject("Speaker GameObject");
        tile.transform.SetParent(Game.gameHolder);
        tile.transform.position = tilePos;
        tile.tag = "DialogueSpeaker";

        DialogueSpeaker speaker = tile.AddComponent<DialogueSpeaker>();
        speaker.speakerName = name;
        speaker.dialogueNumber = dialogueNumber;

        BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
        collider.size = boxSize;
        collider.isTrigger = true;

        return speaker;
    }

    public static void ShowSettings() {
        HideMenu();

        current.settingsMenu.gameObject.SetActive(true);
    }

    public static void HideSettings() {
        current.settingsMenu.gameObject.SetActive(false);
        SaveProgress();

        if (Game.started) {

        } else {
            ShowMenu();
        }
    }

    //exit game
    void ExitToDesktop() {
        Application.Quit();
    }

    [Serializable]
    public class Settings {
        public int vSync = 0;
        public int frameRate = 60;
    }
}
