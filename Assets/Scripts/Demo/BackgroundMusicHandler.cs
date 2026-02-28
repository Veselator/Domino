using UnityEngine;

public class BackgroundMusicHandler : MonoBehaviour
{
    private static BackgroundMusicHandler Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
