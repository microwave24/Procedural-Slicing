using UnityEngine;

public class Grabbing : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private GameObject selectedObject;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.R)){
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform != null)
                {
                    
                    isDragging = true;
                    selectedObject = hit.transform.gameObject;
                    Vector3 hitPoint = hit.point;
                    offset = selectedObject.transform.position - hitPoint;
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
            selectedObject = null;
        }

        if (isDragging && selectedObject != null)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.WorldToScreenPoint(selectedObject.transform.position).z);
            Vector3 objPosition = mainCamera.ScreenToWorldPoint(mousePosition) + offset;
            objPosition.z = 0;
            selectedObject.transform.position = objPosition;
        }
    }
}
