using UnityEngine;

public class KeyTutorialUI : MonoBehaviour
{
    public static KeyTutorialUI Instance;

    [Header("UI")]
    public GameObject tutorialPanel;

    private void Awake()
    {
        Instance = this;
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}