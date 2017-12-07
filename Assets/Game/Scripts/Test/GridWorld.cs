using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridWorld : MonoBehaviour {

    public int gridSize = 20;

    Element[,] grid;

    //Dictionary<Vector3, GameObject> roomsToSpawn = new Dictionary<Vector3, GameObject>();
    List<SpawnRequest> roomsToSpawn = new List<SpawnRequest>();

    public Transform playerTransform;

    int currentSpawnID = 1;

    public bool spawnObjects = false;

    List<Vector2Int> closedList = new List<Vector2Int>();

    public int spawnRoomTries = 20;
    public int cleanDeadEndTries = 20;

    int maxElementSize;
    int visited = 0;

    Vector2Int currentCheck;
    bool building = false;

    int backup = 1;

    bool doneGenerating = false;
    public bool checking = false;

    List<Vector2Int> checkopen = new List<Vector2Int>();
    List<Vector2Int> checkclosed = new List<Vector2Int>();

    List<GameObject> listOfRooms = new List<GameObject>();
    List<GameObject> listOfHalls = new List<GameObject>();

    void Start () {
        foreach (GameObject go in Resources.LoadAll<GameObject>("rooms")) {
            if (go.GetComponent<RoomGrid>()) {
                listOfRooms.Add(go);
            }
        }

        foreach (GameObject go in Resources.LoadAll<GameObject>("halls")) {
            listOfHalls.Add(go);
        }

        grid = new Element[gridSize, gridSize];
        maxElementSize = gridSize * gridSize;
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

        closedList.Clear();
        building = false;
        visited = 0;
        currentCheck = Vector2Int.zero;
        backup = 0;

        GenerateRooms();
        //we need to change the actual wall for an entrance if needed

        CheckEntrances(false);

        FillRemaining();

        CreateMaze();

        CheckEntrances(true);

        doneGenerating = true;

        Check();

        CleanDeadEnds();

        if (spawnObjects) {
            Spawn();
            PrepareHalls();

            //StaticBatchingUtility.Combine(gameObject);

            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    void cleanIfNotReachable() {

    }

    void Check() {
        //Vector2Int ve = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));

        checkopen.Clear();
        checkclosed.Clear();

        while (checkopen.Count == 0) {
            Vector2Int check = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            if(grid[check.x, check.y].floor)
                checkopen.Add(check);
        }

        while (doneGenerating && checking && checkopen.Count > 0) {
            Vector2Int current = checkopen[0];

            checkclosed.Add(current);
            checkopen.Remove(current);

            if ((!grid[current.x, current.y].walleast || grid[current.x, current.y].entranceeast) && current.x + 1 < gridSize) {
                if (!checkopen.Contains(new Vector2Int(current.x + 1, current.y)) && !checkclosed.Contains(new Vector2Int(current.x + 1, current.y)))
                    checkopen.Add(new Vector2Int(current.x + 1, current.y));
            }

            if ((!grid[current.x, current.y].wallwest || grid[current.x, current.y].entrancewest) && current.x - 1 >= 0) {
                if (!checkopen.Contains(new Vector2Int(current.x - 1, current.y)) && !checkclosed.Contains(new Vector2Int(current.x - 1, current.y)))
                    checkopen.Add(new Vector2Int(current.x - 1, current.y));
            }

            if ((!grid[current.x, current.y].wallnorth || grid[current.x, current.y].entrancenorth) && current.y + 1 < gridSize) {
                if (!checkopen.Contains(new Vector2Int(current.x, current.y + 1)) && !checkclosed.Contains(new Vector2Int(current.x, current.y + 1)))
                    checkopen.Add(new Vector2Int(current.x, current.y + 1));
            }

            if ((!grid[current.x, current.y].wallsouth || grid[current.x, current.y].entrancesouth) && current.y - 1 >= 0) {
                if (!checkopen.Contains(new Vector2Int(current.x, current.y - 1)) && !checkclosed.Contains(new Vector2Int(current.x, current.y - 1)))
                    checkopen.Add(new Vector2Int(current.x, current.y - 1));
            }

            if (checkopen.Count <= 0)
                checking = false;
        }
    }

    void PrepareHalls() {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (grid[x, y].floor && grid[x, y].roomID <= 0) {
                    int[] walls = new int[] {
                        grid[x, y].entrancenorth ? 2 : (grid[x, y].wallnorth ? 1 : 0),
                        grid[x, y].entranceeast ? 2 : (grid[x, y].walleast ? 1 : 0),
                        grid[x, y].entrancesouth ? 2 : (grid[x, y].wallsouth ? 1 : 0),
                        grid[x, y].entrancewest ? 2 : (grid[x, y].wallwest ? 1 : 0)
                    };

                    bool got = false;

                    GameObject hallbase = Instantiate(Resources.Load<GameObject>("halls/floor"));
                    //hallbase.isStatic = true;
                    hallbase.transform.position = new Vector3(x * 4, 0, y * 4);
                    hallbase.transform.SetParent(transform);

                    for (int i = 0; i < 4; i++) {
                        if (walls[i] > 0) {
                            GameObject wall = Instantiate(Resources.Load<GameObject>("halls/" + (walls[i] == 1 ? "wall" : "entrance")));
                            wall.isStatic = true;
                            wall.transform.SetParent(hallbase.transform);
                            wall.transform.localPosition = Vector3.zero;
                            wall.transform.localEulerAngles = new Vector3(0, (i - 1) * 90, 0);
                            got = true;
                        }
                    }

                    if (!got) {
                        print("" + walls[0] + " / " + walls[1] + " / " + walls[2] + " / " + walls[3] + " |");
                    }
                }
            }
        }
    }

    void CleanDeadEnds() {
        for (int i = 0; i < cleanDeadEndTries; i++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (!grid[x, y].floor || grid[x, y].roomID > 0)
                        continue;

                    int wallAmount = 0;
                    int direction = 0;

                    if (grid[x, y].walleast && !grid[x, y].entranceeast)
                        wallAmount++;

                    if (grid[x, y].wallwest && !grid[x, y].entrancewest)
                        wallAmount++;

                    if (grid[x, y].wallnorth && !grid[x, y].entrancenorth)
                        wallAmount++;

                    if (grid[x, y].wallsouth && !grid[x, y].entrancesouth)
                        wallAmount++;

                    bool hasEntrance = grid[x, y].entrancesouth || grid[x, y].entrancenorth || grid[x, y].entrancewest || grid[x, y].entranceeast;

                    if (wallAmount >= 3 && !hasEntrance) {
                        direction = !grid[x, y].walleast ? 1 : (!grid[x, y].wallwest ? 2 : (!grid[x, y].wallnorth ? 3 : (!grid[x, y].wallsouth ? 4 : 0)));

                        grid[x, y].floor = false;
                        grid[x, y].walleast = false;
                        grid[x, y].wallwest = false;
                        grid[x, y].wallnorth = false;
                        grid[x, y].wallsouth = false;
                    }

                    if(direction == 1 && x + 1 < gridSize && grid[x + 1, y].floor && grid[x + 1, y].roomID <= 0) {
                        if (!grid[x + 1, y].wallwest)
                            grid[x + 1, y].wallwest = true;
                    }

                    if (direction == 2 && x - 1 >= 0 && grid[x - 1, y].floor && grid[x - 1, y].roomID <= 0) {
                        if (!grid[x - 1, y].walleast)
                            grid[x - 1, y].walleast = true;
                    }

                    if (direction == 3 && y + 1 < gridSize && grid[x, y + 1].floor && grid[x, y + 1].roomID <= 0) {
                        if (!grid[x, y + 1].wallsouth)
                            grid[x, y + 1].wallsouth = true;
                    }

                    if (direction == 4 && y - 1 >= 0 && grid[x, y - 1].floor && grid[x, y - 1].roomID <= 0) {
                        if (!grid[x, y - 1].wallnorth)
                            grid[x, y - 1].wallnorth = true;
                    }
                }
            }
        }
    }

    void BreakWall(Vector2Int current, Vector2Int neighbour) {
        Vector2Int dif = neighbour - current;

        bool up = dif.x == 0 && dif.y == 1;
        bool down = dif.x == 0 && dif.y == -1;
        bool left = dif.x == -1 && dif.y == 0;
        bool right = dif.x == 1 && dif.y == 0;

        if (up) {
            grid[current.x, current.y].wallnorth = false;
            grid[neighbour.x, neighbour.y].wallsouth = false;
        } else if (down) {
            grid[current.x, current.y].wallsouth = false;
            grid[neighbour.x, neighbour.y].wallnorth = false;
        } else if (left) {
            grid[current.x, current.y].wallwest = false;
            grid[neighbour.x, neighbour.y].walleast = false;
        } else if (right) {
            grid[current.x, current.y].walleast = false;
            grid[neighbour.x, neighbour.y].wallwest = false;
        }
    }

    void CreateMaze() {
        while (visited < maxElementSize) {

            if (building) {
                List<Vector2Int> possibleChecks = new List<Vector2Int>();

                if (currentCheck.x - 1 >= 0 && grid[currentCheck.x - 1, currentCheck.y].roomID == -1)
                    possibleChecks.Add(new Vector2Int(-1, 0));

                if (currentCheck.x + 1 < gridSize && grid[currentCheck.x + 1, currentCheck.y].roomID == -1)
                    possibleChecks.Add(new Vector2Int(1, 0));

                if (currentCheck.y - 1 >= 0 && grid[currentCheck.x, currentCheck.y - 1].roomID == -1)
                    possibleChecks.Add(new Vector2Int(0, -1));

                if (currentCheck.y + 1 < gridSize && grid[currentCheck.x, currentCheck.y + 1].roomID == -1)
                    possibleChecks.Add(new Vector2Int(0, 1));

                if (possibleChecks.Count <= 0) {
                    if (backup < 0) {
                        grid[currentCheck.x, currentCheck.y].floor = false;
                        visited += 1;
                        continue;
                    }

                    if (backup > 0)
                        currentCheck = closedList[backup];

                    backup--;

                    if (backup < 0)
                        building = false;
                } else {
                    Element currentElement = grid[currentCheck.x, currentCheck.y];
                    Vector2Int neighbourcheck = currentCheck + possibleChecks[Random.Range(0, possibleChecks.Count)];
                    Element neighbourElement = grid[neighbourcheck.x, neighbourcheck.y];

                    if (neighbourElement.roomID == -1 && currentElement.roomID == 0) {

                        BreakWall(currentCheck, neighbourcheck);

                        grid[neighbourcheck.x, neighbourcheck.y].roomID = 0;
                        visited += 1;

                        closedList.Add(currentCheck);
                        //openList.Remove(currentCheck);

                        currentCheck = neighbourcheck;

                        if (closedList.Count > 0) {
                            backup = closedList.Count - 1;
                        }
                    }
                }
            } else {
                currentCheck = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
                if (grid[currentCheck.x, currentCheck.y].floor && grid[currentCheck.x, currentCheck.y].roomID == -1) {
                    visited += 1;
                    grid[currentCheck.x, currentCheck.y].roomID = 0;
                    building = true;
                }
            }
        }
    }

    void FillRemaining() {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (grid[x, y].floor)
                    continue;

                grid[x, y] = Element.allwalls;
            }
        }
    }

    void CheckEntrances(bool breakWalls) {
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                if (!grid[x, y].floor)
                    continue;

                if (breakWalls) {
                    
                    List<int> changedRoomID = new List<int>();

                    if (changedRoomID.Contains(grid[x, y].roomID))
                        continue;

                    if (grid[x, y].entranceposiblilitynorth && y + 1 < gridSize) {
                        if (grid[x, y + 1].roomID == 0 ? grid[x, y + 1].wallsouth : grid[x, y + 1].entranceposiblilitysouth) {
                            if ((x + 1 < gridSize && !grid[x + 1, y].entrancenorth) && (x - 1 >= 0 && !grid[x - 1, y].entrancenorth)) {
                                grid[x, y].entrancenorth = true;
                                grid[x, y + 1].entrancesouth = true;
                                changedRoomID.Add(grid[x, y].roomID);
                            }
                        }
                    }

                    if (grid[x, y].entranceposiblilitysouth && y - 1 >= 0) {
                        if (grid[x, y - 1].roomID == 0 ? grid[x, y - 1].wallnorth : grid[x, y - 1].entranceposiblilitynorth) {
                            if ((x + 1 < gridSize && !grid[x + 1, y].entrancesouth) && (x - 1 >= 0 && !grid[x - 1, y].entrancesouth)) {
                                grid[x, y].entrancesouth = true;
                                grid[x, y - 1].entrancenorth = true;
                                changedRoomID.Add(grid[x, y].roomID);
                            }
                        }
                    }

                    if (grid[x, y].entranceposiblilityeast && x + 1 < gridSize) {
                        if (grid[x + 1, y].roomID == 0 ? grid[x + 1, y].wallwest : grid[x + 1, y].entranceposiblilitywest) {
                            if ((y + 1 < gridSize && !grid[x, y + 1].entranceeast) && (y - 1 >= 0 && !grid[x, y - 1].entranceeast)) {
                                grid[x, y].entranceeast = true;
                                grid[x + 1, y].entrancewest = true;
                                changedRoomID.Add(grid[x, y].roomID);
                            }
                        }
                    }

                    if (grid[x, y].entranceposiblilitywest && x - 1 >= 0) {
                        if (grid[x - 1, y].roomID == 0 ? grid[x - 1, y].walleast : grid[x - 1, y].entranceposiblilityeast) {
                            if ((y + 1 < gridSize && !grid[x, y + 1].entrancewest) && (y - 1 >= 0 && !grid[x, y - 1].entrancewest)) {
                                grid[x, y].entrancewest = true;
                                grid[x - 1, y].entranceeast = true;
                                changedRoomID.Add(grid[x, y].roomID);
                            }
                        }
                    }
                    
                }

                if (grid[x, y].entrancewest) {
                    if (x - 1 >= 0) {
                        if (grid[x - 1, y].entranceeast) {
                            continue;
                        }
                        else if (grid[x - 1, y].entranceposiblilityeast) {
                            grid[x - 1, y].entranceeast = true;
                        } 
                        else if (grid[x - 1, y ].walleast) {
                            if (breakWalls) {
                                grid[x - 1, y].entranceeast = true;
                            } else {
                                grid[x, y].entrancewest = false;
                                grid[x, y].entranceposiblilitywest = true;
                                roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endwest"));
                            }
                        }
                    } 
                    else {
                        grid[x, y].entrancewest = false;
                        grid[x, y].entranceposiblilitywest = true;
                        roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endwest"));
                    }
                }

                if (grid[x, y].entranceeast) {
                    if (x + 1 < gridSize) {
                        if (grid[x + 1, y].entrancewest) {
                            continue;
                        } 
                        else if (grid[x + 1, y].entranceposiblilitywest) {
                            grid[x + 1, y].entrancewest = true;
                        } 
                        else if (grid[x + 1, y].wallwest) {
                            if (breakWalls) {
                                grid[x + 1, y].entrancewest = true;
                            } else {
                                grid[x, y].entranceeast = false;
                                grid[x, y].entranceposiblilityeast = true;
                                roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endeast"));
                            }
                        }
                    } else {
                        grid[x, y].entranceeast = false;
                        grid[x, y].entranceposiblilityeast = true;
                        roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endeast"));
                    }
                }

                if (grid[x, y].entrancenorth) {
                    if (y + 1 < gridSize) {
                        if (grid[x, y + 1].entrancesouth) {
                            continue;
                        } 
                        else if (grid[x, y + 1].entranceposiblilitysouth) {
                            grid[x, y + 1].entrancesouth = true;
                        } 
                        else if (grid[x, y + 1].wallsouth) {
                            if (breakWalls) {
                                grid[x, y + 1].entrancesouth = true;
                            } else {
                                grid[x, y].entrancenorth = false;
                                grid[x, y].entranceposiblilitynorth = true;
                                roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endnorth"));
                            }
                        }
                    } else {
                        grid[x, y].entrancenorth = false;
                        grid[x, y].entranceposiblilitynorth = true;
                        roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endnorth"));
                    }
                }

                if (grid[x, y].entrancesouth) {
                    if (y - 1 >= 0) {
                        if (grid[x, y - 1].entrancenorth) {
                            continue;
                        } 
                        else if (grid[x, y - 1].entranceposiblilitynorth) {
                            grid[x, y - 1].entrancenorth = true;
                        } 
                        else if (grid[x, y - 1].wallnorth) {
                            if (breakWalls) {
                                grid[x, y - 1].entrancenorth = true;
                            } else {
                                grid[x, y].entrancesouth = false;
                                grid[x, y].entranceposiblilitysouth = true;
                                roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endsouth"));
                            }
                        }
                    } else {
                        grid[x, y].entrancesouth = false;
                        grid[x, y].entranceposiblilitysouth = true;
                        roomsToSpawn.Add(new SpawnRequest(new Vector3(x, 0, y), "halls/endsouth"));
                    }
                }
            }
        }
    }

    void Spawn() {
        foreach(SpawnRequest request in roomsToSpawn) {
            if(request.itemobject == null) {
                GameObject go = Instantiate(Resources.Load<GameObject>(request.itemname));
                go.transform.position = request.position * 4;
                go.transform.SetParent(transform);
            }
            else {
                GameObject go = Instantiate(request.itemobject);
                go.transform.position = request.position * 4;
                go.transform.SetParent(transform);
            }

        } 
    }

    void GenerateRooms() {
        if (spawnRoomTries > 0) {
            for (int i = 0; i < spawnRoomTries; i++) {

                int x2 = Random.Range(0, gridSize);
                int y2 = Random.Range(0, gridSize);

                AddRoom(listOfRooms[Random.Range(0, listOfRooms.Count)], new Vector2(x2, y2));
            }
        } else {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {

                    int x2 = x + Random.Range(0, 3);
                    int y2 = y + Random.Range(0, 3);

                    AddRoom(listOfRooms[Random.Range(0, listOfRooms.Count)], new Vector2(x == 0 ? 0 : x2, y == 0 ? 0 : y2));

                }
            }
        }
    }

    void AddRoom(GameObject go, Vector2 position) {
        RoomGrid room = go.GetComponent<RoomGrid>();

        Element[,] old = (Element[,])grid.Clone();

        int roomsize = 0;

        Vector2Int spot  = Vector2Int.zero;

        for (int y = 0; y < RoomGrid.size; y++) {
            for (int x = 0; x < RoomGrid.size; x++) {
                if (room.grid2[(y * RoomGrid.size) + x] == 0)
                    continue;

                if ((int)position.x + x >= gridSize || (int)position.y + y >= gridSize) { // if the room is going off the edges, dont put it there
                    grid = old;
                    return;
                }

                if (grid[(int)position.x + x, (int)position.y + y].floor) { //if the room is overlapping, dont put it there
                    grid = old;
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

                if(Random.Range(0, 10) == 0) {
                    spot = new Vector2Int(x, y);
                }

                grid[(int)position.x + x, (int)position.y + y] = element;
                roomsize++;
            }
        }
        visited += roomsize;

        if (currentSpawnID == 1) {
            playerTransform.position = new Vector3(((int)position.x + spot.x) * 4, 0, ((int)position.y + spot.y) * 4);
        }

        currentSpawnID++;

        Vector3 key = new Vector3(position.x, 0, position.y);

        roomsToSpawn.Add(new SpawnRequest(key, go));
    }

    private void OnDrawGizmosSelected() {
        if (grid == null)
            return;

        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                Color current = grid[x, y].roomID > 0 ? Color.white : Color.gray;

                if (grid[x, y].floor) {
                    //Gizmos.color = grid[x, y].roomID <= 0 ? (grid[x, y].roomID == 0 ? Color.green : Color.black) : current;
                    Gizmos.color = checkclosed.Contains(new Vector2Int(x, y)) ? Color.green : Color.white;
                    Gizmos.DrawCube(new Vector3(x, 0, y - 100), new Vector3(0.7f, 0.1f, 0.7f));
                }

                
                if (grid[x, y].wallnorth) {
                    Gizmos.color = grid[x, y].entrancenorth ? Color.red : (grid[x, y].entranceposiblilitynorth ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x, 1, y + 0.4f - 100), new Vector3(0.7f, 2f, 0.1f));
                }

                if (grid[x, y].walleast) {
                    Gizmos.color = grid[x, y].entranceeast ? Color.red : (grid[x, y].entranceposiblilityeast ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x + 0.4f, 1, y - 100), new Vector3(0.1f, 2f, 0.7f));
                }

                if (grid[x, y].wallsouth) {
                    Gizmos.color = grid[x, y].entrancesouth ? Color.red : (grid[x, y].entranceposiblilitysouth ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x, 1, y - 0.4f - 100), new Vector3(0.7f, 2f, 0.1f));
                }

                if (grid[x, y].wallwest) {
                    Gizmos.color = grid[x, y].entrancewest ? Color.red : (grid[x, y].entranceposiblilitywest ? Color.yellow : current);
                    Gizmos.DrawCube(new Vector3(x - 0.4f, 1, y - 100), new Vector3(0.1f, 2f, 0.7f));
                }
                
            }
        }
    }
}

public struct SpawnRequest {
    public Vector3 position;
    public string itemname;
    public GameObject itemobject;

    public SpawnRequest(Vector3 v, string s) {
        position = v;
        itemname = s;
        itemobject = null;
    }

    public SpawnRequest(Vector3 v, GameObject g) {
        position = v;
        itemname = "";
        itemobject = g;
    }
}

[SerializeField]
public struct Element {
    public static Element justFloor = new Element() { roomID = -1, floor = true };
    public static Element allwalls = new Element() { roomID = -1, floor = true, walleast = true, wallnorth = true, wallsouth = true, wallwest = true };

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
        roomID = -1; // 0 is hallway
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
