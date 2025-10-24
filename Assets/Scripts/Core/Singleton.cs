using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    private static bool isShuttingDown = false;

    //for scripts checking if application is quitting or not, if true then just return early
    public static bool IsShuttingDown() => isShuttingDown;

    [SerializeField] private bool persistAcrossScenes = true;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != (T)(object)this && !isShuttingDown)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        Debug.Log($"{typeof(T).Name} initialized (persist={persistAcrossScenes})");
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
        Instance = null;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == (T)(object)this) Instance = null;
    }

}