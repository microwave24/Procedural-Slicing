using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using NUnit.Framework;

public class Slicing2D : MonoBehaviour
{
   
    public GameObject obj;
    public Mesh mesh;
    public Vector3[] vertices;
    public int[] tri;
    public Transform point1;
    public Transform point2;
    public GameObject p3;
    public GameObject p4;
    public List<Vector3> intersections = new List<Vector3>();
    public List<int> intersectionsIndicies = new List<int>();
    public List<int> intersectedTriangles = new List<int>();
    private Plane slicingPlane;

    public List<Vector3> topTriangles = new List<Vector3>();
    public List<Vector3> bottomTriangles = new List<Vector3>();
    void Start()
    {
        
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        tri = mesh.triangles;
        getIntersectionPoints(point1.position, point2.position, mesh.triangles);
        splitMesh(mesh, slicingPlane);
        Destroy(obj);
    }

    

    Vector3 findLineIntersectionOnPlane(Vector3 v1, Vector3 v2, Vector3 planeNormal, float D){
        float A = planeNormal.x;
        float B = planeNormal.z;
        float C = planeNormal.y;
        
        float t = -(A * v1.x + B * v1.z + C * v1.y + D) / (A * (v2.x - v1.x) + B * (v2.z - v1.z) + C * (v2.y - v1.y));
        if (t < 0 || t > 1)
        {
            return Vector3.forward;
        }

        float x = v1.x + t * (v2.x - v1.x);
        float y = v1.z + t * (v2.z - v1.z); 
        float z = v1.y + t * (v2.y - v1.y);

        

        return new Vector3(x, z, y);
    }

