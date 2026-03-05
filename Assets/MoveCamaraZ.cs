using UnityEngine;

public class MoveCameraZ : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraZPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 newPos = cameraTransform.position;
            newPos.z = cameraZPosition;
            cameraTransform.position = newPos;
        }
    }
}