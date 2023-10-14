using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldButton : MonoBehaviour
{
    // Unity accessible data
    public UnityEvent buttonPressed;

    private void Awake()
    {
        Physics.queriesHitTriggers = true;
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse press detected");
    }

    private void OnMouseUpAsButton()
    {
        Debug.Log("Reset button pressed");
        if (buttonPressed != null)
        {
            buttonPressed.Invoke();
        }
    }
}
