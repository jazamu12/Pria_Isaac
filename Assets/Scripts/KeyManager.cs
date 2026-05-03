using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;
    public int _keysCollected = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddKey()
    {
        _keysCollected++;

        if (_keysCollected == 1)
            KeyTutorialUI.Instance.ShowTutorial();

        Debug.Log($"Llaves: {_keysCollected}");
    }

    public bool HasEnoughKeys(int required)
    {
        return _keysCollected >= required;
    }

    public void ResetKeys()
    {
        _keysCollected = 0;
    }
}