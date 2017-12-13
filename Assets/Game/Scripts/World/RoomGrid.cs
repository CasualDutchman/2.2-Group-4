using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGrid : MonoBehaviour {

    public static int size = 10;

    //public bool[] grid = new bool[size * size];

    public int[] grid = new int[size * size];

    public bool secondFloor;
    public int[] gridsecond = new int[size * size];

    [SerializeField]
    public GameObject[] entranceObjects = new GameObject[size * size * 2];

    public List<GameObject> Changables() {
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject go in entranceObjects) {
            if (go != null)
                temp.Add(go);
        }

        return temp;
    }

    void Start () {
		
	}
	
	void Update () {
		
	}
}
