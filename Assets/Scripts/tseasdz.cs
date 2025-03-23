using System.Collections.Generic;
using UnityEngine;

public class tseasdz : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        print("Number of vertices: " + vertices.Length);

        int[] triangles = mesh.triangles;
        Debug.Log("Number of triangles: " + (triangles.Length / 3));

        HashSet<Vector3> uniqueVertices = new HashSet<Vector3>(vertices);
        Debug.Log("Number of unique vertices: " + uniqueVertices.Count);
    }
}
