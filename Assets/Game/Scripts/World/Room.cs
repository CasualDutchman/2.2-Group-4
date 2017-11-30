using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public Transform centerTransform;
    public Bounds bounds;

    public bool isTunnel;
    public bool isEnd;

    public Transform[] entrances;

    public Vector3 boundCenter {
        get {
            return transform.TransformVector(transform.position + bounds.center);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(centerTransform.position + new Vector3(0, bounds.center.y, 0), transform.rotation, bounds.extents * 2);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
