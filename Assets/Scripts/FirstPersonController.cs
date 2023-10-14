using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    // References
    [SerializeField] private Transform cameraTransform;
    //[SerializeField] private CharacterController characterController;

    [SerializeField] private bl_Joystick moveJoystick;//Joystick reference for assign in inspector
    [SerializeField] private bl_Joystick lookJoystick;//Joystick reference for assign in inspector

    // Player settings
    [SerializeField] private float cameraSensitivity;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveInputDeadZone;

    // Touch detection
    private int leftFingerId, rightFingerId;
    private float halfScreenWidth;

    // Camera control
    private Vector2 lookInput;
    //private float cameraPitch;

    // Player movement
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;

    // Start is called before the first frame update
    void Start()
    {
        // id = -1 means the finger is not being tracked
        leftFingerId = -1;
        rightFingerId = -1;

        // only calculate once
        // Need to recalculate when rotating screen...
        halfScreenWidth = Screen.width / 2;
    }

    // Update is called once per frame
    void Update()
    {
        // Handles input
        GetTouchInput();


        //if (rightFingerId != -1) {
            // Ony look around if the right finger is being tracked
            LookAround();
        //}

        //if (leftFingerId != -1)
        //{
            // Ony move if the left finger is being tracked
            Move();
        //}
    }

    void GetTouchInput() {
        // Iterate through all the detected touches
        for (int i = 0; i < Input.touchCount; i++)
        {

            Touch t = Input.GetTouch(i);

            // Check each touch's phase
            switch (t.phase)
            {
                case UnityEngine.TouchPhase.Began:

                    if (t.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        // Start tracking the left finger if it was not previously being tracked
                        leftFingerId = t.fingerId;

                        // Set the start position for the movement control finger
                        moveTouchStartPosition = t.position;

                        Debug.Log("Moving");
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        // Start tracking the rightfinger if it was not previously being tracked
                        rightFingerId = t.fingerId;

                        Debug.Log("Rotating");
                    }

                    break;
                case UnityEngine.TouchPhase.Ended:
                case UnityEngine.TouchPhase.Canceled:

                    if (t.fingerId == leftFingerId)
                    {
                        // Stop tracking the left finger
                        leftFingerId = -1;
                        Debug.Log("Stopped tracking left finger");
                    }
                    else if (t.fingerId == rightFingerId)
                    {
                        // Stop tracking the right finger
                        rightFingerId = -1;
                        Debug.Log("Stopped tracking right finger");
                    }

                    break;
                case UnityEngine.TouchPhase.Moved:

                    // Get input for looking around
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = t.deltaPosition * cameraSensitivity * Time.deltaTime;
                    }
                    else if (t.fingerId == leftFingerId) {

                        // calculating the position delta from the start position
                        moveInput = t.position - moveTouchStartPosition;
                    }

                    break;
                case UnityEngine.TouchPhase.Stationary:
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }

    void LookAround() 
    {
        //        float sign = 1.0f;
        //#if UNITY_WEBGL 
        //        sign *= -1f;
        //#endif
        if (lookJoystick == null || cameraTransform == null)
        {
            return;
        }

        // vertical (pitch) rotation
        Vector2 lookDelta = new Vector2(lookJoystick.Horizontal, lookJoystick.Vertical) * cameraSensitivity * Time.deltaTime;

        //float cameraPitch = Mathf.Clamp(cameraTransform.localRotation.eulerAngles.y - lookDelta.y, -90f, 90f);
        //float cameraYaw = cameraTransform.localRotation.eulerAngles.x + lookDelta.x;
        //cameraTransform.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        cameraTransform.Rotate(-lookDelta.y, lookDelta.x, 0);
        cameraTransform.eulerAngles = new Vector3(cameraTransform.eulerAngles.x, cameraTransform.eulerAngles.y, 0);
        //cameraTransform.Rotate(cameraPitch, cameraYaw, 0);
    }

    void Move() 
    {
        //        float sign = 1.0f;
        //#if UNITY_WEBGL 
        //        sign *= -1f;
        //#endif

        if (lookJoystick == null || cameraTransform == null)
        {
            return;
        }

        // Don't move if the touch delta is shorter than the designated dead zone
        //if (moveInput.sqrMagnitude <= moveInputDeadZone) return;

        // Multiply the normalized direction by the speed
        Vector3 moveDelta = new Vector3(moveJoystick.Horizontal, 0, moveJoystick.Vertical) * Time.deltaTime * moveSpeed;

        // Move relatively to the local transform's direction
        cameraTransform.Translate(moveDelta);
    }

}
