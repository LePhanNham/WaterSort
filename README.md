# 🌊 Water Sort Puzzle

A polished Unity-based implementation of the classic Water Sort Puzzle game with modern features, smooth animations, and persistent level progression.

## 📋 Overview

Water Sort Puzzle is a relaxing and challenging logic game where players must sort colored water into tubes until each tube contains only one color. The game features intuitive controls, beautiful animations, and progressively challenging levels.

## ✨ Features

### Core Gameplay
- **Intuitive Pour Mechanics** - Click to select and pour water between tubes
- **Visual Feedback** - Smooth water stream animations with DOTween
- **Win/Lose Detection** - Automatic win condition checking and deadlock detection
- **Undo System** - Revert moves when stuck
- **Progressive Difficulty** - Increasing number of colors as levels advance

### UI/UX
- **Home Screen** - Clean main menu with level display
- **Level Progression** - Persistent save system tracks current level
- **Win/Lose Popups** - Animated popup dialogs with options
- **Settings Panel** - Toggle sound effects and background music
- **Responsive Layout** - Grid-based tube positioning with animations

### Audio
- **Sound Effects** - Pour sounds, button clicks, win/lose sounds
- **Background Music** - Ambient music with toggle control
- **Smart Audio** - Pour sound stops when animation completes

### Technical Features
- **Level Data Persistence** - PlayerPrefs-based save system
- **Object Pooling** - Efficient water stream effect reuse
- **State Machine** - Clean game state management
- **MVC Architecture** - Separation of data, view, and controller logic
- **Deadlock Detection** - Automatic detection of unwinnable states

## 🎮 How to Play

1. **Select a Tube** - Click on a tube to select it
2. **Pour Water** - Click on another tube to pour water
3. **Match Colors** - Only pour onto same color or empty tubes
4. **Complete Levels** - Sort all colors into separate tubes
5. **Use Undo** - Revert moves if you make a mistake

### Rules
- Can only pour if top colors match or target tube is empty
- Cannot pour into a full tube
- Cannot pour from an empty tube
- Cannot pour from a completed tube (single color, full)

## 🛠️ Tech Stack

- **Engine**: Unity 2020+
- **Language**: C#
- **Animation**: DOTween
- **UI**: Unity UI (uGUI) with TextMeshPro
- **Data Storage**: PlayerPrefs
- **Patterns**: Singleton, Command (Undo), State Machine, MVC

## 📂 Project Structure

```
Assets/_GAME/
├── Scripts/
│   ├── Manager/
│   │   ├── GameManager.cs          # Core game manager (Singleton)
│   │   ├── UIManager.cs             # UI control and events
│   │   ├── SoundManager.cs          # Audio management
│   │   └── LevelDataManager.cs      # Level persistence
│   ├── Controller/
│   │   └── GameplayController.cs    # Game logic and flow
│   ├── View/
│   │   ├── TubeView.cs              # Individual tube visualization
│   │   └── WaterStreamEffect.cs     # Pour animation effects
│   ├── Data/
│   │   ├── TubeData.cs              # Tube state data
│   │   ├── GameConfig.cs            # Level configuration
│   │   └── RuleValidator.cs         # Game rule validation
│   ├── Systems/
│   │   └── UndoSystem.cs            # Command pattern for undo
│   └── GameStates.cs                # State machine states
```

## 🚀 Setup Instructions

### Prerequisites
- Unity 2020.3 or higher
- DOTween (Free or Pro version)
- TextMeshPro (included with Unity)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/sonat_intern_test.git
   cd sonat_intern_test-main
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Add" and select the project folder
   - Open the project

3. **Install Dependencies**
   - DOTween: Window → Package Manager → My Assets → DOTween
   - TextMeshPro: Import TMP Essential Resources if prompted

4. **Play**
   - Open the main scene in `Assets/_GAME/Scenes/`
   - Press Play in Unity Editor

## 🎨 Customization

### Adjusting Difficulty
Edit `GameConfig` ScriptableObject:
- `totalTubes` - Number of tubes in level
- `tubeCapacity` - Maximum liquid per tube
- `startingColors` - Colors in first level
- `colorsPerLevel` - Colors added per level
- `maxColors` - Maximum colors in any level

### Visual Settings
- **Water Stream Width**: `WaterStreamEffect.cs` → sizeDelta (currently 120f)
- **Animation Speed**: `TubeView.cs` → adjust DOTween durations
- **Grid Layout**: Adjust in Scene → TubesContainer → GridLayoutGroup

## 🔧 Key Systems

### Level Progression
```csharp
// Current level saved to PlayerPrefs
LevelDataManager.Instance.currentLevel
// Complete level and advance
LevelDataManager.Instance.CompleteLevel()
```

### Win/Lose Conditions
- **Win**: All tubes contain single colors (completed)
- **Lose**: Deadlock detected (no valid moves remaining)

### Undo System
- Uses Command pattern
- Stores pour operations: source, target, amount, color
- Reverses moves with full state restoration

## 📊 Performance Optimizations

- **Water Stream Pooling** - Reuses GameObject instead of spawning/destroying
- **Grid Layout** - Disabled after initial positioning for smooth animations
- **DOTween Efficiency** - Kill tweens on destroy to prevent memory leaks
- **Minimal Allocations** - Dictionary for grid positions, reused collections

## 🎯 Future Enhancements

- [ ] Star rating system (moves-based)
- [ ] Level select screen
- [ ] Hint system
- [ ] Daily challenges
- [ ] Leaderboards
- [ ] More visual themes
- [ ] Particle effects
- [ ] Achievement system

## 🐛 Known Issues

- None currently reported

## 📝 License

This project is part of an internship test assignment.

---

Built with ❤️ using Unity