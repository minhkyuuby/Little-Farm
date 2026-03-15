using UnityEngine;

public abstract class SingletonWrapper<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isShuttingDown;
    private static readonly object InstanceLock = new();

    protected virtual bool PersistAcrossScenes => true;

    public static T Instance
    {
        get
        {
            if (_isShuttingDown)
            {
                return null;
            }

            if (_instance != null)
            {
                return _instance;
            }

            lock (InstanceLock)
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = Object.FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    var obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _isShuttingDown = false;

        if (_instance == null)
        {
            _instance = this as T;

            if (PersistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        _isShuttingDown = true;
    }
}