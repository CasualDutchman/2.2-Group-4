using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTest : MonoBehaviour {

    public string detectingName;

    public int amount;

    public FrustumPlanes FP;
    public Matrix4x4 m, m2;

    void Start() {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Matrix4x4.Frustum(FP));
        m = Camera.main.projectionMatrix;
        m2 = Matrix4x4.Frustum(FP);
        amount = planes.Length;
    }

	void Update () {

	}

    public bool IsVisibleFrom(Renderer renderer, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    void OnDrawGizmosSelected() {
        Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.forward, Quaternion.identity, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, 60, 20, 0.1f, 1920 / 1080);
    }
}
