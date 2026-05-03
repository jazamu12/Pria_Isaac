using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Rotación")]
    public float rotationSpeed = 90f;
    public Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KeyManager.Instance.AddKey();
            Destroy(gameObject);
        }
    }
}