using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneratorGrid : MonoBehaviour {

    public static WorldGeneratorGrid instance;

    public Room beginBlock;

    public int maxIterations;
    public int iterations;

    public List<GameObject> endPieces = new List<GameObject>();
    public List<GameObject> pieces = new List<GameObject>();

    GameObject veryLastPiece; 

    List<Room> todo = new List<Room>();
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
        foreach (GameObject go in Resources.LoadAll<GameObject>("rooms")) {
            Room b = go.GetComponent<Room>();
            if (b.isTunnel) {
                if (b.isEnd) {
                    endPieces.Add(go);
                }else {
                    pieces.Add(go);
                }
            }else {
                if (b.isEnd) {
                    endPieces.Add(go);
                    veryLastPiece = go;
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

            if (timer >= 3.1f) {
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

        Room room = trans.parent.GetComponent<Room>();

        bool inIteration = iterations < maxIterations;
        int max = inIteration ? pieces.Count : endPieces.Count;

        int index = Random.Range(0, max);

        //print(tries);

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

        Room roo = objectToSpawn.GetComponent<Room>();

        int ind = 0;

        if (roo.entrances.Length > 0) {
            ind = Random.Range(0, roo.entrances.Length);

            Transform connection = roo.entrances[ind];

            objectToSpawn.transform.eulerAngles = (trans.eulerAngles - connection.eulerAngles) + new Vector3(0, 180, 0);

            objectToSpawn.transform.position = trans.position + (objectToSpawn.transform.position - connection.position);

        }  

        objectToSpawn.SetActive(false);

        bool isColliding = Physics.CheckBox(roo.centerTransform.position, roo.bounds.extents - (Vector3.one), Quaternion.Euler(objectToSpawn.transform.eulerAngles));


        objectToSpawn.SetActive(true);
        //position = roo.centerTransform.position;
        //size = roo.bounds.extents * 2 - (Vector3.one);
        //euler = objectToSpawn.transform.eulerAngles;

        print(isColliding);

        if (isColliding) {
            Destroy(objectToSpawn);
            tries++;
            return;
        }else {
            roo.entrances[ind] = null;

            foreach (Transform t in roo.entrances) {
                if (t == null)
                    continue;

                RaycastHit[] colliderHitssss = Physics.BoxCastAll(t.position + (t.forward * 2), Vector3.one * 1.95f, t.forward * 2);

                position = t.position + (t.forward * 2);
                size = Vector3.one * 3.9f;

                foreach (RaycastHit hit in colliderHitssss) {
                    if (hit.collider.GetComponent<Wall>()) {
                        if (hit.collider.GetComponent<Wall>().canBeEntrance) {
                            Vector3 pos = hit.collider.transform.position;
                            Vector3 eul = hit.collider.transform.eulerAngles;

                            print(hit.collider.name);
                            print(pos);

                            GameObject g = Instantiate(t.parent.gameObject);

                            g.transform.position = pos;
                            g.transform.eulerAngles = eul;
                        }
                    }
                }
            }
        }

        //objectToSpawn.layer = LayerMask.NameToLayer("Room");

        todolist.Remove(trans);
        //Destroy(trans.gameObject);

        foreach (Transform t in roo.entrances) {
            if (t != null)
                todolist.Add(t);
        }

        //previousPiece = objectToSpawn;

        tries = 0;

        iterations++;
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow - new Color(0, 0, 0, 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.Euler(euler), size);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
