using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : class
{
    private static T instance;
    public static T Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null && instance != this as T)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this as T;
        }

        DontDestroyOnLoad(gameObject);
    }
}