using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Unity.Mathematics;
using NUnit.Framework.Internal.Filters;
// using System.Numerics; // Removed to avoid conflict with UnityEngine types



public class Slicing3D : MonoBehaviour
{
    public GameObject obj;
    public Mesh mesh;
    public List<Vector3> v = new List<Vector3>();
    public List<Vector3> intersections = new List<Vector3>();
    private List<int> intersectedTriangles = new List<int>();
    private Plane slicingPlane;


    public List<Vector3> newVertices = new List<Vector3>();
    List<int> newTriangles = new List<int>();

    List<Vector3> topVerticies = new List<Vector3>();
    List<Vector3> bottomVerticies = new List<Vector3>();

    List<int> topTriangles = new List<int>();
    List<int> bottomTriangles = new List<int>();
    bool isIntersected = false;

    public Transform p1, p2, p3;
    public GameObject debug1;

    public Material crossSectionMaterial;

    void Start()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        newVertices = mesh.vertices.ToList<Vector3>();
        newTriangles = mesh.triangles.ToList<int>();
        getIntersectionPoints(p1.position, p2.position, p3.position, mesh.triangles);
        
        
        if(isIntersected == true){
            splitMesh(mesh, slicingPlane);
            Destroy(obj);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

    }

    void getIntersectionPoints(Vector3 p1, Vector3 p2, Vector3 p3, int[] triangles){
        
        slicingPlane = new Plane(p1, p2, p3);
        Vector3 planeNormal = slicingPlane.normal;

        bool oneAbove = false;
        bool oneBelow = false;
        for(int i = 0; i < mesh.vertices.Length; i++){
            if(oneAbove == true && oneBelow == true){
                break;
            }

            if(slicingPlane.GetDistanceToPoint(mesh.vertices[i]) >= 0) {
                oneAbove = true;
            } else{
                oneBelow = true;
            }

        }

        if(oneAbove == true && oneBelow == true){
            isIntersected = true;
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            var CurrentTriangle = new List<Vector3>();

            Vector3 v1 = mesh.vertices[triangles[i]];
            Vector3 v2 = mesh.vertices[triangles[i + 1]];
            Vector3 v3 = mesh.vertices[triangles[i + 2]];

            CurrentTriangle.Add(v1);
            CurrentTriangle.Add(v2);
            CurrentTriangle.Add(v3);

            Vector3 i_v1 = findLineIntersectionOnPlane(v1, v2, planeNormal, slicingPlane.distance);
            Vector3 i_v2 = findLineIntersectionOnPlane(v2, v3, planeNormal, slicingPlane.distance);
            Vector3 i_v3 = findLineIntersectionOnPlane(v3, v1, planeNormal, slicingPlane.distance);

            var localIntersects = new List<Vector3>();

            if (!float.IsPositiveInfinity(i_v1.x) && 
                !float.IsPositiveInfinity(i_v1.y) && 
                !float.IsPositiveInfinity(i_v1.z))
            {
                intersections.Add(i_v1);
                localIntersects.Add(i_v1);
            }
            if (!float.IsPositiveInfinity(i_v2.x) && 
                !float.IsPositiveInfinity(i_v2.y) && 
                !float.IsPositiveInfinity(i_v2.z))
            {
                intersections.Add(i_v2);
                localIntersects.Add(i_v2);
            }
            if (!float.IsPositiveInfinity(i_v3.x) && 
                !float.IsPositiveInfinity(i_v3.y) && 
                !float.IsPositiveInfinity(i_v3.z))
            {
                intersections.Add(i_v3);
                localIntersects.Add(i_v3);
            }

            if (!(
                float.IsPositiveInfinity(i_v1.x) && float.IsPositiveInfinity(i_v1.y) && float.IsPositiveInfinity(i_v1.z)
            ) || !(
                float.IsPositiveInfinity(i_v2.x) && float.IsPositiveInfinity(i_v2.y) && float.IsPositiveInfinity(i_v2.z)
            ) || !(
                float.IsPositiveInfinity(i_v3.x) && float.IsPositiveInfinity(i_v3.y) && float.IsPositiveInfinity(i_v3.z)
            ))
            {
                // Handle the case where at least one intersection point is valid
                intersectedTriangles.Add(i);
                intersectedTriangles.Add(i + 1);
                intersectedTriangles.Add(i + 2);
            }

            if(localIntersects.Count > 0){
                SplitTriangle(mesh, CurrentTriangle.ToArray(), localIntersects[0], localIntersects[1]);
            }
        }
    }

