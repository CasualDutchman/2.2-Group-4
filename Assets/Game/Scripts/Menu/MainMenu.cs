using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

//Author: Pieter
public class MainMenu : MonoBehaviour {

    //eventSystem is used for controller input, but is not used in the end. Still kept it in, in case we wanted splitscreen
    public EventSystem eventSystem;
    GameObject lastHoverOver;

    public GameObject optionsObjects, mainMenuOptions;

    void Update () {
        if (eventSystem.currentSelectedGameObject != null)
            lastHoverOver = eventSystem.currentSelectedGameObject;

        if (Mathf.Abs(Input.GetAxis("ControllerOne Vertical")) > 0 && eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(lastHoverOver);
    }

    //When you want to play with 2 players
    public void OnTwoPlay() {
        PlayerPrefs.SetInt("PlayerCount", 2);
        StartCoroutine(Play(1));
    }

    //When you want to play the tutorial
    public void OnTutorial() {
        StartCoroutine(Play(2));
    }

    //Demo functions were used to test different styles of enemies during testing
    //normal
    public void Demo1() {
        PlayerPrefs.SetInt("EnemySpawned", 65);
        PlayerPrefs.SetFloat("Multi", 1f);
        PlayerPrefs.SetFloat("EnemyMulti", 0.9f);
        PlayerPrefs.SetFloat("Speedy", 3f);
        PlayerPrefs.SetFloat("HPMulti", 1f);

        OnPlay();
    }

    //a lot, faster killing, fast, faster!
    public void Demo2() {
        PlayerPrefs.SetInt("EnemySpawned", 110);
        PlayerPrefs.SetFloat("Multi", 1f);
        PlayerPrefs.SetFloat("EnemyMulti", 0.9f);
        PlayerPrefs.SetFloat("Speedy", 3.5f);
        PlayerPrefs.SetFloat("HPMulti", 1.8f);

        OnPlay();
    }

    //not a lot, hard to kill, slow
    public void Demo3() {
        PlayerPrefs.SetInt("EnemySpawned", 50);
        PlayerPrefs.SetFloat("Multi", 0.7f);
        PlayerPrefs.SetFloat("EnemyMulti", 1f);
        PlayerPrefs.SetFloat("Speedy", 2f);
        PlayerPrefs.SetFloat("HPMulti", 2.2f);

        OnPlay();
    }

    //When you want to play on your own
    public void OnPlay() {
        PlayerPrefs.SetInt("PlayerCount", 1);
        StartCoroutine(Play(1));
    }

    //Start the loading of the scene
    IEnumerator Play(int i) {
        AsyncOperation async = SceneManager.LoadSceneAsync(i);

        while (!async.isDone) {
            yield return null;
        }
    }

    //When you want to show the option screen
    public void OnOptions() {
        if (!optionsObjects.activeSelf) {
            optionsObjects.SetActive(true);
            mainMenuOptions.SetActive(false);
        }
    }

    //When you want to go back to the main screen from the options
    public void OnMainMenu() {
        if (!mainMenuOptions.activeSelf) {
            mainMenuOptions.SetActive(true);
            optionsObjects.SetActive(false);
        }
    }

    //When you want to quit the game
    public void OnQuit() {
        Application.Quit();
    }
}
