using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public EventSystem eventSystem;

    GameObject lastHoverOver;

	void Start () {

	}
	
	void Update () {
        if (eventSystem.currentSelectedGameObject != null)
            lastHoverOver = eventSystem.currentSelectedGameObject;

        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0 && eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(lastHoverOver);
    }

    public void OnPlay() {
        StartCoroutine(Play());
    }

    IEnumerator Play() {
<<<<<<< HEAD
        AsyncOperation async = SceneManager.LoadSceneAsync("Game");
=======
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
>>>>>>> master
        //async.allowSceneActivation = false;

        while (!async.isDone) {
            //if (async.progress >= 0.9) {
                //async.allowSceneActivation = true;
                yield return null;
            //}
        }
    }

    public void OnOptions() {

    }

    public void OnQuit() {
        Application.Quit();
    }
}
