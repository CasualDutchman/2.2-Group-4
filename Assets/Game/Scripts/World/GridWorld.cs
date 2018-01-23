using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridWorld : MonoBehaviour {

    public int maxEnemySpawned = 60;
    int currentEnemyCount;

    public GameObject[] enemies;

    List<Transform> spawnpoints = new List<Transform>();

    public GameObject[] weaponPrefabs;

    public GameObject doorObj;
    List<Vector3> doorLocations = new List<Vector3>();

    float timer;
    public float timeTillSpawn;

    public int gridSize = 20;
    public int floors = 1;

    Element[,,] grid;

    public GameObject Floor, Ceiling, Wall, Entrance;
    public Material allTextureMaterial;

    //Dictionary<Vector3, GameObject> roomsToSpawn = new Dictionary<Vector3, GameObject>();
    List<SpawnRequest> roomsToSpawn = new List<SpawnRequest>();

    Dictionary<Vector3Int, int> roomEntrances = new Dictionary<Vector3Int, int>();

    List<GameObject> staticRooms = new List<GameObject>();

    public PlayerManager playerManager;

    int currentSpawnID = 1;

    public Vector3 scale;

    public bool spawnObjects = false;

    public int spawnRoomTries = 20;
    public int cleanDeadEndTries = 20;

    int maxElementSize;
    int visited = 0;

    Vector3Int currentCheck;

    bool buildingMaze = false;

    int backup = 1;

    bool doneGenerating = false;
    public bool checking = false;

    bool doneSpawning = false;

    List<Vector3Int> checkopen = new List<Vector3Int>();
    List<Vector3Int> checkclosed = new List<Vector3Int>();

    List<Vector3Int> closedList = new List<Vector3Int>();

    List<GameObject> listOfAllRooms = new List<GameObject>();
    List<GameObject> listOfFloorRooms = new List<GameObject>();

    List<GameObject> listOfHalls = new List<GameObject>();

    public static GridWorld instance;

    private void Awake() {
        instance = this;
    }

    void Start () {
        foreach (GameObject go in Resources.LoadAll<GameObject>("rooms")) {
            if (!go.GetComponent<RoomGrid>().secondFloor) {
                listOfFloorRooms.Add(go);
            }

            listOfAllRooms.Add(go);
        }

        foreach (GameObject go in Resources.LoadAll<GameObject>("halls")) {
            listOfHalls.Add(go);
        }

        grid = new Element[gridSize, floors, gridSize];
        maxElementSize = gridSize * gridSize * floors;
        GenerateGrid();
    }

    [ContextMenu("Test")]
    void GenerateGrid() {
        grid = new Element[gridSize, floors, gridSize];
        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    grid[x, f, y] = new Element();
                }
            }
        }

        buildingMaze = false;
        visited = 0;
        currentCheck = Vector3Int.zero;

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

            StaticBatchingUtility.Combine(staticRooms.ToArray(), gameObject);

            doneSpawning = true;
        }
    }

    void Update() {
        if (doneSpawning) {
            
            GetComponent<NavMeshSurface>().BuildNavMesh();

            SpawnDoors();

            //SpawnWeapons();
            SpawnSpawnPoints();
            playerManager.Play();

            

            doneSpawning = false;
        }
    }

    public void OnPlayerDeath(Player player, int lives) {
        playerManager.Respawn(player, lives);
        SpawnEnemy(0, player.transform.position, transform);
    }

    void SpawnDoors() {
        foreach (Vector3 doorLoc in doorLocations) {
            if (Random.value < 0.3f)
                continue;

            Quaternion rot = Quaternion.Euler(0, doorLoc.z % 1 != 0 ? (Random.value < 0.5 ? 90 : -90) : (Random.value < 0.5 ? 0 : -180), 0);

            GameObject go = Instantiate(doorObj, new Vector3(doorLoc.x * scale.x, doorLoc.y * scale.y, doorLoc.z * scale.z), rot);

            go.transform.parent = transform;
        }
    }

    void SpawnSpawnPoints() {
        GameObject spawns = new GameObject("spawns");
        List<Vector3> spawnpoints = new List<Vector3>();
        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if(grid[x, f, y].floor) {
                        if (Random.Range(0, 5) > 0)
                            continue;

                        if (Vector3.Distance(playerManager.playerOneSpawn.position, new Vector3(x * scale.x, f * scale.y, y * scale.z)) < 16)
                            continue;

                        spawnpoints.Add(new Vector3(x, f, y));
                    }
                }
            }
        }

        for (int i = 0; i < maxEnemySpawned; i++) {
            int index = Random.Range(0, spawnpoints.Count);
            Vector3 spawn = spawnpoints[index];
            spawnpoints.RemoveAt(index);

            int rand = Random.Range(0, 100);
            int enemytype = rand < 10 ? 2 : (rand < 20 ? 1 : 0);

            SpawnEnemy(enemytype, new Vector3(spawn.x * scale.x, spawn.y * scale.y, spawn.z * scale.z), spawns.transform);
        }
    }

    Enemy SpawnEnemy(int index, Vector3 pos, Transform parent) {
        GameObject go = Instantiate(enemies[index]);
        go.transform.position = pos;
        go.GetComponent<NavMeshAgent>().enabled = true;
        go.transform.parent = parent;
        currentEnemyCount++;

        return go.GetComponent<Enemy>();
    }

    void Check() {
        //Vector2Int ve = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));

        checkopen.Clear();
        checkclosed.Clear();

        while (checkopen.Count == 0) {
            Vector3Int check = new Vector3Int(Random.Range(0, gridSize), Random.Range(0, floors), Random.Range(0, gridSize));
            if(grid[check.x, check.y, check.z].floor)
                checkopen.Add(check);
        }

        while (doneGenerating && checking && checkopen.Count > 0) {
            Vector3Int current = checkopen[0];

            checkclosed.Add(current);
            checkopen.Remove(current);

            //check est
            if ((!grid[current.x, current.y, current.z].walleast || grid[current.x, current.y, current.z].entranceeast) && current.x + 1 < gridSize) {
                if (!checkopen.Contains(new Vector3Int(current.x + 1, current.y, current.z)) && !checkclosed.Contains(new Vector3Int(current.x + 1, current.y, current.z)))
                    checkopen.Add(new Vector3Int(current.x + 1, current.y, current.z));
            }

            //check west
            if ((!grid[current.x, current.y, current.z].wallwest || grid[current.x, current.y, current.z].entrancewest) && current.x - 1 >= 0) {
                if (!checkopen.Contains(new Vector3Int(current.x - 1, current.y, current.z)) && !checkclosed.Contains(new Vector3Int(current.x - 1, current.y, current.z)))
                    checkopen.Add(new Vector3Int(current.x - 1, current.y, current.z));
            }

            //check north
            if ((!grid[current.x, current.y, current.z].wallnorth || grid[current.x, current.y, current.z].entrancenorth) && current.z + 1 < gridSize) {
                if (!checkopen.Contains(new Vector3Int(current.x, current.y, current.z + 1)) && !checkclosed.Contains(new Vector3Int(current.x, current.y, current.z + 1)))
                    checkopen.Add(new Vector3Int(current.x, current.y, current.z + 1));
            }

            //check south
            if ((!grid[current.x, current.y, current.z].wallsouth || grid[current.x, current.y, current.z].entrancesouth) && current.z - 1 >= 0) {
                if (!checkopen.Contains(new Vector3Int(current.x, current.y, current.z - 1)) && !checkclosed.Contains(new Vector3Int(current.x, current.y, current.z - 1)))
                    checkopen.Add(new Vector3Int(current.x, current.y, current.z - 1));
            }

            //===
            //check up
            if (grid[current.x, current.y, current.z].goesup && current.y + 1 < floors) {
                if (!checkopen.Contains(new Vector3Int(current.x, current.y + 1, current.z)) && !checkclosed.Contains(new Vector3Int(current.x, current.y + 1, current.z)))
                    checkopen.Add(new Vector3Int(current.x, current.y + 1, current.z));
            }

            //check down
            if (grid[current.x, current.y, current.z].goesdown && current.y - 1 >= 0) {
                if (!checkopen.Contains(new Vector3Int(current.x, current.y - 1, current.z)) && !checkclosed.Contains(new Vector3Int(current.x, current.y - 1, current.z)))
                    checkopen.Add(new Vector3Int(current.x, current.y - 1, current.z));
            }

            if (checkopen.Count <= 0)
                checking = false;
        }
    }

    void PrepareHalls() {
        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (grid[x, f, y].floor && grid[x, f, y].roomID <= 0) {
                        int[] walls = new int[] {
                        grid[x, f, y].entrancenorth ? 2 : (grid[x, f, y].wallnorth ? 1 : 0),
                        grid[x, f, y].entranceeast ? 2 : (grid[x, f, y].walleast ? 1 : 0),
                        grid[x, f, y].entrancesouth ? 2 : (grid[x, f, y].wallsouth ? 1 : 0),
                        grid[x, f, y].entrancewest ? 2 : (grid[x, f, y].wallwest ? 1 : 0)
                    };

                        GameObject baseObj = new GameObject("base");
                        baseObj.transform.position = new Vector3(x * scale.x, f * scale.y, y * scale.z);
                        baseObj.transform.SetParent(transform);
                        baseObj.isStatic = true;

                        MeshFilter filter = baseObj.AddComponent<MeshFilter>();
                        MeshCollider collider = baseObj.AddComponent<MeshCollider>();
                        MeshRenderer render = baseObj.AddComponent<MeshRenderer>();
                        render.material = allTextureMaterial;

                        List<CombineInstance> combine = new List<CombineInstance>();

                        CombineInstance CIfloor = new CombineInstance();
                        CIfloor.mesh = Floor.GetComponent<MeshFilter>().sharedMesh;
                        CIfloor.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                        combine.Add(CIfloor);

                        CombineInstance CIceiling = new CombineInstance();
                        CIceiling.mesh = Ceiling.transform.GetComponent<MeshFilter>().sharedMesh;
                        CIceiling.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                        combine.Add(CIceiling);

                        for (int i = 0; i < 4; i++) {
                            if (walls[i] > 0) {
                                CombineInstance CIwall = new CombineInstance();
                                CIwall.mesh = walls[i] == 1 ? Wall.GetComponent<MeshFilter>().sharedMesh : Entrance.GetComponent<MeshFilter>().sharedMesh;
                                CIwall.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, (i - 1) * 90, 0), Vector3.one);
                                combine.Add(CIwall);
                            }
                        }

                        Mesh mesh = new Mesh();
                        mesh.CombineMeshes(combine.ToArray());
                        filter.mesh = mesh;

                        collider.sharedMesh = mesh;

                        staticRooms.Add(baseObj);
                    }
                }
            }
        }
    }

    void CleanDeadEnds() {
        for (int i = 0; i < cleanDeadEndTries; i++) {
            for (int f = 0; f < floors; f++) {
                for (int y = 0; y < gridSize; y++) {
                    for (int x = 0; x < gridSize; x++) {
                        if (!grid[x, f, y].floor || grid[x, f, y].roomID > 0)
                            continue;

                        int wallAmount = 0;
                        int direction = 0;

                        if (grid[x, f, y].walleast && !grid[x, f, y].entranceeast)
                            wallAmount++;

                        if (grid[x, f, y].wallwest && !grid[x, f, y].entrancewest)
                            wallAmount++;

                        if (grid[x, f, y].wallnorth && !grid[x, f, y].entrancenorth)
                            wallAmount++;

                        if (grid[x, f, y].wallsouth && !grid[x, f, y].entrancesouth)
                            wallAmount++;

                        bool hasEntrance = grid[x, f, y].entrancesouth || grid[x, f, y].entrancenorth || grid[x, f, y].entrancewest || grid[x, f, y].entranceeast;

                        if (wallAmount >= 3 && !hasEntrance) {
                            direction = !grid[x, f, y].walleast ? 1 : (!grid[x, f, y].wallwest ? 2 : (!grid[x, f, y].wallnorth ? 3 : (!grid[x, f, y].wallsouth ? 4 : 0)));

                            grid[x, f, y].floor = false;
                            grid[x, f, y].walleast = false;
                            grid[x, f, y].wallwest = false;
                            grid[x, f, y].wallnorth = false;
                            grid[x, f, y].wallsouth = false;
                        }

                        if (direction == 1 && x + 1 < gridSize && grid[x + 1, f, y].floor && grid[x + 1, f, y].roomID <= 0) {
                            if (!grid[x + 1, f, y].wallwest)
                                grid[x + 1, f, y].wallwest = true;
                        }

                        if (direction == 2 && x - 1 >= 0 && grid[x - 1, f, y].floor && grid[x - 1, f, y].roomID <= 0) {
                            if (!grid[x - 1, f, y].walleast)
                                grid[x - 1, f, y].walleast = true;
                        }

                        if (direction == 3 && y + 1 < gridSize && grid[x, f, y + 1].floor && grid[x, f, y + 1].roomID <= 0) {
                            if (!grid[x, f, y + 1].wallsouth)
                                grid[x, f, y + 1].wallsouth = true;
                        }

                        if (direction == 4 && y - 1 >= 0 && grid[x, f, y - 1].floor && grid[x, f, y - 1].roomID <= 0) {
                            if (!grid[x, f, y - 1].wallnorth)
                                grid[x, f, y - 1].wallnorth = true;
                        }
                    }
                }
            }
        }
    }

    void BreakWall(Vector3Int current, Vector3Int neighbour) {
        Vector3Int dif = neighbour - current;

        bool up = dif.x == 0 && dif.z == 1;
        bool down = dif.x == 0 && dif.z == -1;
        bool left = dif.x == -1 && dif.z == 0;
        bool right = dif.x == 1 && dif.z == 0;

        if (up) {
            grid[current.x, current.y, current.z].wallnorth = false;
            grid[neighbour.x, neighbour.y, neighbour.z].wallsouth = false;
        } else if (down) {
            grid[current.x, current.y, current.z].wallsouth = false;
            grid[neighbour.x, neighbour.y, neighbour.z].wallnorth = false;
        } else if (left) {
            grid[current.x, current.y, current.z].wallwest = false;
            grid[neighbour.x, neighbour.y, neighbour.z].walleast = false;
        } else if (right) {
            grid[current.x, current.y, current.z].walleast = false;
            grid[neighbour.x, neighbour.y, neighbour.z].wallwest = false;
        }
    }

    void CreateMaze() {
    //void Update(){
        while (visited < maxElementSize) {

            if (buildingMaze) {
                List<Vector3Int> possibleChecks = new List<Vector3Int>();

                if (currentCheck.x - 1 >= 0 && grid[currentCheck.x - 1, currentCheck.y, currentCheck.z].roomID == -1)
                    possibleChecks.Add(new Vector3Int(-1, 0, 0));

                if (currentCheck.x + 1 < gridSize && grid[currentCheck.x + 1, currentCheck.y, currentCheck.z].roomID == -1)
                    possibleChecks.Add(new Vector3Int(1, 0, 0));

                if (currentCheck.z - 1 >= 0 && grid[currentCheck.x, currentCheck.y, currentCheck.z - 1].roomID == -1)
                    possibleChecks.Add(new Vector3Int(0, 0, -1));

                if (currentCheck.z + 1 < gridSize && grid[currentCheck.x, currentCheck.y, currentCheck.z + 1].roomID == -1)
                    possibleChecks.Add(new Vector3Int(0, 0, 1));

                if (possibleChecks.Count <= 0) {
                    if (backup < 0) {
                        grid[currentCheck.x, currentCheck.y, currentCheck.z].floor = false;
                        visited += 1;
                        //continue;
                        return;
                    }

                    if (backup > 0)
                        currentCheck = closedList[backup];

                    backup--;

                    if (backup < 0)
                        buildingMaze = false;
                } else {
                    
                    Element currentElement = grid[currentCheck.x, currentCheck.y, currentCheck.z];
                    Vector3Int neighbourcheck = currentCheck + possibleChecks[Random.Range(0, possibleChecks.Count)];
                    Element neighbourElement = grid[neighbourcheck.x, neighbourcheck.y, neighbourcheck.z];

                    if (neighbourElement.roomID == -1 && currentElement.roomID == 0) {

                        BreakWall(currentCheck, neighbourcheck);

                        grid[neighbourcheck.x, neighbourcheck.y, neighbourcheck.z].roomID = 0;
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
                currentCheck = new Vector3Int(Random.Range(0, gridSize), Random.Range(0, floors), Random.Range(0, gridSize));
                if (grid[currentCheck.x, currentCheck.y, currentCheck.z].floor && grid[currentCheck.x, currentCheck.y, currentCheck.z].roomID == -1) {
                    visited += 1;
                    grid[currentCheck.x, currentCheck.y, currentCheck.z].roomID = 0;
                    buildingMaze = true;
                }
            }
        }
    }

    void FillRemaining() {
        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (grid[x, f, y].floor)
                        continue;

                    grid[x, f, y] = Element.allwalls;
                }
            }
        }
    }

    void CheckEntrances(bool breakWalls) {
        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    if (!grid[x, f, y].floor)
                        continue;

                    if (breakWalls) {

                        List<int> changedRoomID = new List<int>();

                        if (changedRoomID.Contains(grid[x, f, y].roomID))
                            continue;

                        if (grid[x, f, y].entranceposiblilitynorth && y + 1 < gridSize) {
                            if (grid[x, f, y + 1].roomID == 0 ? grid[x, f, y + 1].wallsouth : grid[x, f, y + 1].entranceposiblilitysouth) {
                                if ((x + 1 < gridSize && !grid[x + 1, f, y].entrancenorth) && (x - 1 >= 0 && !grid[x - 1, f, y].entrancenorth)) {
                                    grid[x, f, y].entrancenorth = true;
                                    //===
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, true);
                                    //===
                                    grid[x, f, y + 1].entrancesouth = true;
                                    changedRoomID.Add(grid[x, f, y].roomID);

                                    if (!doorLocations.Contains(new Vector3(x, f, y + 0.5f))) {
                                        doorLocations.Add(new Vector3(x, f, y + 0.5f));
                                    }
                                }
                            }
                        }

                        if (grid[x, f, y].entranceposiblilitysouth && y - 1 >= 0) {
                            if (grid[x, f, y - 1].roomID == 0 ? grid[x, f, y - 1].wallnorth : grid[x, f, y - 1].entranceposiblilitynorth) {
                                if ((x + 1 < gridSize && !grid[x + 1, f, y].entrancesouth) && (x - 1 >= 0 && !grid[x - 1, f, y].entrancesouth)) {
                                    grid[x, f, y].entrancesouth = true;
                                    //===
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, true);
                                    //===
                                    grid[x, f, y - 1].entrancenorth = true;
                                    changedRoomID.Add(grid[x, f, y].roomID);

                                    if (!doorLocations.Contains(new Vector3(x, f, y - 0.5f))) {
                                        doorLocations.Add(new Vector3(x, f, y - 0.5f));
                                    }
                                }
                            }
                        }

                        if (grid[x, f, y].entranceposiblilityeast && x + 1 < gridSize) {
                            if (grid[x + 1, f, y].roomID == 0 ? grid[x + 1, f, y].wallwest : grid[x + 1, f, y].entranceposiblilitywest) {
                                if ((y + 1 < gridSize && !grid[x, f, y + 1].entranceeast) && (y - 1 >= 0 && !grid[x, f, y - 1].entranceeast)) {
                                    grid[x, f, y].entranceeast = true;
                                    //===
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, true);
                                    //===
                                    grid[x + 1, f, y].entrancewest = true;
                                    changedRoomID.Add(grid[x, f, y].roomID);

                                    if (!doorLocations.Contains(new Vector3(x + 0.5f, f, y))) {
                                        doorLocations.Add(new Vector3(x + 0.5f, f, y));
                                    }
                                }
                            }
                        }

                        if (grid[x, f, y].entranceposiblilitywest && x - 1 >= 0) {
                            if (grid[x - 1, f, y].roomID == 0 ? grid[x - 1, f, y].walleast : grid[x - 1, f, y].entranceposiblilityeast) {
                                if ((y + 1 < gridSize && !grid[x, f, y + 1].entrancewest) && (y - 1 >= 0 && !grid[x, f, y - 1].entrancewest)) {
                                    grid[x, f, y].entrancewest = true;
                                    //===
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, true);
                                    //===
                                    grid[x - 1, f, y].entranceeast = true;
                                    changedRoomID.Add(grid[x, f, y].roomID);

                                    if (!doorLocations.Contains(new Vector3(x - 0.5f, f, y))) {
                                        doorLocations.Add(new Vector3(x - 0.5f, f, y));
                                    }
                                }
                            }
                        }

                    }

                    if (grid[x, f, y].entrancewest) {
                        if (x - 1 >= 0) {
                            if (grid[x - 1, f, y].entranceeast) {
                                continue;
                            } else if (grid[x - 1, f, y].entranceposiblilityeast) {
                                grid[x - 1, f, y].entranceeast = true;
                                //==
                                ChangeToWall(grid[x - 1, f, y].roomID, x - 1, f, y, true);
                            } else if (grid[x - 1, f, y].walleast) {
                                if (breakWalls) {
                                    grid[x - 1, f, y].entranceeast = true;
                                    //==
                                    ChangeToWall(grid[x - 1, f, y].roomID, x - 1, f, y, true);
                                } else {
                                    grid[x, f, y].entrancewest = false;
                                    grid[x, f, y].entranceposiblilitywest = true;
                                    //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endwest"));
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                                }
                            }
                        } else {
                            grid[x, f, y].entrancewest = false;
                            grid[x, f, y].entranceposiblilitywest = true;
                            //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endwest"));
                            ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                        }
                    }

                    if (grid[x, f, y].entranceeast) {
                        if (x + 1 < gridSize) {
                            if (grid[x + 1, f, y].entrancewest) {
                                continue;
                            } else if (grid[x + 1, f, y].entranceposiblilitywest) {
                                grid[x + 1, f, y].entrancewest = true;
                                //==
                                ChangeToWall(grid[x + 1, f, y].roomID, x + 1, f, y, true);
                            } else if (grid[x + 1, f, y].wallwest) {
                                if (breakWalls) {
                                    grid[x + 1, f, y].entrancewest = true;
                                    //==
                                    ChangeToWall(grid[x + 1, f, y].roomID, x + 1, f, y, true);
                                } else {
                                    grid[x, f, y].entranceeast = false;
                                    grid[x, f, y].entranceposiblilityeast = true;
                                    //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endeast"));
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                                }
                            }
                        } else {
                            grid[x, f, y].entranceeast = false;
                            grid[x, f, y].entranceposiblilityeast = true;
                            //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endeast"));
                            ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                        }
                    }

                    if (grid[x, f, y].entrancenorth) {
                        if (y + 1 < gridSize) {
                            if (grid[x, f, y + 1].entrancesouth) {
                                continue;
                            } else if (grid[x, f, y + 1].entranceposiblilitysouth) {
                                grid[x, f, y + 1].entrancesouth = true;
                                //==
                                ChangeToWall(grid[x, f, y + 1].roomID, x, f, y + 1, true);
                            } else if (grid[x, f, y + 1].wallsouth) {
                                if (breakWalls) {
                                    grid[x, f, y + 1].entrancesouth = true;
                                    //==
                                    ChangeToWall(grid[x, f, y + 1].roomID, x, f, y + 1, true);
                                } else {
                                    grid[x, f, y].entrancenorth = false;
                                    grid[x, f, y].entranceposiblilitynorth = true;
                                    //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endnorth"));
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                                }
                            }
                        } else {
                            grid[x, f, y].entrancenorth = false;
                            grid[x, f, y].entranceposiblilitynorth = true;
                            //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endnorth"));
                            ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                        }
                    }

                    if (grid[x, f, y].entrancesouth) {
                        if (y - 1 >= 0) {
                            if (grid[x, f, y - 1].entrancenorth) {
                                continue;
                            } else if (grid[x, f, y - 1].entranceposiblilitynorth) {
                                grid[x, f, y - 1].entrancenorth = true;
                                //==
                                ChangeToWall(grid[x, f, y - 1].roomID, x, f, y - 1, true);
                            } else if (grid[x, f, y - 1].wallnorth) {
                                if (breakWalls) {
                                    grid[x, f, y - 1].entrancenorth = true;
                                    //==
                                    ChangeToWall(grid[x, f, y - 1].roomID, x, f, y - 1, true);
                                } else {
                                    grid[x, f, y].entrancesouth = false;
                                    grid[x, f, y].entranceposiblilitysouth = true;
                                    //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endsouth"));
                                    ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                                }
                            }
                        } else {
                            grid[x, f, y].entrancesouth = false;
                            grid[x, f, y].entranceposiblilitysouth = true;
                            //roomsToSpawn.Add(new SpawnRequest(0, new Vector3(x, 0, y), "halls/endsouth"));
                            ChangeToWall(grid[x, f, y].roomID, x, f, y, false);
                        }
                    }
                }
            }
        }
    }

    void ChangeToWall(int roomID, int x, int y, int z, bool entrance) {
        if (roomID <= 0)
            return;

        foreach (SpawnRequest request in roomsToSpawn) {
            if (request.roomID != roomID)
                continue;

            int posX = x - (int)request.position.x;
            int posY = y - (int)request.position.y;
            int posZ = z - (int)request.position.z;

            int index = (posY * RoomGrid.size * RoomGrid.size) + (posZ * RoomGrid.size) + posX;

            if (entrance) {
                request.changeToEntrance.Add(index);
            } else {
                request.changeToWall.Add(index);
            }
        }
    }

    void Spawn() {
        foreach(SpawnRequest request in roomsToSpawn) {
            GameObject go;
            if (request.itemobject == null) {
                go = Instantiate(Resources.Load<GameObject>(request.itemname));
            } else {
                go = Instantiate(request.itemobject);
            }
            go.transform.position = new Vector3(request.position.x * scale.x, request.position.y * scale.y, request.position.z * scale.z);
            go.transform.SetParent(transform);

            RoomGrid room = go.GetComponent<RoomGrid>();

            GameObject wallstuff = go.transform.GetChild(1).gameObject;

            foreach (int i in request.changeToEntrance) {
                Vector3 pos = room.entranceObjects[i].transform.position;
                Quaternion rot = room.entranceObjects[i].transform.rotation;

                DestroyImmediate(room.entranceObjects[i]);

                GameObject entrance = Instantiate(Resources.Load<GameObject>("halls/entrance"));
                entrance.transform.position = pos;
                entrance.transform.rotation = rot;
                entrance.transform.SetParent(wallstuff.transform);
            }

            foreach (int i in request.changeToWall) {
                Vector3 pos = room.entranceObjects[i].transform.position;
                Quaternion rot = room.entranceObjects[i].transform.rotation;

                DestroyImmediate(room.entranceObjects[i]);

                GameObject entrance = Instantiate(Resources.Load<GameObject>("halls/wall"));
                entrance.transform.position = pos;
                entrance.transform.rotation = rot;
                entrance.transform.SetParent(wallstuff.transform);
            }

            MeshFilter meshFilter = wallstuff.AddComponent<MeshFilter>();
            MeshCollider collider = wallstuff.AddComponent<MeshCollider>();
            MeshRenderer render = wallstuff.AddComponent<MeshRenderer>();
            render.material = allTextureMaterial;

            List<CombineInstance> combine = new List<CombineInstance>();

            foreach (var meshfilter in wallstuff.GetComponentsInChildren<MeshFilter>()) {
                if (meshfilter == null || meshfilter.sharedMesh == null)
                    continue;

                CombineInstance instance = new CombineInstance();
                instance.mesh = meshfilter.sharedMesh;

                Vector3 pos = meshfilter.transform.localPosition;
                if (meshfilter.transform.parent.gameObject != wallstuff) {
                    pos = meshfilter.transform.parent.localPosition;
                }

                instance.transform = Matrix4x4.TRS(pos, meshfilter.transform.localRotation, meshfilter.transform.localScale);
                combine.Add(instance);
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray());
            meshFilter.mesh = mesh;

            collider.sharedMesh = mesh;

            foreach (Transform child in wallstuff.transform) {
                Destroy(child.gameObject);
            }

            staticRooms.Add(wallstuff);
        } 
    }

    void GenerateRooms() {
        if (spawnRoomTries > 0) {
            for (int i = 0; i < spawnRoomTries; i++) {

                int x2 = Random.Range(0, gridSize);
                int y2 = Random.Range(0, floors);
                int z2 = Random.Range(0, gridSize);

                GameObject floor;

                if ((floors >= 1 && y2 < floors - 1)) {
                    floor = listOfAllRooms[Random.Range(0, listOfAllRooms.Count)];
                }else{
                    floor = listOfFloorRooms[Random.Range(0, listOfFloorRooms.Count)];
                }

                AddRoom(floor, new Vector3(x2, y2, z2));
            }
        } else {
            for (int f = 0; f < floors; f++) {
                for (int y = 0; y < gridSize; y++) {
                    for (int x = 0; x < gridSize; x++) {

                        int x2 = x + Random.Range(0, 3);
                        int y2 = y + Random.Range(0, 3);
                        int f2 = f;

                        AddRoom((floors >= 1 && f2 < floors - 1) ? listOfFloorRooms[Random.Range(0, listOfFloorRooms.Count)] : listOfAllRooms[Random.Range(0, listOfAllRooms.Count)], new Vector3(x == 0 ? 0 : x2, f2, y == 0 ? 0 : y2));
                    }
                }
            }
        }
    }

    void AddRoom(GameObject go, Vector3 position) {
        RoomGrid room = go.GetComponent<RoomGrid>();

        Element[,,] old = (Element[,,])grid.Clone();

        int roomsize = 0;

        Vector3Int spot  = Vector3Int.zero;
        Vector3Int spot2 = Vector3Int.zero;

        //for (int f = 0; f < (room.secondFloor ? 2 : 1); f++) {
        for (int y = 0; y < RoomGrid.size; y++) {
            for (int x = 0; x < RoomGrid.size; x++) {
                if (room.grid[(y * RoomGrid.size) + x] > 0) {

                    if ((int)position.x + x >= gridSize || (int)position.z + y >= gridSize) { // if the room is going off the edges, dont put it there
                        grid = old;
                        return;
                    }

                    if (grid[(int)position.x + x, (int)position.y, (int)position.z + y].floor) { //if the room is overlapping, dont put it there
                        grid = old;
                        return;
                    }

                    Element element = new Element() {
                        roomID = currentSpawnID,

                        floor = room.grid[(y * RoomGrid.size) + x] > 0,

                        wallwest = (int)position.x + x - 1 < 0 ? true : (x - 1 < 0 ? true : (room.grid[(y * RoomGrid.size) + (x - 1)] > 0 ? false : true)),
                        walleast = (int)position.x + x + 1 >= gridSize ? true : (x + 1 >= RoomGrid.size ? true : (room.grid[(y * RoomGrid.size) + (x + 1)] > 0 ? false : true)),
                        wallsouth = (int)position.z + y - 1 < 0 ? true : (y - 1 < 0 ? true : (room.grid[((y - 1) * RoomGrid.size) + x] > 0 ? false : true)),
                        wallnorth = (int)position.z + y + 1 >= gridSize ? true : (y + 1 >= RoomGrid.size ? true : (room.grid[((y + 1) * RoomGrid.size) + x] > 0 ? false : true)),

                        entrancewest = room.grid[(y * RoomGrid.size) + x] == 2,
                        entranceeast = room.grid[(y * RoomGrid.size) + x] == 4,
                        entrancesouth = room.grid[(y * RoomGrid.size) + x] == 3,
                        entrancenorth = room.grid[(y * RoomGrid.size) + x] == 5,

                        entranceposiblilitywest = room.grid[(y * RoomGrid.size) + x] == 6,
                        entranceposiblilityeast = room.grid[(y * RoomGrid.size) + x] == 8,
                        entranceposiblilitysouth = room.grid[(y * RoomGrid.size) + x] == 7,
                        entranceposiblilitynorth = room.grid[(y * RoomGrid.size) + x] == 9,

                        goesup = room.grid[(y * RoomGrid.size) + x] == 10
                    };

                    if(element.entranceeast || element.entrancenorth || element.entrancesouth || element.entrancewest || element.entranceposiblilityeast || element.entranceposiblilitynorth || element.entranceposiblilitysouth || element.entranceposiblilitywest) {
                        Vector3Int entrancekey = new Vector3Int((int)position.x + x, (int)position.y, (int)position.z + y);
                        if (!roomEntrances.ContainsKey(entrancekey))
                            roomEntrances.Add(entrancekey, (y * RoomGrid.size) + x);
                    }

                    if (Random.Range(0, 10) == 0) {
                        spot = new Vector3Int(x, 0, y);
                    }
                    if (Random.Range(0, 10) == 0) {
                        spot2 = new Vector3Int(x, 0, y);
                    }

                    grid[(int)position.x + x, (int)position.y, (int)position.z + y] = element;
                    roomsize++;
                }

                if (room.secondFloor && room.gridsecond[(y * RoomGrid.size) + x] > 0) {
                    int height = (int)position.y + 1;

                    if ((int)position.x + x >= gridSize || (int)position.z + y >= gridSize) { // if the room is going off the edges, dont put it there
                        grid = old;
                        return;
                    }

                    if(height >= floors) {
                        grid = old;
                        return;
                    }

                    if (grid[(int)position.x + x, height, (int)position.z + y].floor) { //if the room is overlapping, dont put it there
                        grid = old;
                        return;
                    }

                    Element element = new Element() {
                        roomID = currentSpawnID,

                        floor = room.gridsecond[(y * RoomGrid.size) + x] > 0,

                        wallwest = (int)position.x + x - 1 < 0 ? true : (x - 1 < 0 ? true : (room.gridsecond[(y * RoomGrid.size) + (x - 1)] > 0 ? false : true)),
                        walleast = (int)position.x + x + 1 >= gridSize ? true : (x + 1 >= RoomGrid.size ? true : (room.gridsecond[(y * RoomGrid.size) + (x + 1)] > 0 ? false : true)),
                        wallsouth = (int)position.z + y - 1 < 0 ? true : (y - 1 < 0 ? true : (room.gridsecond[((y - 1) * RoomGrid.size) + x] > 0 ? false : true)),
                        wallnorth = (int)position.z + y + 1 >= gridSize ? true : (y + 1 >= RoomGrid.size ? true : (room.gridsecond[((y + 1) * RoomGrid.size) + x] > 0 ? false : true)),

                        entrancewest = room.gridsecond[(y * RoomGrid.size) + x] == 2,
                        entranceeast = room.gridsecond[(y * RoomGrid.size) + x] == 4,
                        entrancesouth = room.gridsecond[(y * RoomGrid.size) + x] == 3,
                        entrancenorth = room.gridsecond[(y * RoomGrid.size) + x] == 5,

                        entranceposiblilitywest = room.gridsecond[(y * RoomGrid.size) + x] == 6,
                        entranceposiblilityeast = room.gridsecond[(y * RoomGrid.size) + x] == 8,
                        entranceposiblilitysouth = room.gridsecond[(y * RoomGrid.size) + x] == 7,
                        entranceposiblilitynorth = room.gridsecond[(y * RoomGrid.size) + x] == 9,

                        goesdown = room.grid[(y * RoomGrid.size) + x] == 10
                    };

                    if (element.entranceeast || element.entrancenorth || element.entrancesouth || element.entrancewest || element.entranceposiblilityeast || element.entranceposiblilitynorth || element.entranceposiblilitysouth || element.entranceposiblilitywest) {
                        Vector3Int entrancekey = new Vector3Int((int)position.x + x, (int)position.y + 1, (int)position.z + y);
                        if(!roomEntrances.ContainsKey(entrancekey))
                            roomEntrances.Add(entrancekey, (RoomGrid.size * RoomGrid.size) + (y * RoomGrid.size) + x);
                    }

                    if (Random.Range(0, 10) == 0) {
                        spot = new Vector3Int(x, 0, y);
                    }
                    if (Random.Range(0, 10) == 0) {
                        spot2 = new Vector3Int(x, 0, y);
                    }

                    grid[(int)position.x + x, height, (int)position.z + y] = element;
                    roomsize++;
                }
            }
        }
        //}
        visited += roomsize;

        if (currentSpawnID == 1) {
            playerManager.playerOneSpawn.position = new Vector3(((int)position.x + spot.x) * scale.x, ((int)position.y + spot.y) * scale.y, ((int)position.z + spot.z) * scale.z);
            playerManager.playerTwoSpawn.position = new Vector3(((int)position.x + spot2.x) * scale.x, ((int)position.y + spot2.y) * scale.y, ((int)position.z + spot2.z) * scale.z);
        }

        Vector3 key = new Vector3(position.x, position.y, position.z);

        SpawnRequest request = new SpawnRequest(currentSpawnID, key, go);

        roomsToSpawn.Add(request);

        currentSpawnID++;
    }

    private void OnDrawGizmosSelected() {
        if (grid == null)
            return;

        for (int f = 0; f < floors; f++) {
            for (int y = 0; y < gridSize; y++) {
                for (int x = 0; x < gridSize; x++) {
                    Color current = grid[x, f, y].roomID > 0 ? Color.white : Color.gray;

                    if (grid[x, f, y].floor) {
                        //Gizmos.color = grid[x, y].roomID <= 0 ? (grid[x, y].roomID == 0 ? Color.green : Color.black) : current;
                        Gizmos.color = checkclosed.Contains(new Vector3Int(x, f, y)) ? Color.green : current;
                        Gizmos.DrawCube(new Vector3(x, (f * 5) + 0, y - 100), new Vector3(0.7f, 0.1f, 0.7f));
                    }

                    
                    if (grid[x, f, y].wallnorth) {
                        Gizmos.color = grid[x, f, y].entrancenorth ? Color.red : (grid[x, f, y].entranceposiblilitynorth ? Color.yellow : current);
                        Gizmos.DrawCube(new Vector3(x, (f * 5) + 1, y + 0.4f - 100), new Vector3(0.7f, 2f, 0.1f));
                    }

                    if (grid[x, f, y].walleast) {
                        Gizmos.color = grid[x, f, y].entranceeast ? Color.red : (grid[x, f, y].entranceposiblilityeast ? Color.yellow : current);
                        Gizmos.DrawCube(new Vector3(x + 0.4f, (f * 5) + 1, y - 100), new Vector3(0.1f, 2f, 0.7f));
                    }

                    if (grid[x, f, y].wallsouth) {
                        Gizmos.color = grid[x, f, y].entrancesouth ? Color.red : (grid[x, f, y].entranceposiblilitysouth ? Color.yellow : current);
                        Gizmos.DrawCube(new Vector3(x, (f * 5) + 1, y - 0.4f - 100), new Vector3(0.7f, 2f, 0.1f));
                    }

                    if (grid[x, f, y].wallwest) {
                        Gizmos.color = grid[x, f, y].entrancewest ? Color.red : (grid[x, f, y].entranceposiblilitywest ? Color.yellow : current);
                        Gizmos.DrawCube(new Vector3(x - 0.4f, (f * 5) + 1, y - 100), new Vector3(0.1f, 2f, 0.7f));
                    }
                    
                }
            }
        }
    }
}

public struct SpawnRequest {
    public int roomID;
    public List<int> changeToWall;
    public List<int> changeToEntrance;

    public Vector3 position;
    public string itemname;
    public GameObject itemobject;

    public SpawnRequest(int id, Vector3 v, string s) {
        roomID = id;

        position = v;
        itemname = s;
        itemobject = null;

        changeToWall = new List<int>();
        changeToEntrance = new List<int>();
    }

    public SpawnRequest(int id, Vector3 v, GameObject g) {
        roomID = id;

        position = v;
        itemname = "";
        itemobject = g;

        changeToWall = new List<int>();
        changeToEntrance = new List<int>();
    }
}

[SerializeField]
public struct Element {
    public static Element justFloor = new Element() { roomID = -1, floor = true };
    public static Element allwalls = new Element() { roomID = -1, floor = true, walleast = true, wallnorth = true, wallsouth = true, wallwest = true };

    public int roomID; // 0 is hallway
    public bool floor;

    public bool goesup;
    public bool goesdown;

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

        goesup = false;
        goesdown = false;

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
