using UnityEngine;

public class DoorOpenTrigger : MonoBehaviour
{
    public DoorRotate door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.OpenDoor();
        }
    }
}