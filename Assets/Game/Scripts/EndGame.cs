using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : Item {

    public GameObject EndGui;

	public override string Message() {
        return "Exit Building";
    }

    public override void Interact(Player player) {
        Time.timeScale = 0;
        EndGui.transform.SetAsLastSibling();
        EndGui.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToGame() {
        //from demo 2
        PlayerPrefs.SetInt("EnemySpawned", 110);
        PlayerPrefs.SetFloat("Multi", 1f);
        PlayerPrefs.SetFloat("EnemyMulti", 0.9f);
        PlayerPrefs.SetFloat("Speedy", 3.5f);
        PlayerPrefs.SetFloat("HPMulti", 1.8f);

        PlayerPrefs.SetInt("PlayerCount", 1);
        StartCoroutine(Play(1));
    }

    public void GoToMenu() {
        StartCoroutine(Play(0));
    }

    IEnumerator Play(int i) {
        AsyncOperation async = SceneManager.LoadSceneAsync(i);

        while (!async.isDone) {
            yield return null;
        }
    }
}
