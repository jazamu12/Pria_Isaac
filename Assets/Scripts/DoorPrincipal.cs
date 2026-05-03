using UnityEngine;

public class DoorPrincipal : MonoBehaviour
{
    public enum DoorSide
    {
        Left,
        Right
    }

    [Header("Checkpoint Settings")]
    public Transform spawnPoint;

    [Header("Door Settings")]
    public DoorSide doorSide;
    public float rotationAmount = 90f;
    public float speed = 2f;

    [Header("Lock Settings")]
    public int keysRequired = 3;

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
            if (KeyManager.Instance.HasEnoughKeys(keysRequired))
            {
                isOpen = true;
                Debug.Log("Puerta desbloqueada!");
            }
            else
            {
                int faltan = keysRequired - KeyManager.Instance._keysCollected;
                Debug.Log($"Faltan {faltan} llaves para abrir esta puerta");
            }
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
        {
            playerNear = true;

            if (spawnPoint != null)
            {
                CheckpointManager.Instance.SaveCheckpoint(spawnPoint.position);
                Debug.Log("Checkpoint guardado correctamente");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}