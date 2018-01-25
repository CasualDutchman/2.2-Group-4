using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class RoomGrid : MonoBehaviour {

    //Max size of the room
    public static int size = 10;

    //grid for the bottum floor
    public int[] grid = new int[size * size];

    //When the room has a second floor
    public bool secondFloor;
    public int[] gridsecond = new int[size * size];

    //All the entrance objects that can change in the generation.
    public GameObject[] entranceObjects = new GameObject[size * size * 2];

    public List<GameObject> Changables() {
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject go in entranceObjects) {
            if (go != null)
                temp.Add(go);
        }

        return temp;
    }
}
