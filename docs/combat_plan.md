# Clair Obscur: Expedition 33 — Combat System Architecture & Plan

## 1. Executive Summary & Objective

This document defines the complete technical and gameplay architecture for recreating the hybrid turn-based / active-reaction combat system of **"Clair Obscur: Expedition 33"** (Sandfall Interactive) in Unity 6 (URP).

The core philosophy of this combat system is **"Active Turn-Based"**:
1. **Strategic Layer (Turn-Based)**: Timeline initiative, AP management, elemental affinities, Break gauge, character-specific stances/meters, and Picto/Lumina builds.
2. **Execution Layer (Real-Time Reactive)**:
   - **Defense**: Real-time Dodge, Parry (with tight sweet spots and counterattacks), and Jump over ground sweeps.
   - **Offense**: Timed follow-ups (QTE rings) during skills and real-time Free Aim targeting enemy weak points.
3. **Game Feel ("Juice")**: Micro hit-stops, camera shakes, slow-motion on perfect parries, dynamic combat camera, and acoustic/visual telegraph clarity.

---

## 2. Mechanic Inventory

| Mechanic | Status | Scope Justification |
| :--- | :--- | :--- |
| **Timeline Turn Order** | **In Scope** | Agility-driven initiative queue with visible turn bar and delay/advance manipulations. |
| **Active Defense (Parry)** | **In Scope** | Tight timing window (~100–120ms); rewards full damage negation, counter-strike, and Gradient charge. |
| **Active Defense (Dodge)** | **In Scope** | Wider timing window (~200–250ms); evades damage with evasive repositioning. |
| **Active Defense (Jump)** | **In Scope** | Reactive defense specifically for attacks flagged as `GroundSweep` / low attacks. |
| **Timed Offensive Inputs (QTE)** | **In Scope** | Expanding/shrinking timing ring during attack execution; sweet spot awards crit/extra hit. |
| **Free Aim Mode** | **In Scope** | Real-time over-the-shoulder weak point targeting with limited timer/stamina and bullet AP cost. |
| **Action Points (AP)** | **In Scope** | Shared party pool, generated via basic attacks and defense, consumed by skills and Free Aim. |
| **Break & Stun System** | **In Scope** | Secondary enemy posture bar; broken enemies take massive bonus damage and forfeit turns. |
| **Gradient Super Attacks** | **In Scope** | Meter built from successful parries/combos, spent on high-impact cinematic attacks. |
| **Gustave Mechanic (Overcharge)** | **In Scope** | Build charge pips via offensive skills; unleash overcharge burst for exponential damage. |
| **Maelle Mechanic (Stances)** | **In Scope** | Stance switching (Offensive / Defensive / Virtuoso) modifying AP efficiency and parry windows. |
| **Pictos & Luminas Build System**| **Simplified** | ScriptableObject equip loadout: Pictos offer passive perks; mastered Pictos unlock as Lumina budget perks. |
| **Elemental Affinities** | **Simplified**| Weakness (+50% dmg, +Break), Neutral, Resistance (-50%), and Immune. (4 elements: Physical, Fire, Ice, Light). |
| **Character Roster (Rest)** | **Out of Scope**| Lune, Sciel, Monoco reserved for post-slice expansion; Gustave and Maelle provide core diversity. |
| **Open World Encounter Transitions**| **Out of Scope**| Dedicated battle arena / test encounter scene; exploration overworld deferred to later project phases. |

---

## 3. System Architecture & Boundaries

The architecture enforces strict separation of concerns, composition over inheritance, and event-driven decoupling adhering to `AGENTS.md`. Core calculation logic is pure C# (unit-testable without Unity engine dependencies), bridged to MonoBehaviours via interface adapters.

