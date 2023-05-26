// Copyright (c) Solipsist Studios Inc.
// Licensed under the MIT License.

using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneAnchorController : NetworkBehaviour
{
    //[SerializeField] private NetworkObject parentNetworkObject = null;
    //[SerializeField] private GameObject sceneVisuals = null; // Root of physical/visible object
    private ulong? ownerId = null;
    //public NetworkVariable<Vector3> HeadPosition = new NetworkVariable<Vector3>(
    //        default,
    //        NetworkVariableReadPermission.Everyone,
    //        NetworkVariableWritePermission.Owner
    //    );

    private void Update()
    {
        if (IsOwner)
        {
            Debug.Log("Scene anchor owned by client: " + NetworkManager.Singleton.LocalClientId);
        }
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
