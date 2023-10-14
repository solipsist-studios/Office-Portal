using UnityEngine;

public class SnapToWorld : MonoBehaviour
{
    private Camera currentCamera;

    private void Start()
    {
        this.currentCamera = FindCamera();
    }

    private void Update()
    {
        if (this.currentCamera == null)
        {
            Debug.LogWarning("[SnapToWorld] No Camera found!");
            return;
        }

        RaycastHit rayHit;
        if (Physics.Raycast(this.currentCamera.transform.position, this.currentCamera.transform.forward, out rayHit, 1.0f))//, worldLayerMask))
        {
            this.transform.position = rayHit.point;
        }
    }

    private Camera FindCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        Camera result = null;
        int camerasSum = 0;
        foreach (var camera in cameras)
        {
            if (camera.enabled)
            {
                result = camera;
                camerasSum++;
            }
        }
        if (camerasSum > 1)
        {
            result = null;
        }
        return result;
    }
}
