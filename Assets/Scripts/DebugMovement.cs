using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMovement : MonoBehaviour
{
    // Unity accessible data
    public float speed = 1;
    public float lookSpeed = 1;

    // Other data members
    private Vector2 rotation = Vector2.zero;

    private void Update()
    {
#if UNITY_EDITOR
        // Look
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);
        transform.eulerAngles = new Vector2(rotation.x, rotation.y) * lookSpeed;

        // Move
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 fwdMovement = this.transform.forward * verticalInput;
        Vector3 horizMovement = this.transform.right * horizontalInput;
        Vector3 movement = fwdMovement + horizMovement;
        movement = movement.normalized * speed * Time.deltaTime;

        this.transform.position += movement;
#endif
    }
}
