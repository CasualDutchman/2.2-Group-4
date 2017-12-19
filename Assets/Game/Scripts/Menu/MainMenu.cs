using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public EventSystem eventSystem;

    public GameObject optionsObjects, mainMenuOptions;

    GameObject lastHoverOver;

	void Start () {

	}
	
	void Update () {
        if (eventSystem.currentSelectedGameObject != null)
            lastHoverOver = eventSystem.currentSelectedGameObject;

        if (Mathf.Abs(Input.GetAxis("ControllerOne Vertical")) > 0 && eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(lastHoverOver);
    }

    public void OnTwoPlay() {
        PlayerPrefs.SetInt("PlayerCount", 2);
        StartCoroutine(Play());
    }

    public void OnPlay() {
        PlayerPrefs.SetInt("PlayerCount", 1);
        StartCoroutine(Play());
    }

    IEnumerator Play() {
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
        //async.allowSceneActivation = false;

        while (!async.isDone) {
            //if (async.progress >= 0.9) {
                //async.allowSceneActivation = true;
                yield return null;
            //}
        }
    }

    public void OnOptions() {
        if (!optionsObjects.activeSelf) {
            optionsObjects.SetActive(true);
            mainMenuOptions.SetActive(false);
        }
    }

    public void OnMainMenu() {
        if (!mainMenuOptions.activeSelf) {
            mainMenuOptions.SetActive(true);
            optionsObjects.SetActive(false);
        }
    }

    public void OnQuit() {
        Application.Quit();
    }
}
