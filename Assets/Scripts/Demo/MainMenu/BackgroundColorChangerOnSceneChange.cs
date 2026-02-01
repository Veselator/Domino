using UnityEngine;


[DefaultExecutionOrder(100)]
public class BackgroundColorChangerOnSceneChange : MonoBehaviour
{
    [SerializeField] private CurrentMainColorManager _colorManager;
    private GameSceneManager _sceneManager;

    private void Start()
    {
        if (_colorManager == null) return;
        _sceneManager = GameSceneManager.Instance;

        _sceneManager.OnLoadingStarted += ChangeBackgroundColor;
    }

    private void OnDestroy()
    {
        _sceneManager.OnLoadingStarted -= ChangeBackgroundColor;
    }

    private void ChangeBackgroundColor(int arg1, bool arg2)
    {
        _colorManager.AssignNewColorPalette();
    }
}
