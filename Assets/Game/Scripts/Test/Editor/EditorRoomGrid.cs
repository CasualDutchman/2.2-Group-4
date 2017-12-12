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
                if (GUILayout.Button(GetTexture(room.grid2[(y * RoomGrid.size) + x]), GUIStyle.none, GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) {

                    if(Event.current.modifiers == EventModifiers.Control) {
                        room.grid2[(y * RoomGrid.size) + x] = 0;
                    }
                    else if (Event.current.modifiers == EventModifiers.Shift) {
                        if (room.grid2[(y * RoomGrid.size) + x] == 2)
                            room.grid2[(y * RoomGrid.size) + x] = 6;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 3)
                            room.grid2[(y * RoomGrid.size) + x] = 7;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 4)
                            room.grid2[(y * RoomGrid.size) + x] = 8;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 5)
                            room.grid2[(y * RoomGrid.size) + x] = 9;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 6)
                            room.grid2[(y * RoomGrid.size) + x] = 2;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 7)
                            room.grid2[(y * RoomGrid.size) + x] = 3;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 8)
                            room.grid2[(y * RoomGrid.size) + x] = 4;

                        else if (room.grid2[(y * RoomGrid.size) + x] == 9)
                            room.grid2[(y * RoomGrid.size) + x] = 5;
                    } 
                    else {
                        room.grid2[(y * RoomGrid.size) + x]++;
                        if (room.grid2[(y * RoomGrid.size) + x] > 9)
                            room.grid2[(y * RoomGrid.size) + x] = 0;
                    }

                    
                }
                
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.EndVertical();
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
                }

                
            }
        }

        tex.Apply();

        return tex;
    }
}
