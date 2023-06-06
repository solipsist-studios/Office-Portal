// Copyright (c) Solipsist Studios Inc.
// Licensed under the MIT License.

#if UNITY_WSA || UNITY_ANDROID
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
#endif

using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using Solipsist;
using Solipsist.Models;


public class SceneAnchorController : NetworkBehaviour
{
    //[SerializeField] private NetworkObject parentNetworkObject = null;
    //[SerializeField] private GameObject sceneVisuals = null; // Root of physical/visible object
    
    private AzureSpatialAnchors spatialAnchorManager = null;
    private ulong? ownerId = null;
    private bool isSceneAnchorSet = false;

    // Handily, we only have anchors on AR platforms, where we DO want to disable the bg
    // Probably need to make this more robust in the future though...
    [SerializeField] private List<GameObject> disableWhenAnchored = new List<GameObject>();

    //public NetworkVariable<Vector3> HeadPosition = new NetworkVariable<Vector3>(
    //        default,
    //        NetworkVariableReadPermission.Everyone,
    //        NetworkVariableWritePermission.Owner
    //    );
    
    public void SetSceneAnchor()
    {
        if (this.isSceneAnchorSet)
        {
            return;
        }

        OnAnchorSet();

#if UNITY_WSA || UNITY_ANDROID
        UnityDispatcher.InvokeOnAppThread(async () =>
        {
            await this.spatialAnchorManager.AnchorObjectAsync(this.gameObject, new AnchorObjectModel());
        });
#endif
    }

    private void Start()
    {
        this.spatialAnchorManager = FindObjectOfType<AzureSpatialAnchors>();

        if (this.spatialAnchorManager == null)
        {
            Debug.LogError("SceneAnchorController requires an AzureSpatialAnchors component to exist in the scene!");
            return;
        }

#if UNITY_WSA || UNITY_ANDROID
        this.spatialAnchorManager.AnchorLocatedCallback += SpatialAnchorManager_AnchorLocatedCallback;
#endif
    }

#if UNITY_WSA || UNITY_ANDROID
    private void SpatialAnchorManager_AnchorLocatedCallback(AnchorObjectModel model, AnchorLocatedEventArgs eventArgs)
    {
        // Attach the anchor components
        Pose anchorPose = eventArgs.Anchor.GetPose();
        this.transform.SetPositionAndRotation(anchorPose.position, anchorPose.rotation);


        this.spatialAnchorManager.AnchorGameObject(this.gameObject, eventArgs.Anchor);

        OnAnchorSet();
    }
#endif

    private void OnAnchorSet()
    {
        // TODO: Change to "Update Anchor"
        this.isSceneAnchorSet = true;
        foreach (GameObject obj in this.disableWhenAnchored)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {
        //NetworkObject netObj = 

        //if (this.parentNetworkObject == null ||
        //    !this.parentNetworkObject.IsSpawned)
        //{
        //    return;
        //}

        // TODO: Only show scene object in immersive mode

        // Only show our own scene
        //if (this.sceneVisuals != null)
        //{
        //    this.sceneVisuals.SetActive(this.parentNetworkObject.IsOwner);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryTakeOwnershipServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        // TODO: Add thread safety!!
        if (!IsOwnedByServer)
        {
            return;
        }

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            GetComponent<NetworkObject>().ChangeOwnership(clientId);
            this.ownerId = clientId;
        }
    }

    public void TryTakeOwnership(SelectEnterEventArgs selectEnterEventArgs)
    {
        // Call ServerRPC to grant ownership
        TryTakeOwnershipServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void ReleaseOwnershipServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (this.ownerId != clientId)
        {
            return;
        }

        GetComponent<NetworkObject>().RemoveOwnership();
        this.ownerId = null;
    }

    public void ReleaseOwnership(SelectExitEventArgs selectExitEventArgs)
    {
        // Call ServerRPC to release ownership
        ReleaseOwnershipServerRpc();
    }
}
