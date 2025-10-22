using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    private static bool isShuttingDown = false;

    public static bool IsShuttingDown() => isShuttingDown;

    protected virtual void Awake()
    {
        if (Instance != null && !isShuttingDown)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
        Debug.Log($"{typeof(T)} initialized");
        if (Instance == null)
            Debug.LogError($"Singleton<{typeof(T)}> used on not type of {typeof(T)} component");
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
        Instance = null;
        Destroy(gameObject);
    }

}