    bool IsPointOnEdge(Vector3 A, Vector3 B, Vector3 P, bool debug = false)
    {
        Vector3 AB = B - A;
        Vector3 AP = P - A;

        // Check if P is collinear with AB using cross product
        if(debug == true){
            Debug.Log(Vector3.Cross(AB, AP).sqrMagnitude);
        }

        if (Vector3.Cross(AB, AP).sqrMagnitude > 1e-9f) return false;

        // Check if P is within the bounds of segment AB
        float dotProduct = Vector3.Dot(AP, AB);

        return dotProduct >= 0 && dotProduct <= AB.sqrMagnitude;
    }

    Vector3 findLineIntersectionOnPlane(Vector3 v1, Vector3 v2, Vector3 planeNormal, float D) {
        float A = planeNormal.x;
        float B = planeNormal.y; // y is up
        float C = planeNormal.z;
        
        float d = A * (v2.x - v1.x) + B * (v2.y - v1.y) + C * (v2.z - v1.z);
        
        if (Mathf.Abs(d) == 0) {
            return Vector3.positiveInfinity; // No intersection (parallel case)
        }
        
        float n = -(A * v1.x + B * v1.y + C * v1.z + D);
        float t = n / d;
        
        if (t < 0 || t > 1) {
            return Vector3.positiveInfinity; // Intersection is outside segment
        }
        
        float x = v1.x + t * (v2.x - v1.x);
        float y = v1.y + t * (v2.y - v1.y); // Corrected: y stays as vertical
        float z = v1.z + t * (v2.z - v1.z); // Corrected: z is now the horizontal axis
        
        return new Vector3(x, y, z);
    }



