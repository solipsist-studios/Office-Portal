using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachmentType
{
    Hook,
    Loop
}

public class AttachmentPoint : MonoBehaviour
{
    // Unity accessible data
    public AttachmentType attachmentType;

    // Other data members
    public AttachableObject parentObject { get; set; }
    public List<AttachmentPoint> collidingAttachmentPoints { get; private set; } = new List<AttachmentPoint>();
    public AttachmentPoint attachedPoint { get; private set; }
    public bool isAttached
    {
        get
        {
            return this.attachedPoint != null;
        }
    }

    public void Attach(AttachmentPoint other)
    {
        this.attachedPoint = other;
    }

    public void Detach()
    {
        this.attachedPoint = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parentObject == null)
        {
            // Log initialization failure
            return;
        }

        if (parentObject.state == AttachableObjectState.Attached)
        {
            // Something is being attached TO this.
            return;
        }

        AttachmentPoint otherPoint = other.GetComponent<AttachmentPoint>();

        // Only attach to other attachment points
        if (otherPoint != null)
        {
            collidingAttachmentPoints.Add(otherPoint);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentObject == null)
        {
            // Log initialization failure
            return;
        }

        if (parentObject.state == AttachableObjectState.Attached)
        {
            // Something is being attached TO this.
            return;
        }

        AttachmentPoint otherPoint = other.GetComponent<AttachmentPoint>();

        // Only attach to other attachment points
        if (otherPoint != null)
        {
            // Only detach from other attachment points
            //surface.RemoveCollidingAttachmentPoint(this);
            collidingAttachmentPoints.Remove(otherPoint);
        }
    }
}
