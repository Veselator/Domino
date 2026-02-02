using UnityEngine;
using UnityEngine.InputSystem;

public class ExitOnButtonPressed : MonoBehaviour
{
    [SerializeField] private InputActionReference ExitButton;

    private void Start()
    {
        ExitButton.action.performed += _ => Exit();
        ExitButton.action.Enable();
    }

    private void OnDestroy()
    {
        ExitButton.action.Disable();
    }

    private void Exit()
    {
        GameSceneManager.Instance.QuitGame();
    }
}
