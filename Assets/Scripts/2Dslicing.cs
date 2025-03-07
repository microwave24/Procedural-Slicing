using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;

public class Slicing2D : MonoBehaviour
{
   
    public GameObject obj;
    public Mesh mesh;
    public Vector3[] vertices;
    public Transform point1;
    public Transform point2;
    public GameObject point3;
    public List<Vector3> intersections = new List<Vector3>();
    void Start()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        getIntersectionPoints(point1.position, point2.position, mesh.triangles);

        for(int i = 0; i < intersections.Count; i++){
            Instantiate(point3, intersections[i], Quaternion.identity);
        }

        
    }

    Vector3 findLineIntersectionOnPlane(Vector3 v1, Vector3 v2, Vector3 planeNormal, float D){
        float A = planeNormal.x;
        float B = planeNormal.y;
        float C = planeNormal.z;
        
        float t = -(A * v1.x + B * v1.z + C * v1.y + D) / (A * (v2.x - v1.x) + B * (v2.z - v1.z) + C * (v2.y - v1.y));
        if (t < 0 || t > 1)
        {
            return Vector3.forward;
        }

        float x = v1.x + t * (v2.x - v1.x);
        float y = v1.z + t * (v2.z - v1.z); 
        float z = v1.y + t * (v2.y - v1.y); 

        return new Vector3(x, y, z);
    }

    void getIntersectionPoints(Vector3 p1, Vector3 p2, int[] triangles){
        Vector3 planeNormal = Vector3.Cross(p1- p2, Vector3.forward).normalized;
        Plane slicingPlane = new Plane(planeNormal, point1.position);
        //---- dubugging 
        Debug.DrawLine(p1, p2, Color.red, 10f);
        Vector3 planeCenter = (p1 + p2) / 2;
        Vector3 planeUp = Vector3.Cross(planeNormal, p2 - p1).normalized;
        Vector3 planeRight = (p2 - p1).normalized;

        float planeSize = 10f;
        Vector3 corner1 = planeCenter + planeRight * planeSize + planeUp * planeSize;
        Vector3 corner2 = planeCenter + planeRight * planeSize - planeUp * planeSize;
        Vector3 corner3 = planeCenter - planeRight * planeSize + planeUp * planeSize;
        Vector3 corner4 = planeCenter - planeRight * planeSize - planeUp * planeSize;

        Debug.DrawLine(corner1, corner2, Color.green, 10f);
        Debug.DrawLine(corner2, corner4, Color.green, 10f);
        Debug.DrawLine(corner4, corner3, Color.green, 10f);
        Debug.DrawLine(corner3, corner1, Color.green, 10f);
        //---- dubugging 

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Vector3 i_v1 = findLineIntersectionOnPlane(v1, v2, planeNormal, slicingPlane.distance);
            Vector3 i_v2 = findLineIntersectionOnPlane(v2, v3, planeNormal, slicingPlane.distance);
            Vector3 i_v3 = findLineIntersectionOnPlane(v3, v1, planeNormal, slicingPlane.distance);

            if(i_v1 != Vector3.forward){
                intersections.Add(i_v1);
            }
            if(i_v2 != Vector3.forward){
                intersections.Add(i_v2);
            }
            if(i_v3 != Vector3.forward){
                intersections.Add(i_v3);
            }
        }
    }


    void Update()
    {
            
    }


}