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
                GUILayout.Space(3);
                //room.grid2[(y * RoomGrid.size) + x].floor = EditorGUILayout.Toggle(room.grid2[(y * RoomGrid.size) + x].floor, GUILayout.MaxWidth(13));
                if (GUILayout.Button(GetTexture(room.grid[(y * RoomGrid.size) + x]), GUIStyle.none, GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) {

                    if(Event.current.modifiers == EventModifiers.Control) {
                        room.grid[(y * RoomGrid.size) + x] = 0;
                    }
                    else if (Event.current.modifiers == EventModifiers.Shift) {
                        if (room.grid[(y * RoomGrid.size) + x] == 2)
                            room.grid[(y * RoomGrid.size) + x] = 6;

                        else if (room.grid[(y * RoomGrid.size) + x] == 3)
                            room.grid[(y * RoomGrid.size) + x] = 7;

                        else if (room.grid[(y * RoomGrid.size) + x] == 4)
                            room.grid[(y * RoomGrid.size) + x] = 8;

                        else if (room.grid[(y * RoomGrid.size) + x] == 5)
                            room.grid[(y * RoomGrid.size) + x] = 9;

                        else if (room.grid[(y * RoomGrid.size) + x] == 6)
                            room.grid[(y * RoomGrid.size) + x] = 2;

                        else if (room.grid[(y * RoomGrid.size) + x] == 7)
                            room.grid[(y * RoomGrid.size) + x] = 3;

                        else if (room.grid[(y * RoomGrid.size) + x] == 8)
                            room.grid[(y * RoomGrid.size) + x] = 4;

                        else if (room.grid[(y * RoomGrid.size) + x] == 9)
                            room.grid[(y * RoomGrid.size) + x] = 5;
                    } 
                    else {
                        room.grid[(y * RoomGrid.size) + x]++;
                        if (room.grid[(y * RoomGrid.size) + x] > 10)
                            room.grid[(y * RoomGrid.size) + x] = 0;
                    }

                }
                
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.EndVertical();

        room.secondFloor = EditorGUILayout.Toggle("Has second floor", room.secondFloor);

        if (room.secondFloor) {
            GUILayout.BeginVertical();
            for (int y = RoomGrid.size - 1; y >= 0; y--) {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < RoomGrid.size; x++) {
                    GUILayout.Space(3);
                    //room.grid2[(y * RoomGrid.size) + x].floor = EditorGUILayout.Toggle(room.grid2[(y * RoomGrid.size) + x].floor, GUILayout.MaxWidth(13));
                    if (GUILayout.Button(GetTexture(room.gridsecond[(y * RoomGrid.size) + x]), GUIStyle.none, GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) {

                        if (Event.current.modifiers == EventModifiers.Control) {
                            room.gridsecond[(y * RoomGrid.size) + x] = 0;
                        } else if (Event.current.modifiers == EventModifiers.Shift) {
                            if (room.gridsecond[(y * RoomGrid.size) + x] == 2)
                                room.gridsecond[(y * RoomGrid.size) + x] = 6;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 3)
                                room.gridsecond[(y * RoomGrid.size) + x] = 7;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 4)
                                room.gridsecond[(y * RoomGrid.size) + x] = 8;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 5)
                                room.gridsecond[(y * RoomGrid.size) + x] = 9;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 6)
                                room.gridsecond[(y * RoomGrid.size) + x] = 2;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 7)
                                room.gridsecond[(y * RoomGrid.size) + x] = 3;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 8)
                                room.gridsecond[(y * RoomGrid.size) + x] = 4;

                            else if (room.gridsecond[(y * RoomGrid.size) + x] == 9)
                                room.gridsecond[(y * RoomGrid.size) + x] = 5;
                        } else {
                            room.gridsecond[(y * RoomGrid.size) + x]++;
                            if (room.gridsecond[(y * RoomGrid.size) + x] > 10)
                                room.gridsecond[(y * RoomGrid.size) + x] = 0;
                        }
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(3);
            }
            GUILayout.EndVertical();
        }

        for (int f = 0; f < (room.secondFloor ? 2 : 1); f++) {
            for (int y = 0; y < RoomGrid.size; y++) {
                for (int x = 0; x < RoomGrid.size; x++) {
                    if ((f == 0 ? room.grid[(y * RoomGrid.size) + x] : room.gridsecond[(y * RoomGrid.size) + x]) > 1 && (f == 0 ? room.grid[(y * RoomGrid.size) + x] : room.gridsecond[(y * RoomGrid.size) + x]) < 10) {
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(x + "." + f + "." + y);

                        room.entranceObjects[(f * RoomGrid.size * RoomGrid.size) + (y * RoomGrid.size) + x] = (GameObject)EditorGUILayout.ObjectField(room.entranceObjects[(f * RoomGrid.size * RoomGrid.size) + (y * RoomGrid.size) + x], typeof(GameObject), true);

                        GUILayout.EndHorizontal();
                    } else {
                        room.entranceObjects[(f * RoomGrid.size * RoomGrid.size) + (y * RoomGrid.size) + x] = null;
                    }
                }
            }
        }
    }

    Texture GetTexture(int i) {
        Texture2D tex = new Texture2D(15, 15);

        for(int y = 0; y < 15; y++) {
            for (int x = 0; x < 15; x++) {
                switch (i) {
                    case 0: {
                            tex.SetPixel(x, y, new Color(0.8f, 0.8f, 0.8f)); 
                        }; break;
                    case 1: {
                            tex.SetPixel(x, y, Color.white);
                        }; break;
                    case 2: {
                            if (x <= 2) {
                                tex.SetPixel(x, y, Color.black);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 3: {
                            if (y <= 2) {
                                tex.SetPixel(x, y, Color.black);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 4: {
                            if (x >= 12) {
                                tex.SetPixel(x, y, Color.black);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 5: {
                            if (y >= 12) {
                                tex.SetPixel(x, y, Color.black);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 6: {
                            if (x <= 2) {
                                tex.SetPixel(x, y, Color.red);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 7: {
                            if (y <= 2) {
                                tex.SetPixel(x, y, Color.red);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 8: {
                            if (x >= 12) {
                                tex.SetPixel(x, y, Color.red);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 9: {
                            if (y >= 12) {
                                tex.SetPixel(x, y, Color.red);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                    case 10: {
                            if (x > 2 && x < 13 && y > 2 && y < 13) {
                                tex.SetPixel(x, y, Color.black);
                            } else {
                                tex.SetPixel(x, y, Color.white);
                            }
                        }; break;
                }

                
            }
        }

        tex.Apply();

        return tex;
    }
}
