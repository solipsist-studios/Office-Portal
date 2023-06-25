using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInputSelector : MonoBehaviour
{
    public List<MonoBehaviour> touchComponents = new List<MonoBehaviour>();
    public List<MonoBehaviour> ARComponents = new List<MonoBehaviour>();

    void Start()
    {
        bool isAR = true;

#if UNITY_WEBGL || UNITY_ANDROID
        // TODO: Handle Oculus case
        isAR = false;
#endif

        foreach (MonoBehaviour touchComp in touchComponents)
        {
            touchComp.enabled = !isAR;
        }
        foreach (MonoBehaviour arComp in ARComponents)
        {
            arComp.enabled = isAR;
        }
    }
}
