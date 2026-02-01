using UnityEngine;

public class PlayerSettingLoaderManager : MonoBehaviour
{
    private PlayerSettingsHandler[] handlers;

    public void Load(BingoInitConfig config, PiecesPrefabsFactory factory, PieceSelectorManager selectorManager, PlayerSettingsHandler[] playersettingsHandlers)
    {
        handlers = playersettingsHandlers;

        for (int i = 0; i < handlers.Length && i < config.players.Length; i++)
        {
            PlayerInfo playerInfo = config.players[i];

            // Назначаем начальную фишку (каждому игроку свою)
            int pieceId = i % factory.PiecesCount;
            PiecePrefabInfo piecePrefab = factory.GetPrefabById(pieceId);

            // Регистрируем в менеджере выбора фишек
            selectorManager.RegisterPlayer(handlers[i], pieceId);

            // Инициализируем хендлер
            handlers[i].Init(
                playerInfo.playerName,
                playerInfo.isAI,
                piecePrefab,
                playerInfo.aiDifficulty,
                selectorManager
            );
        }
    }
}