using UnityEngine;

public class WinManager : MonoBehaviour
{
    [Header("Win Settings")]
    public GameObject winPanel;
    public int keysRequired = 3;

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void Update()
    {
        if (KeyManager.Instance != null && KeyManager.Instance.HasEnoughKeys(keysRequired))
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("winPanel no asignado en el Inspector");
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("¡Has ganado!");

        // Desactivar este script para que no se llame varias veces
        this.enabled = false;
    }
}