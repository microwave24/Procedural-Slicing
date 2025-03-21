using UnityEngine;

public class tseasdz : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        int[] triangles = mesh.triangles;
        Debug.Log("Number of triangles: " + (triangles.Length / 3));
    }
}
