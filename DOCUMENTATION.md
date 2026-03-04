# Turn-Based Framework for Unity — Documentation

## Table of Contents

- [Part 1 — Framework](#part-1--framework)
  - [1. Architecture Overview](#1-architecture-overview)
  - [2. Game Flow Lifecycle](#2-game-flow-lifecycle)
  - [3. Initialization System](#3-initialization-system)
  - [4. GlobalFlags (Event Bus)](#4-globalflags-event-bus)
  - [5. Turns Manager](#5-turns-manager)
  - [6. Players System](#6-players-system)
  - [7. Order System](#7-order-system)
  - [8. Rules System](#8-rules-system)
  - [9. Phase Handlers](#9-phase-handlers)
  - [10. Map System](#10-map-system)
  - [11. Predictor](#11-predictor)
- [Part 2 — Demo (Bingo Game)](#part-2--demo-bingo-game)
  - [12. Demo Overview](#12-demo-overview)
  - [13. Bingo Map](#13-bingo-map)
  - [14. Bingo Win Rules](#14-bingo-win-rules)
  - [15. AI System](#15-ai-system)
  - [16. Player Controls](#16-player-controls)
  - [17. Visual System](#17-visual-system)
  - [18. UI System](#18-ui-system)
  - [19. Main Menu](#19-main-menu)
  - [20. Scene Management](#20-scene-management)
  - [21. Camera & Effects](#21-camera--effects)

---

# Part 1 — Framework

Source: `Assets/Scripts/TurnBasedGameflow/`

The framework provides a generic, reusable turn-based game flow engine. It is game-agnostic — the concrete game logic (win conditions, map structure, AI) is injected through abstract classes, ScriptableObject rules, and configuration assets.

---

## 1. Architecture Overview

The framework follows several design principles:

| Pattern | Where it is used |
|---|---|
| **Singleton** | Every manager (`TBS_TurnsManager`, `TBS_PlayersManager`, `TBS_OrderManager`, etc.) exposes a static `Instance` property set in `Awake()`. |
| **Event Bus** | `GlobalFlags` is a centralized event hub. All communication between managers goes through it using `UnityEvent`. |
| **Chain of Responsibility** | `TBS_InitManager` initializes all subsystems through a chain of `BaseInitHandler` subclasses. |
| **Factory Method** | `TBS_BasePlayerFactory` creates players. Concrete games override it (e.g. `BingoPlayersFactory`). |
| **Strategy** | Turn order is determined by interchangeable `TBS_BaseOrderRule` ScriptableObjects. |
| **Template Method** | Abstract rule classes (`RuleToWinOrDefeat`, `RuleBeforeTurn`, etc.) define the contract; concrete ScriptableObjects implement them. |
| **ScriptableObject-driven config** | All rules, order strategies, and game parameters live in `ScriptableObject` assets, editable in the Unity Inspector. |

### Folder Structure

```
Assets/Scripts/TurnBasedGameflow/
├── Init/                  # Initialization chain
│   ├── TBS_InitManager.cs
│   ├── TBS_InitConfigSO.cs
│   ├── BaseInitHandler.cs
│   └── InitHandlers.cs
├── Order/                 # Turn order logic
│   ├── TBS_OrderManager.cs
│   ├── TBS_BaseOrderRule.cs
│   ├── TBS_PlayerFirstRule.cs
│   └── TBS_BotFirstRule.cs
├── Players/               # Player abstractions
│   ├── IPlayer.cs
│   ├── BasePlayer.cs
│   ├── HumanPlayer.cs
│   ├── AIPlayer.cs
│   ├── TBS_PlayersManager.cs
│   └── TBS_BasePlayerFactory.cs
├── Rules/                 # Rule system
│   ├── IRule.cs
│   ├── RuleTypes.cs  (contains RuleSO, abstract rule classes, RuleWinResult, enums)
│   ├── TBS_RulesManager.cs
│   ├── TBS_RulesConfigSO.cs
│   ├── TBS_CurrentRulesConfigSO.cs
│   ├── ChangeTurnOrderRule.cs
│   ├── WaitTimeBeforeGameStarts.cs
│   ├── WaitTimeAfterTurn.cs
│   ├── WaitTimeAfterRound.cs
│   └── WaitTimeAfterGameEnd.cs
├── GlobalFlags.cs         # Event bus
├── TBS_TurnsManager.cs    # Core turn/round loop
├── TBS_Predictor.cs       # Win prediction helper
├── TBS_BaseMap.cs          # Abstract map base
├── TBS_BeforeGameStartHandler.cs
├── TBS_BeforeTurnStartHandler.cs
├── TBS_BeforeTurnEndHandler.cs
├── TBS_RoundEndHandler.cs
└── TBS_BeforeGameEndHandler.cs
```

---

## 2. Game Flow Lifecycle

The framework orchestrates the following lifecycle. Each arrow represents a `GlobalFlags` event:

```
OnGameStartedQuery             →  (rules: BeforeGameStart execute)
OnGameStartedAllowed           →  game begins
│
├── OnRoundStarted(roundId)
│   │
│   ├── OnTurnStartedPrepared(turnId, playerId)  →  (rules: BeforeTurn execute)
│   │   OnTurnStarted(turnId, playerId)           →  player.Act() is called
│   │   ... player performs action ...
│   │   OnTurnEnded(turnId, playerId)
│   │   NextTurnQuery(turnId, playerId)           →  (rules: BeforeTurnEnd execute, then WinOrDefeat check)
│   │   OnNextTurnAllowed                         →  turn counter increments, next player
│   │   └── (loop back to OnTurnStartedPrepared)
│   │
│   OnRoundEnded(RuleWinResult)                   →  when a win/draw/max-turns is detected
│   NextRoundQuery(roundId, result)               →  (rules: AfterRound execute, map reloads)
│   NextRoundAllowed                              →  round counter increments
│   └── (loop back to OnRoundStarted)
│
OnGameEndedQuery(players)      →  (rules: BeforeEndOfGame execute)
OnGameEnded(players)           →  final result screen
```

### Key Concepts

- **Turn**: A single action by one player.
- **Cycle**: One complete pass through all players in the current order. `OnFullCycleEnded` fires when the last player in the order finishes.
- **Round**: A set of turns until a win/draw condition is met. After each round the map is reset, scores are awarded, and order may change.
- **Game**: A series of rounds (up to `maxRoundsCount`). After the last round, the overall winner is determined by `OverallScore`.

---

## 3. Initialization System

**Files:** `Init/TBS_InitManager.cs`, `Init/BaseInitHandler.cs`, `Init/InitHandlers.cs`, `Init/TBS_InitConfigSO.cs`

### TBS_InitConfigSO

A `ScriptableObject` that holds all game configuration:

| Field | Type | Description |
|---|---|---|
| `players` | `PlayerInfo[]` | Player definitions (name, isAI, aiDifficulty). |
| `currentRulesConfig` | `TBS_CurrentRulesConfigSO` | Which rules are active for the current game. |
| `rulesConfig` | `TBS_RulesConfigSO` | Registry of all available rules (id → ScriptableObject). |
| `orderRule` | `TBS_BaseOrderRule` | Strategy for determining turn order. |
| `areTurnsInfinite` | `bool` | If true, turns never exceed `maxTurnsCount`. |
| `maxTurnsCount` | `int` | Maximum turns per round (when not infinite). |
| `maxRoundsCount` | `int` | Number of rounds in the game. |

### Initialization Chain (Chain of Responsibility)

`TBS_InitManager.Awake()` constructs a chain of `BaseInitHandler` subclasses:

```
PlayersManagerInitHandler
  → OrderManagerInitHandler
    → RulesManagerInitHandler
      → MapInitHandler
        → BeforeTurnStartInitHandler
          → BeforeTurnEndInitHandler
            → BeforeGameStartInitHandler
              → RoundEndInitHandler
                → BeforeGameEndInitHandler
                  → PredictorInitHandler
                    → TurnsManagerInitHandler
```

Each handler:
1. Checks if its corresponding singleton `Instance` exists.
2. Calls `.Init(...)` on it.
3. Passes control to the next handler. If `Instance` is null, the chain throws an exception.

### Usage

```csharp
// Called externally (e.g. from BingoLoadManager):
TBS_InitManager.Instance.Init();       // initialize all subsystems
TBS_InitManager.Instance.StartGame();  // begin the game loop
```

---

## 4. GlobalFlags (Event Bus)

**File:** `GlobalFlags.cs`

`GlobalFlags` is a plain C# class (not a MonoBehaviour) that acts as the central event bus. All inter-system communication flows through it. It uses `UnityEvent` and `UnityEvent<T>`.

### Events

| Event | Parameters | When it fires |
|---|---|---|
| `OnGameStartedQuery` | — | Game start requested. |
| `OnGameStartedAllowed` | — | All before-game-start rules passed. |
| `OnRoundStarted` | `int roundId` | A new round begins. |
| `OnTurnStartedPrepared` | `int turnId, int playerId` | A turn is about to start (before-turn rules will run). |
| `OnTurnStarted` | `int turnId, int playerId` | Turn officially started — player acts now. |
| `OnTurnEnded` | `int turnId, int playerId` | Player finished their action. |
| `NextTurnQuery` | `int turnId, int playerId` | Request to proceed to next turn (end-of-turn rules run). |
| `OnNextTurnAllowed` | — | All end-of-turn rules passed; advance to next turn. |
| `OnFullCycleEnded` | — | All players in the order have taken one turn. |
| `OnRoundEnded` | `RuleWinResult` | Round finished (win or draw detected). |
| `NextRoundQuery` | `int roundId, RuleWinResult` | Request to proceed to next round. |
| `NextRoundAllowed` | — | After-round rules passed; advance to next round. |
| `OnGameEndedQuery` | `List<IPlayer>` | Game end requested (before-game-end rules run). |
| `OnGameEnded` | `List<IPlayer>` | Game officially ended. |
| `OnHumansTurnStarted` | `int playerId` | A human player's turn started (for UI/input). |
| `OnHumansTurnEnded` | `int playerId` | A human player's turn ended (for UI/input). |

### Trigger Methods

Each event has a corresponding `Trigger*()` method:

```csharp
globalFlags.TriggerOnTurnEnded(turnId, playerId);
globalFlags.TriggerAllowNextTurn();
globalFlags.TriggerOnRoundEnded(new RuleWinResult(GameWinCheckResult.Win, playerId));
```

---

## 5. Turns Manager

**File:** `TBS_TurnsManager.cs`

The central coordinator of the game loop. It listens to `GlobalFlags` events and drives the turn/round/game progression.

### Properties

| Property | Type | Description |
|---|---|---|
| `CurrentTurn` | `int` | Zero-based index of the current turn within a round. |
| `CurrentRound` | `int` | Zero-based index of the current round. |
| `MaxRounds` | `int` | Total number of rounds in the game. |
| `CurrentPlayer` | `IPlayer` | The player whose turn it currently is. |

### Events

| Event | Description |
|---|---|
| `OnTurnChanged(int)` | Fires whenever the turn counter changes. |

### Key Logic

- **StartGame()**: Triggers `OnGameStartedQuery` → after before-game rules pass → starts first round and first turn.
- **HandleEndOfTurn()**: Called when a player finishes. Triggers `NextTurnQuery` to run end-of-turn rules and win checks.
- **OnNextTurnAllowed()**: Increments the turn counter, advances to the next player, triggers the next turn.
- **OnRoundEnded()**: Awards `OverallScore` to the winner, increments the round counter, or ends the game if max rounds reached.
- **Max turns**: If `areTurnsInfinite` is false and `currentTurn >= maxTurns`, the round ends with a Draw.

---

## 6. Players System

**Files:** `Players/IPlayer.cs`, `Players/BasePlayer.cs`, `Players/HumanPlayer.cs`, `Players/AIPlayer.cs`, `Players/TBS_PlayersManager.cs`, `Players/TBS_BasePlayerFactory.cs`

### IPlayer Interface

```csharp
public interface IPlayer
{
    string Name { get; }
    int ID { get; }
    float Points { get; set; }         // per-round score
    float OverallScore { get; set; }   // cumulative across rounds
    bool IsAI { get; }
    void Act();                         // called on the player's turn
    void Init(GlobalFlags gf);
    void Init(GlobalFlags gf, TBS_TurnsManager turnsManager);
}
```

### BasePlayer

Abstract implementation of `IPlayer` with standard fields and constructors. `Act()` is virtual and meant to be overridden.

### HumanPlayer / AIPlayer

Minimal concrete classes that set `IsAI` to `false` / `true`. Game-specific behavior is added by subclassing (e.g. `BingoAi`).

### TBS_PlayersManager

Manages the list of players during a game session.

| Method | Description |
|---|---|
| `Init(globalFlags, config)` | Creates players via `TBS_BasePlayerFactory`. |
| `GetPlayerByID(id)` | Returns player by index. |
| `GetPlayersCount()` | Number of players. |
| `GetNextPlayer(id)` | Circular next player. |
| `IsPlayerAi(playerId)` | Checks if a player is AI. |
| `AddOverallScoreToPlayerWithId(who, howMuch)` | Adds to OverallScore. |
| `ResetPlayersPoints()` | Resets per-round points for all players. |
| `GetPlayersOutOverall()` | Returns players sorted by `OverallScore` descending. |

### TBS_BasePlayerFactory

```csharp
public virtual IPlayer CreatePlayer(PlayerInfo info, int id)
```

Default implementation creates `AIPlayer` or `HumanPlayer`. Override this in your game to create custom player types (see `BingoPlayersFactory`).

---

## 7. Order System

**Files:** `Order/TBS_OrderManager.cs`, `Order/TBS_BaseOrderRule.cs`, `Order/TBS_PlayerFirstRule.cs`, `Order/TBS_BotFirstRule.cs`

### TBS_OrderManager

Manages the play order — a list of player IDs that determines who goes when.

| Property / Method | Description |
|---|---|
| `Order` | `List<int>` — player IDs in turn order. |
| `CurrentPlayerPointer` | Index into the `Order` list. |
| `CurrentPlayerID` | ID of the player whose turn it is. |
| `NextPlayer()` | Advances the pointer. Fires `OnFullCycleEnded` when wrapping around. |
| `IsTurnEndsCycle(turnId)` | Returns true if this turn is the last in a full cycle. |
| `Reload()` | Resets pointer to 0 (called between rounds). |
| `ReverseOrder()` | Reverses the order list. |

### TBS_BaseOrderRule

```csharp
public abstract List<int> GetTurnOrder(IReadOnlyCollection<IPlayer> players);
```

A `ScriptableObject`-based strategy for determining turn order.

### Built-in Order Rules

| Rule | Behavior |
|---|---|
| `TBS_PlayerFirstRule` | Human players go first, then AI players. |
| `TBS_BotFirstRule` | AI players go first, then human players. |

---

## 8. Rules System

**Files:** `Rules/IRule.cs`, `Rules/RuleTypes.cs`, `Rules/TBS_RulesManager.cs`, `Rules/TBS_RulesConfigSO.cs`, `Rules/TBS_CurrentRulesConfigSO.cs`

The rules system is the most extensible part of the framework. Rules are `ScriptableObject` assets that plug into specific lifecycle phases.

### IRule Interface

```csharp
public interface IRule
{
    string ID { get; set; }
    RuleType ruleType { get; }
    IEnumerator ExecuteRule();
    IEnumerator ExecuteRule(int turnId, int playerId);
}
```

### RuleSO (Base Class)

All rule ScriptableObjects inherit from `RuleSO : ScriptableObject, IRule`. It provides default (no-op) implementations of `ExecuteRule`.

### Rule Types

| RuleType enum | Abstract class | When it runs | Signature |
|---|---|---|---|
| `BeforeStartGame` | `RuleBeforeGameStart` | Before the game begins | `ExecuteRule()` |
| `BeforeTurn` | `RuleBeforeTurn` | Before each turn starts | `ExecuteRule(turnId, playerId)` |
| `BeforeTurnEnd` | `RuleBeforeTurnEnd` | After a turn, before win check | `ExecuteRule(turnId, playerId)` |
| `ToWinOrDefeat` | `RuleToWinOrDefeat` | After each turn (win/draw check) | `CheckIsPlayerWon(playerId, context)` → `RuleWinResult` |
| `ToCalculatePoints` | `RuleToCalculatePoints` | Scoring (called manually) | `CalculatePoints(turnId, playerId)` |
| `ToAllowAction` | `RuleToAllowAction` | Action validation (called manually) | `IsActionAllowed(params)` |
| `AfterEndOfCycle` | `RuleAfterCycleEnd` | After all players have taken one turn | `ExecuteRule(turnId, playerId)` |
| `AfterEndOfRound` | `RuleAfterEndOfRound` | After a round ends | `ExecuteRule(roundId, result)` |
| `AfterEndOfGame` | `RuleBeforeEndOfGame` | Before the game officially ends | `ExecuteRule(List<IPlayer>)` |

### RuleWinResult

```csharp
public class RuleWinResult
{
    public GameWinCheckResult Result { get; }  // None, Draft, Win
    public int WinnerPlayerID { get; }         // -1 for draw/none
}
```

### TBS_RulesManager

Central manager that loads, caches, and provides rules to the phase handlers.

- **Loading**: Rules are loaded from `TBS_CurrentRulesConfigSO` (list of rule IDs) and resolved against `TBS_RulesConfigSO` (ID → `RuleSO` registry).
- **Caching**: On load or change, rules are sorted into typed lists (`RulesBeforeTurnCashed`, `RulesToWinOrDefeatCashed`, etc.) for fast access.
- **Runtime modification**: `AddRule(id)` / `RemoveRule(id)` allow adding/removing rules during gameplay. Changes trigger automatic recaching.

### TBS_RulesConfigSO

A `ScriptableObject` that acts as the **registry of all available rules**. Each entry is a `RuleEntry`:

```csharp
[Serializable]
public class RuleEntry
{
    public string ruleID;
    public RuleSO rule;
    public int priority;
}
```

### TBS_CurrentRulesConfigSO

A `ScriptableObject` listing which rules are **active** in the current game:

```csharp
public LinkedRuleInfo[] rules;  // each has a linkedRuleID string
```

### Built-in Framework Rules

| Rule | Type | Description |
|---|---|---|
| `WaitTimeBeforeGameStarts` | BeforeStartGame | Waits N seconds before the game begins. |
| `WaitSomeTimeAfterTurn` | BeforeTurnEnd | Waits N seconds after each turn. |
| `WaitSomeTimeAfterRound` | AfterEndOfRound | Waits N seconds between rounds. |
| `WaitTimeAfterGameEnd` | AfterEndOfGame | Waits N seconds before showing end screen. |
| `ChangeTurnOrderRule` | AfterEndOfRound | Reverses turn order if the current first player won. |

---

## 9. Phase Handlers

These `MonoBehaviour` singletons listen to `GlobalFlags` events and execute the corresponding rules from `TBS_RulesManager`.

### TBS_BeforeGameStartHandler

**Listens to:** `OnGameStartedQuery`
**Executes:** `RulesBeforeGameStartCashed` (coroutines, sequentially)
**Then triggers:** `OnGameStartedAllowed`

### TBS_BeforeTurnStartHandler

**Listens to:** `OnTurnStartedPrepared`
**Executes:** `RulesBeforeTurnCashed` (coroutines, sequentially)
**Then triggers:** `OnTurnStarted`

### TBS_BeforeTurnEndHandler

**Listens to:** `NextTurnQuery`
**Executes** (in order):
1. `RulesAfterTurnCashed` — post-turn rules.
2. `RulesAfterCycleEndedCashed` — if the current turn ends a full player cycle.
3. `RulesToWinOrDefeatCashed` — win/draw checks. If any returns `Win` or `Draft`, triggers `OnRoundEnded` and stops.

**Then triggers:** `OnNextTurnAllowed` (if no win/draw was found).

### TBS_RoundEndHandler

**Listens to:** `NextRoundQuery`
**Does:**
1. Resets player points.
2. Reloads the map.
3. Reloads the order (pointer back to 0).
4. Executes `RulesAfterEndOfRoundCashed`.

**Then triggers:** `NextRoundAllowed`

### TBS_BeforeGameEndHandler

**Listens to:** `OnGameEndedQuery`
**Executes:** `RulesBeforeEndOfGameCashed`
**Then triggers:** `OnGameEnded`

---

## 10. Map System

**File:** `TBS_BaseMap.cs`

```csharp
public abstract class TBS_BaseMap : MonoBehaviour
{
    public static TBS_BaseMap Instance { get; }
    public abstract IEnumerable Map { get; }
    public abstract object LastModifiedThing { get; }
    public abstract void Init(TBS_InitConfigSO config);
    public abstract void Reload();
}
```

The map is an abstract concept — it could be a grid, a board, a hex map, etc. The framework only requires:
- `Map` — iterable representation of the game state.
- `LastModifiedThing` — the last element that was changed (used by win-check rules).
- `Init()` — create the map from config.
- `Reload()` — reset the map between rounds.

---

## 11. Predictor

**File:** `TBS_Predictor.cs`

A helper that evaluates hypothetical game states without modifying the actual map.

```csharp
public RuleWinResult PredictWin(int playerId, TBS_Context context)
```

It iterates over all `RulesToWinOrDefeatCashed` and passes a `TBS_Context` — an abstract container for hypothetical state. If any rule returns `Win` or `Draft`, the result is returned immediately.

### TBS_Context

```csharp
public abstract class TBS_Context
{
    public abstract IEnumerable Context { get; }
}
```

Game-specific implementations (e.g. `BingoContext`) store hypothetical pieces that overlay the real map state, allowing AI to simulate moves.

---

# Part 2 — Demo (Bingo Game)

Source: `Assets/Scripts/Demo/`

The demo is a **Connect Four**-style game (called "Bingo" in the project) built on top of the framework. Two players drop pieces into columns; the first to align 4 in a row (horizontal, vertical, or diagonal) wins the round.

---

## 12. Demo Overview

The demo shows how to extend the framework:

| Framework Abstraction | Demo Implementation |
|---|---|
| `TBS_BaseMap` | `BingoMap` — column-based grid |
| `TBS_InitConfigSO` | `BingoInitConfig` — adds `mapWidth`, `mapHeight`, piece prefabs |
| `TBS_BasePlayerFactory` | `BingoPlayersFactory` — creates AI players with difficulty levels |
| `RuleToWinOrDefeat` | `BingoWinRule` → `RuleFourInRowHorizontal`, `RuleFourInRowVertical`, `RuleFourInRowDiagonals`, `RuleDraftIfMapIsFilled` |
| `TBS_Context` | `BingoContext` — stores hypothetical pieces for AI prediction |
| `AIPlayer` | `BingoAi` → `EasyBingoAi`, `MiddleBingoAi`, `HardBingoAi` |

### Entry Point

`BingoLoadManager.Start()`:
1. `TBS_InitManager.Instance.Init()` — initializes all framework systems.
2. `PlayerControlls.Instance.Init()` — sets up input.
3. `VisualPieceFactory.Instance.Init(config)` — loads piece prefabs.
4. `BingoVisualMapGenerator.Init()` — generates the visual grid.
5. `TBS_InitManager.Instance.StartGame()` — begins the game loop.

---

## 13. Bingo Map

**File:** `Demo/BingoMap.cs`

Extends `TBS_BaseMap` with a column-based grid (Connect Four layout).

### Data Structures

- **`BingoMap`**: Contains an array of `PieceColumn` objects. Exposes `Width`, `Height`, `TotalNumOfElements`.
- **`PieceColumn`**: A queue-based column with a maximum height. Supports `AddElement`, `IsFilled`, indexed access via `this[int]`.
- **`Piece`**: Represents a single game token. Has `playerId`, `X`, `Y`. Equality is based on `playerId`.

### Key Methods

| Method | Description |
|---|---|
| `AddPiece(playerId, columnId)` | Drops a piece into a column. Fires `OnElementAdded(x, y, playerId)`. |
| `IsColumnFilled(columnId)` | Checks if a column is full. |
| `IsEntireMapFilled()` | Checks if the entire grid is full (draw condition). |
| `GetAvailableColumns()` | Returns IDs of columns that still have space. |
| `GetMap()` | Returns a cached `Piece[][]` matrix (row-major, y=0 is bottom). |
| `Reload()` | Clears all columns. Fires `OnMapReset`. |

### Events

| Event | Description |
|---|---|
| `OnElementAdded(x, y, playerId)` | A piece was successfully placed. |
| `OnMapCreated` | The map was initialized. |
| `OnMapReset` | The map was cleared (between rounds). |

---

## 14. Bingo Win Rules

**Files:** `Demo/Rules/`

All win rules extend `BingoWinRule : RuleToWinOrDefeat`, which provides:
- Access to `BingoMap` and its matrix.
- `GetPieceOwner(x, y, context)` — returns the owner considering both the real map and a hypothetical `BingoContext`.
- `IsInBounds(x, y)` — bounds checking.

### RuleFourInRowHorizontal

Checks 7 cells in the target piece's row (3 left + piece + 3 right). Counts consecutive pieces owned by the same player. Returns `Win` if 4+ found.

### RuleFourInRowVertical

Same logic but scans vertically (3 below + piece + 3 above).

### RuleFourInRowDiagonals

Checks both diagonal directions (`/` and `\`). For each diagonal, walks back 3 steps from the target piece, then scans forward counting consecutive matches.

### RuleDraftIfMapIsFilled

Returns `Draft` if the entire map is filled (no available columns). Supports both real map and hypothetical context.

### BingoWinResult

Extends `RuleWinResult` with a `List<Piece> WinPieces` — the specific pieces that formed the winning line (used for visual highlighting).

### BingoContext

Extends `TBS_Context`. Stores hypothetical pieces in a `Dictionary<(int, int), Piece>`. Used by the AI to simulate "what if I place a piece here?" without modifying the real map.

| Method | Description |
|---|---|
| `SetPiece(x, y, piece)` | Places a hypothetical piece. |
| `ClearPiece(x, y)` | Removes a hypothetical piece. |
| `GetPiece(x, y)` | Returns the hypothetical piece at position, or null. |
| `SetTargetPiece(piece)` | Sets the piece to check win conditions against. |

---

## 15. AI System

**Files:** `Demo/AI/`

### BingoAi (Base)

Extends `AIPlayer`. On `Init`, caches the `BingoMap` and `TBS_Predictor`. Provides a `Put(columnId)` helper that places a piece and triggers `OnTurnEnded`.

### EasyBingoAi

**Strategy**: Picks a random available column.

```csharp
int selectedColumnId = availableColumns[Random.Range(0, availableColumns.Count)];
```

### MiddleBingoAi

**Strategy**: Look-ahead by one move.
1. For each available column, simulate placing own piece → check if it results in a win. If yes, take it.
2. For each available column, simulate placing opponent's piece → check if opponent would win. If yes, block it.
3. If neither applies, pick a random column.

Uses `BingoContext` and `TBS_Predictor.PredictWin()` for simulation.

### HardBingoAi

**Strategy**: Extends `MiddleBingoAi` with positional heuristics.
1. First, try to win or block (inherited from `MiddleBingoAi`).
2. If no immediate win/block: prefer center columns (sorted by distance from center). Among those, prefer columns shorter than the tallest column (to build lower for future connections).

---

## 16. Player Controls

**File:** `Demo/PlayerControlls.cs`

Handles human player input using Unity's New Input System (`InputActionReference`).

### Input Actions

| Action | Behavior |
|---|---|
| **Right** | Moves the column selector to the right (skips filled columns). |
| **Left** | Moves the column selector to the left (skips filled columns). |
| **PressToRelease** | Drops the piece into the selected column. |

### Flow

1. Listens to `OnHumansTurnStarted` → enables controls.
2. Player moves left/right → `OnMove(columnId)` fires (for visual pointer).
3. Player presses release → `BingoMap.AddPiece(playerId, columnId)` is called → `OnTurnEnded` is triggered → controls disable.

---

## 17. Visual System

**Files:** `Demo/Visual/`

### BingoVisualMapGenerator

Generates a grid of `Transform` points in world space based on `mapWidth`, `mapHeight`, and spacing values. Centers the grid horizontally. Creates a `BingoVisualMap` instance with the generated points.

### BingoVisualMap

Handles the visual representation of the game board:
- **Piece placement**: When `OnElementAdded` fires, instantiates a piece prefab above the grid and animates it falling into position using `UniversalAnimator`.
- **Win highlighting**: On round end with a win, sequentially highlights each winning piece (color flash + scale + particles).
- **Board clear**: After highlighting (or on draw), pieces are removed with a randomized falling-off animation.
- **Visual table**: The board background sprite is animated to appear with an overshoot effect on initialization.

### BingoVisualUserPointer

Shows the player's piece above the selected column during their turn. Listens to `OnMove`, `OnHumansTurnStarted`, `OnHumansTurnEnded`. Changes its sprite/color to match the current player's piece.

### VisualPieceFactory

Maps player IDs to their piece `GameObject` prefabs (loaded from `BingoInitConfig`).

### UniversalAnimator

A versatile coroutine-based animation component used throughout the demo:

| Category | Methods |
|---|---|
| **Position** | `Animate(endPos, speed)`, `AnimateWithOffset(offset, speed, destroy)` |
| **Scale** | `AnimateScale(target, duration)`, `AnimateScaleWithOvershoot(...)` |
| **Sprite size** | `AnimateSpriteSize(...)`, `AnimateSpriteSizeWithOvershoot(...)` |
| **Image (UI)** | `AnimateImageSize(...)`, `AnimateImageSizeWithOvershoot(...)`, `AnimateImageFill(...)` |
| **Color** | `InterpolateColor(...)`, `InterpolateColorWithEasing(...)` |
| **Alpha/Fade** | `FadeIn(...)`, `FadeOut(...)`, `FadeTo(...)`, `FadeInOut(...)` |
| **Pulse** | `PulseColor(...)`, `BlinkAlpha(...)` |
| **Text** | `AppearingText(...)`, `TextTypingAnimation(...)`, `GlitchTextAnimation(...)`, `StyledTypingAnimation(...)` |
| **Particles** | `PlayParticles()` |

Supports easing functions: Linear, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseOutBack, EaseOutElastic, EaseOutBounce.

### CurrentMainColorManager

Manages color palettes for the game's visual theme. Stores an array of `ColorPalette` structs (mainColor, secondColor, thirdColor). Randomly picks a new palette on each assignment. Persists across scenes with `DontDestroyOnLoad`.

### BackgroundColorChanger

Smoothly transitions the background shader colors (`_BackgroundColor`, `_LineColor`) when the color palette changes.

### BackgroundRotatingLines

Continuously rotates the background line pattern by updating the `_Angle` shader property each frame.

---

## 18. UI System

**Files:** `Demo/Visual/UI/`

All UI components use `[DefaultExecutionOrder(100)]` to ensure they initialize after the framework.

| Component | Listens to | Displays |
|---|---|---|
| `TurnText` | `TBS_TurnsManager.OnTurnChanged` | `"{PlayerName}'s turn"` |
| `StepNumberText` | `TBS_TurnsManager.OnTurnChanged` | `"Step {N}"` |
| `RoundText` | `GlobalFlags.OnRoundStarted` | `"Round {N}/{MaxRounds}"` |
| `PointsText` | `GlobalFlags.OnRoundEnded` | Per-player `OverallScore` list |

### GameEndScreen

Activates on `OnGameEnded`. Plays a sequence of animations:
1. Background fades to dark.
2. Winner name appears with styled typing animation.
3. Loser name appears with typing animation.
4. "PLAY AGAIN" and "MENU" buttons animate in with overshoot.

### Other UI Components

| Component | Description |
|---|---|
| `BackToMenuOnClick` | Button handler that calls `GameSceneManager.ExitToMenu()`. |
| `ReloadSceneOnClick` | Button handler that calls `GameSceneManager.ReloadScene()`. |

---

## 19. Main Menu

**Files:** `Demo/MainMenu/`

### MainMenuInitManager

Entry point for the main menu scene. Initializes the piece selector, game startup handler, and loads saved player settings.

### PlayerSettingsHandler

Per-player UI panel. Exposes:
- Player name (`TMP_InputField`)
- AI toggle (`Toggle`)
- AI difficulty slider (`Slider`, only visible when AI is enabled)
- Piece selector (left/right buttons + preview image)

### PlayerSettingLoaderManager

Loads default player settings from `BingoInitConfig` and populates `PlayerSettingsHandler` instances.

### PieceSelectorManager

Ensures each player has a unique piece. Tracks which piece ID is assigned to which player. `SelectNextPiece(handler, direction)` cycles through available (non-occupied) pieces.

### PiecesPrefabsFactory

Holds an array of `PiecePrefabInfo` (prefab + icon). Used by the piece selector and the visual system.

### GameStartupHandler

Collects all player settings from `PlayerSettingsHandler` panels, writes them into `BingoInitConfig`, and loads the game scene via `GameSceneManager`.

### AiDifficultyTextLinker

Links a `Slider` value to a text label showing the AI difficulty name.

---

## 20. Scene Management

**File:** `Demo/GameSceneManager.cs`

A persistent singleton (`DontDestroyOnLoad`) that manages scene transitions.

| Static Method | Description |
|---|---|
| `ReloadScene()` | Reloads the current scene. |
| `ExitToMenu()` | Loads the main menu scene. |
| `LoadNextScene()` | Loads the next level in the list. |

Supports both synchronous and asynchronous loading. Async loading includes:
- Minimum loading time.
- Progress events (`OnLoadingProgress`, `OnLoadingStarted`, `OnLoadingCompleted`).

---

## 21. Camera & Effects

### CameraShake

Coroutine-based camera shake using Perlin noise. Supports:
- Random drag shake (sustained)
- Hit shake (short burst)
- Light hit shake

Uses an `AnimationCurve` for falloff and applies both positional offset and rotation.

### SoftCameraTracker

Implements `ICameraTracker`. Smoothly tracks the mouse position with a blend factor, creating a subtle parallax-like camera movement.

### ParallaxEffect

Moves game objects relative to camera movement by a `parallaxFactor`, creating depth perception. Supports X and/or Y axes. Persists correctly across scene loads.

### BackgroundMusicHandler

Simple singleton with `DontDestroyOnLoad` that ensures background music persists across scene transitions.

### PlaySoundWithRandomPitch

Plays an `AudioSource` with a randomized pitch for variety in sound effects.
