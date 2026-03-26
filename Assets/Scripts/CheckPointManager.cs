using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private Vector3 _lastCheckpoint;
    private bool _hasCheckpoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCheckpoint(Vector3 position)
    {
        _lastCheckpoint = position;
        _hasCheckpoint = true;
        Debug.Log("Checkpoint guardado en: " + position);
    }

    public Vector3 GetCheckpoint(Vector3 fallback)
    {
        Debug.Log("HasCheckpoint: " + _hasCheckpoint);
        return _hasCheckpoint ? _lastCheckpoint : fallback;
    }
}