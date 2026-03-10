using UnityEngine;

public class DoorPrincipal : MonoBehaviour
{
    public enum DoorSide
    {
        Left,
        Right
    }

    [Header("Door Settings")]
    public DoorSide doorSide;
    public float rotationAmount = 90f;
    public float speed = 2f;

    private bool playerNear = false;
    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.rotation;

        float direction = (doorSide == DoorSide.Left) ? -1f : 1f;

        openRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + rotationAmount * direction,
            transform.eulerAngles.z
        );
    }

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            isOpen = true;
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * speed
        );
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }
}