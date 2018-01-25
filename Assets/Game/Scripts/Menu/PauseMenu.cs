using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Author: Pieter
public class PauseMenu : MonoBehaviour {

    //different screens
    public GameObject pauseMenu, optionsMenu, reallyQuit;

	void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) { //what happens when pressing escape
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

    //Pause the game, show the mouse and set the timescale to 0
    public void Pause() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    //Resume the game, hide the mouse and set the timescale to 1
    public void Resume() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    //show the options screen
    public void OnOptions() {
        if (!optionsMenu.activeSelf) {
            optionsMenu.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    //Show the pause screen
    public void OnPauseMenu() {
        if (!pauseMenu.activeSelf) {
            pauseMenu.SetActive(true);
            optionsMenu.SetActive(false);
        }
    }

    //When the Quit button is pressed
    public void OnQuit() {
        if (!reallyQuit.activeSelf) {
            reallyQuit.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    //When you dont want to quit after a confirmation
    public void OnCancelQuit() {
        if (!pauseMenu.activeSelf) {
            pauseMenu.SetActive(true);
            reallyQuit.SetActive(false);
        }
    }

    //When you want to go to the main menu after the confirmation
    public void OnReallyQuit() {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }

}
