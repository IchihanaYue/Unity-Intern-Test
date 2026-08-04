# Match-3 Intern Test Project


## What Was Implemented

### 1. 3D Pyramid Board (84 Cells)
- **Pyramid Structure**: Replaced the 2D grid with a 4-tier symmetric pyramid centered at `(0, 0, 0)`:
  - Layer 0 (Bottom): 7x7 (49 cells)
  - Layer 1: 5x5 (25 cells)
  - Layer 2: 3x3 (9 cells)
  - Layer 3 (Top): 1x1 (1 cell)
  - **Total Cells**: 84 cells (divisible by 3, allowing 28 complete matching trios).
- **Layer Overlap & Blocking**: Upper tiles physically cover lower tiles. Lower tiles remain dimmed and un-clickable until all upper tiles covering them are removed.

### 2. Guaranteed All Fish Types Spawning
- Board initialization in `Board.cs` now guarantees that at least one matching trio (3 items) of every single fish type (`TYPE_ONE` through `TYPE_SEVEN`) is spawned on every new game round before filling and shuffling the remaining board slots.

### 3. Game Modes

- **Manual Play**: The classic mode where players tap unblocked top-tier tiles to collect them into the 5-slot holder row. Matching 3 identical fish explodes them and shifts remaining items left.
- **Autoplay (Win)**: A smart bot that automatically solves the board. Uses a weighted scoring algorithm to prioritize completing 3-matches, forming pairs, clearing higher layers first, and avoiding holder overflow when space is tight. Action delay is set to 0.5s per move.
- **Auto Lose**: An autoplay mode that intentionally picks distinct fish types to fill the holder without making 3-matches, demonstrating the Game Over state. Action delay is set to 0.5s per move.
- **Time Attack Mode**:
  - 60-second countdown timer displayed in the top UI section (`TIME: 60s`).
  - **Item Return**: Tapping any fish in the 5-slot holder row animates it back to its exact original cell position and layer on the main board.
  - **No Holder Full Loss**: Filling up the 5-slot holder does not cause a loss in this mode. The player only loses if the 60s timer expires before clearing the board.

### 4. Home Screen & UI Improvements
- **4 Home Screen Buttons**: Clean vertical menu layout with **PLAY**, **TIME ATTACK**, **AUTOPLAY (WIN)**, and **AUTO LOSE**.
- **Inspector Priority**: Serialized Inspector references (`btnPlay`, `btnTimer`, `btnAutoWin`, `btnAutoLose`) are used first, with automatic fallback formatting if unassigned.
- **Auto-Fit Button Text**: Added dynamic text scaling (`bestFit`) to ensure labels remain centered and legible across resolution changes.
- **Win/Loss Mutual Exclusion**: Added state locking in `GameManager` (`m_isEnding`) so Win and Loss events never trigger simultaneously or overlap outcome panels.

---

## Detailed Code Changes by File

### `Assets/Scripts/GameSettings.cs` & `gamesettings.asset`
- Set `BoardSizeX = 7` and `BoardSizeY = 7`.
- Added settings for `BottomRowSize = 5`, `LayerCount = 4`, and `TotalTriples = 16`.

### `Assets/Scripts/Board/Cell.cs`
- Added `LayerZ` coordinate property.
- Added `OriginalCell` property to store where an item originated.
- Added `SetSortingOrder()` to update sprite rendering order per layer.
- Added `SetBlockedVisual()` to handle color dimming when a cell is covered by upper tiles.

### `Assets/Scripts/Board/Item.cs`
- Added `OriginalCell` reference directly on `Item` so its origin travels with the item even when holder items shift left.
- Added `SetSortingOrder()` and `SetColor()` helpers for visual feedback.

### `Assets/Scripts/Board/Board.cs`
- Converted grid storage from a 2D array to a 3D array `Cell[x, y, z]`.
- Added bottom holder array `m_bottomCells`.
- Updated `CreateBoard()` to instantiate the 4-layer pyramid tiers centered around origin.
- Rewrote `Fill()` to trim remainder cells, ensure all 7 fish types spawn in triples, and shuffle items.
- Added `CheckAndExplodeHolderMatches()` and `ShiftHolderItemsLeft()` to handle holder row matching.
- Added `IsCellBlocked()` using distance checks ($dist < 0.95f$) against upper layer tiles, and `UpdateBlockedVisuals()` to update board tile dimming.

### `Assets/Scripts/Controllers/BoardController.cs`
- Replaced legacy 2D drag/swap logic with single-click 3D raycasting for unblocked tiles.
- Added click handling for Time Attack mode to return holder items back to `item.OriginalCell`.
- Added `ReturnItemToBoardCoroutine()` to animate returning items and shift remaining holder items left.
- Added `SelectAutoplayMove()` candidate move scoring AI for Autoplay Win mode.
- Bypassed holder-full game over checks when running in Time Attack mode.

### `Assets/Scripts/Controllers/GameManager.cs`
- Added `TIME_ATTACK` to `eLevelMode` and `GAME_WIN` to `eStateGame`.
- Added `m_isEnding` flag to guard against simultaneous Win and Loss calls.
- Added `LoadLevelTimeAttack()` and `LoadLevelAutoPlay()` helper methods.

### `Assets/Scripts/Controllers/LevelTime.cs` & `LevelMoves.cs`
- Updated `LevelTime` to format text as `TIME:\n60s` and ensured its Text object is active.
- Hid the move counter in `LevelMoves`.

### `Assets/Scripts/UI/UIMainManager.cs` & `UIPanelGameOver.cs`
- Added routing for Time Attack and Autoplay level loads.
- Updated `ShowGameOverPanel()` to match outcome panels by name (`PanelWin` vs `PanelGameOver`).

### `Assets/Scripts/UI/UIPanelGame.cs`
- Added `ShowLevelCondition()` helper to toggle top UI condition text visibility.

### `Assets/Scripts/UI/UIPanelMain.cs`
- Added `btnPlay`, `btnAutoWin`, and `btnAutoLose` fields prioritizing Inspector assignments.
- Added `EnsureButtonText()` helper to auto-create dynamic centered text components with best-fit scaling.
- Configured 4 vertical buttons on the Home screen.

---

## How to Test in Unity

1. Open `Assets/Scenes/Game.unity`.
2. Press **Play**.
3. Choose a mode from the Home screen:
   - **PLAY**: Standard manual matching.
   - **TIME ATTACK**: 60s timer mode (click holder items to send them back to the board).
   - **AUTOPLAY (WIN)**: Watch the AI solve the board.
   - **AUTO LOSE**: Watch the AI demo a loss state.
