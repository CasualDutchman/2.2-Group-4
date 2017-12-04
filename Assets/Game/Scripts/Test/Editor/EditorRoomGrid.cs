using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomGrid))]
public class EditorRoomGrid : Editor {

    RoomGrid room;

    void OnEnable() {
        room = (RoomGrid)target;
    }

    public override void OnInspectorGUI() {
        GUILayout.BeginVertical();
        for (int y = RoomGrid.size - 1; y >= 0; y--) {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < RoomGrid.size; x++) {
                room.grid[(y * RoomGrid.size) + x] = EditorGUILayout.Toggle(room.grid[(y * RoomGrid.size) + x], GUILayout.MaxWidth(13));
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.BeginVertical();
        for (int y = RoomGrid.size - 1; y >= 0; y--) {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < RoomGrid.size; x++) {
                //room.grid2[(y * RoomGrid.size) + x].floor = EditorGUILayout.Toggle(room.grid2[(y * RoomGrid.size) + x].floor, GUILayout.MaxWidth(13));
                if (GUILayout.Button(GetTexture(room.grid2[(y * RoomGrid.size) + x]), GUILayout.MaxWidth(12), GUILayout.MaxHeight(12))) {

                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    Texture GetTexture(int i) {
        Texture2D tex = new Texture2D(12, 12);

        for(int y = 0; y < 13; y++) {
            for (int x = 0; x < 13; x++) {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();

        return tex;
    }
}