    void SplitTriangle(Mesh mesh, Vector3[] verts, Vector3 intersect1, Vector3 intersect2){
        // get the edge that was not intersected by the straight blade
        (Vector3, Vector3) IntersectionlessEdge = (Vector3.zero, Vector3.zero);
        (int, int) IntersectionlessEdgeIndex = (-1,-1);
        
        if(IsPointOnEdge(verts[0], verts[1], intersect1) == false && IsPointOnEdge(verts[0], verts[1], intersect2) == false){
            
            IntersectionlessEdge = (verts[0], verts[1]);
            IntersectionlessEdgeIndex = (0, 1);
        }
        else if(IsPointOnEdge(verts[1], verts[2], intersect1) == false && IsPointOnEdge(verts[1], verts[2], intersect2) == false){
            
            IntersectionlessEdge = (verts[1], verts[2]);
            IntersectionlessEdgeIndex = (1, 2);
        }
        else if(IsPointOnEdge(verts[2], verts[0], intersect1) == false && IsPointOnEdge(verts[2], verts[0], intersect2) == false){
            IntersectionlessEdge = (verts[2], verts[0]);
            IntersectionlessEdgeIndex = (2, 0);
        } else {
            return;
        }

        // getting the connecting middle point for subdivision on the intersectionless edge
        Vector3 midPoint = (IntersectionlessEdge.Item1 + IntersectionlessEdge.Item2) / 2;
        midPoint = new Vector3(midPoint.x, midPoint.y, midPoint.z);

        int basePoint0 = 0, basePoint1 = 0, basePoint2 = 0;
        for (int j = 0; j < mesh.triangles.Length; j += 3)
        {
            if (mesh.vertices[mesh.triangles[j]] == verts[0] && mesh.vertices[mesh.triangles[j + 1]] == verts[1] && mesh.vertices[mesh.triangles[j + 2]] == verts[2])
            {
                basePoint0 = mesh.triangles[j];
                basePoint1 = mesh.triangles[j + 1];
                basePoint2 = mesh.triangles[j + 2];

                newTriangles.RemoveRange(j, 3);
            break;
            }
        }

        // add the new verticies and remember where we stored them
        int midPointIndex = GetOrAddVertexIndex(newVertices, midPoint);
        int intersect1Index = GetOrAddVertexIndex(newVertices, intersect1);
        int intersect2Index = GetOrAddVertexIndex(newVertices, intersect2);

        // a triangle can be cut in three ways depending on which edge was not intersected by a straight blade
        // for each case, we have to draw the smaller triangles in a different way
        if(IntersectionlessEdgeIndex == (0, 1)){
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
        else if(IntersectionlessEdgeIndex == (1, 2)){
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
        else if(IntersectionlessEdgeIndex == (2, 0)){
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


        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
    }

    void splitMesh(Mesh mesh, Plane slicingPlane)
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 v1 = mesh.vertices[mesh.triangles[i]];
            Vector3 v2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 v3 = mesh.vertices[mesh.triangles[i + 2]];

            Vector3 triPos = (v1 + v2 + v3) / 3;
            float d = slicingPlane.GetDistanceToPoint(triPos);

            if (d > 0)
            {
                int i1 = GetOrAddVertexIndex(topVerticies, v1);
                int i2 = GetOrAddVertexIndex(topVerticies, v2);
                int i3 = GetOrAddVertexIndex(topVerticies, v3);

                topTriangles.Add(i1);
                topTriangles.Add(i2);
                topTriangles.Add(i3);
            }
            else
            {
                int i1 = GetOrAddVertexIndex(bottomVerticies, v1);
                int i2 = GetOrAddVertexIndex(bottomVerticies, v2);
                int i3 = GetOrAddVertexIndex(bottomVerticies, v3);

                bottomTriangles.Add(i1);
                bottomTriangles.Add(i2);
                bottomTriangles.Add(i3);
            }
        }
        // Create new game objects for the top and bottom parts
        GameObject topPart = new GameObject("TopPart");
        GameObject bottomPart = new GameObject("BottomPart");

        topPart.AddComponent<MeshFilter>();
        topPart.AddComponent<MeshRenderer>();
        bottomPart.AddComponent<MeshFilter>();
        bottomPart.AddComponent<MeshRenderer>();

        // Create new meshes and assign their triangles, UVs and verticies
        Mesh topMesh = new Mesh();
        Mesh bottomMesh = new Mesh();


        topMesh.vertices = topVerticies.ToArray();
        topMesh.triangles = topTriangles.ToArray();

        bottomMesh.vertices = bottomVerticies.ToArray();
        bottomMesh.triangles = bottomTriangles.ToArray();

        topPart.GetComponent<MeshFilter>().mesh = topMesh;
        bottomPart.GetComponent<MeshFilter>().mesh = bottomMesh;

        GenerateProjectedUVs(topMesh, mesh);
        GenerateProjectedUVs(bottomMesh,mesh);

        topPart.transform.position += obj.transform.position;
        bottomPart.transform.position += obj.transform.position;

        topPart.GetComponent<MeshRenderer>().material = obj.GetComponent<MeshRenderer>().material;
        bottomPart.GetComponent<MeshRenderer>().material = obj.GetComponent<MeshRenderer>().material;

        topPart.AddComponent<MeshCollider>().sharedMesh = topMesh;
        bottomPart.AddComponent<MeshCollider>().sharedMesh = bottomMesh;

        fill(topMesh, slicingPlane, crossSectionMaterial, topPart, true);
        fill(bottomMesh, slicingPlane, crossSectionMaterial, bottomPart, false);

        topMesh.RecalculateNormals();
        topMesh.RecalculateTangents();
        topMesh.RecalculateBounds();

        bottomMesh.RecalculateNormals();
        bottomMesh.RecalculateTangents();
        bottomMesh.RecalculateBounds();

    }

    void GenerateProjectedUVs(Mesh mesh, Mesh baseMesh, float uvScale = 1f)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        // Use base mesh bounds for UV normalization
        Bounds bounds = baseMesh.bounds;
        
        float rangeX = bounds.size.x;
        float rangeY = bounds.size.y;
        float rangeZ = bounds.size.z;

        if (rangeX == 0) rangeX = 1; // Prevent division by zero
        if (rangeY == 0) rangeY = 1;
        if (rangeZ == 0) rangeZ = 1;

        // Auto-detect best projection plane (XY, XZ, or YZ)
        bool projectXY = rangeZ >= rangeX && rangeZ >= rangeY;  // If depth (Z) is the largest, project on XY
        bool projectXZ = rangeY >= rangeX && rangeY >= rangeZ;  // If height (Y) is the largest, project on XZ
        bool projectYZ = rangeX >= rangeY && rangeX >= rangeZ;  // If width (X) is the largest, project on YZ

        for (int i = 0; i < vertices.Length; i++)
        {
            float u, v;

            if (projectXY)
            {
                u = (vertices[i].x - bounds.min.x) / rangeX;
                v = (vertices[i].y - bounds.min.y) / rangeY;
            }
            else if (projectXZ)
            {
                u = (vertices[i].x - bounds.min.x) / rangeX;
                v = (vertices[i].z - bounds.min.z) / rangeZ;
            }
            else
            {
                u = (vertices[i].y - bounds.min.y) / rangeY;
                v = (vertices[i].z - bounds.min.z) / rangeZ;
            }

            uvs[i] = new Vector2(u * uvScale, v * uvScale);
        }

        mesh.uv = uvs;
    }

