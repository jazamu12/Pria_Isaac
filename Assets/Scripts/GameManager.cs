using UnityEngine;
using StarterAssets;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Canvas DeathScreen;
    private ThirdPersonController _player;
    private Vector3 _spawnPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _player = FindObjectOfType<ThirdPersonController>();
        _spawnPosition = _player.transform.position;
        DeathScreen.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) _player.TakeDamage(10f);
        if (Input.GetKeyDown(KeyCode.J)) _player.Heal(10f);
        Retry();
    }

    public void Death()
    {
        DeathScreen.enabled = true;
        Time.timeScale = 0f;
    }

    private void Retry()
    {
        if (DeathScreen.enabled && Input.GetKeyDown(KeyCode.Return))
        {
            Time.timeScale = 1f;

            Vector3 checkpoint = CheckpointManager.Instance.GetCheckpoint(_spawnPosition);
            _player.Respawn(checkpoint);  // ← el jugador se teletransporta a sí mismo

            DeathScreen.enabled = false;
        }
    }
}