using UnityEngine;

public class Singleton<_INSTANCE_> : MonoBehaviour
    where _INSTANCE_ : Singleton<_INSTANCE_>
{
    [SerializeField] private bool DestroyOnLoad = false;

    private static _INSTANCE_ _instance = null;

    public static _INSTANCE_ Instance
    {
        get
        {
            if( _instance == null )
            {
                _instance = FindObjectOfType<_INSTANCE_>();
            }

            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    public virtual void Awake()
    {
        if( !DestroyOnLoad )
        {
            DontDestroyOnLoad( gameObject );
        }

        if( _instance == null )
        {
            _instance = this as _INSTANCE_;
        }
        else if( _instance != this )
        {
            Debug.LogError( $"{_instance.name} is added two times.", this );

            Destroy( this );
        }
    }

    protected virtual void OnDestroy()
    {
        if( _instance == this )
        {
            _instance = null;
        }
    }
}