```
+---------------------------------------------------------------------------------------+
|                                    BATTLE MANAGER                                     |
|  - Controls overall battle flow & coordinates subsystems                              |
+-------------------------------------------+-------------------------------------------+
                                            |
         +----------------------------------+----------------------------------+
         |                                  |                                  |
         v                                  v                                  v
+-----------------------+      +-----------------------+      +-----------------------+
|  BATTLE STATE MACHINE |      |    TIMELINE ENGINE    |      |    PARTY & AP POOL    |
| - Intro               |      | - Initiative Queue    |      | - Shared AP (0..MAX)  |
| - TurnStart           |      | - Turn Order Calc     |      | - Gradient Meter      |
| - PlayerSelect        |      | - Delay / Haste       |      | - Character States    |
| - ActionExecute       |      +-----------------------+      +-----------------------+
| - EnemyTelegraph      |
| - ActiveDefenseWindow |
| - TurnEnd / Victory   |
+-----------+-----------+
            |
            | Dispatches Combat Events
            v
+---------------------------------------------------------------------------------------+
|                                   SUBSYSTEM LAYERS                                    |
|                                                                                       |
|  +--------------------+   +--------------------+   +-------------------------------+  |
|  |   COMBAT ENGINE    |   |   DEFENSE ENGINE   |   |        FREE AIM ENGINE        |  |
|  | - Damage Formula   |   | - Timing Window    |   | - Over-the-shoulder Camera    |  |
|  | - Break Gauge Calc |   | - Parry / Dodge    |   | - Weak-Point Raycast Colliders|  |
|  | - Status Effects   |   | - Input Buffer     |   | - Countdown / Shot Ammo       |  |
|  +--------------------+   +--------------------+   +-------------------------------+  |
|                                                                                       |
|  +--------------------+   +--------------------+   +-------------------------------+  |
|  |  GAME FEEL ENGINE  |   |    CAMERA RIG      |   |            UI LAYER           |  |
|  | - Hit-Stop / Shake |   | - Battle Overview  |   | - Timeline HUD & AP Pips      |  |
|  | - Slow-Mo on Parry |   | - Action Framing   |   | - Command Ring & QTE Prompts  |  |
|  | - Floating Numbers |   | - Free Aim Cam     |   | - Timing Visualizer Bar       |  |
|  +--------------------+   +--------------------+   +-------------------------------+  |
+---------------------------------------------------------------------------------------+
```

### Communication Contracts
- **Events Over Singletons**: Subsystems broadcast domain events via `CombatEvents` (e.g., `OnAttackTelegraphed`, `OnDefenseEvaluated`, `OnDamageApplied`, `OnBreakStateChanged`, `OnTurnShifted`).
- **Input System Integration**: New Unity Input System maps actions (`Navigate`, `Confirm`, `Dodge`, `Parry`, `Jump`, `FreeAimFire`) directly into the `InputBuffer` without polling state inside view classes.

---

## 4. Data Schemas (ScriptableObjects)

Designers configure content without touching code:

1. **`CombatActorDataSO`**: Base stats (MaxHP, Agility, BaseAttack, Defense, BreakThreshold), elemental affinities, archetype type (Gustave/Maelle/Enemy).
2. **`SkillDefinitionSO`**: AP cost, targeting rules (Single, All, Self), damage multipliers, Break damage, QTE configuration (window duration, sweet spot offset, bonus damage multiplier), animations and VFX keys.
3. **`EnemyAttackPatternSO`**: Defines an enemy attack sequence:
   - `telegraphDuration`: Windup anticipation time in seconds.
   - `attackType`: `Standard`, `GroundSweep` (Jump required), `Unblockable` (Dodge only).
   - `hitWindows`: Array of timing strikes (offset, parryWindowDuration, dodgeWindowDuration, damage). Supports multi-hit attacks.
   - `feintDelay`: Optional rhythm alteration/stutter.
4. **`StatusEffectDefinitionSO`**: ID, tick trigger (`TurnStart`, `TurnEnd`, `OnHit`), duration in turns, stat modifications, stacking rules.
5. **`PictoDefinitionSO` & `LuminaDefinitionSO`**: Passive stat modifiers (e.g., +15% Parry Window, +1 AP on counterattack, +20% Break damage).

---

## 5. Reactive Timing Model & Game Feel Pipeline

### Timing Windows & Input Buffering
- **Input Buffer**: When a defensive button is pressed, the input is timestamped and buffered for `50ms`. If the active window opens within that buffer, the input registers cleanly.
- **Timing Windows (Configurable per attack)**:
  - **Parry Window**: Sweet spot centered at hit frame: `[t_hit - 60ms, t_hit + 60ms]`.
  - **Dodge Window**: Generous window: `[t_hit - 150ms, t_hit + 100ms]`.
  - **Jump Window**: Active airborne window for `GroundSweep` telegraphs.
