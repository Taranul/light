using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Expedition33.Combat
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Actor Data")]
        [SerializeField] private CombatActorDataSO _playerData;
        [SerializeField] private CombatActorDataSO _enemyData;

        [Header("Scene Views")]
        [SerializeField] private CombatActorView _playerView;
        [SerializeField] private CombatActorView _enemyView;

        [Header("Subsystems")]
        [SerializeField] private CombatHUD _hud;
        [SerializeField] private CombatAudioPlayer _audioPlayer;
        [SerializeField] private GameFeelManager _gameFeel;
        [SerializeField] private InputBuffer _inputBuffer;
        [SerializeField] private TimingVisualizerUI _timingVisualizer;
        [SerializeField] private OffensiveQTEWidget _qteWidget;
        [SerializeField] private CombatCameraController _cameraController;
        [SerializeField] private FreeAimHUD _freeAimHUD;
        [SerializeField] private BattleResultScreen _resultScreen;
        [SerializeField] private DebugCheatPanel _cheatPanel;

        [Header("Skills & Patterns")]
        [SerializeField] private List<SkillDefinitionSO> _playerSkills;
        [SerializeField] private EnemyAttackPatternSO[] _enemyAttackPatterns;

        [Header("Pictos & Luminas Loadout")]
        [SerializeField] private List<PictoDefinitionSO> _equippedPictos;
        [SerializeField] private List<LuminaDefinitionSO> _equippedLuminas;

        private BattleStateMachine _stateMachine;
        private TurnTimeline _timeline;
        private ActionPointPool _apPool;
        private CombatActorStats _playerStats;
        private CombatActorStats _enemyStats;
        private BreakMeter _enemyBreakMeter;
        private OverchargeMeter _playerOverchargeMeter;
        private StanceController _playerStanceController;
        private StatusEffectController _playerStatusController;
        private StatusEffectController _enemyStatusController;
        private GradientMeter _gradientMeter;
        private PictoLoadout _playerLoadout;

        private bool _playerActionChosen;
        private CombatActionType _selectedAction;
        private SkillDefinitionSO _selectedSkill;
        private int _enemyTurnCounter;

        public CombatActorStats PlayerStats => _playerStats;
        public CombatActorStats EnemyStats => _enemyStats;
        public ActionPointPool APPool => _apPool;
        public BattleStateMachine StateMachine => _stateMachine;
        public BreakMeter EnemyBreakMeter => _enemyBreakMeter;
        public OverchargeMeter PlayerOverchargeMeter => _playerOverchargeMeter;
        public StanceController PlayerStanceController => _playerStanceController;
        public StatusEffectController PlayerStatusController => _playerStatusController;
        public StatusEffectController EnemyStatusController => _enemyStatusController;
        public GradientMeter GradientMeter => _gradientMeter;
        public PictoLoadout PlayerLoadout => _playerLoadout;
        public BattleResultScreen ResultScreen => _resultScreen;
        public DebugCheatPanel CheatPanel => _cheatPanel;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            _stateMachine = new BattleStateMachine(BattleState.Intro);
            _timeline = new TurnTimeline();
            _apPool = new ActionPointPool(maxAP: 6, startingAP: 2);

            // Setup player stats
            if (_playerData != null)
            {
                _playerStats = _playerData.CreateStats();
            }
            else
            {
                _playerStats = new CombatActorStats("Gustave", 120, 25, 6, 12, true);
            }

            // Setup enemy stats
            if (_enemyData != null)
            {
                _enemyStats = _enemyData.CreateStats();
            }
            else
            {
                _enemyStats = new CombatActorStats("Expedition Stalker", 90, 18, 4, 9, false);
            }

            // Setup default skills if none assigned
            if (_playerSkills == null || _playerSkills.Count == 0)
            {
                CreateFallbackSkills();
            }

            // Ensure fallback default patterns if none configured
            if (_enemyAttackPatterns == null || _enemyAttackPatterns.Length == 0)
            {
                CreateFallbackPatterns();
            }

            // Setup views
            Color playerTint = _playerData != null ? _playerData.TintColor : new Color(0.85f, 0.95f, 1f);
            Color enemyTint = _enemyData != null ? _enemyData.TintColor : new Color(1f, 0.55f, 0.55f);

            _playerView.Initialize(_playerStats, playerTint);
            _enemyView.Initialize(_enemyStats, enemyTint);

            _playerView.PlayIdle();
            _enemyView.PlayIdle();

            // Register into timeline
            _timeline.RegisterActor(_playerStats);
            _timeline.RegisterActor(_enemyStats);

            // Setup Milestone 5 Mechanics
            _enemyBreakMeter = new BreakMeter(maxBreak: 100, brokenDamageMultiplier: 1.75f);
            _playerOverchargeMeter = new OverchargeMeter(maxCharges: 3);
            _playerStanceController = new StanceController(CombatStance.Balanced);
            _playerStatusController = new StatusEffectController();
            _enemyStatusController = new StatusEffectController();

            _enemyBreakMeter.OnBreakChanged += (cur, max) =>
            {
                _hud?.UpdateEnemyBreak(cur, max, _enemyBreakMeter.IsBroken);
            };

            _enemyBreakMeter.OnBroken += () =>
            {
                _audioPlayer?.PlayBreakShatter();
                _gameFeel?.TriggerHitStop(true);
                _gameFeel?.TriggerCameraShake(2.0f);
            };

            _playerOverchargeMeter.OnOverchargeChanged += (cur, max) =>
            {
                _hud?.UpdatePlayerOvercharge(cur, max);
            };

            _playerStanceController.OnStanceChanged += stance =>
            {
                _hud?.UpdatePlayerStance(stance);
            };

            // Setup Milestone 6: Gradient Meter & Picto Loadout
            _gradientMeter = new GradientMeter(maxCharge: 100);
            _gradientMeter.OnGradientChanged += (cur, max) =>
            {
                _hud?.UpdateGradient(cur, max);
            };
            _gradientMeter.OnGradientReady += () =>
            {
                _audioPlayer?.PlayOverchargeSurge();
            };

            _playerLoadout = new PictoLoadout(maxLuminaPoints: 10);
            if (_equippedPictos != null && _equippedPictos.Count > 0)
            {
                foreach (var p in _equippedPictos)
                    _playerLoadout.TryEquipPicto(p);
            }
            else
            {
                CreateFallbackLoadout();
            }

            if (_equippedLuminas != null && _equippedLuminas.Count > 0)
            {
                foreach (var l in _equippedLuminas)
                    _playerLoadout.TryEquipLumina(l);
            }

            int startOvercharge = _playerLoadout.GetStartingOvercharge();
            if (startOvercharge > 0)
            {
                _playerOverchargeMeter.AddCharge(startOvercharge);
            }

            // Register Cheat Panel
            if (_cheatPanel != null)
            {
                _cheatPanel.Register(_apPool, _playerOverchargeMeter, _enemyBreakMeter, _gradientMeter, _playerStats);
            }

            // Connect Result Screen
            if (_resultScreen != null)
            {
                _resultScreen.OnRetryRequested += HandleRetryBattle;
            }

            // Connect HUD
            if (_hud != null)
            {
                _hud.SetupPlayerStatus(_playerStats, _apPool, _playerSkills);
                _hud.SetupEnemyStatus(_enemyStats);
                _hud.OnAttackSelected += HandlePlayerAttackSelected;
                _hud.OnSkillSelected += HandlePlayerSkillSelected;
                _hud.OnFreeAimSelected += HandlePlayerFreeAimSelected;
                _hud.OnGradientSelected += HandlePlayerGradientSelected;
                _hud.OnStanceToggleRequested += HandleStanceToggleRequested;

                _hud.UpdateEnemyBreak(_enemyBreakMeter.CurrentBreak, _enemyBreakMeter.MaxBreak, false);
                _hud.UpdatePlayerOvercharge(_playerOverchargeMeter.CurrentCharges, _playerOverchargeMeter.MaxCharges);
                _hud.UpdatePlayerStance(_playerStanceController.CurrentStance);
                _hud.UpdateGradient(_gradientMeter.CurrentCharge, _gradientMeter.MaxCharge);
            }

            RefreshTimelineHUD();

            StartCoroutine(BattleRoutine());
        }

        private void CreateFallbackLoadout()
        {
            var p1 = PictoDefinitionSO.Create(
                "Painter's Vigor",
                "Enhances physical strike force and builds extra Gradient on parry.",
                luminaCost: 2,
                atkBonus: 0.12f,
                defBonus: 0.08f,
                passive: PictoPassiveType.GradientOnParry,
                passiveValue: 15f
            );
            var p2 = PictoDefinitionSO.Create(
                "Cadence of Lumiere",
                "Extends parry window by 40ms and drains life on successful strikes.",
                luminaCost: 3,
                atkBonus: 0.05f,
                passive: PictoPassiveType.LifeSteal,
                passiveValue: 0.20f
            );
            var l1 = LuminaDefinitionSO.Create(
                "Lumina of Flow",
                "Generates +1 AP on player turn start.",
                cost: 2,
                effect: LuminaEffectType.APPerTurn,
                value: 1f
            );
            var l2 = LuminaDefinitionSO.Create(
                "Lumina of Radiance",
                "Builds +5 bonus Gradient charge on every hit.",
                cost: 1,
                effect: LuminaEffectType.GradientOnHit,
                value: 5f
            );

            _playerLoadout.TryEquipPicto(p1);
            _playerLoadout.TryEquipPicto(p2);
            _playerLoadout.TryEquipLumina(l1);
            _playerLoadout.TryEquipLumina(l2);
        }

        private void CreateFallbackSkills()
        {
            _playerSkills = new List<SkillDefinitionSO>
            {
                SkillDefinitionSO.CreateSkill("Overcharge Cleave", 3, 2.2f, "Heavy blow discharging Overcharge for bonus break force.", breakDamage: 50, status: StatusEffectType.Burn, statusDuration: 2, consumesOvercharge: true),
                SkillDefinitionSO.CreateSkill("Swift Flurry", 2, 1.5f, "Quick dual-hit flurry inflicting Weaken.", breakDamage: 25, status: StatusEffectType.Weaken, statusDuration: 2, consumesOvercharge: false)
            };
        }

        private void CreateFallbackPatterns()
        {
            var p1 = EnemyAttackPatternSO.CreateDefaultPattern("Stalker Claw Combo", AttackTelegraphType.Standard, 1.0f, 22);
            var p2 = EnemyAttackPatternSO.CreateDefaultPattern("Ground Shockwave", AttackTelegraphType.GroundSweep, 1.1f, 26);
            _enemyAttackPatterns = new[] { p1, p2 };
        }

        private bool CanSelectPlayerAction()
        {
            return _stateMachine != null
                && _stateMachine.CurrentState == BattleState.PlayerTurn
                && !_playerActionChosen;
        }

        private void HandlePlayerAttackSelected()
        {
            if (!CanSelectPlayerAction())
                return;

            _audioPlayer?.PlayButtonClick();
            _selectedAction = CombatActionType.Attack;
            _selectedSkill = null;
            _playerActionChosen = true;
        }

        private void HandlePlayerSkillSelected(SkillDefinitionSO skill)
        {
            if (!CanSelectPlayerAction() || skill == null)
                return;

            _audioPlayer?.PlayButtonClick();
            if (_apPool.CanSpend(skill.APCost))
            {
                _selectedAction = CombatActionType.Skill;
                _selectedSkill = skill;
                _playerActionChosen = true;
            }
        }

        private void HandlePlayerFreeAimSelected()
        {
            if (!CanSelectPlayerAction())
                return;

            _audioPlayer?.PlayButtonClick();
            if (_apPool.CanSpend(1))
            {
                _selectedAction = CombatActionType.FreeAim;
                _selectedSkill = null;
                _playerActionChosen = true;
            }
        }

        private void HandlePlayerGradientSelected()
        {
            if (!CanSelectPlayerAction())
                return;

            _audioPlayer?.PlayButtonClick();
            if (_gradientMeter != null && _gradientMeter.IsReady)
            {
                _selectedAction = CombatActionType.GradientAttack;
                _selectedSkill = null;
                _playerActionChosen = true;
            }
        }

        private void HandleRetryBattle()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleStanceToggleRequested()
        {
            var next = _playerStanceController.CycleNextStance();
            _audioPlayer?.PlayStanceSwitch();
            FloatingCombatText.Spawn(_playerView.transform.position + Vector3.up * 1.8f, $"STANCE: {next}", Color.white, 1.2f);
        }

        private void ApplyDamageAndBreakToEnemy(
            float baseDamageMultiplier,
            int baseBreakAmount,
            bool isCrit,
            string actionLabel,
            Vector3 hitPosition)
        {
            float stanceAtk = _playerStanceController != null ? _playerStanceController.GetAttackMultiplier() : 1.0f;
            float statusAtk = _playerStatusController != null ? _playerStatusController.GetAttackMultiplier() : 1.0f;
            float incomingMod = _enemyStatusController != null ? _enemyStatusController.GetIncomingDamageMultiplier() : 1.0f;
            float brokenMod = (_enemyBreakMeter != null && _enemyBreakMeter.IsBroken) ? _enemyBreakMeter.BrokenDamageMultiplier : 1.0f;
            float pictoAtk = _playerLoadout != null ? (1.0f + _playerLoadout.GetTotalAttackBonus()) : 1.0f;

            float combinedMultiplier = baseDamageMultiplier * stanceAtk * statusAtk * incomingMod * brokenMod * pictoAtk;

            DamageResult result = DamageCalculator.CalculateDamage(_playerStats, _enemyStats, combinedMultiplier, isCrit);
            _enemyStats.ApplyDamage(result.MitigatedDamage);

            // Life Steal passive from equipped Pictos
            if (_playerLoadout != null)
            {
                foreach (var picto in _playerLoadout.Pictos)
                {
                    if (picto.PassiveType == PictoPassiveType.LifeSteal && picto.PassiveValue > 0f)
                    {
                        int heal = Mathf.RoundToInt(result.MitigatedDamage * picto.PassiveValue);
                        if (heal > 0 && !_playerStats.IsDefeated)
                        {
                            _playerStats.Heal(heal);
                            FloatingCombatText.Spawn(_playerView.transform.position + Vector3.up * 1.8f, $"+{heal} HP", Color.green, 1.1f);
                        }
                    }
                }
            }

            // Build team Gradient charge on hit
            int gradHitBonus = _playerLoadout != null ? _playerLoadout.GetGradientOnHitBonus() : 0;
            _gradientMeter?.AddCharge(12 + gradHitBonus);

            bool brokenThisHit = _enemyBreakMeter != null && _enemyBreakMeter.AddBreak(baseBreakAmount);

            _enemyView.PlayHitReact();
            _enemyView.FlashColor(isCrit || brokenThisHit ? Color.yellow : Color.red, 0.25f);
            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(isCrit || brokenThisHit);
            _gameFeel?.TriggerCameraShake(brokenThisHit ? 2.0f : (isCrit ? 1.5f : 1.0f));

            string critTag = isCrit ? " [CRIT]" : "";
            string brokenTag = (_enemyBreakMeter != null && _enemyBreakMeter.IsBroken) ? " [1.75x BROKEN]" : "";
            string labelTag = !string.IsNullOrEmpty(actionLabel) ? $" [{actionLabel.ToUpper()}]" : "";

            Color textColor = (_enemyBreakMeter != null && _enemyBreakMeter.IsBroken) ? new Color(1f, 0.5f, 0.2f) : (isCrit ? Color.yellow : Color.white);

            FloatingCombatText.Spawn(
                hitPosition,
                $"-{result.MitigatedDamage}{labelTag}{critTag}{brokenTag}",
                textColor,
                isCrit || (_enemyBreakMeter != null && _enemyBreakMeter.IsBroken) ? 1.5f : 1.1f
            );

            if (brokenThisHit)
            {
                FloatingCombatText.Spawn(_enemyView.transform.position + Vector3.up * 1.5f, "★ BROKEN! ★", new Color(1f, 0.4f, 0.1f), 1.8f);
            }
        }

        private void RefreshTimelineHUD()
        {
            if (_hud != null && _timeline != null)
            {
                List<CombatActorStats> upcoming = _timeline.PreviewUpcomingTurns(6);
                _hud.UpdateTimelinePreview(upcoming);
            }
        }

        private IEnumerator BattleRoutine()
        {
            yield return new WaitForSeconds(0.45f);

            bool isFirstTurn = true;

            while (!_playerStats.IsDefeated && !_enemyStats.IsDefeated)
            {
                if (isFirstTurn)
                {
                    RefreshTimelineHUD();
                }

                CombatActorStats activeActor = _timeline.GetNextTurnActor();
                if (activeActor == null)
                    break;

                if (isFirstTurn)
                {
                    _hud?.ActivateCurrentTurn(0.24f);
                    _cameraController?.BlendToTurn(activeActor.IsPlayer, 0.40f);
                    isFirstTurn = false;
                }

                if (activeActor.IsPlayer)
                {
                    yield return PlayerTurnRoutine();
                }
                else
                {
                    yield return EnemyTurnRoutine();
                }

                _stateMachine.ChangeState(BattleState.TurnEnd);

                if (_playerStats.IsDefeated || _enemyStats.IsDefeated)
                {
                    _hud?.ReturnActiveTurnIcon(0.16f);
                    _cameraController?.BlendToDefault(0.45f, null);
                    yield return new WaitForSeconds(0.20f);
                    break;
                }

                List<CombatActorStats> nextUpcoming = _timeline.PreviewUpcomingTurns(6);
                bool nextIsPlayer = nextUpcoming.Count > 0 && nextUpcoming[0].IsPlayer;
                _hud?.TransitionTimelineToNextTurn(nextUpcoming, 0.36f);
                _cameraController?.BlendToTurn(nextIsPlayer, 0.38f);
                yield return new WaitForSeconds(0.38f);
            }

            // Battle resolution
            if (_enemyStats.IsDefeated)
            {
                _stateMachine.ChangeState(BattleState.Victory);
                _enemyView.PlayDeath();
                _audioPlayer?.PlayVictory();
                yield return new WaitForSeconds(0.8f);
                _resultScreen?.ShowVictory();
            }
            else if (_playerStats.IsDefeated)
            {
                _stateMachine.ChangeState(BattleState.Defeat);
                _playerView.PlayDeath();
                _audioPlayer?.PlayDefeat();
                yield return new WaitForSeconds(0.8f);
                _resultScreen?.ShowDefeat();
            }
        }

        private IEnumerator PlayerTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.PlayerTurn);
            _audioPlayer?.PlayTurnStart();

            // Status effects on turn start
            _playerStatusController.ProcessTurnStart(_playerStats);
            _hud?.UpdatePlayerStatuses(_playerStatusController.ActiveEffects);

            if (_playerStats.IsDefeated)
                yield break;

            // Stance bonus AP (e.g. Virtuoso stance)
            int bonusAP = _playerStanceController.GetBonusAPGeneration();
            if (bonusAP > 0)
            {
                _apPool.Generate(bonusAP);
            }

            // Lumina bonus AP (e.g. Lumina of Flow)
            int luminaAP = _playerLoadout != null ? _playerLoadout.GetBonusAPPerTurn() : 0;
            if (luminaAP > 0)
            {
                _apPool.Generate(luminaAP);
            }

            _hud?.SetCommandMenuVisible(true);

            _playerActionChosen = false;
            while (!_playerActionChosen)
            {
                yield return null;
            }

            _hud?.SetCommandMenuVisible(false);
            _stateMachine.ChangeState(BattleState.ActionExecuting);

            if (_selectedAction == CombatActionType.Attack)
            {
                yield return ExecutePlayerBasicAttack();
            }
            else if (_selectedAction == CombatActionType.Skill && _selectedSkill != null)
            {
                yield return ExecutePlayerSkill(_selectedSkill);
            }
            else if (_selectedAction == CombatActionType.FreeAim)
            {
                yield return ExecutePlayerFreeAim();
            }
            else if (_selectedAction == CombatActionType.GradientAttack)
            {
                yield return ExecutePlayerGradientAttack();
            }
        }

        private IEnumerator ExecutePlayerBasicAttack()
        {
            _apPool.Generate(1);
            _audioPlayer?.PlayAPGain();

            _cameraController?.BlendToPlayerAction(0.35f);

            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.42f, () => arrived = true);
            while (!arrived)
                yield return null;

            _playerView.PlayAttack(speed: 1.35f);

            QTEOutcome qteResult = QTEOutcome.None;
            bool qteComplete = false;
            float attackElapsed = 0f;

            if (_qteWidget != null)
            {
                var qteDef = new QTEWindowDefinition(0.75f, 0.50f, 0.08f, 0.16f);
                _qteWidget.StartQTE(qteDef, _enemyView.transform.position, res =>
                {
                    qteResult = res;
                    qteComplete = true;
                });
            }
            else
            {
                yield return new WaitForSeconds(0.50f);
                attackElapsed = 0.50f;
                qteComplete = true;
            }

            while (!qteComplete || attackElapsed < 0.78f)
            {
                attackElapsed += Time.deltaTime;
                yield return null;
            }

            float multiplier = 1f;
            bool isCrit = false;
            int breakAmount = 15;

            if (qteResult == QTEOutcome.Perfect)
            {
                multiplier = 1.5f;
                isCrit = true;
                breakAmount = 30;
                _playerOverchargeMeter.AddCharge(1);
                _audioPlayer?.PlayQTESuccess();
                FloatingCombatText.Spawn(_playerView.transform.position + Vector3.up * 1.5f, "+1 OVERCHARGE", new Color(1f, 0.85f, 0.2f), 1.3f);
            }
            else if (qteResult == QTEOutcome.Good)
            {
                multiplier = 1.15f;
                breakAmount = 20;
            }
            else
            {
                multiplier = 0.85f;
                breakAmount = 10;
            }

            ApplyDamageAndBreakToEnemy(multiplier, breakAmount, isCrit, "Strike", _enemyView.transform.position);

            yield return new WaitForSeconds(0.4f);

            if (!_enemyStats.IsDefeated)
            {
                _enemyView.PlayIdle();
            }

            _cameraController?.BlendToPlayerTurn(0.4f, null, triggerTurnPunch: false);

            bool returned = false;
            _playerView.AnimateReturn(0.4f, () => returned = true);
            while (!returned)
                yield return null;
        }

        private IEnumerator ExecutePlayerSkill(SkillDefinitionSO skill)
        {
            _apPool.TrySpend(skill.APCost);

            _cameraController?.BlendToPlayerAction(0.35f);

            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.38f, () => arrived = true);
            while (!arrived)
                yield return null;

            _playerView.PlayAttack(speed: 1.25f);

            QTEOutcome qteResult = QTEOutcome.None;
            bool qteComplete = false;
            float attackElapsed = 0f;

            if (_qteWidget != null)
            {
                _qteWidget.StartQTE(skill.QTEConfig, _enemyView.transform.position, res =>
                {
                    qteResult = res;
                    qteComplete = true;
                });
            }
            else
            {
                yield return new WaitForSeconds(0.6f);
                attackElapsed = 0.6f;
                qteComplete = true;
            }

            while (!qteComplete || attackElapsed < 0.84f)
            {
                attackElapsed += Time.deltaTime;
                yield return null;
            }

            float bonusMultiplier = 1f;
            bool isCrit = false;

            if (qteResult == QTEOutcome.Perfect)
            {
                bonusMultiplier = 1.4f;
                isCrit = true;
                _audioPlayer?.PlayQTESuccess();
            }
            else if (qteResult == QTEOutcome.Good)
            {
                bonusMultiplier = 1.1f;
            }

            float extraMultiplier = 0f;
            int extraBreak = 0;

            if (skill.ConsumesOvercharge)
            {
                int charges = _playerOverchargeMeter.ConsumeAllCharges();
                if (charges > 0)
                {
                    extraMultiplier = charges * 0.35f;
                    extraBreak = charges * 25;
                    _audioPlayer?.PlayOverchargeSurge();
                    FloatingCombatText.Spawn(_playerView.transform.position + Vector3.up * 1.5f, $"OVERCHARGE x{charges}!", Color.cyan, 1.4f);
                }
            }

            float finalMultiplier = (skill.DamageMultiplier + extraMultiplier) * bonusMultiplier;
            int finalBreak = skill.BreakDamage + extraBreak;

            ApplyDamageAndBreakToEnemy(finalMultiplier, finalBreak, isCrit, skill.SkillName, _enemyView.transform.position);

            if (skill.InflictedStatus != StatusEffectType.None && !_enemyStats.IsDefeated)
            {
                _enemyStatusController.ApplyStatus(skill.InflictedStatus, skill.StatusDuration, potency: 10);
                _hud?.UpdateEnemyStatuses(_enemyStatusController.ActiveEffects);
            }

            yield return new WaitForSeconds(0.45f);

            if (!_enemyStats.IsDefeated)
            {
                _enemyView.PlayIdle();
            }

            _cameraController?.BlendToPlayerTurn(0.38f, null, triggerTurnPunch: false);

            bool returned = false;
            _playerView.AnimateReturn(0.38f, () => returned = true);
            while (!returned)
                yield return null;
        }

        private IEnumerator ExecutePlayerFreeAim()
        {
            // 1. Blend Camera to over-the-shoulder
            bool camReady = false;
            if (_cameraController != null)
            {
                _cameraController.BlendToFreeAim(0.35f, () => camReady = true);
            }
            else
            {
                camReady = true;
            }

            while (!camReady)
                yield return null;

            // 2. Activate Free Aim HUD
            Camera activeCam = (_cameraController != null && _cameraController.TargetCamera != null)
                ? _cameraController.TargetCamera
                : Camera.main;

            if (_freeAimHUD != null)
            {
                _freeAimHUD.Show(activeCam);
                _freeAimHUD.UpdateAmmo(_apPool.CurrentAP);
            }

            const float totalDuration = 4.0f;
            float timer = totalDuration;
            bool exitRequested = false;

            Action<Vector2> handleFire = screenPos =>
            {
                if (!_apPool.CanSpend(1) || _enemyStats.IsDefeated || activeCam == null)
                    return;

                _apPool.TrySpend(1);
                _freeAimHUD?.UpdateAmmo(_apPool.CurrentAP);
                _audioPlayer?.PlayGunshot();

                // Fire raycast into 3D world
                Ray ray = activeCam.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 60f))
                {
                    var hitbox = hit.collider.GetComponent<CombatHitbox>();
                    if (hitbox != null)
                    {
                        bool isWeak = hitbox.HitboxType == HitboxType.WeakPoint;
                        float baseMult = isWeak ? 2.0f : 1.0f;
                        int breakAmount = isWeak ? 40 : 15;

                        ApplyDamageAndBreakToEnemy(baseMult, breakAmount, isWeak, isWeak ? "Weak Point" : "Shot", hit.point);

                        if (isWeak)
                        {
                            _audioPlayer?.PlayWeakPointHit();
                        }

                        if (_enemyStats.IsDefeated)
                        {
                            exitRequested = true;
                        }
                    }
                    else
                    {
                        // Struck environment / missed
                        FloatingCombatText.Spawn(hit.point, "MISS", Color.gray, 0.9f);
                    }
                }
            };

            Action handleCancel = () =>
            {
                exitRequested = true;
            };

            if (_freeAimHUD != null)
            {
                _freeAimHUD.OnFireRequested += handleFire;
                _freeAimHUD.OnCancelRequested += handleCancel;
            }

            // Real-time Aim Loop
            while (timer > 0f && !exitRequested && !_enemyStats.IsDefeated)
            {
                timer -= Time.deltaTime;

                if (_freeAimHUD != null && activeCam != null)
                {
                    _freeAimHUD.UpdateTimer(timer, totalDuration);

                    // Hover detection for dynamic reticle highlight
                    Ray hoverRay = activeCam.ScreenPointToRay(_freeAimHUD.CrosshairScreenPosition);
                    if (Physics.Raycast(hoverRay, out RaycastHit hoverHit, 60f))
                    {
                        var hitbox = hoverHit.collider.GetComponent<CombatHitbox>();
                        _freeAimHUD.SetTargetState(hitbox != null ? hitbox.HitboxType : null);
                    }
                    else
                    {
                        _freeAimHUD.SetTargetState(null);
                    }
                }
                else if (_freeAimHUD != null)
                {
                    _freeAimHUD.UpdateTimer(timer, totalDuration);
                }

                if (_apPool.CurrentAP <= 0)
                {
                    yield return new WaitForSeconds(0.4f);
                    break;
                }

                yield return null;
            }

            // Cleanup & Exit
            if (_freeAimHUD != null)
            {
                _freeAimHUD.OnFireRequested -= handleFire;
                _freeAimHUD.OnCancelRequested -= handleCancel;
                _freeAimHUD.Hide();
            }

            if (_enemyStats.IsDefeated && _cameraController != null)
            {
                bool camReturned = false;
                _cameraController.BlendToDefault(0.35f, () => camReturned = true);
                while (!camReturned)
                    yield return null;
            }
        }

        private IEnumerator ExecutePlayerGradientAttack()
        {
            if (_gradientMeter == null || !_gradientMeter.ConsumeForSuperAttack())
                yield break;

            _audioPlayer?.PlayOverchargeSurge();

            // 1. Camera zoom/cinematic blend
            bool camReady = false;
            if (_cameraController != null)
            {
                _cameraController.BlendToFreeAim(0.25f, () => camReady = true);
            }
            else
            {
                camReady = true;
            }
            while (!camReady) yield return null;

            yield return new WaitForSeconds(0.2f);

            // 2. High-speed approach
            _cameraController?.BlendToPlayerAction(0.28f);
            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.28f, () => arrived = true);
            while (!arrived) yield return null;

            // 3. Multi-hit cinematic sequence
            _playerView.PlayAttack(speed: 1.8f, fixedTimeOffset: 0.25f);
            yield return new WaitForSeconds(0.42f);

            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(false);
            _gameFeel?.TriggerCameraShake(1.5f);
            _enemyView.PlayHitReact();
            _enemyView.FlashColor(Color.yellow, 0.2f);
            FloatingCombatText.Spawn(_enemyView.transform.position + Vector3.up * 1.2f, "LUMIERE...", Color.yellow, 0.8f);

            yield return new WaitForSeconds(0.12f);

            // Finisher explosion
            _playerView.PlayAttack(speed: 1.5f, fixedTimeOffset: 0.20f);
            yield return new WaitForSeconds(0.52f);

            _audioPlayer?.PlayWeakPointHit();
            _audioPlayer?.PlayBreakShatter();

            // Massive damage + break force (ApplyDamageAndBreakToEnemy handles hit react, flash, hitstop, and shake)
            float gradientDmgMult = 3.6f;
            int gradientBreak = 80;
            ApplyDamageAndBreakToEnemy(gradientDmgMult, gradientBreak, true, "Lumiere Eclipse", _enemyView.transform.position);

            yield return new WaitForSeconds(0.6f);

            if (!_enemyStats.IsDefeated)
            {
                _enemyView.PlayIdle();
            }

            // 4. Return to home pos while keeping camera behind player
            bool returned = false;
            _playerView.AnimateReturn(0.35f, () => returned = true);

            bool camReturned = _cameraController == null;
            if (_cameraController != null)
            {
                if (_enemyStats.IsDefeated)
                    _cameraController.BlendToDefault(0.35f, () => camReturned = true);
                else
                    _cameraController.BlendToPlayerTurn(0.35f, () => camReturned = true, triggerTurnPunch: false);
            }

            while (!returned || !camReturned) yield return null;
        }

        private IEnumerator EnemyTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.EnemyTurn);

            // Process enemy turn start status effects (e.g. burn dot)
            _enemyStatusController.ProcessTurnStart(_enemyStats);
            _hud?.UpdateEnemyStatuses(_enemyStatusController.ActiveEffects);

            if (_enemyStats.IsDefeated)
            {
                yield break;
            }

            // Check if enemy is posture broken
            if (_enemyBreakMeter.IsBroken)
            {
                FloatingCombatText.Spawn(_enemyView.transform.position + Vector3.up * 1.5f, "TURN SKIPPED (BROKEN)", new Color(1f, 0.5f, 0.2f), 1.6f);
                _audioPlayer?.PlayTelegraphWindup();

                _enemyBreakMeter.ConsumeBrokenTurn();
                _hud?.UpdateEnemyBreak(_enemyBreakMeter.CurrentBreak, _enemyBreakMeter.MaxBreak, _enemyBreakMeter.IsBroken);

                yield return new WaitForSeconds(1.2f);
                yield break;
            }

            EnemyAttackPatternSO pattern = _enemyAttackPatterns[_enemyTurnCounter % _enemyAttackPatterns.Length];
            _enemyTurnCounter++;

            yield return new WaitForSeconds(0.6f);

            _stateMachine.ChangeState(BattleState.ActionExecuting);
            yield return ExecuteEnemyAttackPatternWithActiveDefense(pattern);
        }

        private IEnumerator ExecuteEnemyAttackPatternWithActiveDefense(EnemyAttackPatternSO pattern)
        {
            _cameraController?.BlendToEnemyAttack(0.36f);

            bool arrived = false;
            _enemyView.AnimateApproach(_playerView.HomePosition, 0.42f, () => arrived = true);
            while (!arrived)
                yield return null;

            foreach (HitWindowDefinition strike in pattern.Strikes)
            {
                if (_playerStats.IsDefeated)
                    break;

                float hitTime = Mathf.Max(0.15f, strike.HitTimeOffset);

                float stanceParryWindowMult = _playerStanceController != null ? _playerStanceController.GetParryWindowMultiplier() : 1.0f;
                float cheatParryMult = _cheatPanel != null ? _cheatPanel.GetDebugParryWindowMultiplier() : 1.0f;
                float pictoParryExt = _playerLoadout != null ? _playerLoadout.GetParryWindowExtension() : 0f;
                float parryWindowMult = stanceParryWindowMult * cheatParryMult * (1.0f + pictoParryExt);

                float primaryHalfWindow = strike.TelegraphType == AttackTelegraphType.GroundSweep
                    ? strike.JumpHalfWindow
                    : strike.ParryHalfWindow * parryWindowMult;
                float maxHalfWindow = Mathf.Max(primaryHalfWindow, Mathf.Max(strike.DodgeHalfWindow, strike.JumpHalfWindow));

                float totalStrikeDuration = hitTime + maxHalfWindow + 0.15f;
                float hitNormalized = hitTime / totalStrikeDuration;
                float parryHalfNormalized = primaryHalfWindow / totalStrikeDuration;
                float dodgeHalfNormalized = strike.DodgeHalfWindow / totalStrikeDuration;

                if (_timingVisualizer != null)
                {
                    _timingVisualizer.Show(
                        pattern.AttackName,
                        strike.TelegraphType,
                        hitNormalized,
                        parryHalfNormalized,
                        dodgeHalfNormalized
                    );
                }

                // Sync Surprise Uppercut's natural ~1.08s swing peak to hitTime
                _enemyView.PlayAttack(speed: 1.08f / hitTime, fixedTimeOffset: 0f);
                _audioPlayer?.PlayTelegraphWindup();

                if (_inputBuffer != null)
                {
                    _inputBuffer.Clear();
                    _inputBuffer.SetListening(true);
                }

                float elapsed = 0f;
                float listenEnd = hitTime + maxHalfWindow;
                DefenseActionType capturedAction = DefenseActionType.None;
                float capturedTimestamp = -1f;
                bool defenseAnimPlayed = false;

                while (elapsed < listenEnd)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / totalStrikeDuration);

                    if (_timingVisualizer != null)
                    {
                        _timingVisualizer.SetCursorProgress(progress);
                    }

                    if (_inputBuffer != null && capturedAction == DefenseActionType.None)
                    {
                        if (_inputBuffer.TryConsumeInput(Time.unscaledTime, out DefenseActionType action, out float inputTs))
                        {
                            float inputAge = Mathf.Max(0f, Time.unscaledTime - inputTs);
                            capturedAction = action;
                            capturedTimestamp = Mathf.Max(0f, elapsed - inputAge);

                            if (action == DefenseActionType.Dodge && strike.TelegraphType != AttackTelegraphType.GroundSweep)
                            {
                                _playerView.PlayDodge();
                                defenseAnimPlayed = true;
                            }
                            else if (action == DefenseActionType.Jump && strike.TelegraphType == AttackTelegraphType.GroundSweep)
                            {
                                _playerView.PlayJump(0.35f);
                                defenseAnimPlayed = true;
                            }
                        }
                    }

                    if (capturedAction != DefenseActionType.None && elapsed >= hitTime)
                    {
                        break;
                    }

                    yield return null;
                }

                if (_inputBuffer != null)
                {
                    _inputBuffer.SetListening(false);
                }

                DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                    capturedAction,
                    capturedTimestamp,
                    hitTime,
                    strike,
                    parryWindowMult
                );

                if (_timingVisualizer != null)
                {
                    _timingVisualizer.ShowResult(outcome);
                }

                yield return ResolveDefenseOutcome(outcome, strike, defenseAnimPlayed);
                yield return new WaitForSeconds(0.4f);
            }

            if (_timingVisualizer != null)
            {
                _timingVisualizer.Hide();
            }

            if (!_playerStats.IsDefeated)
            {
                _playerView.PlayIdle();
            }

            bool returned = false;
            _enemyView.AnimateReturn(0.4f, () => returned = true);
            while (!returned)
                yield return null;
        }

        private IEnumerator ResolveDefenseOutcome(DefenseOutcome outcome, HitWindowDefinition strike, bool defenseAnimPlayed)
        {
            int gradDefBonus = _playerLoadout != null ? _playerLoadout.GetGradientOnDefenseBonus() : 0;

            switch (outcome)
            {
                case DefenseOutcome.ParrySuccess:
                    _apPool.Generate(2);
                    _playerOverchargeMeter.AddCharge(1);
                    _gradientMeter?.AddCharge(25 + gradDefBonus);
                    _audioPlayer?.PlayAPGain();
                    _audioPlayer?.PlayParrySuccess();
                    _gameFeel?.TriggerHitStop(true);
                    _gameFeel?.TriggerCameraShake(1.6f);

                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "PERFECT PARRY! (+2 AP, +1 OVERCHARGE)",
                        Color.green,
                        1.4f
                    );

                    yield return new WaitForSeconds(0.2f);

                    bool counterComplete = false;
                    _playerView.PlayCounterAttack(_enemyView.transform.position, () =>
                    {
                        ApplyDamageAndBreakToEnemy(1.4f, 40, true, "Counter", _enemyView.transform.position);
                    }, () => counterComplete = true);

                    while (!counterComplete)
                        yield return null;
                    break;

                case DefenseOutcome.DodgeSuccess:
                    _apPool.Generate(1);
                    _gradientMeter?.AddCharge(15 + gradDefBonus);
                    _audioPlayer?.PlayAPGain();
                    if (!defenseAnimPlayed)
                    {
                        _playerView.PlayDodge();
                    }
                    _audioPlayer?.PlayDodgeSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "DODGED! (+1 AP)",
                        new Color(1f, 0.9f, 0.2f),
                        1.2f
                    );
                    yield return new WaitForSeconds(0.45f);
                    break;

                case DefenseOutcome.JumpSuccess:
                    _apPool.Generate(1);
                    _gradientMeter?.AddCharge(15 + gradDefBonus);
                    _audioPlayer?.PlayAPGain();
                    if (!defenseAnimPlayed)
                    {
                        _playerView.PlayJump(0.35f);
                    }
                    _audioPlayer?.PlayJumpSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "JUMPED! (+1 AP)",
                        new Color(0.3f, 0.8f, 1f),
                        1.2f
                    );
                    yield return new WaitForSeconds(0.55f);
                    break;

                default:
                    _gradientMeter?.AddCharge(8);
                    float stanceDefMod = _playerStanceController != null ? _playerStanceController.GetDefenseMultiplier() : 1.0f;
                    float enemyAtkMod = _enemyStatusController != null ? _enemyStatusController.GetAttackMultiplier() : 1.0f;
                    float incomingMod = _playerStatusController != null ? _playerStatusController.GetIncomingDamageMultiplier() : 1.0f;
                    float pictoDef = _playerLoadout != null ? (1.0f + _playerLoadout.GetTotalDefenseBonus()) : 1.0f;

                    float combinedMultiplier = enemyAtkMod * incomingMod / Mathf.Max(0.1f, stanceDefMod * pictoDef);
                    DamageResult result = DamageCalculator.CalculateDamage(_enemyStats, _playerStats, combinedMultiplier, false);

                    if (_cheatPanel != null && _cheatPanel.GodModeActive)
                    {
                        FloatingCombatText.Spawn(_playerView.transform.position, "IMMUNE (GOD MODE)", Color.green, 1.2f);
                    }
                    else
                    {
                        _playerStats.ApplyDamage(result.MitigatedDamage);
                        _playerView.PlayHitReact();
                        _playerView.FlashColor(Color.red, 0.22f);
                        _audioPlayer?.PlayHit();
                        _gameFeel?.TriggerHitStop(false);
                        _gameFeel?.TriggerCameraShake(1f);

                        FloatingCombatText.Spawn(
                            _playerView.transform.position,
                            $"-{result.MitigatedDamage}",
                            Color.red,
                            1f
                        );
                    }
                    yield return new WaitForSeconds(0.4f);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_hud != null)
            {
                _hud.OnAttackSelected -= HandlePlayerAttackSelected;
                _hud.OnSkillSelected -= HandlePlayerSkillSelected;
                _hud.OnFreeAimSelected -= HandlePlayerFreeAimSelected;
                _hud.OnGradientSelected -= HandlePlayerGradientSelected;
                _hud.OnStanceToggleRequested -= HandleStanceToggleRequested;
            }

            if (_resultScreen != null)
            {
                _resultScreen.OnRetryRequested -= HandleRetryBattle;
            }
        }
    }
}
