using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class LocalCameraManager : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 2, 0);
    public float distance = 5f;
    public float sensitivity = 5f;
    public float pitchMin = -20f;
    public float pitchMax = 80f;

    private float yaw = 0f;
    private float pitch = 10f;
    private Transform localPlayerTransform;

    IEnumerator Start()
    {
        // Wait until the NetworkManager is active and a local player is spawned (mainly for host)
        while (localPlayerTransform == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                foreach (var netObj in FindObjectsOfType<NetworkObject>())
                {
                    if (netObj.IsOwner)
                    {
                        localPlayerTransform = netObj.transform;
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    void LateUpdate()
    {
        if (localPlayerTransform == null)
            return;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = localPlayerTransform.position - (rotation * Vector3.forward * distance) + offset;

        transform.position = desiredPosition;
        transform.LookAt(localPlayerTransform.position + offset);
    }
}
