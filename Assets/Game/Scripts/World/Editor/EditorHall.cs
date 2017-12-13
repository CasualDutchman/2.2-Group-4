using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hall))]
public class EditorHall : Editor {

    Hall room;

    void OnEnable() {
        room = (Hall)target;
    }

    public override void OnInspectorGUI() {
        GUILayout.BeginHorizontal();
            GUILayout.Space(26);
            if (GUILayout.Button(room.wallsides[0] == 1 ? "W" : (room.wallsides[0] == 2 ? "E" : ""), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25))) {
                room.wallsides[0]++;
                if (room.wallsides[0] > 2)
                    room.wallsides[0] = 0;
            }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
            if (GUILayout.Button(room.wallsides[1] == 1 ? "W" : (room.wallsides[1] == 2 ? "E" : ""), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25))) {
                room.wallsides[1]++;
                if (room.wallsides[1] > 2)
                    room.wallsides[1] = 0;
            }
            GUILayout.Space(25);
            if (GUILayout.Button(room.wallsides[2] == 1 ? "W" : (room.wallsides[2] == 2 ? "E" : ""), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25))) {
                room.wallsides[2]++;
                if (room.wallsides[2] > 2)
                    room.wallsides[2] = 0;
            }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
            GUILayout.Space(26);
            if (GUILayout.Button(room.wallsides[3] == 1 ? "W" : (room.wallsides[3] == 2 ? "E" : ""), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25))) {
                room.wallsides[3]++;
                if (room.wallsides[3] > 2)
                    room.wallsides[3] = 0;
            }
        GUILayout.EndHorizontal();

        GUILayout.Label("" + room.wallsides[0] + " / " + room.wallsides[1] + " / " + room.wallsides[2] + " / " + room.wallsides[3] + "");
    }
}
