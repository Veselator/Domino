using UnityEngine;

public class MainMenuInitManager : MonoBehaviour
{
    [SerializeField] private BingoInitConfig _bingoInitConfig;
    [SerializeField] private PlayerSettingLoaderManager _playerSettingLoaderManager;
    [SerializeField] private PieceSelectorManager _pieceSelectorManager;
    [SerializeField] private PiecesPrefabsFactory _pieceFactory;
    [SerializeField] private PlayerSettingsHandler[] _handlers;

    [SerializeField] private GameStartupHandler _gameStartupHandler;

    public void Start()
    {
        _pieceSelectorManager.Init(_pieceFactory);

        _gameStartupHandler.Init(_handlers);
        _playerSettingLoaderManager.Load(_bingoInitConfig, _pieceFactory, _pieceSelectorManager, _handlers);
    }
}