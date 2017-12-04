using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorld : MonoBehaviour {

    public int gridSize = 20;

    Element[,] grid;

    Dictionary<Vector3, GameObject> roomsToSpawn = new Dictionary<Vector3, GameObject>();

    int currentSpawnID = 1;

    public bool spawnObjects = false;

    List<Vector2Int> openList = new List<Vector2Int>();
    List<Vector2Int> closedList = new List<Vector2Int>();

    bool generating = true;
    
    public int tries = 0;

    public int maxElements = 0;
    public float covered = 0;

    void Start () {
        grid = new Element[gridSize, gridSize];
        GenerateGrid();
    }

    [ContextMenu("Test")]
    void GenerateGrid() {
        grid = new Element[gridSize, gridSize];
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                grid[x, y] = new Element();
            }
        }

        GenerateRooms();

        GenerateHallways();

        GenerateHalls();

        if (spawnObjects)
            Spawn();
    }

    void Addwalls() {
        //*
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (!grid[x, y].floor)
                    continue;

                if (grid[x, y].roomID > 0)
                    continue;

                Element element = new Element() {
                    roomID = 0,

                    floor = grid[x, y].floor,

                    wallwest = x - 1 < 0 ? true : (grid[x - 1, y].floor && grid[x - 1, y].roomID > 0 ? true : !grid[x - 1, y].floor),
                    walleast = x + 1 >= gridSize ? true : (grid[x + 1, y].floor && grid[x + 1, y].roomID > 0 ? true : !grid[x + 1, y].floor),

                    wallsouth = y - 1 < 0 ? true : (grid[x, y - 1].floor && grid[x, y - 1].roomID > 0 ? true : !grid[x, y - 1].floor),
                    wallnorth = y + 1 >= gridSize ? true : (grid[x, y + 1].floor && grid[x, y + 1].roomID > 0 ? true : !grid[x, y + 1].floor),

                    //entrancewest = x - 1 < 0 ? false : (grid[x - 1, y].floor && grid[x - 1, y].roomID > 0 ? true : false),
                    //entranceeast = x + 1 >= gridSize ? false : (grid[x + 1, y].floor && grid[x + 1, y].roomID > 0 ? true : false),

                    //entrancesouth = y - 1 < 0 ? false : (grid[x, y - 1].floor && grid[x, y - 1].roomID > 0 ? true : false),
                    //entrancenorth = y + 1 >= gridSize ? false : (grid[x, y + 1].floor && grid[x, y + 1].roomID > 0 ? true : false),
                };

                if (element.wallwest && x - 1 >= 0) {
                    if (grid[x - 1, y].entranceeast) {
                        element.entrancewest = true;
                    } else if (grid[x - 1, y].entranceposiblilityeast) {
                        if (y + 1 < gridSize && grid[x, y + 1].floor && grid[x, y + 1].roomID == 0 && !grid[x, y + 1].entrancewest) {
                            print("do");
                            element.entrancewest = true;
                            grid[x - 1, y].entranceeast = true;
                        } else if (y - 1 >= 0 && grid[x, y - 1].floor && grid[x, y + 1].roomID == 0 && !grid[x, y - 1].entrancewest) {
                            print("do123");
                            element.entrancewest = true;
                            grid[x - 1, y].entranceeast = true;
                        } else {
                            int b = 0;
                            if (y + 1 < gridSize && !grid[x, y + 1].floor) {
                                b++;
                            }
                            if (y - 1 >= 0 && !grid[x, y - 1].floor) {
                                b++;
                            }
                            if (b == 2) {
                                element.entrancewest = true;
                                grid[x - 1, y].entranceeast = true;
                            }
                        }
                    }
                }

                if (element.walleast && x + 1 < gridSize) {
                    if (grid[x + 1, y].entrancewest)
                        element.entranceeast = true;
                }

                if (element.wallsouth && y - 1 >= 0) {
                    if (grid[x, y - 1].entrancenorth) {
                        element.entrancesouth = true;
                    }
                }

                if (element.wallnorth && y + 1 < gridSize) {
                    if (grid[x, y + 1].entrancesouth) {
                        element.entrancenorth = true;
                    }
                }

                grid[x, y] = element;

                Vector3 key = new Vector3(x * 4, 0, y * 4);

                bool turned = grid[x, y].entrancewest && grid[x, y].entranceeast;

                if (!roomsToSpawn.ContainsKey(key))
                    roomsToSpawn.Add(key, Resources.Load<GameObject>("rooms/hallway" + (turned ? "90" : "")));
            }
        }
    }

    void Spawn() {
        foreach(KeyValuePair<Vector3, GameObject> room in roomsToSpawn) {
            GameObject go = Instantiate(room.Value);
            go.transform.position = room.Key;
            go.transform.SetParent(transform);
        } 
    }

    Vector2Int[] checking = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };

    void ChangeGrid(int x, int y, int i, bool b) {
        switch (i) {
            case 0: grid[x, y].entrancesouth = b; break;
            case 1: grid[x, y].entrancenorth = b; break;
            case 2: grid[x, y].entrancewest = b; break;
            case 3: grid[x, y].entranceeast = b; break;

            case 4: grid[x, y].entrancenorth = b; break;
            case 5: grid[x, y].entrancesouth = b; break;
            case 6: grid[x, y].entranceeast = b; break;
            case 7: grid[x, y].entrancewest = b; break;
        }
    }

    void ChangeGrid(int x, int y, Element el) {
        grid[x, y] = el;
    }

    void Update() {
        if (openList.Count > 0) {
            Vector2Int checkingpos = openList[0];

            openList.Remove(checkingpos);
            closedList.Add(checkingpos);

            Element checkingElement = grid[checkingpos.x, checkingpos.y];

            for(int i = 0; i < checking.Length; i++){ // check all sides
                Vector2Int check = checking[i];
                Vector2Int neighbourCheck = checkingpos + check;

                if (closedList.Contains(neighbourCheck)) { // if already checked, dont check again
                    //print("int list");
                    continue;
                }

                if (neighbourCheck.x < 0 || neighbourCheck.x >= gridSize || neighbourCheck.y < 0 || neighbourCheck.y >= gridSize) // if outside of grid, dont check
                    continue;

                Element neighbour = grid[neighbourCheck.x, neighbourCheck.y];

                bool facingEntrance = i == 0 ? checkingElement.entrancenorth : (i == 1 ? checkingElement.entrancesouth : (i == 2 ? checkingElement.entranceeast : checkingElement.entrancewest));
                bool facingWall = i == 0 ? checkingElement.wallnorth : (i == 1 ? checkingElement.wallsouth : (i == 2 ? checkingElement.walleast : checkingElement.wallwest));
                bool facingEntrancePosible = i == 0 ? checkingElement.entranceposiblilitynorth : (i == 1 ? checkingElement.entranceposiblilitysouth : (i == 2 ? checkingElement.entranceposiblilityeast : checkingElement.entranceposiblilitywest));
                bool facingEntranceNeighbour = i == 0 ? neighbour.entrancesouth : (i == 1 ? neighbour.entrancenorth : (i == 2 ? neighbour.entrancewest : neighbour.entranceeast));
                bool facingEntranceNeighbourPosible = i == 0 ? neighbour.entranceposiblilitysouth : (i == 1 ? neighbour.entranceposiblilitynorth : (i == 2 ? neighbour.entranceposiblilitywest : neighbour.entranceposiblilityeast));

                if (check == checking[i]) {
                    if (neighbour.floor) {
                        if (facingEntrance) {
                            if (facingEntranceNeighbour) {

                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    continue;
                                }
                            }
                            else if (facingEntranceNeighbourPosible) {
                                ChangeGrid(neighbourCheck.x, neighbourCheck.y, i, true);

                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    continue;
                                }
                            } 
                            else if (neighbour.wallsouth) {
                                ChangeGrid(checkingpos.x, checkingpos.y, 4 + i, false);
                                continue;
                            }
                            else {
                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    continue;
                                }
                            }
                        } 
                        else if (facingEntranceNeighbour) {
                            if (facingEntrancePosible) {
                                ChangeGrid(checkingpos.x, checkingpos.y, 4 + i, true);

                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    continue;
                                }
                            }
                        }
                        else if (facingEntranceNeighbourPosible) {
                            if (tries > 0) {
                                ChangeGrid(checkingpos.x, checkingpos.y, 4 + i, true);

                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    tries--;
                                    continue;
                                }
                            }
                        }
                        else if (facingEntrancePosible) {
                            if(tries > 0) {
                                ChangeGrid(checkingpos.x, checkingpos.y, 4 + i, true);

                                if (!openList.Contains(neighbourCheck)) {
                                    openList.Add(neighbourCheck);
                                    tries--;
                                    continue;
                                }
                            }
                        }
                        else if (checkingElement.roomID == neighbour.roomID) {
                            if (!openList.Contains(neighbourCheck)) {
                                openList.Add(neighbourCheck);
                                continue;
                            }
                        } 
                    } 
                    else {
                        if (facingEntrance) {
                            //grid[checkingpos.x, checkingpos.y] = Element.justFloor;
                            ChangeGrid(checkingpos.x, checkingpos.y, Element.justFloor);

                            if (!openList.Contains(neighbourCheck)) {
                                openList.Add(neighbourCheck);
                                continue;
                            }
                        }
                        else if (facingWall) {
                            continue;
                        }
                        else if(tries > 0) {
                            for(int j = 0; j < checking.Length; j++) {
                                Vector2Int newCheck = neighbourCheck + checking[j];

                                if (closedList.Contains(newCheck))
                                    continue;

                                if (openList.Contains(newCheck))
                                    continue;

                                if (newCheck.x < 0 || newCheck.x >= gridSize || newCheck.y < 0 || newCheck.y >= gridSize) // if outside of grid, dont check
                                    continue;

                                Element newElement = grid[newCheck.x, newCheck.y];

                                bool newNeighbourPosible = j == 0 ? newElement.entranceposiblilitysouth : (j == 1 ? newElement.entranceposiblilitynorth : (j == 2 ? newElement.entranceposiblilitywest : newElement.entranceposiblilityeast));

                                if (newElement.floor) {
                                    if (newNeighbourPosible) {
                                        ChangeGrid(neighbourCheck.x, neighbourCheck.y, j, true);

                                        if (!openList.Contains(neighbourCheck)) {
                                            openList.Add(neighbourCheck);
                                        }
                                        tries--;
                                        break;
                                    }
                                    else if (newElement.roomID == checkingElement.roomID) {
                                        ChangeGrid(neighbourCheck.x, neighbourCheck.y, Element.justFloor);

                                        if (!openList.Contains(neighbourCheck)) {
                                            openList.Add(neighbourCheck);
                                        }
                                        tries--;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

        } else if(openList.Count <= 0 && !generating) {
            covered = (float)closedList.Count / (float)maxElements;

            maxElements = 0;
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (grid[x, y].floor) {
                        maxElements++;
                        if(!closedList.Contains(new Vector2Int(x, y))) {
                            if(openList.Count <= 0) {
                                openList.Add(new Vector2Int(x, y));
                                tries += 3;
                            }
                        }
                    }
                }
            }
            closedList.Clear();
        }
    }

    void GenerateHalls() {
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                if (openList.Count <= 0) {
                    if (grid[x, y].floor) {
                        openList.Add(new Vector2Int(x, y));
                        break;
                    }
                }
            }
            if (openList.Count > 0)
                break;
        }

        generating = false;

        /*
        while (openList.Count > 0) {
            Vector2Int checkingpos = openList[0];

            openList.Remove(checkingpos);
            closedList.Add(checkingpos);

            Element checkingElement = grid[checkingpos.x, checkingpos.y];

            foreach (Vector2Int check in checking) {
                Vector2Int neighbourCheck = checkingpos + check;

                if (closedList.Contains(neighbourCheck))
                    continue;

                if (neighbourCheck.x < 0 || neighbourCheck.x >= gridSize || neighbourCheck.y < 0 || neighbourCheck.y >= gridSize)
                    continue;

                Element neighbour = grid[neighbourCheck.x, neighbourCheck.y];

                openList.Add(neighbourCheck);
            }

        }*/
    }

    void GenerateHallways() {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (grid[x, y].entrancewest) {
                    if (x - 1 >= 0) {
                        if (!grid[x - 1, y].floor) {
                            grid[x - 1, y] = Element.justFloor;
                        }else {
                            if(grid[x - 1, y].entranceposiblilityeast) {
                                grid[x - 1, y].entranceeast = true;
                            }
                        }
                    }
                }

                if (grid[x, y].entranceeast) {
                    if (x + 1 < gridSize) {
                        if (!grid[x + 1, y].floor) { 
                            grid[x + 1, y] = Element.justFloor;
                        } else {
                            if (grid[x + 1, y].entranceposiblilitywest) {
                                grid[x + 1, y].entrancewest = true;
                            }
                        }
                    }
                }

                if (grid[x, y].entrancesouth) {
                    if (y - 1 >= 0) {
                        if (!grid[x, y - 1].floor) { 
                            grid[x, y - 1] = Element.justFloor;
                        } else {
                            if (grid[x, y - 1].entranceposiblilitynorth) {
                                grid[x, y - 1].entrancenorth = true;
                            }
                        }
                    }
                }

                if (grid[x, y].entrancenorth) {
                    if (y + 1 < gridSize) {
                        if (!grid[x, y + 1].floor) { 
                            grid[x, y + 1] = Element.justFloor;
                        } else {
                            if (grid[x, y + 1].entranceposiblilitysouth) {
                                grid[x, y + 1].entrancesouth = true;
                            }
                        }
                    }
                }
            }
        }

        
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (grid[x, y].floor)
                    continue;

                if (x - 1 < 0 || x + 1 >= gridSize || y - 1 < 0 || y + 1 >= gridSize)
                    continue;

                if (grid[x - 1, y].floor && grid[x + 1, y].floor && !grid[x, y - 1].floor && !grid[x, y + 1].floor) {
                    if ((y - 2 >= 0 && grid[x, y - 2].floor && grid[x, y - 2].roomID == 0) || (y + 2 < gridSize && grid[x, y + 2].floor && grid[x, y + 2].roomID == 0))
                        continue;

                    grid[x, y].roomID = 0;
                    grid[x, y].floor = true;
                    continue;
                }
            }
        }
    }

    void GenerateRooms() {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (grid[x, y].floor)
                    continue;

                if (x != 0 && y != 0 && Random.Range(0, 2) == 0)
                    continue;

                int x2 = x + Random.Range(0, 2);
                int y2 = y + Random.Range(0, 2);

                //print(x + " // " + y);

                AddRoom(Resources.Load<GameObject>("rooms/Room_0" + (Random.Range(0, 2) + 1 + "")), new Vector2(x2, y2));
            }
        }
    }

    void AddRoom(GameObject go, Vector2 position) {
        RoomGrid room = go.GetComponent<RoomGrid>();

        Element[,] old = (Element[,])grid.Clone();

        for (int y = 0; y < RoomGrid.size; y++) {
            for (int x = 0; x < RoomGrid.size; x++) {
                if (!room.grid[(y * RoomGrid.size) + x])
                    continue;

                if ((int)position.x + x >= gridSize) {
                    grid = old;
                    //print("too big on x");
                    return;
                }

                if ((int)position.y + y >= gridSize) {
                    grid = old;
                    //print("too big for y");
                    return;
                }

                if (grid[(int)position.x + x, (int)position.y + y].floor) {
                    grid = old;
                    //print("already contains something");
                    return;
                }

                Element element = new Element() {
                    roomID = currentSpawnID,

                    floor = room.grid2[(y * RoomGrid.size) + x] > 0,

                    wallwest = (int)position.x + x - 1 < 0 ? true : (x - 1 < 0 ? true : (room.grid2[(y * RoomGrid.size) + (x - 1)] > 0 ? false : true)),
                    walleast = (int)position.x + x + 1 >= gridSize ? true : (x + 1 >= RoomGrid.size ? true : (room.grid2[(y * RoomGrid.size) + (x + 1)] > 0 ? false : true)),
                    wallsouth = (int)position.y + y - 1 < 0 ? true : (y - 1 < 0 ? true : (room.grid2[((y - 1) * RoomGrid.size) + x] > 0 ? false : true)),
                    wallnorth = (int)position.y + y + 1 >= gridSize ? true : (y + 1 >= RoomGrid.size ? true : (room.grid2[((y + 1) * RoomGrid.size) + x] > 0 ? false : true)),

                    entrancewest = room.grid2[(y * RoomGrid.size) + x] == 2,
                    entranceeast = room.grid2[(y * RoomGrid.size) + x] == 4,
                    entrancesouth = room.grid2[(y * RoomGrid.size) + x] == 3,
                    entrancenorth = room.grid2[(y * RoomGrid.size) + x] == 5,

                    entranceposiblilitywest = room.grid2[(y * RoomGrid.size) + x] == 6,
                    entranceposiblilityeast = room.grid2[(y * RoomGrid.size) + x] == 8,
                    entranceposiblilitysouth = room.grid2[(y * RoomGrid.size) + x] == 7,
                    entranceposiblilitynorth = room.grid2[(y * RoomGrid.size) + x] == 9,
                };

                /*
                if (element.entrancewest) {
                    if((int)position.x + (x - 1) >= 0) {
                        grid[(int)position.x + (x - 1), (int)position.y + y] = Element.justFloor;
                    }
                }

                if (element.entranceeast) {
                    if ((int)position.x + (x + 1) < gridSize) {
                        grid[(int)position.x + (x + 1), (int)position.y + y] = Element.justFloor;
                    }
                }

                if (element.entrancesouth) {
                    if ((int)position.y + (y - 1) >= 0) {
                        grid[(int)position.x + x, (int)position.y + (y - 1)] = Element.justFloor;
                    }
                }

                if (element.entrancenorth) {
                    if ((int)position.y + (y + 1) < gridSize) {
                        grid[(int)position.x + x, (int)position.y + (y + 1)] = Element.justFloor;
                    }
                }
                */

                grid[(int)position.x + x, (int)position.y + y] = element;
            }
        }

        currentSpawnID++;

        Vector3 key = new Vector3(position.x * 4, 0, position.y * 4);

        if (!roomsToSpawn.ContainsKey(key))
            roomsToSpawn.Add(key, go);
    }

    private void OnDrawGizmosSelected() {
        if (grid == null)
            return;

        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                Color current = grid[x, y].roomID > 0 ? Color.white : Color.gray;

                if (grid[x, y].floor) {
                    Gizmos.color = closedList.Contains(new Vector2Int(x, y)) ? Color.blue : current;
                    Gizmos.DrawCube(new Vector3(x, 0, y), new Vector3(0.7f, 0.1f, 0.7f));
                }

                if (grid[x, y].wallnorth) {
                    Gizmos.color = grid[x, y].entrancenorth ? Color.red : (grid[x, y].entranceposiblilitynorth ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x, 1, y + 0.4f), new Vector3(0.7f, 2f, 0.1f));
                }

                if (grid[x, y].walleast) {
                    Gizmos.color = grid[x, y].entranceeast ? Color.red : (grid[x, y].entranceposiblilityeast ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x + 0.4f, 1, y), new Vector3(0.1f, 2f, 0.7f));
                }

                if (grid[x, y].wallsouth) {
                    Gizmos.color = grid[x, y].entrancesouth ? Color.red : (grid[x, y].entranceposiblilitysouth ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x, 1, y - 0.4f), new Vector3(0.7f, 2f, 0.1f));
                }

                if (grid[x, y].wallwest) {
                    Gizmos.color = grid[x, y].entrancewest ? Color.red : (grid[x, y].entranceposiblilitywest ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x - 0.4f, 1, y), new Vector3(0.1f, 2f, 0.7f));
                }
            }
        }
    }
}

[SerializeField]
public struct Element {
    public static Element justFloor = new Element() { floor = true };

    public int roomID; // 0 is hallway
    public bool floor;

    public bool wallnorth;
    public bool walleast;
    public bool wallsouth;
    public bool wallwest;

    public bool entrancenorth;
    public bool entranceeast;
    public bool entrancesouth;
    public bool entrancewest;

    public bool entranceposiblilitynorth;
    public bool entranceposiblilityeast;
    public bool entranceposiblilitysouth;
    public bool entranceposiblilitywest;

    public Element(int i) {
        roomID = 0; // 0 is hallway
        floor = false;

        wallnorth = false;
        walleast = false;
        wallsouth = false;
        wallwest = false;

        entrancenorth = false;
        entranceeast = false;
        entrancesouth = false;
        entrancewest = false;

        entranceposiblilitynorth = false;
        entranceposiblilityeast = false;
        entranceposiblilitysouth = false;
        entranceposiblilitywest = false;
}
}