- **Defense Outcomes**:
  - `PerfectParry`: Negates 100% damage, triggers 0.15s hit-stop, camera punch, +2 AP, +15 Gradient, and automatic counter-attack.
  - `Dodge`: Negates 100% damage, triggers evasive side-step animation and +1 AP.
  - `Miss / Failed Timing`: Takes full damage, hit reaction animation, Break gauge vulnerability.

### Game Feel ("Juice") Specifications
1. **Hit-Stop**: Coroutine pauses animation/time scale (`Time.timeScale = 0f`) for `0.08s` on regular hits, `0.15s` on crits/parries using unscaled time.
2. **Camera Shake & Framing**: URP camera impulse on hit; dynamic framing zooms into attacker and defender during QTE and Parry sequences.
3. **Timing Visualizer (Debug & Learning Tool)**: On-screen bar showing the attack rhythm timeline, approach marker, and color-coded zones (Green = Parry, Yellow = Dodge, Red = Too early/late).

---

## 6. Implementation Milestones

### Milestone 1: Turn Foundation & Basic 1v1 Loop
- **Objective**: Operational turn timeline, basic attack selection, and animation state transitions.
- **Deliverables**: Battle state machine, timeline initiative, Player & Enemy actors using `Idle.fbx`, basic attack execution with `Surprise Uppercut.fbx`, hit reaction with `Zombie Reaction Hit.fbx`, damage calculation, and basic HUD (HP bars + Turn timeline).
- **Verification**: User can start battle, take turns, damage enemy, defeat enemy, or get defeated.

### Milestone 2: Active Defense (Parry, Dodge, Jump)
- **Objective**: Real-time reaction layer during enemy turns.
- **Deliverables**: `EnemyAttackPatternSO` system with telegraph phase, `InputBuffer` and `TimingEngine`, Dodge with `Standing Dodge Right.fbx`, Jump with `Jump.fbx`, Parry with hit-stop / audio-visual flash, counter-attack trigger. On-screen Timing Visualizer HUD.
- **Verification**: Enemy attacks with single and multi-hit telegraphs; player can reliably dodge, parry, or jump, receiving instant visual/text feedback.

### Milestone 3: AP Economy, Skills & QTE Timed Offense
- **Objective**: Team AP pool, offensive skill selection, and attack QTE rings.
- **Deliverables**: Shared AP meter, Skill menu, shrinking QTE circle widget on attack strike, timing-based bonus damage/Break, floating damage numbers.
- **Verification**: Player spends AP to cast skills, hits QTE timing for critical strikes, and generates AP through basic attacks and parries.

### Milestone 4: Free Aim Mode & Weak Points
- **Objective**: Real-time over-the-shoulder weak point shooting.
- **Deliverables**: Camera blend to over-the-shoulder, crosshair HUD, 4-second countdown, AP-cost bullet firing, enemy hitboxes with designated Weak Point colliders (e.g., Head vs Body), bonus Break damage on precision hit.
- **Verification**: In command menu, select Free Aim, aim at enemy head, fire shots before timer expires, confirm weak point crit.

### Milestone 5: Break System, Statuses & Character Mechanics (COMPLETED & VERIFIED)
- **Objective**: Posture break and unique mechanics for Gustave (Overcharge arm) & Maelle (Stances).
- **Deliverables**:
  - Break gauge for enemies (`BreakMeter.cs`); "Broken" posture state conferring turn skip and 1.75x damage multiplier.
  - Gustave: Overcharge mechanical arm meter (0 to 3 charges), gaining charges on Perfect Parry and QTEs, discharging in high-damage `Overcharge Cleave`.
  - Stance toggle (`Balanced`, `Offensive`, `Defensive`, `Virtuoso`) with custom damage, defense, and parry window multipliers.
  - Status effects: Burn (DOT), Haste (Speed up), Weaken (Damage down), Vulnerable (Incoming damage up).
  - Combat HUD updated with dedicated Enemy Break bar, Player Stance indicator, Overcharge pip gauge, and active Status tags.
  - 28 unit tests passing with 0 failures in `Expedition33.Tests.Editor`.
