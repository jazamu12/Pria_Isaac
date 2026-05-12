using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBotones : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;

    public void LoadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}