using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TBS_InitConfigSO", menuName = "TurnBasedGameflow/TBS_InitConfigSO")]
public class TBS_InitConfigSO : ScriptableObject
{
    // Конфиг пошаговой системы
    // Инициализирует конкретную игровую сессию

    [Header("Игроки в конкретной сессии")]
    public PlayerInfo[] players;

    [Header("Конфиг с ТЕКУЩИМИ правилами, которые непосредственно задействованы в данной игре на старте")]
    public TBS_CurrentRulesConfigSO currentRulesConfig;

    [Header("Конфиг ВСЕХ возможных правил")]
    public TBS_RulesConfigSO rulesConfig;

    [Header("Правило определения порядка хода")]
    public TBS_BaseOrderRule orderRule;

    [Header("Ходы бесконечные?")]
    public bool areTurnsInfinite;
    [Header("Если да - максимальное количество ходов")]
    public int maxTurnsCount;

    [Header("Максимальное количество раундов")]
    public int maxRoundsCount;
}

[Serializable]
public struct PlayerInfo
{
    public string playerName;
    public bool isAI;
    public int aiDifficulty;
}