    void getIntersectionPoints(Vector3 p1, Vector3 p2, int[] triangles){
        Vector3 planeNormal = Vector3.Cross(p1- p2, Vector3.forward).normalized;
        slicingPlane = new Plane(planeNormal, point1.position);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            var CurrentTriangle = new List<Vector3>();

            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            CurrentTriangle.Add(v1);
            CurrentTriangle.Add(v2);
            CurrentTriangle.Add(v3);

            Vector3 i_v1 = findLineIntersectionOnPlane(v1, v2, planeNormal, slicingPlane.distance);
            Vector3 i_v2 = findLineIntersectionOnPlane(v2, v3, planeNormal, slicingPlane.distance);
            Vector3 i_v3 = findLineIntersectionOnPlane(v3, v1, planeNormal, slicingPlane.distance);

            var localIntersects = new List<Vector3>();

            if(i_v1 != Vector3.forward){
                intersections.Add(i_v1);
                localIntersects.Add(i_v1);
            }
            if(i_v2 != Vector3.forward){
                intersections.Add(i_v2);
                localIntersects.Add(i_v2);
            }
            if(i_v3 != Vector3.forward){
                intersections.Add(i_v3);
                localIntersects.Add(i_v3);
            }

            if(i_v1 != Vector3.forward || i_v2 != Vector3.forward || i_v3 != Vector3.forward){
                intersectedTriangles.Add(i);
                intersectedTriangles.Add(i + 1);
                intersectedTriangles.Add(i + 2);
            }
            if(localIntersects.Count > 0){
                SplitTriangle(mesh, CurrentTriangle.ToArray(), localIntersects[0], localIntersects[1]);
            }
            
        }

        

    }

    bool IsPointOnEdge(Vector3 A, Vector3 B, Vector3 P)
    {
        A = new Vector3(A.x, A.y, 0);
        B = new Vector3(B.x, B.y, 0);
        P = new Vector3(P.x, P.y, 0);
       
        Vector3 edge = B - A;
        Vector3 vp = P - A;
            // Check if the point is collinear with the edge (cross product should be zero)
        float cross = Vector3.Cross(edge, vp).magnitude;
        
        if(cross < 0.00001){
            return true;
        }
        return false;
    }

    void splitMesh(Mesh mesh, Plane slicingPlane)
    {
        Debug.DrawRay(Vector3.zero, slicingPlane.normal, Color.red, 1000f);
        List<Vector2> topUVs = new List<Vector2>();
        List<Vector2> bottomUVs = new List<Vector2>();
        for(int i = 0; i < mesh.triangles.Length; i += 3){
            bool above = true;
            for(int j = 0; j < 3; j++){
                if(Vector3.Dot(slicingPlane.normal, mesh.vertices[mesh.triangles[i + j]]) > 0){
                    above = false;
                    break;
                }
            }

            if(above == true){
                topTriangles.Add(mesh.vertices[mesh.triangles[i]]);
                topTriangles.Add(mesh.vertices[mesh.triangles[i + 1]]);
                topTriangles.Add(mesh.vertices[mesh.triangles[i + 2]]);
            }
            else{
                bottomTriangles.Add(mesh.vertices[mesh.triangles[i]]);
                bottomTriangles.Add(mesh.vertices[mesh.triangles[i + 1]]);
                bottomTriangles.Add(mesh.vertices[mesh.triangles[i + 2]]);
            }

        }

        

        // Create new game objects for the top and bottom parts
        GameObject topPart = new GameObject("TopPart");
        GameObject bottomPart = new GameObject("BottomPart");

        // Add MeshFilter and MeshRenderer components to the new game objects
        topPart.AddComponent<MeshFilter>();
        topPart.AddComponent<MeshRenderer>();
        bottomPart.AddComponent<MeshFilter>();
        bottomPart.AddComponent<MeshRenderer>();

        // Create new meshes for the top and bottom parts
        Mesh topMesh = new Mesh();
        Mesh bottomMesh = new Mesh();

        // Assign vertices and triangles to the top mesh
        topMesh.vertices = topTriangles.ToArray();
        topMesh.triangles = Enumerable.Range(0, topTriangles.Count).ToArray();
        topMesh.RecalculateNormals();

        // Assign vertices and triangles to the bottom mesh
        bottomMesh.vertices = bottomTriangles.ToArray();
        bottomMesh.triangles = Enumerable.Range(0, bottomTriangles.Count).ToArray();
        bottomMesh.RecalculateNormals();

        // Assign the meshes to the MeshFilter components
        topPart.GetComponent<MeshFilter>().mesh = topMesh;
        bottomPart.GetComponent<MeshFilter>().mesh = bottomMesh;

        // Optionally, set the same material as the original object
        topPart.GetComponent<MeshRenderer>().material = obj.GetComponent<MeshRenderer>().material;
        bottomPart.GetComponent<MeshRenderer>().material = obj.GetComponent<MeshRenderer>().material;
    }
        

    void SplitTriangle(Mesh mesh, Vector3[] verts, Vector3 intersect1, Vector3 intersect2){

        // get the edge that was not intersected by the straight blade
        (Vector3, Vector3) IntersectionlessEdge = (Vector3.zero, Vector3.zero);
        (int, int)[] edges = new (int, int)[3];
        
        if(IsPointOnEdge(verts[0], verts[1], intersect1) == false && IsPointOnEdge(verts[0], verts[1], intersect2) == false){
            
            IntersectionlessEdge = (verts[0], verts[1]);
            edges[2] = (0, 1);
        }
        else if(IsPointOnEdge(verts[1], verts[2], intersect1) == false && IsPointOnEdge(verts[1], verts[2], intersect2) == false){
            
            IntersectionlessEdge = (verts[1], verts[2]);
            edges[2] = (1, 2);
        }
        else if(IsPointOnEdge(verts[2], verts[0], intersect1) == false && IsPointOnEdge(verts[2], verts[0], intersect2) == false){
            IntersectionlessEdge = (verts[2], verts[0]);
            edges[2] = (2, 0);
        }

        // getting the connecting middle point for subdivision on the intersectionless edge
        Vector3 midPoint = (IntersectionlessEdge.Item1 + IntersectionlessEdge.Item2) / 2;
        midPoint = new Vector3(midPoint.x, midPoint.y, midPoint.z);
        
        // triangle filling
        var newVertices = new List<Vector3>(mesh.vertices);
        var newTriangles = new List<int>(mesh.triangles);

        int baseTriangleIndex = 0;

        int basePoint0 = 0, basePoint1 = 0, basePoint2 = 0;
        for (int j = 0; j < mesh.triangles.Length; j += 3)
        {
            if (vertices[mesh.triangles[j]] == verts[0] && vertices[mesh.triangles[j + 1]] == verts[1] && vertices[mesh.triangles[j + 2]] == verts[2])
            {
                baseTriangleIndex = j;
                basePoint0 = mesh.triangles[j];
                basePoint1 = mesh.triangles[j + 1];
                basePoint2 = mesh.triangles[j + 2];

                newTriangles.RemoveRange(j, 3);
            break;
            }
        }

        // add the new verticies and remember where we stored them
        newVertices.Add(midPoint);
        int midPointIndex = newVertices.Count - 1;


        newVertices.Add(intersect1);
        int intersect1Index = newVertices.Count - 1;

        newVertices.Add(intersect2);
        int intersect2Index = newVertices.Count - 1;
        // a triangle can be cut in three ways depending on which edge was not intersected by a straight blade
        // for each case, we have to draw the smaller triangles in a different way
        if(edges[2] == (0, 1)){

            print("0-1");
            newTriangles.Add(basePoint0);
            newTriangles.Add(midPointIndex);
            newTriangles.Add(intersect2Index);

            newTriangles.Add(basePoint1);
            newTriangles.Add(intersect1Index);
            newTriangles.Add(midPointIndex);

            newTriangles.Add(intersect1Index);
            newTriangles.Add(basePoint2);
            newTriangles.Add(intersect2Index);

            newTriangles.Add(midPointIndex);
            newTriangles.Add(intersect1Index);
            newTriangles.Add(intersect2Index);


        }
        else if(edges[2] == (1, 2)){
            newTriangles.Add(basePoint0);
            newTriangles.Add(intersect1Index);
            newTriangles.Add(intersect2Index);
            

            newTriangles.Add(intersect1Index);
            newTriangles.Add(basePoint1);
            newTriangles.Add(midPointIndex);
            
            newTriangles.Add(intersect2Index);
            newTriangles.Add(midPointIndex);
            newTriangles.Add(basePoint2);
            

            newTriangles.Add(intersect1Index);
            newTriangles.Add(midPointIndex);
            newTriangles.Add(intersect2Index);
            
            
        }
        else if(edges[2] == (2, 0)){
            newTriangles.Add(intersect1Index);
            newTriangles.Add(basePoint1);
            newTriangles.Add(intersect2Index);
            

            newTriangles.Add(basePoint0);
            newTriangles.Add(intersect1Index);
            newTriangles.Add(midPointIndex);
            
            newTriangles.Add(midPointIndex);
            newTriangles.Add(intersect2Index);
            newTriangles.Add(basePoint2);
            
            newTriangles.Add(intersect1Index);
            newTriangles.Add(intersect2Index);
            newTriangles.Add(midPointIndex);    
        }
        
        // regen the mesh
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();
        
    }

    void Update()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        tri = mesh.triangles;
    }

}