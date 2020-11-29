using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : class
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this as T)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        DontDestroyOnLoad(gameObject);
    }
}