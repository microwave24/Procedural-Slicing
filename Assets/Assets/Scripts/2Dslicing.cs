using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public List<int> tri = new List<int>();
    void Start()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        getIntersectionsInTriangles(mesh.triangles);
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

    bool IsWithinSegmentBounds(Vector3 p1, Vector3 p2, Vector3 point)
    {
        // Check if point's x is within the bounds of the segment defined by p1 and p2
        bool withinXBounds = (point.x >= p1.x && point.x <= p2.x) || (point.x >= p2.x && point.x <= p1.x);
        

        // Check if point's y is within the bounds of the segment defined by p1 and p2
        bool withinYBounds = (point.y >= p1.y && point.y <= p2.y) || (point.y >= p2.y && point.y <= p1.y);
        // Return true if the point is within both the x and y bounds
        return withinXBounds == true && withinYBounds == true;
        
    }

    Vector3 getIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        (float m1, float c1) = getLine(p1, p2);
        (float m2, float c2) = getLine(p3, p4);
        float x = (c2 - c1) / (m1 - m2);
        float y = m1 * x + c1;

        Vector3 intersection = new Vector3(x, y, 0);
        // Check if the intersection is within the bounds of the segments
        if (IsWithinSegmentBounds(p1, p2, intersection))
        {
            
            return intersection;
        }
        else
        {
            return Vector3.zero;  // Return zero if no valid intersection
        }
    }

    int getIntersectionsInTriangles(int[] triangles)
    {
        int intersectionCount = 0;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            if(getIntersection(point1.position, point2.position, v1, v2) != Vector3.zero)
            {
                var z = Instantiate(intersection.gameObject, getIntersection(point1.position, point2.position, v1, v2), Quaternion.identity);
                intersectionCount++;
            }
            if(getIntersection(point1.position, point2.position, v2, v3) != Vector3.zero)
            {
                var z = Instantiate(intersection.gameObject, getIntersection(point1.position, point2.position, v2, v3), Quaternion.identity);
                intersectionCount++;
            }
            if(getIntersection(point1.position, point2.position, v3, v1) != Vector3.zero)
            {
                var z = Instantiate(intersection.gameObject, getIntersection(point1.position, point2.position, v3, v1), Quaternion.identity);
                intersectionCount++;
            }
        }
        return intersectionCount;
    }

    void Update()
    {
        //print(getIntersectionsInTriangles(mesh.triangles));
        tri = mesh.triangles.ToList<int>();
    }


}