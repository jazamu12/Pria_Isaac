using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Canvas DeathScreen;
    private ThirdPersonController _player;
    private Vector3 _spawnPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        _player = FindObjectOfType<ThirdPersonController>();

        if (_player != null)
            _spawnPosition = _player.transform.position;

        if (DeathScreen != null)
            DeathScreen.enabled = false;
    }

    private void Update()
    {
        // ── FIX: Solo escucha el input si la muerte screen está activa ──
        if (DeathScreen != null && DeathScreen.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                Retry();
        }
    }

    public void Death()
    {
        if (DeathScreen != null)
            DeathScreen.enabled = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (EnemyAI enemy in FindObjectsOfType<EnemyAI>())
            enemy.ResetToStart();
    }

    private void Retry()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ── FIX: Refresca la referencia al jugador por si se perdió ──
        if (_player == null)
            _player = FindObjectOfType<ThirdPersonController>();

        if (_player != null)
        {
            Vector3 checkpoint = CheckpointManager.GetOrCreate().GetCheckpoint(_spawnPosition);
            _player.Respawn(checkpoint);
        }

        if (DeathScreen != null)
            DeathScreen.enabled = false;
    }
}