using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachableObjectState
{
    Unattached,
    Attached
}

[RequireComponent(typeof(Interactable))]
public class AttachableObject : MonoBehaviour
{
    // Other data members
    private AttachmentPoint targetPoint;
    private AttachmentPoint sourcePoint;
    private bool shouldAttach;

    // Unity accessible data
    public bool showCollision = false;
    public AttachableObjectState state = AttachableObjectState.Unattached;
    public List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();

    // ⚡Events⚡
    public event EventHandler<AttachmentPoint> OnAttached;
    public event EventHandler<AttachmentPoint> OnDetached;

    public void Attach()
    {
        // Need something to attach to
        if (!IsValidAttachment())
        {
            return;
        }

        // Disable rigidbody movement
        // TODO: instead, add a Fixed Joint
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        // Set rotation to point up, and snap to 90 degree increments
        Quaternion curRot = this.transform.rotation;
        float yRot = SnapToRightAngle(curRot.eulerAngles.y, this.targetPoint.parentObject.transform.rotation.eulerAngles.y);
        curRot.eulerAngles = new Vector3(0, yRot, 0);
        this.transform.rotation = curRot;

        // Recalculate position delta after rotation
        Vector3 transformToTarget = this.targetPoint.transform.position - this.sourcePoint.transform.position;
        this.transform.position += transformToTarget;

        this.sourcePoint.Attach(this.targetPoint);
        this.targetPoint.Attach(this.sourcePoint);
        this.state = AttachableObjectState.Attached;

        if (this.OnAttached != null)
        {
            this.OnAttached.Invoke(this, this.sourcePoint);
        }
    }

    public void Detach()
    {
        this.state = AttachableObjectState.Unattached;

        // Enable rigidbody movement
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }

        if (this.sourcePoint != null)
        {
            this.sourcePoint.Detach();
        }
        if (this.targetPoint != null)
        {
            this.targetPoint.Detach();
        }

        if (this.OnDetached != null)
        {
            this.OnDetached.Invoke(this, this.sourcePoint);
        }

        this.targetPoint = null;
    }

    private bool IsValidAttachment()
    {
        if (this.targetPoint == null || this.sourcePoint == null || this.targetPoint.isAttached)
        {
            return false;
        }

        return this.targetPoint.attachmentType != this.sourcePoint.attachmentType;
    }

    private float SnapToRightAngle(float angle, float offset)
    {
        float localAngle = angle - offset;
        float quadrant = MathF.Round(localAngle / 90.0f);
        float newAngle = quadrant * 90.0f + offset;
        return newAngle;

        //const float two_pi = 360.0f;
        //const float quad_1 = 45.0f;
        //const float quad_2 = 135.0f;
        //const float quad_3 = 225.0f;
        //const float quad_4 = 315.0f;
        //const float angle_1 = 0.0f;
        //const float angle_2 = 90.0f;
        //const float angle_3 = 180.0f;
        //const float angle_4 = 270.0f;

        //const float pi_over_4 = 45.0f;
        //const float pi_over_2 = 90.0f;

        //

        //if (Mathf.Abs(angle) >= two_pi)
        //{
        //    angle %= two_pi;
        //}

        //if (angle < 0)
        //{
        //    angle += two_pi;
        //}

        //if (angle >= quad_4 || angle < quad_1)
        //{
        //    angle = angle_1;
        //}
        //else if (angle >= quad_1 && angle < quad_2)
        //{
        //    angle = angle_2;
        //}
        //else if (angle >= quad_2 && angle < quad_3)
        //{
        //    angle = angle_3;
        //}
        //else if (angle >= quad_3 && angle < quad_4)
        //{
        //    angle = angle_4;
        //}

        //return Mathf.Floor(angle + pi_over_4) / pi_over_2;

        //return angle;
    }

    private void Awake()
    {
        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
        {
            attachmentPoint.parentObject = this;
        }
    }

    private void Start()
    {
        Interactable interactable = this.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.OnInteractionStarted += OnInteractableHeld;
            interactable.OnInteractionEnded += OnInteractableReleased;
        }
    }

    private void OnInteractableHeld(object sender, System.EventArgs e)
    {
        if (this.state == AttachableObjectState.Attached)
        {
            Detach();
        }
    }

    private void OnInteractableReleased(object sender, System.EventArgs e)
    {
        if (this.state == AttachableObjectState.Unattached)
        {
            this.shouldAttach = true;
        }
    }

    private void FixedUpdate()
    {
        if (this.state == AttachableObjectState.Attached)
        {
            return;
        }

        // This needs to be done in update b/c MRTK will otherwise set the isKinematic /
        // useGravity properties based on the wrong stored values.
        if (this.shouldAttach)
        {
            this.shouldAttach = false;
            Attach();
            return;
        }

        // Determine closest attachment point to attachment transform
        float minDistToAttachPoint = float.MaxValue;
        this.targetPoint = null;
        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
        {
            attachmentPoint.GetComponent<MeshRenderer>().enabled = false;

            foreach (AttachmentPoint collidingPoint in attachmentPoint.collidingAttachmentPoints)
            {
                bool isValidAttachment = attachmentPoint.attachmentType != collidingPoint.attachmentType;
                if (!isValidAttachment)
                {
                    continue;
                }

                Vector3 transformToTarget = collidingPoint.transform.position - attachmentPoint.transform.position;
                float distToTargetPoint = transformToTarget.magnitude;

                if (this.targetPoint == null || distToTargetPoint < minDistToAttachPoint)
                {
                    this.targetPoint = collidingPoint;
                    this.sourcePoint = attachmentPoint;
                    minDistToAttachPoint = distToTargetPoint;
                }
            }
        }

        if (this.targetPoint != null && showCollision)
        {
            // Set the visibility of the highlight
            this.targetPoint.GetComponent<MeshRenderer>().enabled = true;
        }

        // if attached, show the transform preview
        //

    }
}
