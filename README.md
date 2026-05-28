# 🚔 Police Chasing 2D

<div align="center">

![Game Banner](https://github.com/Chandan-Baskey/PoliceChasing-2Game/blob/64377d1d47b14c746a35ac5e2db2d528d2c645ec/%7B77884281-BEEE-4E6F-88F5-377ED839269B%7D.png)

<br/>

> **A fast-paced top-down 2D escape game built in Unity — collect cash, dodge cops, survive as long as you can.**

<br/>

![Unity](https://img.shields.io/badge/Unity-2022%2B-black?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mobile-blue?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Active%20Development-orange?style=for-the-badge)

</div>

---

## 📖 Table of Contents

- [About the Game](#-about-the-game)
- [Gameplay](#-gameplay)
- [Features](#-features)
- [Game Architecture](#-game-architecture)
- [Script Reference](#-script-reference)
- [Game Systems Deep Dive](#-game-systems-deep-dive)
- [Difficulty Scaling](#-difficulty-scaling)
- [Controls](#-controls)
- [Installation & Setup](#-installation--setup)
- [Project Structure](#-project-structure)
- [Prefabs](#-prefabs)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)

---

## 🎮 About the Game

**Police Chasing 2D** is an arcade-style top-down 2D game developed in **Unity** using **C#**. The player controls a getaway car, racing through the streets collecting cash bags while evading an increasingly aggressive police pursuit. The longer you survive and the more cash you collect, the harder it gets — more cops spawn faster, and they chase smarter.

The game blends simple one-touch/keyboard controls with escalating tension through a dynamic difficulty system, making each run feel unique and progressively more challenging.

---

## 🕹️ Gameplay

```
┌─────────────────────────────────────────────────────────┐
│                   GAME LOOP                             │
│                                                         │
│   Start ──► Drive Forward ──► Collect Cash              │
│                │                    │                   │
│                │              Score increases            │
│                │                    │                   │
│           Police Spawn         Difficulty ramps up       │
│                │                    │                   │
│           Police Chase         More cops, faster spawn  │
│                │                                        │
│           Caught by Police ──► Game Resets              │
└─────────────────────────────────────────────────────────┘
```

The player's car **always moves forward** automatically — your only job is to **steer** and **survive**. Cash bags spawn ahead of you, police cars spawn behind and chase you with predictive AI.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🚗 **Auto-Drive Mechanics** | Car always moves forward; player controls only rotation |
| 🧠 **Predictive Police AI** | Cops predict your future position and intercept, not just follow |
| 💰 **Dynamic Cash Spawning** | Cash bags appear ahead of the player randomly across the road |
| 📈 **Progressive Difficulty** | Speed, spawn rates, and police aggression scale with your score |
| 🚔 **Multi-Police Spawning** | Up to 4 police cars can chase simultaneously |
| 🚦 **Checkpoint System** | Visual checkpoints with respawn points and one-time activation |
| 🌀 **Portal Teleportation** | Bidirectional portals with anti-loop protection |
| 📷 **Smooth Camera Follow** | Camera smoothly tracks player with configurable clamped bounds |
| 🎨 **Siren Light Flashing** | Police cars flash red/blue siren lights dynamically |
| 🏁 **Scene Progression** | Finish triggers load the next scene automatically |

---

## 🏗️ Game Architecture

The game is built around a clean **singleton + component** architecture:

```
GameManager (Singleton)
    ├── SpawnPoliceLoop()       ← timed coroutine
    ├── SpawnCashLoop()         ← timed coroutine
    └── OnCashCollected()       ← triggers difficulty scaling

Player (Singleton)
    ├── Movement (FixedUpdate)  ← physics-based forward drive
    ├── Rotation (Update)       ← keyboard input
    └── Collision Handling      ← cash collection, police hit

PoliceAI (per instance)
    ├── Predictive Steering     ← target = player pos + velocity * time
    └── Siren Flash             ← color toggle coroutine

GameControl (per Player)
    ├── Checkpoint tracking
    └── Death / Respawn coroutine

Supporting Systems
    ├── Checkpoint.cs           ← one-shot activation
    ├── Portal.cs               ← bidirectional teleport
    └── CameraControl.cs        ← SmoothDamp follow with bounds
```

---

## 📜 Script Reference

### `Player.cs`
The core player controller. Uses Unity's **new Input System** (`UnityEngine.InputSystem`).

```csharp
// Always drives forward using physics
rb.linearVelocity = transform.up * moveSpeed;

// Rotates on A/D keys
if (Keyboard.current.aKey.isPressed)  rot =  1;
if (Keyboard.current.dKey.isPressed)  rot = -1;
transform.Rotate(0, 0, rotateSpeed * rot);
```

**Key Properties:**
- `moveSpeed` — forward velocity magnitude
- `rotateSpeed` — degrees rotated per frame
- `Player.Instance` — global singleton reference used by AI and managers

**Collision Handling:**
- `Tag: Cash` → destroys the cash bag, triggers score
- `Tag: Police / PoliceBlock` → reloads current scene (game over)

---

### `PoliceAI.cs`
Implements **predictive pursuit AI** — the police don't just chase where you are, they aim where you *will be*.

```csharp
// Predict future player position
Vector2 predictedPos = (Vector2)playerTransform.position
                      + playerRb.linearVelocity * predictionTime;

// Rotate toward predicted position
float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                    rotateSpeed * Time.fixedDeltaTime);

// Speed = base + difficulty bonus from GameManager
float currentSpeed = chaseSpeed + GameManager.Instance.difficultyBonus;
rb.linearVelocity = transform.up * currentSpeed;
```

**Tunable Parameters:**
| Parameter | Default | Effect |
|---|---|---|
| `chaseSpeed` | 5f | Base movement speed of police |
| `rotateSpeed` | 120°/s | How quickly police turns to face player |
| `predictionTime` | 0.4s | How far ahead police predicts player position |

**Siren System:** Alternates between red and blue every 0.2 seconds using a timer, referencing an assigned `SpriteRenderer`.

---

### `GameManager.cs`
Central game controller, handles all **spawning logic** and **difficulty scaling**.

```csharp
// Police spawn: behind player at spawnDistance
Vector3 spawnPos = Player.Instance.transform.position
                 - Player.Instance.transform.up * spawnDistance;

// Cash spawn: ahead of player, random lateral offset
float randomX = Random.Range(-roadHalfWidth, roadHalfWidth);
Vector3 spawnPos = Player.Instance.transform.position
                 + Player.Instance.transform.up * spawnAheadDist
                 + Vector3.right * randomX;
```

**Difficulty Scaling via `OnCashCollected(int score)`:**
```csharp
difficultyBonus   = (score / 3) * 1.2f;          // police speed bonus
cashSpawnInterval = Mathf.Max(1f, 2.5f - score * 0.1f);  // cash spawns faster
spawnInterval     = Mathf.Max(4f, 8f - score * 0.3f);    // police spawns faster
```

---

### `GameControl.cs`
Handles **player death and respawn** logic attached to the player object.

```csharp
IEnumerator Respawn(float duration)
{
    transform.localScale = Vector3.zero;       // visually hide player
    yield return new WaitForSeconds(duration); // wait 0.5s
    transform.position = checkpointPos;        // teleport to last checkpoint
    transform.localScale = Vector3.one;        // restore visibility
}
```

Triggers on `Tag: Obstacle` collision. On `Tag: Finish`, advances to the next scene using `SceneManager.LoadScene(buildIndex + 1)`.

---

### `Checkpoint.cs`
One-shot activation checkpoint with visual feedback.

```csharp
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        gameController.UpdateCheckpoint(respawnPoint.position);
        spriteRenderer.sprite = active;  // swap to active sprite
        coll.enabled = false;            // prevent re-triggering
    }
}
```

Each checkpoint has a separate `respawnPoint` Transform, allowing precise spawn placement independent of the checkpoint's visual position.

---

### `Portal.cs`
Bidirectional portal system with **loop prevention** using a `HashSet`.

```csharp
private HashSet<GameObject> portalObjects = new HashSet<GameObject>();

private void OnTriggerEnter2D(Collider2D collision)
{
    if (portalObjects.Contains(collision.gameObject)) return; // prevent loop

    // Tell destination portal to ignore this object temporarily
    destinationPortal.portalObjects.Add(collision.gameObject);
    collision.transform.position = destination.position;
}

private void OnTriggerExit2D(Collider2D collision)
{
    portalObjects.Remove(collision.gameObject); // re-enable after exit
}
```

---

### `PlayerControl.cs`
Alternative player controller (platformer-style) with **acceleration-based movement** and **wall detection flip**.

```csharp
// Smooth speed ramp using MoveTowards
float target = btnPressed ? 1f : 0f;
speedMultiplier = Mathf.MoveTowards(speedMultiplier, target,
                    acceleration * Time.fixedDeltaTime);

// Wall detection flips direction
bool isWallTouch = Physics2D.OverlapBox(wallCheckPoint.position,
                    wallCheckSize, 0f, wallLayer);
if (isWallTouch) Flip();
```

Supports **platform riding** by adding platform velocity to player velocity when `isOnPlatform` is true.

---

### `CameraControl.cs`
Smooth follow camera with **clamped world bounds**.

```csharp
Vector3 targetPosition = target.position + positionOffset;
targetPosition = new Vector3(
    Mathf.Clamp(targetPosition.x, xLimits.x, xLimits.y),
    Mathf.Clamp(targetPosition.y, yLimits.x, yLimits.y),
    -10
);
transform.position = Vector3.SmoothDamp(transform.position,
                        targetPosition, ref velocity, smoothTime);
```

Uses `Vector3.SmoothDamp` for butter-smooth camera follow. Called in `LateUpdate` to ensure it runs after all game objects have moved.

---

## 📊 Game Systems Deep Dive

### Police Spawn System

```
Time 0s         ──► Wait firstSpawnDelay (3s)
Time 3s         ──► Spawn Police #1 (behind player)
Time 3+8=11s    ──► Spawn Police #2
Time 19s        ──► Spawn Police #3
Time 27s        ──► Spawn Police #4 (maxPolice reached, no more)
                    (until one is destroyed/despawned)
```

Police are spawned **behind the player** using `transform.up` (the direction the car faces), ensuring they always start in a chasing position.

### Cash Spawn System

Cash bags spawn **ahead** of the player at `spawnAheadDist` units, with a random lateral offset within `±roadHalfWidth`. This ensures the player always has a reason to keep moving forward and the cash is naturally in their path.

### Difficulty Curve

| Cash Collected | Police Speed Bonus | Cash Spawn Interval | Police Spawn Interval |
|---|---|---|---|
| 0 | +0.0 | 2.5s | 8.0s |
| 5 | +2.0 | 2.0s | 6.5s |
| 10 | +4.0 | 1.5s | 5.0s |
| 15 | +6.0 | 1.0s | 4.0s (min) |
| 20+ | +8.0 | 1.0s (min) | 4.0s (min) |

---

## 🎮 Controls

### Top-Down Chase Mode (`Player.cs`)
| Input | Action |
|---|---|
| `A` Key | Rotate Left |
| `D` Key | Rotate Right |
| *(Car moves forward automatically)* | — |

### Platformer Mode (`PlayerControl.cs`)
| Input | Action |
|---|---|
| Mouse Hold / Tap | Move & accelerate |
| *(Auto wall-bounce)* | Flip direction on wall |

---

## 🚀 Installation & Setup

### Prerequisites
- **Unity 2022.3 LTS** or newer
- **TextMeshPro** package (included in Unity)
- **Unity Input System** package

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/Chandan-Baskey/PoliceChasing-2Game.git

# 2. Open in Unity Hub
#    File → Open Project → select cloned folder

# 3. Install required packages (if prompted)
#    Window → Package Manager
#    - Input System
#    - TextMeshPro

# 4. Open the main scene
#    Assets → Scenes → [Main Scene]

# 5. Press Play ▶
```

### Scene Setup Checklist
- [ ] `GameManager` GameObject exists with `GameManager.cs` attached
- [ ] `Player` GameObject tagged `"Player"` with `Player.cs` and `Rigidbody2D`
- [ ] `policePrefab` and `cashPrefab` assigned in `GameManager` Inspector
- [ ] Camera has `CameraControl.cs` with Player tag set
- [ ] Obstacles tagged `"Obstacle"`, finish zone tagged `"Finish"`

---

## 📁 Project Structure

```
PoliceChasing-2Game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player.cs           ← Top-down player controller
│   │   ├── PlayerControl.cs    ← Platformer player controller
│   │   ├── GameManager.cs      ← Spawning + difficulty system
│   │   ├── GameControl.cs      ← Death + respawn + scene flow
│   │   ├── PoliceAI.cs         ← Predictive pursuit AI
│   │   ├── Checkpoint.cs       ← One-shot checkpoint activation
│   │   ├── Portal.cs           ← Teleportation system
│   │   └── CameraControl.cs    ← Smooth bounded camera
│   ├── Prefabs/
│   │   ├── PoliceCar.prefab    ← Police vehicle with AI + physics
│   │   └── Cash Bag.prefab     ← Collectible cash with trigger
│   ├── Sprites/                ← Game art assets
│   └── Scenes/                 ← Unity scene files
├── ProjectSettings/
└── README.md
```

---

## 📦 Prefabs

### `Cash Bag.prefab`
- **Tag:** `Cash`
- **Components:** `SpriteRenderer`, `CapsuleCollider2D` (Is Trigger: ✅)
- **Behavior:** Destroyed on player contact, triggers score update

### `PoliceCar.prefab`
- **Tag:** `Police`
- **Components:** `SpriteRenderer`, `CapsuleCollider2D`, `Rigidbody2D`, `PoliceAI`
- **Physics:** Gravity Scale = 0 (top-down), Angular Damping = 0.05
- **Behavior:** Predictive chase AI, siren flash, speed scales with difficulty

---

## 🗺️ Roadmap

- [x] Core top-down driving mechanics
- [x] Predictive police AI
- [x] Dynamic difficulty scaling
- [x] Checkpoint + respawn system
- [x] Portal teleportation
- [x] Multi-police spawning
- [ ] Score UI with high score persistence
- [ ] Sound effects & background music
- [ ] Mobile touch steering
- [ ] Roadblock obstacles
- [ ] Multiple maps / environments
- [ ] Police helicopter variant
- [ ] Leaderboard system

---

## 🤝 Contributing

Contributions are welcome! Here's how to get started:

```bash
# Fork the repo, then:
git checkout -b feature/your-feature-name
# Make your changes
git commit -m "feat: describe your change"
git push origin feature/your-feature-name
# Open a Pull Request
```

Please ensure:
- Code follows existing naming conventions (PascalCase for classes, camelCase for fields)
- New MonoBehaviours include XML summary comments
- Public serialized fields use `[SerializeField]` where appropriate

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

Made with ❤️ and Unity by **Chandan Baskey**

⭐ Star this repo if you enjoyed it!

</div>
