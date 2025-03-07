using UnityEngine;
using System;
using System.Collections.Generic;

public class Slicing2D : MonoBehaviour
{
    // 1. get points of intersection
    // 2. add verticies at points of intersection
    // 3. add triangles to create new mesh
    // 4. add UVs to new mesh
    // 5. add normals to new mesh
    // 6. render new mesh in two new game objects

    public GameObject obj;
    public Mesh mesh;
    public Vector3[] vertices;
    public Transform point1;
    public Transform point2;
    public Transform intersection;
    void Start()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
    }

    (float, float)getLine(Vector3 p1, Vector3 p2)
    {
        float m = (p2.y - p1.y) / (p2.x - p1.x);

        if(p2.x - p1.x == 0)
        {
            m = 999;
        }
        float c = p1.y - m * p1.x;
        return (m, c);
    }

    Vector3 getIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        (float m1, float c1) = getLine(p1, p2);
        (float m2, float c2) = getLine(p3, p4);
        float x = (c2 - c1) / (m1 - m2);
        float y = m1 * x + c1;
        return new Vector3(x, y, 0);
    }

    int getTotalEdges(Vector3[] verticies)
    {
        
        return 0;
    }

    void Update()
    {
        intersection.position = getIntersection(point1.position, point2.position, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        print(getTotalEdges(vertices));
    }


}
