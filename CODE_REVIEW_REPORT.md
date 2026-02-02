# Code Review Report: Turn-Based Framework for Unity

**Дата:** 2 февраля 2026
**Ревьюер:** Claude (Senior C# Unity Developer)
**Версия:** В разработке

---

## Общая оценка

| Компонент | Оценка | Комментарий |
|-----------|--------|-------------|
| **Фреймворк (TurnBasedGameflow)** | **7.5/10** | Хорошая архитектурная база, есть области для улучшения |
| **Демо-игра (Bingo)** | **7.0/10** | Достойная реализация, демонстрирует возможности фреймворка |

---

# ЧАСТЬ 1: АНАЛИЗ ФРЕЙМВОРКА (TurnBasedGameflow)

## Критерий 1: Соответствие принципам SOLID

### Single Responsibility Principle (SRP) — 7/10

**Положительные моменты:**
- Чёткое разделение ответственности между менеджерами: `TBS_PlayersManager`, `TBS_OrderManager`, `TBS_RulesManager`
- Каждый Handler отвечает за одну фазу игрового цикла
- `GlobalFlags` выполняет единственную задачу — маршрутизация событий

**Проблемные области:**
- `TBS_TurnsManager` (строки 23-29) нарушает SRP — управляет и ходами, и раундами:
```csharp
// Автор сам признаёт проблему в комментарии:
// "Может это на отлдную сущность вынести"
// "Но это нарушает SRP"
private int _currentRound = 0;
private int _maxRounds;
```

- `TBS_InitManager.Init()` (строки 21-145) — функция слишком длинная и делает слишком много (инициализирует 10+ компонентов)

**Рекомендации:**
- Выделить `TBS_RoundsManager` для управления раундами
- Разбить `Init()` на несколько подметодов или использовать паттерн Chain of Responsibility

### Open/Closed Principle (OCP) — 8/10

**Положительные моменты:**
- Система правил отлично расширяема через `RuleSO` и его наследников
- Новые типы игроков добавляются через наследование `BasePlayer`
- `TBS_BaseOrderRule` позволяет создавать новые стратегии порядка ходов

**Пример хорошей реализации:**
```csharp
// RuleTypes.cs - абстрактные классы открыты для расширения
public abstract class RuleToWinOrDefeat : RuleSO
{
    public abstract RuleWinResult CheckIsPlayerWon(int playerId, TBS_Context context = null);
}
```

**Проблемные области:**
- `RecalculateCashedRules()` в `TBS_RulesManager.cs:86-133` — switch по RuleType требует модификации при добавлении новых типов

### Liskov Substitution Principle (LSP) — 8/10

**Положительные моменты:**
- `IPlayer` корректно реализован в `BasePlayer`, `HumanPlayer`, `AIPlayer`
- `TBS_BaseMap` можно заменить на `BingoMap` без изменения клиентского кода
- `TBS_BasePlayerFactory` расширяется через `BingoPlayersFactory`

**Потенциальная проблема:**
- `IPlayer.Init()` имеет две перегрузки, что может привести к неконсистентному состоянию объекта

### Interface Segregation Principle (ISP) — 6/10

**Положительные моменты:**
- `IPlayer` — минималистичный интерфейс (8 членов)
- `IRule` содержит только необходимые методы

**Проблемные области:**
- `IRule` содержит два метода `ExecuteRule()` с разными сигнатурами — не все правила используют оба:
```csharp
public interface IRule
{
    public IEnumerator ExecuteRule();
    public IEnumerator ExecuteRule(int turnId, int playerId);
}
```
- `IPlayer` содержит `Points` и `OverallScore` — не все игры используют эти концепции

**Рекомендации:**
- Разбить `IRule` на `ISimpleRule` и `IContextRule`
- Создать `IScorable` интерфейс для систем с очками

### Dependency Inversion Principle (DIP) — 5/10

**Серьёзная проблема: Singleton-зависимости повсюду**

```csharp
// TBS_PlayersManager.cs:30-31 - хардкод зависимости через Singleton
IPlayer player = TBS_BasePlayerFactory.Instance.CreatePlayer(config.players[i], i);
player.Init(_globalFlags, TBS_TurnsManager.Instance);

// TBS_TurnsManager.cs:53-54
_orderManager = TBS_OrderManager.Instance;
_players = TBS_PlayersManager.Instance;
```

**Положительные моменты:**
- Конфигурация через ScriptableObjects (`TBS_InitConfigSO`)
- Правила внедряются через конфиг

**Рекомендации:**
- Внедрить DI-контейнер (VContainer, Zenject) или использовать Service Locator
- Передавать зависимости через конструктор/метод Init

---

## Критерий 2: Архитектура и паттерны проектирования — 8/10

**Используемые паттерны:**

| Паттерн | Реализация | Оценка |
|---------|------------|--------|
| **Singleton** | Все Manager-классы | Переиспользован |
| **Factory** | `TBS_BasePlayerFactory` | Хорошо |
| **Observer** | `GlobalFlags` + UnityEvent | Отлично |
| **Strategy** | `TBS_BaseOrderRule`, `IRule` | Хорошо |
| **Template Method** | `RuleSO`, `BasePlayer` | Хорошо |

**Архитектурные решения:**

✅ **Event-driven архитектура** — `GlobalFlags` обеспечивает loose coupling между компонентами

✅ **ScriptableObject-based конфигурация** — гибкая настройка без перекомпиляции

✅ **Разделение фреймворка и демо** — демо использует только публичные API

❌ **Избыточное использование Singleton** — затрудняет тестирование и создаёт скрытые зависимости

---

## Критерий 3: Читаемость и именование — 6/10

**Проблемы:**

1. **Кодировка комментариев** — кириллица отображается как мусор:
```csharp
// TBS_InitManager.cs:5
// �������� �� ���������� ������������� ���� ������ � ������ ��������� �������
```

2. **Неконсистентное именование:**
- `RulesBeforeTurnCashed` vs `RulesToCalculatePointsCashed` — "Cashed" вместо "Cached"
- `_isMapFlagDirty` vs `_isNextTurnQueuedFlag` — разные паттерны именования флагов

3. **Юмористические сообщения об ошибках** (непрофессионально для production):
```csharp
// TBS_InitManager.cs:32
Debug.LogError("Where is TBS_PlayersManager?! I`m veeeery angry >:| ");
Debug.LogError("Where is TBS_OrderManager?! I`m so sad without it :( ");
Debug.LogError("WTF where is my TBS_Predictor?");
```

**Положительные моменты:**
- Понятные имена классов и методов
- Хорошее использование XML-комментариев в некоторых местах

---

## Критерий 4: Обработка ошибок и отказоустойчивость — 6/10

**Проблемы:**

1. **Null-проверки без восстановления:**
```csharp
// TBS_InitManager.cs:26-34
if (TBS_PlayersManager.Instance != null) { ... }
else {
    Debug.LogError("...");
    return; // Игра просто не запустится
}
```

2. **Отсутствие валидации входных данных:**
```csharp
// TBS_PlayersManager.cs:36-40
public IPlayer GetPlayerByID(int id) {
    if(id < 0 || id >= _players.Count) return null; // Тихий null
    return _players[id];
}
```

**Положительные моменты:**
- Проверки границ массивов в `TBS_PlayersManager`
- Защита от повторных вызовов в `OnDestroy`

**Рекомендации:**
- Использовать исключения или Result-паттерн для критических ошибок
- Добавить assert'ы в Debug-сборках

---

## Критерий 5: Расширяемость и гибкость — 8/10

**Отличные решения:**

1. **Система правил** — легко добавлять новые правила без изменения ядра
2. **Конфигурация через SO** — изменение параметров без перекомпиляции
3. **Event-система** — слабая связанность компонентов
4. **Абстрактные базовые классы** — точки расширения для игровой логики

**Ограничения:**
- Фиксированный набор RuleType (9 типов) — добавление нового требует изменения enum и switch
- Привязка к Unity (MonoBehaviour, UnityEvent) — нельзя использовать вне Unity

---

## Критерий 6: Unity Best Practices — 7/10

**Соответствует:**
- Использование `[SerializeField]` вместо public полей
- Корректная отписка от событий в `OnDestroy`
- ScriptableObjects для конфигурации
- Coroutines для асинхронных операций

**Нарушает:**
- Инициализация в `Awake()` без проверки порядка загрузки:
```csharp
// Все Manager'ы делают Instance = this в Awake
private void Awake() {
    Instance = this;
}
// Нет гарантии правильного порядка инициализации
```

- Отсутствие `[RequireComponent]` для зависимых компонентов

---

# ЧАСТЬ 2: АНАЛИЗ ДЕМО-ИГРЫ (Bingo)

## Критерий 1: Соответствие принципам SOLID — 7/10

### SRP — 7/10
**Положительно:**
- `BingoMap` отвечает только за логику карты
- `BingoVisualMap` отвечает только за визуализацию
- AI разделён по уровням сложности

**Проблема:**
- `BingoMap.cs` содержит 3 класса в одном файле: `BingoMap`, `PieceColumn`, `Piece`

### OCP — 8/10
- AI легко расширяется через наследование (`EasyBingoAi` → `MiddleBingoAi` → `HardBingoAi`)
- Правила победы расширяемы через `BingoWinRule`

### LSP — 8/10
- `BingoInitConfig` корректно расширяет `TBS_InitConfigSO`
- `BingoPlayersFactory` правильно переопределяет `CreatePlayer`

### ISP — 7/10
- Интерфейсы минималистичны
- `ICameraTracker` — хороший пример сегрегации

### DIP — 5/10
- Те же проблемы с Singleton, что и во фреймворке:
```csharp
// BingoAi.cs:11-12
_map = TBS_BaseMap.Instance as BingoMap;
_predictor = TBS_Predictor.Instance;
```

---

## Критерий 2: Качество реализации игровой логики — 8/10

**Отличные решения:**

1. **Система предсказания** — `TBS_Predictor` + `BingoContext` для AI:
```csharp
// Позволяет AI "симулировать" ходы без изменения состояния
public RuleWinResult PredictWin(int playerId, TBS_Context context)
```

2. **Оптимизация проверки победы:**
```csharp
// RuleFourInRowHorizontal.cs:21-22
int startX = Math.Clamp(targetPiece.X - 3, 0, _map.Width - 1);
int endX = Math.Clamp(targetPiece.X + 3, 0, _map.Width - 1);
// Проверяет только релевантную область, а не всю карту
```

3. **Кеширование карты:**
```csharp
// BingoMap.cs:19-21
private bool _isMapFlagDirty;
public override IEnumerable Map => GetMap(); // Возвращает кешированную версию
```

---

## Критерий 3: Качество AI — 7/10

**Архитектура AI:**
```
BingoAi (база)
  └── EasyBingoAi (рандом)
        └── MiddleBingoAi (блокировка/победа)
              └── HardBingoAi (позиционная игра)
```

**Положительно:**
- Чёткая иерархия сложности
- Использование предиктора для анализа ходов
- Переиспользование логики через наследование

**Проблемы:**
- Хардкод `_humanPlayerId = 1 - ID` — работает только для 2 игроков
- Нет конфигурируемых параметров AI

---

## Критерий 4: Визуальная система — 8/10

**Отличные решения:**

1. **Анимации через `UniversalAnimator`:**
```csharp
animator.AnimateSpriteSizeWithOvershoot(Vector2.zero, targetSize,
    _tableAppearDuration, _tableOvershootFactor, _timeBeforeAnimationStart);
```

2. **Все параметры анимаций конфигурируемы через Inspector**

3. **Корректная очистка ресурсов:**
```csharp
private void OnDestroy() {
    if (_points != null) {
        _map.OnElementAdded -= HandleElementAdded;
        _globalFlags.OnRoundEnded.RemoveListener(HandleRoundEnded);
    }
}
```

**Проблема:**
- `BingoVisualMap` тоже Singleton — усложняет тестирование

---

## Критерий 5: Структура данных — 7/10

**Класс `Piece`:**
```csharp
public class Piece {
    public int playerId { get; private set; } // Иммутабельный
    public int X; // Мутабельный - почему?
    public int Y; // Мутабельный - почему?
}
```

**`PieceColumn` — хорошая инкапсуляция:**
```csharp
public class PieceColumn {
    private Queue<Piece> _column;        // Внутреннее хранилище
    private List<Piece> _columnListCashed; // Для быстрого доступа по индексу
    public Piece this[int id] => GetElement(id); // Индексатор
}
```

---

## Критерий 6: Интеграция с фреймворком — 9/10

**Отлично демонстрирует использование фреймворка:**

1. Расширение конфигурации: `BingoInitConfig : TBS_InitConfigSO`
2. Реализация карты: `BingoMap : TBS_BaseMap`
3. Кастомная фабрика: `BingoPlayersFactory : TBS_BasePlayerFactory`
4. Правила через SO: `BingoWinRule : RuleToWinOrDefeat`
5. Подписка на события `GlobalFlags`

---

# ОБЩИЕ РЕКОМЕНДАЦИИ

## Приоритет 1 (Критично):

1. **Исправить кодировку комментариев** — использовать UTF-8 с BOM
2. **Заменить юмористические LogError на информативные**
3. **Разделить `TBS_TurnsManager`** — вынести управление раундами

## Приоритет 2 (Важно):

4. **Внедрить DI-контейнер** или Service Locator для замены Singleton
5. **Создать интерфейсы для Manager-классов** — улучшит тестируемость
6. **Разбить большие файлы** — `BingoMap.cs` (3 класса → 3 файла)

## Приоритет 3 (Желательно):

7. **Добавить unit-тесты** для правил и AI
8. **Документация API** — XML-комментарии для публичных методов
9. **Оптимизация `RecalculateCashedRules`** — использовать Dictionary вместо switch

---

# ЗАКЛЮЧЕНИЕ

**Фреймворк** представляет собой хорошо продуманную архитектуру для пошаговых игр с гибкой системой правил и событий. Основные проблемы связаны с избыточным использованием Singleton-паттерна и нарушением DIP, что затрудняет тестирование и создаёт скрытые зависимости.

**Демо-игра** качественно демонстрирует возможности фреймворка, показывая как создавать игровую логику, AI и визуализацию. Код чистый и следует большинству Unity best practices.

**Общая оценка: 7.3/10** — Готов для прототипирования и небольших проектов. Для production-использования рекомендуется провести рефакторинг DI-системы.

---

*Отчёт сгенерирован автоматически. Для вопросов обращайтесь к ревьюеру.*
