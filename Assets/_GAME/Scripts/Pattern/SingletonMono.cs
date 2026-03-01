using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;
    
    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                return null;
            }
            
            if (_instance == null)
            {
                _instance = (T)FindAnyObjectByType(typeof(T));
                if (_instance == null && !_isQuitting)
                {
                    Debug.LogWarning(typeof(T).Name + " not found in scene. Make sure it exists.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _isQuitting = true;
            _instance = null;
        }
    }
}
