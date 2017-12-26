﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public CameraController cameraController;
    public Game GameController;
    public Editor EditorController;

    public CanvasGroup mainMenu;
    public Button playBtn;
    public Button editBtn;
    public Button settingsBtn;
    public Button exitBtn;

    private static bool editorMode;

    public static bool EditorMode {
        get {
            return editorMode;
        }
    }

    // Use this for initialization
    void Start () {
        playBtn.onClick.AddListener(StartGame);
        editBtn.onClick.AddListener(EditMode);
        settingsBtn.onClick.AddListener(GoToSettings);
        exitBtn.onClick.AddListener(ExitToDesktop);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //hide main menu
    public void HideMenu() {
        mainMenu.gameObject.SetActive(false);
    }

    //show main menu
    public void ShowMenu() {
        editorMode = false;

        mainMenu.gameObject.SetActive(true);
    }

    private void EditMode() {
        editorMode = true;
        string fileName = "test";

        if (fileName == null) {
            GameController.GetComponent<Game>().GenerateMap();
        } else {
            EditorController.GetComponent<Editor>().LoadMap(fileName);
        }

        HideMenu();

        cameraController.targetOrtho = 16.875f;
        EditorController.GetComponent<Editor>().ShowMenu();
    }

    //start the game
    private void StartGame() {
        string json = SaveLoad.LoadMap("level01");
        List<FileInfo> mapList = new List<FileInfo>();

        // Look through the folder and search for maps with extension OMEGALUL.
        DirectoryInfo info = new DirectoryInfo(SaveLoad.folderName);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo) {
            if (file.Extension == ".OMEGALUL") {
                print(file.Name);
                mapList.Add(file);
            }
        }


        if (json == null) {
            return;
        }

        HideMenu();
        
        cameraController.targetOrtho = 5f;
        GameController.GetComponent<Game>().GenerateMapFromJson(json);
    }

    void GoToSettings() {

    }

    //exit game
    void ExitToDesktop() {
        Application.Quit();
    }
}
