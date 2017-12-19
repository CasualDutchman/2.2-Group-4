using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public GameObject pauseMenu, optionsMenu, reallyQuit;

	void Start () {
		
	}

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (pauseMenu.activeSelf) {
                Resume();
            } 
            else if (optionsMenu.activeSelf) {
                OnPauseMenu();
            } 
            else if (reallyQuit.activeSelf) {
                OnCancelQuit();
            } 
            else {
                Pause();
            }
        }
    }

    public void Pause() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Resume() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnOptions() {
        if (!optionsMenu.activeSelf) {
            optionsMenu.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    public void OnPauseMenu() {
        if (!pauseMenu.activeSelf) {
            pauseMenu.SetActive(true);
            optionsMenu.SetActive(false);
        }
    }

    public void OnQuit() {
        if (!reallyQuit.activeSelf) {
            reallyQuit.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    public void OnCancelQuit() {
        if (!pauseMenu.activeSelf) {
            pauseMenu.SetActive(true);
            reallyQuit.SetActive(false);
        }
    }

    public void OnReallyQuit() {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }

}
