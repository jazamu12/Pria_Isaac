using UnityEngine;
using StarterAssets;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Canvas DeathScreen;
    private ThirdPersonController _player;
    private Vector3 _spawnPosition;

    private void Start()
    {
        _player = FindObjectOfType<ThirdPersonController>();
        _spawnPosition = _player.transform.position; // posición inicial como fallback
        DeathScreen.enabled = false;
    }
    private void Update()
    {
        // Pulsa H para hacer 10 de daño
        if (Input.GetKeyDown(KeyCode.H))
            _player.TakeDamage(10f);

        // Pulsa J para curar 10
        if (Input.GetKeyDown(KeyCode.J))
            _player.Heal(10f);
        
        Death();
        Retry();
    }

    public void Death()
    {
        if (_player.health <= 0)
        {
            DeathScreen.enabled = true;
            // Opcional: congelar al jugador
            Time.timeScale = 0f;
        }
    }

    private void Retry()
    {
        if (DeathScreen.enabled && Input.GetKeyDown(KeyCode.Space))
        {
            // Restaurar tiempo
            Time.timeScale = 1f;

            // Mover jugador al último checkpoint
            Vector3 checkpoint = CheckpointManager.Instance.GetCheckpoint(_spawnPosition);
            _player.transform.position = checkpoint;

            // Restaurar vida
            _player.Heal(100f);

            // Ocultar pantalla de muerte
            DeathScreen.enabled = false;
        }
    }
}
