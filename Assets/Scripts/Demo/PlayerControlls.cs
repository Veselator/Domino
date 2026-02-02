using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlls : MonoBehaviour
{
    // Класс в демке, который отвечает за контроль со стороны игрока

    // Важные переменные
    private int _selectedColumnId = 0; // куда сейчас готов кинуть игрок
    public int CurrentColumnId => _selectedColumnId;
    private int _maxColumnId;
    private int _currentPlayerId;
    private bool _isControllAllowed = false;

    private GlobalFlags _globalFlags;
    private BingoMap _map;
    private TBS_TurnsManager _turnsManager;

    // События кнопок
    // Если делать по полной по SRP, то надо отделить управление
    // И логику перемещения
    // Но это просто небольшая демка, так что пойдёт
    [SerializeField] private InputActionReference Right;
    [SerializeField] private InputActionReference Left;
    [SerializeField] private InputActionReference PressToRelease;

    public event Action<int> OnMove;

    public static PlayerControlls Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        _globalFlags = TBS_InitManager.Instance.GlobalFlags;
        _globalFlags.OnHumansTurnStarted.AddListener(HandleHumansTurnStarted);

        BingoInitConfig config = TBS_InitManager.Instance.InitConfig as BingoInitConfig;
        _maxColumnId = config.mapWidth - 1;
        _map = TBS_BaseMap.Instance as BingoMap;
        _turnsManager = TBS_TurnsManager.Instance;

        // Подписка на события нажатия
        Right.action.performed += _ => TryToMove(1);
        Right.action.Enable();

        Left.action.performed += _ => TryToMove(-1);
        Left.action.Enable();

        PressToRelease.action.performed += _ => Release();
        PressToRelease.action.Enable();
    }

    private void OnDestroy()
    {
        if (_globalFlags != null)
        {
            Right.action.Disable();
            Left.action.Disable();
            PressToRelease.action.Disable();
            _globalFlags.OnHumansTurnStarted.RemoveListener(HandleHumansTurnStarted);
        }
    }

    private void HandleHumansTurnStarted(int playerId)
    {
        _currentPlayerId = playerId;
        _isControllAllowed = true;
    }

    public bool IsPossibleToDoMove(int direction)
    {
        if (!_isControllAllowed) return false;

        int newId = _selectedColumnId + direction;

        while (newId >= 0 && newId <= _maxColumnId)
        {
            if (!_map.IsColumnFilled(newId))
            {
                return true;
            }
            newId += direction;
        }

        return false;
    }

    private void TryToMove(int direction)
    {
        // Попытка переместить указатель
        if (!_isControllAllowed) return; // Не можна двигаться - значит, не можна

        int newId = _selectedColumnId + direction; // Получаем новый id

        while (newId >= 0 && newId <= _maxColumnId)
        {
            if (!_map.IsColumnFilled(newId)) // Если не заполнен - перемещаться можем
            {
                _selectedColumnId = newId;
                //Debug.Log($"Moving pointer to column {_selectedColumnId}");
                OnMove?.Invoke(newId);
                return;
            }
            // Если заполнен - идём дальше
            newId += direction;
        }
    }

    private void Release()
    {
        if (!_isControllAllowed) return;
        // Если можем добавить - значит, заканчиваем ход
        if (_map.AddPiece(_currentPlayerId, _selectedColumnId))
        {
            _isControllAllowed = false;
            _globalFlags.TriggerOnTurnEnded(_turnsManager.CurrentTurn, _currentPlayerId);
        }
    }
}
