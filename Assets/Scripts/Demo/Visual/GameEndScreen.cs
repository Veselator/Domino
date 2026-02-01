using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class GameEndScreen : MonoBehaviour
{
    // Класс, который отвечает за визуальную часть экрана победы
    private GlobalFlags _globalFlags;

    [Header("Ссылочки")]
    [SerializeField] private GameObject _handler;
    [SerializeField] private UniversalAnimator _background;
    [SerializeField] private UniversalAnimator _winText;
    [SerializeField] private UniversalAnimator _loseText;
    [SerializeField] private UniversalAnimator _restartButton;
    [SerializeField] private UniversalAnimator _menuButton;
    [SerializeField] private UniversalAnimator _restartButtonText;
    [SerializeField] private UniversalAnimator _menuButtonText;

    [Header("Параметры анимации")]
    [SerializeField] private float _fadeAnimationDuration = 0.5f;
    [SerializeField] private float _buttonAnimationDuration = 0.5f;
    [SerializeField] private float _timePerCharacter = 0.01f;
    [SerializeField] private float _delay = 0.2f;
    [SerializeField] private Color _backgroundColor = Color.black;
    [SerializeField] private Vector2 _buttonFinalSize = new Vector2(3, 4);

    private void Start()
    {
        _globalFlags = TBS_InitManager.Instance.GlobalFlags;

        _globalFlags.OnGameEnded.AddListener(HandleGameEnded);
        _handler.SetActive(false);
    }


    private void OnDestroy()
    {
        if(_globalFlags != null)
        {
            _globalFlags.OnGameEnded.RemoveListener(HandleGameEnded);
        }
    }

    private void HandleGameEnded(List<IPlayer> players)
    {
        _handler.SetActive(true);
        _restartButton.Rect.sizeDelta = Vector2.zero;
        _menuButton.Rect.sizeDelta = Vector2.zero;
        _background.Image.color = Color.clear;
        _winText.Text.text = "";
        _loseText.Text.text = "";
        _restartButtonText.Text.text = "";
        _menuButtonText.Text.text = "";
        StartCoroutine(AnimateGameEndScreen(players));
    }

    private IEnumerator AnimateGameEndScreen(List<IPlayer> players)
    {
        WaitForSeconds delay = new WaitForSeconds(_delay);

        _background.InterpolateColor(_backgroundColor, _fadeAnimationDuration);
        yield return delay;

        _winText.StyledTypingAnimation(GetWinText(players), _timePerCharacter);
        yield return delay;

        _loseText.TextTypingAnimation(GetLoseText(players), _timePerCharacter);
        yield return delay;

        _restartButton.AnimateImageSizeWithOvershoot(_buttonFinalSize, _buttonAnimationDuration);
        _menuButton.AnimateImageSizeWithOvershoot(_buttonFinalSize, _buttonAnimationDuration);
        yield return delay;
        _restartButtonText.StyledTypingAnimation("PLAY AGAIN", _timePerCharacter);
        _menuButtonText.StyledTypingAnimation("MENU", _timePerCharacter);
    }

    private string GetWinText(List<IPlayer> players)
    {
        return $"PLAYER <color=yellow>{players[0].Name}</color> WON!";
    }

    private string GetLoseText(List<IPlayer> players)
    {
        return $"PLAYER {players.Last().Name} DEFEATED!";
    }
}