- **Verification**: Verified in PlayMode with posture shatter, skipped turns, Overcharge surges, and Stance cycling.

### Milestone 6: Pictos/Luminas Build System & Full Battle Arena Polish (COMPLETED & VERIFIED)
- **Objective**: Build crafting loadouts, Gradient attacks, camera polish, and end-to-end battle flow.
- **Deliverables**:
  - **Picto & Lumina Build System**: Data-driven ScriptableObjects (`PictoDefinitionSO.cs`, `LuminaDefinitionSO.cs`) and runtime budget manager (`PictoLoadout.cs`). Enforces Lumina point cap, aggregates stat modifiers (Attack, Defense, Speed, HP) and passives (Life Steal, Parry Window extension, AP per turn, Gradient on defense/hit, starting Overcharge pips).
  - **Gradient Super Attack**: Team super meter (`GradientMeter.cs`) charging via attacks, successful parries, dodges, and incoming damage. When fully charged (100%), unlocks the high-impact "GRADIENT ART" command triggering "EXPEDITION ARTS: LUMIERE ECLIPSE" with multi-hit cinematic camera zoom, heavy break impact (80 break), and critical damage (3.6x).
  - **End-to-End Battle Flow**: `BattleResultScreen.cs` with animated Victory and Defeat overlays, punch-scaling headlines, "RETRY BATTLE" (instantly reloads battle loop) and "EXIT" buttons.
  - **In-Game Debug/Cheat Panel**: `DebugCheatPanel.cs` toggled via `[F12]`, featuring: instant AP fill (+6 AP), Overcharge fill (+3 pips), Force Break (immediate enemy posture shatter), Fill Gradient (100% ultimate charge), God Mode toggle (immunity to damage), and Parry Window adjuster (2x wide window vs 1x normal).
  - **Complete Automated Verification**: 35 EditMode unit tests across all combat milestones passing with 0 failures in `Expedition33.Tests.Editor`.
- **Verification**: Verified end-to-end flow from turn foundation, reactive defense, AP skills, Free Aim weak points, posture break, to Picto build-crafting, Gradient ultimate attacks, and victory/defeat resolution.

---

## 7. Risks & Mitigation

1. **Input Latency / Framerate Fluctuations**:
   - *Risk*: Fixed-window timing feels unfair if framerate drops.
   - *Mitigation*: Windows evaluated using `Time.unscaledTime` and delta time stamps rather than frame count; generous pre-input buffering (50ms).
2. **Animation vs. Logic Synchronization**:
   - *Risk*: Animations de-sync from logic timing windows.
   - *Mitigation*: Attack patterns are data-driven timelines where animations are keyed to the timing window events, not vice versa.
3. **Cluttered UI / Obscured Telegraphs**:
   - *Risk*: High visual noise during fast-paced parries.
   - *Mitigation*: Clean high-contrast indicator shapes (rings, approach markers) positioned above target or on centered HUD.

---

## 8. Asset & Placeholder Strategy

- **3D Characters & Animations**:
  - Utilize the user-provided FBX assets in `Assets/_Projects/Models/`:
    - `Idle.fbx` -> Idle stance for Player and Enemy.
    - `Surprise Uppercut.fbx` -> Attack / Counter-attack.
    - `Standing Dodge Right.fbx` -> Dodge maneuver.
    - `Jump.fbx` -> Jump evasion.
    - `Zombie Reaction Hit.fbx` -> Hit reaction / stagger.
    - `Zombie Death.fbx` -> Death animation.
  - Blend/retarget with Unity Animator or DOTween Transform offsets for seamless transitions.
- **VFX & Audio**:
  - Procedural sound synthesized via `AudioClip.Create` (crisp parry ding, dodge swoosh, impact thud, countdown tick) so the game feels tactile out-of-the-box.
  - URP particle/quad flashes for parry sparks and QTE timing rings.
- **Arena**: Clean modular floor/lighting environment instantiated in a dedicated test scene (`BattleSandbox.unity`).
