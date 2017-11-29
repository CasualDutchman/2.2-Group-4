using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

    public static WorldGenerator instance;

    public Block beginBlock;

    public int maxIterations;
    public int iterations;

    public List<GameObject> rooms = new List<GameObject>();
    public List<GameObject> tunnels = new List<GameObject>();

    public List<GameObject> roomEnd = new List<GameObject>();
    public List<GameObject> tunnelEnd = new List<GameObject>();

    public List<GameObject> endPieces = new List<GameObject>();
    public List<GameObject> pieces = new List<GameObject>();

    List<Block> todo = new List<Block>();
    List<GameObject> added = new List<GameObject>();
    bool generating = true;

    List<Transform> todolist = new List<Transform>();

    float timer;

    Vector3 position = Vector3.zero;
    Vector3 size = Vector3.one * 7;
    Vector3 euler = Vector3.zero;

    void Awake() {
        instance = this;
    }

    void Start () {
        foreach (GameObject go in Resources.LoadAll<GameObject>("generation")) {
            Block b = go.GetComponent<Block>();
            if (b.isTunnel) {
                if (b.isEnd) {
                    tunnelEnd.Add(go);
                    endPieces.Add(go);
                }else {
                    tunnels.Add(go);
                    pieces.Add(go);
                }
            }else {
                if (b.isEnd) {
                    roomEnd.Add(go);
                    endPieces.Add(go);
                } else {
                    rooms.Add(go);
                    pieces.Add(go);
                }
            }
        }

        todo.Add(beginBlock);

        foreach (Transform t in beginBlock.entrances) {
            todolist.Add(t);
        }
	}
	
	void Update () {

        if (todolist.Count > 0) {
            timer += Time.deltaTime;

            if (timer >= 0.5f) {
                Generate(todolist[0]);
                timer = 0;
            }
        }
	}

    void Generate(Transform trans) {
        if (trans == null) {
            todolist.RemoveAt(0);
            return;
        }

        Block block = trans.parent.GetComponent<Block>();

        RaycastHit[] hits = Physics.BoxCastAll(trans.position + (trans.forward * 7), size - (Vector3.one * 0.1f), trans.forward * 7);
        //bool bigEnough = Physics.CheckBox(trans.position + (trans.forward * 7), Vector3.one * 7);

        position = trans.position + (trans.forward * 7);

        print(hits.Length);

        bool bigEnough = hits.Length <= 0;

        GameObject objectToSpawn = null;

        if (bigEnough && iterations < maxIterations) {
            objectToSpawn = Instantiate(pieces[Random.Range(0, pieces.Count)]);
        } else {
            objectToSpawn = Instantiate(endPieces[Random.Range(0, endPieces.Count)]);
        }

        Block b = objectToSpawn.GetComponent<Block>();

        if (b.entrances.Length > 0) {
            int ind = Random.Range(0, b.entrances.Length);
            Transform connection = b.entrances[ind];

            //print(connection.name);

            objectToSpawn.transform.eulerAngles = (trans.eulerAngles - connection.eulerAngles) + new Vector3(0, 180, 0);

            objectToSpawn.transform.position = trans.position + (objectToSpawn.transform.position - connection.position);

            b.entrances[ind] = null;
        }

        todolist.Remove(trans);

        foreach (Transform t in b.entrances) {
            if(t != null)
                todolist.Add(t);
        }

        iterations++;
    }

    void Generate(Block block) {

        foreach (Transform t in block.entrances) {

            if (t == null)
                continue;

            GameObject go = null;

            bool randomBool = Random.Range(0, 4) == 0;
            int index = Random.Range(0, pieces.Count);

            /*
            GameObject objectToCheck = null;
            int index = Random.Range(0, pieces.Count);

            int entranceIndex = 0;

            Vector3 eul;
            Vector3 pos;

            List<int> indexes = new List<int>();
            for(int i = 0; i < pieces.Count; i++)
                indexes.Add(i);

            bool fits = false;

            for (int k = 0; k < pieces.Count; k++) {
                if (indexes.Count <= 0) {
                    fits = false;
                    break;
                }

                

                objectToCheck = pieces[index];

                print(objectToCheck.name);

                Block blockOfCheck = objectToCheck.GetComponent<Block>();
                int rand = Random.Range(0, blockOfCheck.entrances.Length);
                Transform entrance = blockOfCheck.entrances[rand];

                eul = (t.eulerAngles - entrance.eulerAngles) + new Vector3(0, 180, 0);

                pos = t.position - entrance.position;

                bool fit = Physics.CheckBox(pos, objectToCheck.GetComponent<MeshCollider>().bounds.extents, Quaternion.Euler(eul));

                position = pos;
                euler = eul;
                size = objectToCheck.GetComponent<MeshCollider>().bounds.extents;

                if (!fit) {
                    indexes.Remove(index);
                    index = Random.Range(0, indexes.Count);
                } else {
                    fits = true;
                    break;
                }
            }

            print(fits);

            go = Instantiate(objectToCheck);

            go.transform.SetParent(block.transform);

            Block b = go.GetComponent<Block>();
            int ind = Random.Range(0, b.entrances.Length);
            Transform connection = b.entrances[ind];

            //print(connection.name);

            go.transform.eulerAngles = (t.eulerAngles - connection.eulerAngles) + new Vector3(0, 180, 0);

            go.transform.position = t.position - connection.position;

            b.entrances[index] = null;

            Destroy(connection.gameObject);
            Destroy(t.gameObject);

            todo.Add(b);

            /*/
            bool bigEnough = Physics.CheckBox(t.position + (t.forward * 7), Vector3.one * 7);

            position = t.position + (t.forward * 7);

            print(bigEnough);

            if (randomBool || block.isTunnel) {
                if (!bigEnough) {
                    go = Instantiate(WorldGenerator.instance.tunnelEnd[Random.Range(0, WorldGenerator.instance.tunnelEnd.Count)]);
                }
                else if (WorldGenerator.instance.iterations <= WorldGenerator.instance.maxIterations) {
                    go = Instantiate(WorldGenerator.instance.rooms[Random.Range(0, WorldGenerator.instance.rooms.Count)]);
                } 
                else {
                    go = Instantiate(WorldGenerator.instance.roomEnd[Random.Range(0, WorldGenerator.instance.roomEnd.Count)]);
                }
            } else {
                if (WorldGenerator.instance.iterations <= WorldGenerator.instance.maxIterations && bigEnough) {
                    go = Instantiate(WorldGenerator.instance.tunnels[Random.Range(0, WorldGenerator.instance.tunnels.Count)]);
                } 
                else {
                    go = Instantiate(WorldGenerator.instance.tunnelEnd[Random.Range(0, WorldGenerator.instance.tunnelEnd.Count)]);
                }
            }

            go.transform.SetParent(block.transform);

            Block b = go.GetComponent<Block>();
            if (b.entrances.Length > 0) {
                int ind = Random.Range(0, b.entrances.Length);
                Transform connection = b.entrances[ind];

                //print(connection.name);

                go.transform.eulerAngles = (t.eulerAngles - connection.eulerAngles) + new Vector3(0, 180, 0);

                go.transform.position = t.position + (go.transform.position - connection.position);

                b.entrances[ind] = null;
            }
            //Destroy(connection.gameObject);
            //Destroy(t.gameObject);

            todo.Add(b);
            //*/
        }

        WorldGenerator.instance.iterations++;

        todo.Remove(block);

        block.gameObject.isStatic = true;

        added.Add(block.gameObject);

        if (todo.Count <= 0) {
            print("done");
            StaticBatchingUtility.Combine(added.ToArray(), beginBlock.gameObject);

        }

        
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.yellow - new Color(0, 0, 0, 0.5f);
        //Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.Euler(euler), size * 2);
        Gizmos.DrawCube(position, size * 2);
    }
}
