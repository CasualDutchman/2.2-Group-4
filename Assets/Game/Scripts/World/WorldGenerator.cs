using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

    public static WorldGenerator instance;

    public Block beginBlock;

    public int maxIterations;
    public int iterations;

    public List<GameObject> endPieces = new List<GameObject>();
    public List<GameObject> pieces = new List<GameObject>();

    GameObject veryLastPiece; 

    List<Block> todo = new List<Block>();
    List<GameObject> added = new List<GameObject>();
    bool generating = true;

    List<Transform> todolist = new List<Transform>();

    float timer;

    Vector3 position = Vector3.zero;
    Vector3 size = Vector3.one * 7;
    Vector3 euler = Vector3.zero;

    int tries = 0;

    GameObject previousPiece;

    void Awake() {
        instance = this;
    }

    void Start () {
        foreach (GameObject go in Resources.LoadAll<GameObject>("generation")) {
            Block b = go.GetComponent<Block>();
            if (b.isTunnel) {
                if (b.isEnd) {
                    endPieces.Add(go);
                    veryLastPiece = go;
                }else {
                    pieces.Add(go);
                }
            }else {
                if (b.isEnd) {
                    endPieces.Add(go);
                } else {
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

            if (timer >= 0.1f) {
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

        bool inIteration = iterations < maxIterations;
        int max = inIteration ? pieces.Count : endPieces.Count;

        int index = Random.Range(0, max);

        print(tries);

        GameObject objectToSpawn = null;
        if (inIteration && tries < max) {
            objectToSpawn = Instantiate(pieces[index]);
        } else {
            if (tries < max) {
                objectToSpawn = Instantiate(endPieces[index]);
            }else {
                if (tries > max + (max / 2)) {
                    objectToSpawn = Instantiate(veryLastPiece);
                } else {
                    objectToSpawn = Instantiate(endPieces[Random.Range(0, endPieces.Count)]);
                }
            }
        }

        if (previousPiece != null) {
            if (previousPiece.name == objectToSpawn.name) {
                Destroy(objectToSpawn);
                tries++;
                return;
            }else {
                previousPiece = null;
            }
        }

        objectToSpawn.transform.SetParent(trans.parent);

        Block b = objectToSpawn.GetComponent<Block>();

        int ind = 0;

        if (b.entrances.Length > 0) {
            ind = Random.Range(0, b.entrances.Length);

            Transform connection = b.entrances[ind];

            objectToSpawn.transform.eulerAngles = (trans.eulerAngles - connection.eulerAngles) + new Vector3(0, 180, 0);

            objectToSpawn.transform.position = trans.position + (objectToSpawn.transform.position - connection.position);
        }

        MeshCollider col = objectToSpawn.GetComponent<MeshCollider>();

        Bounds bounds = col.bounds;

        //RaycastHit[] colliderHitssss = Physics.BoxCastAll(col.bounds.center, col.bounds.size - (Vector3.one * 0.05f), trans.forward);
        bool isColliding = Physics.CheckBox(bounds.center, bounds.extents - (Vector3.one * 0.05f), Quaternion.Euler(objectToSpawn.transform.eulerAngles), LayerMask.GetMask("Room"));

        if (isColliding) {
            Destroy(objectToSpawn);
            tries++;
            return;
        }else {
            b.entrances[ind] = null;
        }

        objectToSpawn.layer = LayerMask.NameToLayer("Room");

        position = col.bounds.center;
        size = col.bounds.size - (Vector3.one * 0.05f);
        euler = objectToSpawn.transform.eulerAngles;

        todolist.Remove(trans);
        Destroy(trans.gameObject);

        foreach (Transform t in b.entrances) {
            if (t != null)
                todolist.Add(t);
        }

        previousPiece = objectToSpawn;

        tries = 0;

        iterations++;
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow - new Color(0, 0, 0, 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.Euler(euler), size);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
