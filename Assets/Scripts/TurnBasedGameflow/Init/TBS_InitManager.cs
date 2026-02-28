using UnityEngine;

public class TBS_InitManager : MonoBehaviour
{
    // Отвечает за корректную инициализацию всех систем в рамках пошаговой системы

    private GlobalFlags _globalFlags;
    public GlobalFlags GlobalFlags => _globalFlags;
    public static TBS_InitManager Instance { get; private set; }
    [SerializeField] private TBS_InitConfigSO _initConfig;
    public TBS_InitConfigSO InitConfig => _initConfig;

    BaseInitHandler _baseInit;

    private void Awake()
    {
        Instance = this;
        _globalFlags = new GlobalFlags();

        // Chain of responsibility

        BaseInitHandler turns = new TurnsManagerInitHandler();
        BaseInitHandler predictor = new PredictorInitHandler(turns);
        BaseInitHandler beforeGameEnd = new BeforeGameEndInitHandler(predictor);
        BaseInitHandler roundEnd = new RoundEndInitHandler(beforeGameEnd);
        BaseInitHandler beforeGameStart = new BeforeGameStartInitHandler(roundEnd);
        BaseInitHandler beforeTurnEnd = new BeforeTurnEndInitHandler(beforeGameStart);
        BaseInitHandler beforeTurnStart = new BeforeTurnStartInitHandler(beforeTurnEnd);
        BaseInitHandler map = new MapInitHandler(beforeTurnStart);
        BaseInitHandler rules = new RulesManagerInitHandler(map);
        BaseInitHandler order = new OrderManagerInitHandler(rules);
        _baseInit = new PlayersManagerInitHandler(order);
    }

    // Инициализация пошаговой системы
    // Вызывается из внешней области видимости
    public void Init()
    {
        // Отлов ошибок, когда нужного компонента системы нет!

        InitContext context = new InitContext(_globalFlags, _initConfig);

        _baseInit.Handle(context);

        // Инициализируем игроков
        //if (TBS_PlayersManager.Instance != null)
        //{
        //    TBS_PlayersManager.Instance.Init(_globalFlags, _initConfig);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_PlayersManager?! I`m veeeery angry >:| ");
        //    return;
        //}

        //// После игроков инициализируем порядок ихнего хода
        //if (TBS_OrderManager.Instance != null)
        //{
        //    TBS_OrderManager.Instance.Init(_globalFlags, _initConfig);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_OrderManager?! I`m so sad without it :( ");
        //    return;
        //}

        //// Инициализируем правила
        //if (TBS_RulesManager.Instance != null)
        //{
        //    TBS_RulesManager.Instance.Init(_globalFlags, _initConfig);
        //} 
        //else
        //{
        //    Debug.LogError("Where is TBS_RulesManager?! I WANT IT! ");
        //    return;
        //}

        //// Инициализируем карту
        //if (TBS_BaseMap.Instance != null)
        //{
        //    TBS_BaseMap.Instance.Init(_initConfig);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_BaseMap?! Please, make it! ");
        //    return;
        //}

        //// Дальше Handlers - держатель до начала и после конца хода; после конца раунда; перед началом игры
        //if (TBS_BeforeTurnStartHandler.Instance != null)
        //{
        //    TBS_BeforeTurnStartHandler.Instance.Init(_globalFlags);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_BeforeTurnStartHandler?! I want this one ");
        //    return;
        //}

        //if (TBS_BeforeTurnEndHandler.Instance != null)
        //{
        //    TBS_BeforeTurnEndHandler.Instance.Init(_globalFlags);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_BeforeTurnEndHandler?! You make me unhappy ");
        //    return;
        //}

        //if (TBS_BeforeGameStartHandler.Instance != null)
        //{
        //    TBS_BeforeGameStartHandler.Instance.Init(_globalFlags);
        //}
        //else
        //{
        //    Debug.LogError("Give me my TBS_BeforeGameStartHandler!");
        //    return;
        //}

        //if (TBS_RoundEndHandler.Instance != null)
        //{
        //    TBS_RoundEndHandler.Instance.Init(_globalFlags);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_RoundEndHandler?! I. Want. It. NOW! ");
        //    return;
        //}

        //if (TBS_BeforeGameEndHandler.Instance != null)
        //{
        //    TBS_BeforeGameEndHandler.Instance.Init(_globalFlags);
        //}
        //else
        //{
        //    Debug.LogError("Please, create TBS_BeforeGameEndHandler");
        //    return;
        //}

        //// Инициализируем предсказателя
        //if (TBS_Predictor.Instance != null)
        //{
        //    TBS_Predictor.Instance.Init();
        //}
        //else
        //{
        //    Debug.LogError("WTF where is my TBS_Predictor?");
        //    return;
        //}

        //// Инициализируем менеджер ходов
        //if (TBS_TurnsManager.Instance != null)
        //{
        //    TBS_TurnsManager.Instance.Init(_globalFlags, _initConfig);
        //}
        //else
        //{
        //    Debug.LogError("Where is TBS_TurnsManager?! He is VERY important part of turn based system ");
        //    return;
        //}

        // Начали!
        // Проблема - другие менеджеры не успевали подписаться
        //TBS_TurnsManager.Instance.StartGame();
    }

    public void StartGame()
    {
        TBS_TurnsManager.Instance.StartGame();
    }
}
