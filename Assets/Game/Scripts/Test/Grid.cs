using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    public int gridSize = 20;

    Element[,] grid;

    Dictionary<Vector3, GameObject> roomsToSpawn = new Dictionary<Vector3, GameObject>();

    int currentSpawnID = 1;

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

        /*
        foreach(KeyValuePair<Vector3, GameObject> room in roomsToSpawn) {
            GameObject go = Instantiate(room.Value);
            go.transform.position = room.Key;
            go.transform.SetParent(transform);
        } //*/
    }

    void GenerateHallways() {

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

                if (!grid[x - 1, y].floor && !grid[x + 1, y].floor && grid[x, y - 1].floor && grid[x, y + 1].floor) {
                    if ((x - 2 >= 0 && grid[x - 2, y].floor && grid[x - 2, y].roomID == 0) || (x + 2 < gridSize && grid[x + 2, y].floor && grid[x + 2, y].roomID == 0))
                        continue;

                    grid[x, y].roomID = 0;
                    grid[x, y].floor = true;
                    continue;
                }

                /*
                if (!grid[x - 1, y].floor && grid[x + 1, y].floor && !grid[x, y - 1].floor && grid[x, y + 1].floor) {

                    grid[x, y].floor = true;
                    continue;
                }

                if (grid[x - 1, y].floor && !grid[x + 1, y].floor && grid[x, y - 1].floor && !grid[x, y + 1].floor) {

                    grid[x, y].floor = true;
                    continue;
                }
                //*/
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

    [ContextMenu("instantiate")]
    public void lol() {
        GameObject go = Resources.Load<GameObject>("rooms/Room_0" + (Random.Range(0, 2) + 1 + ""));
        GameObject go2 = Instantiate(go);
        go2.GetComponent<RoomGrid>().grid2 = go.GetComponent<RoomGrid>().grid2;
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
                    print("too big on x");
                    return;
                }

                if ((int)position.y + y >= gridSize) {
                    grid = old;
                    print("too big for y");
                    return;
                }

                if (grid[(int)position.x + x, (int)position.y + y].floor) {
                    grid = old;
                    print("already contains something");
                    return;
                }

                Element element = new Element() {
                    roomID = currentSpawnID,

                    floor = room.grid[(y * RoomGrid.size) + x],

                    wallwest = (int)position.x + x - 1 < 0 ? true : (x - 1 < 0 ? true : (room.grid[(y * RoomGrid.size) + (x - 1)] ? false : true)),
                    walleast = (int)position.x + x + 1 >= gridSize ? true : (x + 1 >= RoomGrid.size ? true : (room.grid[(y * RoomGrid.size) + (x + 1)] ? false : true)),

                    wallsouth = (int)position.y + y - 1 < 0 ? true : (y - 1 < 0 ? true : (room.grid[((y - 1) * RoomGrid.size) + x] ? false : true)),
                    wallnorth = (int)position.y + y + 1 >= gridSize ? true : (y + 1 >= RoomGrid.size ? true : (room.grid[((y + 1) * RoomGrid.size) + x] ? false : true)),
                };

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
                Gizmos.color = grid[x, y].roomID > 0 ? Color.yellow : Color.gray;

                if(grid[x, y].floor)
                    Gizmos.DrawCube(new Vector3(x, 0, y), new Vector3(0.95f, 0.1f, 0.95f));


                if (grid[x, y].wallnorth)
                    Gizmos.DrawCube(new Vector3(x, 1, y + 0.5f), new Vector3(1f, 2f, 0.1f));

                if (grid[x, y].walleast)
                    Gizmos.DrawCube(new Vector3(x + 0.5f, 1, y), new Vector3(0.1f, 2f, 1f));

                if (grid[x, y].wallsouth)
                    Gizmos.DrawCube(new Vector3(x, 1, y - 0.5f), new Vector3(1f, 2f, 0.1f));

                if (grid[x, y].wallwest)
                    Gizmos.DrawCube(new Vector3(x - 0.5f, 1, y), new Vector3(0.1f, 2f, 1f));
            }
        }
    }
}

[SerializeField]
public struct Element {
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
    }
}
