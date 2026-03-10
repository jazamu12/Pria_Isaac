using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    public DoorPrincipal door;
    public DoorPrincipal door2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.CloseDoor();
            door2.CloseDoor();
        }
    }
}