    void fill(Mesh mesh, Plane slicingPlane, Material crossMaterial, GameObject slicedObj, bool isTopSlice){

        List<Vector3> edgeVerticies = new List<Vector3>(); 

        Vector3 center = Vector3.zero;
        for(int i = 0; i < mesh.vertexCount; i++){

            float d = Mathf.Abs(slicingPlane.GetDistanceToPoint(mesh.vertices[i]));
            
            if(d < 0.000001f){
                edgeVerticies.Add(mesh.vertices[i]);
                center += mesh.vertices[i];
            }
        }

        center /= edgeVerticies.Count;
        SortClockwise(edgeVerticies, slicingPlane, center);

        
        edgeVerticies.Add(center);
        int centerIndex = edgeVerticies.Count - 1;


        List<int> crossTriangles = new List<int>();

        if(isTopSlice == true){
            for(int i = 0; i < edgeVerticies.Count - 1; i++){
                if(i + 1 == edgeVerticies.Count - 1){
                    crossTriangles.Add(i);
                    crossTriangles.Add(centerIndex);
                    crossTriangles.Add(0);
                } else{
                    crossTriangles.Add(i);
                    crossTriangles.Add(centerIndex);
                    crossTriangles.Add(i + 1);
                }
            }
        } else{
            for(int i = 0; i < edgeVerticies.Count - 1; i++){
                if(i + 1 == edgeVerticies.Count - 1){
                    crossTriangles.Add(i);
                    crossTriangles.Add(0);
                    crossTriangles.Add(centerIndex);
                    
                } else{
                    crossTriangles.Add(i);
                    crossTriangles.Add(i + 1);
                    crossTriangles.Add(centerIndex);
                }
            }
        }

        


        GameObject crossPart = new GameObject("Cross-section");
        crossPart.AddComponent<MeshFilter>();
        crossPart.AddComponent<MeshRenderer>();

        Mesh crossMesh = new Mesh();

        crossMesh.vertices = edgeVerticies.ToArray();
        crossMesh.triangles = crossTriangles.ToArray();
        crossMesh.RecalculateNormals();
        crossPart.GetComponent<MeshFilter>().mesh = crossMesh;
        crossPart.GetComponent<MeshRenderer>().material = crossMaterial;
        crossPart.transform.position = slicedObj.transform.position;
        crossPart.transform.parent = slicedObj.transform;



        
    }

    void SortClockwise(List<Vector3> vertices, Plane slicingPlane, Vector3 center)
    {
        Vector3 baseVector = (vertices[0] - center).normalized;

        List<(Vector3 vertex, float angle)> angles = new List<(Vector3, float)>();

        for(int i = 0; i < vertices.Count; i++){
            Vector3 relativeVector = (vertices[i] - center).normalized;
            float dot = Vector3.Dot(baseVector, relativeVector);
            float angle = Mathf.Acos(dot);

            Vector3 cross = Vector3.Cross(baseVector, relativeVector);
            if (Vector3.Dot(cross, slicingPlane.normal) < 0)
            {
                angle = (2 * math.PI) - angle;  // Ensure angle is in 0-360 range
            }

            angles.Add((vertices[i], angle));


        }
        angles.Sort((a, b) => a.angle.CompareTo(b.angle));

        for(int i = 0; i < vertices.Count; i++){
            vertices[i] = angles[i].vertex;
        }
    }

    

    int GetOrAddVertexIndex(List<Vector3> vertices, Vector3 vertex, float tolerance = 0.0000001f, bool debug = false)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], vertex) < tolerance)
            {
                return i;
                
            }
        }
        vertices.Add(vertex);
        return vertices.Count - 1;
    }

}
