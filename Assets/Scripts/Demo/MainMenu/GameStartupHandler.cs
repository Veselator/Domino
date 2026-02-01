using UnityEngine;

public class GameStartupHandler : MonoBehaviour
{
    private PlayerSettingsHandler[] _playerSettingsHandlers;
    [SerializeField] private BingoInitConfig _bingoInitConfig;

    public void Init(PlayerSettingsHandler[] handler)
    {
        _playerSettingsHandlers = handler;
    }

    public void StartGame()
    {
        // Заносим в конфиг изменения от каждого игрока
        PlayerInfo[] players = new PlayerInfo[_playerSettingsHandlers.Length];
        GameObject[] piecePrefabs = new GameObject[_playerSettingsHandlers.Length];

        for (int i = 0; i < _playerSettingsHandlers.Length; i++)
        {
            var handler = _playerSettingsHandlers[i];

            players[i] = new PlayerInfo
            {
                playerName = handler.Name,
                isAI = handler.IsAi,
                aiDifficulty = handler.AiDifficultyLevel
            };

            piecePrefabs[i] = handler.ChosenPiecePrefab.Prefab;
        }

        _bingoInitConfig.players = players;
        _bingoInitConfig.piecePrefabsForEachPlayer = piecePrefabs;

        GameSceneManager.Instance.LoadLevelByName("Bingo");
    }
}