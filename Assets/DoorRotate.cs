using UnityEngine;

public class DoorRotate : MonoBehaviour
{
    public Transform door;
    public float rotationAmount = 90f;
    public float speed = 2f;

    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = door.rotation;
        openRotation = Quaternion.Euler(0, door.eulerAngles.y + rotationAmount, 0);
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;

        door.rotation = Quaternion.Slerp(
            door.rotation,
            target,
            Time.deltaTime * speed
        );
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}