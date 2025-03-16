using UnityEngine;

public class Slicer : MonoBehaviour
{
    public Transform pointMesh1, pointMesh2;
    public LineRenderer line;
    Vector3[] points = new Vector3[2];
    bool slicing = false;


    public Slicing2D slicing2D_script;
    void Start()
    {
        line.positionCount = 2;    
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0)){
            if(slicing == false){
                slicing = true;

                points[0] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                points[0].z = -1;

                pointMesh1.gameObject.SetActive(true);
                pointMesh1.position = points[0];
            }

            points[1] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            points[1].z = -1;

            pointMesh2.gameObject.SetActive(true);
            pointMesh2.position = points[1];

            
        }

        if(Input.GetKeyUp(KeyCode.Mouse0)){
            slicing2D_script.cut(points[0],points[1]);

            pointMesh1.gameObject.SetActive(false);
            pointMesh2.gameObject.SetActive(false);

            gameObject.GetComponent<Slicer>().enabled = false;
            slicing = false;
        }

        line.SetPositions(points);
    }
}
