//using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebXR;

using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    // Constants
    private const int KEY_COUNT = 4;
    private readonly Dictionary<string, string> keyParams = new Dictionary<string, string>(KEY_COUNT)
    {
        { "w", "%E1%92%A5%E1%90%A6%E1%91%B3%E1%90%A7%E1%90%A4" },
        { "s", "%E1%90%85%E1%93%B4%E1%90%8A%E1%90%A7%E1%90%A4" },
        { "e", "%E1%90%8B%E1%90%A7%E1%90%B1%E1%90%A2%E1%91%B3%E1%90%A4" },
        { "n", "%E1%90%8A%E1%90%A2%E1%91%AD%E1%90%A6%E1%91%95%E1%91%B3%E1%90%A7%E1%90%A4" }
    };
    private readonly string[] directions = new string[KEY_COUNT] { "w", "s", "e", "n" };

    // Private member data
    private List<int> m_availableKeys = new List<int>();
    private List<AttachableObject> m_attachedKeys = new List<AttachableObject>(KEY_COUNT);
    private Material m_startMaterial = null;
    private bool m_isButtonPressed = false;

    // Unity accessible data
    public string debugQueryString;
    public List<AttachableObject> keyObjects = new List<AttachableObject>(KEY_COUNT);
    public List<GameObject> lockObjects = new List<GameObject>(KEY_COUNT);
    public MeshRenderer buttonMesh;
    public List<Material> keyMaterials = new List<Material>(KEY_COUNT);
    public Material successMaterial;
    public Material neutralMaterial;
    public Material portalMaterial;
    public Transform spawnPoint;
    public Button enterARButton;
    public GameObject unsupportedText;

    // Properties
    private bool m_isCompleted;
    public bool isCompleted
    {
        get { return m_isCompleted; }
        private set
        {
            if (value != m_isCompleted)
            {
                if (value)
                {
                    this.buttonMesh.GetComponent<MeshRenderer>().material = this.successMaterial;
                }
                else
                {
                    this.buttonMesh.GetComponent<MeshRenderer>().material = m_startMaterial;
                }
            }
            m_isCompleted = value;
        }
    }

    public void OnEnterARClicked()
    {
        //WebXRManager.Instance.ToggleAR();
        enterARButton.gameObject.SetActive(false);
    }

    private void Awake()
    {
        foreach (var keyObj in this.keyObjects)
        {
            keyObj.OnAttached += KeyObj_OnAttached;
            keyObj.OnDetached += KeyObj_OnDetached;
            keyObj.gameObject.SetActive(false);
        }
    }

    private void KeyObj_OnAttached(object sender, AttachmentPoint attachPoint)
    {
        // Update combination status
        var key = (AttachableObject)sender;
        this.m_attachedKeys.Add(key);

        CheckKeyConfiguration();
    }

    private void KeyObj_OnDetached(object sender, AttachmentPoint attachPoint)
    {
        // Update combination status
        this.m_attachedKeys.Remove(sender as AttachableObject);

        CheckKeyConfiguration();
    }

    private AttachmentPoint GetUnattachedPointOfType(AttachableObject attachedObj, AttachmentType type)
    {
        if (attachedObj == null)
        {
            Debug.Log("[GetAdjacentAttachmentPoint] Invalid object");
            return null;
        }

        foreach (AttachmentPoint point in attachedObj.attachmentPoints)
        {
            if (point.attachmentType == type && !point.isAttached)
            {
                return point;
            }
        }

        return null;
    }

    private void CheckKeyConfiguration()
    {
        const int locksLayerMask = 1 << 3; // Collide only with the "Locks" layer
        const string strLockPrefix = "LockPart";
        const float keyEpsilon = 0.02069999f;

        // Clear the lock materials
        foreach (GameObject obj in this.lockObjects)
        {
            obj.GetComponentInChildren<MeshRenderer>().material = this.neutralMaterial;
        }

        // Offset >= KEY_COUNT will ensure KeyPart1 is the first one placed
        int offset = KEY_COUNT;
        int correctKeys = 0;

        // Check the attached keys
        for (int i = 1; i <= this.m_attachedKeys.Count; ++i)
        {
            string correctKeyName = "KeyPart" + i;
            AttachableObject attachedKey = this.m_attachedKeys[i - 1];
            if (attachedKey == null || attachedKey.name != correctKeyName)
            {
                continue;
            }

            GameObject initialLockObj = null;
            if (i == 1)
            {
                foreach (GameObject lockObj in this.lockObjects)
                {
                    AttachmentPoint keyAttachment = lockObj.GetComponentInChildren<AttachmentPoint>().attachedPoint;
                    if (keyAttachment != null && keyAttachment.parentObject == attachedKey)
                    {
                        // update offset
                        offset = int.Parse(lockObj.name.Substring(strLockPrefix.Length)) - 1;
                        initialLockObj = lockObj;

                        break;
                    }
                }
            }

            // Get the other attachment point
            AttachmentPoint adjPoint = GetUnattachedPointOfType(attachedKey, AttachmentType.Loop);
            if (adjPoint == null)
            {
                continue;
            }

            // Project down to see if we hit the correct Lock tile
            RaycastHit rayHit;
            if (!Physics.Raycast(adjPoint.transform.position + (Vector3.up * keyEpsilon), Vector3.down, out rayHit, 1.0f, locksLayerMask) ||
                !rayHit.collider.name.StartsWith(strLockPrefix))
            {
                continue;
            }

            int lockIdx = int.Parse(rayHit.collider.name.Substring(strLockPrefix.Length));
            MeshRenderer lockMesh = null;

            // If this is the correct orientation, then update the tile
            if (lockIdx == 0 && i == 1 && initialLockObj != null)
            {
                lockMesh = initialLockObj.GetComponentInChildren<MeshRenderer>();
            }
            else if (i + offset == lockIdx || (i + offset) % KEY_COUNT == lockIdx)
            {
                lockMesh = rayHit.collider.GetComponentInChildren<MeshRenderer>();
            }

            if (lockMesh != null)
            {
                // We found a matching lock, so the orientation must be correct
                lockMesh.material = this.successMaterial;
                ++correctKeys;
            }
        }

        this.isCompleted = correctKeys == KEY_COUNT;
    }

    private void Start()
    {
#if UNITY_WEBGL
        try
        {
            enterARButton.gameObject.SetActive(WebXRManager.Instance.isSupportedAR);
            unsupportedText.SetActive(!WebXRManager.Instance.isSupportedAR);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            enterARButton.gameObject.SetActive(false);
            unsupportedText.SetActive(true);
        }
#else
        enterARButton.gameObject.SetActive(false);
        unsupportedText.SetActive(false);
#endif

#if UNITY_EDITOR
        string queryString = this.debugQueryString;
#else
        string queryString = Application.absoluteURL;
#endif

        if (string.IsNullOrEmpty(queryString) || !queryString.Contains("?"))
        {
            Debug.Log("No query string provided, or no query parameters.");
            return;
        }
        Debug.Log(string.Format("Application URL: {0}", queryString));

        // Get the available keys
        var queryValues = parseQueryString(queryString);
        processQueryValues(queryValues);
    }

    private NameValueCollection parseQueryString(string query)
    {
        return parseQueryString(query, Encoding.UTF8);
    }

    private NameValueCollection parseQueryString(string query, Encoding encoding)
    {
        if (query == null)
            throw new ArgumentNullException("query");
        if (encoding == null)
            throw new ArgumentNullException("encoding");
        if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
            return new NameValueCollection();
        if (query[0] == '?')
            query = query.Substring(1);

        NameValueCollection result = new NameValueCollection();
        parseQueryString(query, encoding, result);
        return result;
    }

    private void parseQueryString(string query, Encoding encoding, NameValueCollection result)
    {
        if (query.Length == 0)
            return;

        string decoded = UnityWebRequest.UnEscapeURL(query, encoding);
        int decodedLength = decoded.Length;
        int namePos = 0;
        bool first = true;
        while (namePos <= decodedLength)
        {
            int valuePos = -1, valueEnd = -1;
            for (int q = namePos; q < decodedLength; q++)
            {
                if (valuePos == -1 && decoded[q] == '=')
                {
                    valuePos = q + 1;
                }
                else if (decoded[q] == '&')
                {
                    valueEnd = q;
                    break;
                }
            }

            if (first)
            {
                first = false;
                if (decoded[namePos] == '?')
                    namePos++;
            }

            string name, value;
            if (valuePos == -1)
            {
                name = null;
                valuePos = namePos;
            }
            else
            {
                name = UnityWebRequest.UnEscapeURL(decoded.Substring(namePos, valuePos - namePos - 1), encoding);
            }
            if (valueEnd < 0)
            {
                namePos = -1;
                valueEnd = decoded.Length;
            }
            else
            {
                namePos = valueEnd + 1;
            }
            value = UnityWebRequest.UnEscapeURL(decoded.Substring(valuePos, valueEnd - valuePos), encoding);

            result.Add(name, value);
            if (namePos == -1)
                break;
        }
    }

    private void processQueryValues(NameValueCollection queryKeyValueParams)
    {
        foreach (var keyVal in this.keyParams)
        {
            if (!string.IsNullOrEmpty(queryKeyValueParams.Get(keyVal.Key)))
            {
                string param = queryKeyValueParams[keyVal.Key];
                string encodedVal = UnityWebRequest.EscapeURL(param, Encoding.UTF8);

                int dirIndex = Array.IndexOf(this.directions, keyVal.Key);

                if (encodedVal != null && encodedVal.ToUpper() == keyVal.Value)
                {
                    this.m_availableKeys.Add(dirIndex);
                }

                // Set the button color to the 0th element in the query params
                if (this.m_startMaterial == null)
                {
                    this.buttonMesh.material = this.m_startMaterial = this.keyMaterials[dirIndex];
                }
            }
        }
    }

    private void Reset()
    {
        foreach (var key in this.m_availableKeys)
        {
            var keyObj = this.keyObjects[key];
            keyObj.Detach();
            keyObj.gameObject.SetActive(true);
            keyObj.transform.position = this.spawnPoint.position;
            keyObj.transform.rotation = Random.rotation;
            
            var physObj = keyObj.GetComponent<Rigidbody>();
            physObj.velocity = Vector3.zero;
        }
    }

    public void OnResetButtonPressed()
    {
        if (!this.m_isButtonPressed)
        {
            this.m_isButtonPressed = true;
            // TODO: Replace with XRInteractionToolkit components
            //var solvers = this.GetComponentsInChildren<Solver>();
            //for (int i = 0; i < solvers.Length; ++i)
            //{
            //    Solver solver = solvers[i];
            //    Destroy(solver);
            //}
        }

        if (!this.isCompleted)
        {
            Reset();
        }
        else
        {
            // Win state!
            foreach (var key in this.m_availableKeys)
            {
                var keyObj = this.keyObjects[key];
                keyObj.GetComponentInChildren<MeshRenderer>().material = this.portalMaterial;
                // TODO: Import Portal component
                //keyObj.GetComponentInChildren<Portal>().ResetDimensions();
            }
        }
    }
}
