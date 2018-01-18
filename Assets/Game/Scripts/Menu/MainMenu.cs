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

    //normal
    public void Demo1() {
        PlayerPrefs.SetInt("EnemySpawned", 50);
        PlayerPrefs.SetFloat("Multi", 1f);
        PlayerPrefs.SetFloat("EnemyMulti", 0.9f);
        PlayerPrefs.SetFloat("Speedy", 3f);
        PlayerPrefs.SetFloat("HPMulti", 1f);

        //OnPlay();
    }

    //a lot, faster killing, fast, faster!
    public void Demo2() {
        PlayerPrefs.SetInt("EnemySpawned", 100);
        PlayerPrefs.SetFloat("Multi", 1.6f);
        PlayerPrefs.SetFloat("EnemyMulti", 2f);
        PlayerPrefs.SetFloat("Speedy", 4f);
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

    public void OnPlay() {
        PlayerPrefs.SetInt("PlayerCount", 1);
        Demo1();
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
