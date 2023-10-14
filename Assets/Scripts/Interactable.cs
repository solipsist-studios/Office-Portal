//using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour//ObjectManipulator
{
    private Camera m_currentCamera;
    private Rigidbody m_rigidbody;
    private Vector3 m_screenPoint;
    private Vector3 m_currentVelocity;
    private Vector3 m_previousPos;
    private bool m_isHeld = false;

    // Unity accessible data
    public Vector3 offset;

    // ⚡Events⚡
    public event EventHandler OnInteractionStarted;
    public event EventHandler OnInteractionEnded;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        m_currentCamera = FindCamera();
        Debug.Log("[Interactable] Name=" + gameObject.name + " Camera=" + m_currentCamera.name);
    }

    public void StartInteraction()
    {
        if (OnInteractionStarted != null)
        {
            OnInteractionStarted(this, new EventArgs());
        }
    }

    public void EndInteraction()
    {
        if (OnInteractionEnded != null)
        {
            OnInteractionEnded(this, new EventArgs());
        }
    }

    //protected override void OnSelectEntered(SelectEnterEventArgs args)
    //{
    //    StartInteraction();
    //    base.OnSelectEntered(args);
    //}

    //protected override void OnSelectExited(SelectExitEventArgs args)
    //{
    //    EndInteraction();
    //    base.OnSelectExited(args);
    //}

    //private void OnMouseDown()
    //{
    //    if (!m_isHeld)
    //    {
    //        m_screenPoint = m_currentCamera.WorldToScreenPoint(gameObject.transform.position);
    //        offset = gameObject.transform.position - m_currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(m_screenPoint.z));

    //        m_isHeld = true;
    //        StartInteraction();
    //    }
    //}

    //private void OnMouseUp()
    //{
    //    m_rigidbody.velocity = m_currentVelocity;

    //    m_isHeld = false;

    //    EndInteraction();
    //}

    private void FixedUpdate()
    { 
        if (m_currentCamera != null && m_isHeld)
        {
            const int transparentFXLayer = 1 << 1; // TransparentFX layer
            Vector3 curScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
            Vector3 curWorldPoint = m_currentCamera.ScreenToWorldPoint(curScreenPoint);
            Vector3 cameraToScreenPointVec = curWorldPoint - m_currentCamera.transform.position;
            RaycastHit rayHit;

            // Clip to our ground plane
            if (Physics.Raycast(m_currentCamera.transform.position, cameraToScreenPointVec.normalized, out rayHit, cameraToScreenPointVec.magnitude, transparentFXLayer))
            {
                curWorldPoint = rayHit.point;
            }

            // Enable rigidbody movement
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.MovePosition(curWorldPoint + offset);
            m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
            m_previousPos = transform.position;

            // Align rotation to camera
            transform.forward = m_currentCamera.transform.forward;
            transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private Vector3 GetMousePosWithScreenZ(float screenZ)
    {
        return new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ);
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
