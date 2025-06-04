using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;

    private Transform followTarget;

    void Update()
    {
        if (followTarget == null)
        {
            // Pan camera with arrow keys or WASD
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            transform.position += new Vector3(h, v, 0f) * panSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 pos = followTarget.position;
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }

        // Zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Camera cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }

        // Center on selected unit when F is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            Unit selected = UnitManager.Instance?.GetSelectedUnit();
            if (selected != null)
            {
                Vector3 pos = selected.transform.position;
                transform.position = new Vector3(pos.x, pos.y, transform.position.z);
            }
        }
    }

    public void Follow(Transform target)
    {
        followTarget = target;
    }

    public void ClearFollow(Transform target)
    {
        if (followTarget == target)
            followTarget = null;
    }
}
