using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Canvas optionsCanvas;

    private void Start()
    {
        Time.timeScale = 1f; // ── FIX: asegura que el tiempo corre al entrar a la escena
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseCanvas.enabled = false;
        optionsCanvas.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Pause()
    {
        pauseCanvas.enabled = true;
        Time.timeScale = 0f;

        // Liberar y mostrar el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        pauseCanvas.enabled = false;
        Time.timeScale = 1f;

        // Volver a bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        Debug.Log("Options ejecutado");
        pauseCanvas.enabled = false;
        optionsCanvas.enabled = true;
    }

    public void CloseOptions()
    {
        Debug.Log("Cerrar opciones");
        optionsCanvas.enabled = false;
        pauseCanvas.enabled = true;
    }

    public void QuitGame()
    {
        Debug.Log("Salir ejecutado");
        Application.Quit();
    }
  
    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}