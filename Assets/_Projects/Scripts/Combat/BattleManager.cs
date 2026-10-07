using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        [Header("Skills & Patterns")]
        [SerializeField] private List<SkillDefinitionSO> _playerSkills;
        [SerializeField] private EnemyAttackPatternSO[] _enemyAttackPatterns;

        private BattleStateMachine _stateMachine;
        private TurnTimeline _timeline;
        private ActionPointPool _apPool;
        private CombatActorStats _playerStats;
        private CombatActorStats _enemyStats;

        private bool _playerActionChosen;
        private CombatActionType _selectedAction;
        private SkillDefinitionSO _selectedSkill;
        private int _enemyTurnCounter;

        public CombatActorStats PlayerStats => _playerStats;
        public CombatActorStats EnemyStats => _enemyStats;
        public ActionPointPool APPool => _apPool;
        public BattleStateMachine StateMachine => _stateMachine;

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

            // Connect HUD
            if (_hud != null)
            {
                _hud.SetupPlayerStatus(_playerStats, _apPool, _playerSkills);
                _hud.SetupEnemyStatus(_enemyStats);
                _hud.OnAttackSelected += HandlePlayerAttackSelected;
                _hud.OnSkillSelected += HandlePlayerSkillSelected;
                _hud.OnFreeAimSelected += HandlePlayerFreeAimSelected;
                _hud.OnPassSelected += HandlePlayerPassSelected;
            }

            RefreshTimelineHUD();

            StartCoroutine(BattleRoutine());
        }

        private void CreateFallbackSkills()
        {
            _playerSkills = new List<SkillDefinitionSO>
            {
                SkillDefinitionSO.CreateSkill("Overcharge Cleave", 3, 2.2f, "Heavy devastating blow with bonus break force."),
                SkillDefinitionSO.CreateSkill("Swift Flurry", 2, 1.5f, "Quick dual-hit flurry.")
            };
        }

        private void CreateFallbackPatterns()
        {
            var p1 = EnemyAttackPatternSO.CreateDefaultPattern("Stalker Claw Combo", AttackTelegraphType.Standard, 1.0f, 22);
            var p2 = EnemyAttackPatternSO.CreateDefaultPattern("Ground Shockwave", AttackTelegraphType.GroundSweep, 1.1f, 26);
            _enemyAttackPatterns = new[] { p1, p2 };
        }

        private void HandlePlayerAttackSelected()
        {
            _audioPlayer?.PlayButtonClick();
            _selectedAction = CombatActionType.Attack;
            _selectedSkill = null;
            _playerActionChosen = true;
        }

        private void HandlePlayerSkillSelected(SkillDefinitionSO skill)
        {
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
            _audioPlayer?.PlayButtonClick();
            if (_apPool.CanSpend(1))
            {
                _selectedAction = CombatActionType.FreeAim;
                _selectedSkill = null;
                _playerActionChosen = true;
            }
        }

        private void HandlePlayerPassSelected()
        {
            _audioPlayer?.PlayButtonClick();
            _selectedAction = CombatActionType.Pass;
            _selectedSkill = null;
            _playerActionChosen = true;
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
            yield return new WaitForSeconds(0.6f);

            while (!_playerStats.IsDefeated && !_enemyStats.IsDefeated)
            {
                RefreshTimelineHUD();
                CombatActorStats activeActor = _timeline.GetNextTurnActor();

                if (activeActor == null)
                    break;

                if (activeActor.IsPlayer)
                {
                    yield return PlayerTurnRoutine();
                }
                else
                {
                    yield return EnemyTurnRoutine();
                }

                _stateMachine.ChangeState(BattleState.TurnEnd);
                yield return new WaitForSeconds(0.4f);
            }

            // Battle resolution
            if (_enemyStats.IsDefeated)
            {
                _stateMachine.ChangeState(BattleState.Victory);
                _enemyView.PlayDeath();
                _audioPlayer?.PlayVictory();
                _hud?.SetTurnBanner("<color=#55FF88>VICTORY!</color>");
                _hud?.AddLog("Enemy defeated in battle!");
            }
            else if (_playerStats.IsDefeated)
            {
                _stateMachine.ChangeState(BattleState.Defeat);
                _playerView.PlayDeath();
                _audioPlayer?.PlayDefeat();
                _hud?.SetTurnBanner("<color=#FF5555>DEFEAT</color>");
                _hud?.AddLog("Party was defeated...");
            }
        }

        private IEnumerator PlayerTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.PlayerTurn);
            _hud?.SetTurnBanner("PLAYER TURN");
            _hud?.AddLog("Choose Attack, Skill, Free Aim, or Pass.");
            _audioPlayer?.PlayTurnStart();
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
            else
            {
                _hud?.AddLog($"{_playerStats.Name} passed the turn.");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator ExecutePlayerBasicAttack()
        {
            _apPool.Generate(1);
            _audioPlayer?.PlayAPGain();

            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.42f, () => arrived = true);
            while (!arrived)
                yield return null;

            _playerView.PlayAttack();

            QTEOutcome qteResult = QTEOutcome.None;
            bool qteComplete = false;

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
                qteComplete = true;
            }

            while (!qteComplete)
                yield return null;

            float multiplier = 1f;
            bool isCrit = false;

            if (qteResult == QTEOutcome.Perfect)
            {
                multiplier = 1.5f;
                isCrit = true;
                _audioPlayer?.PlayQTESuccess();
                _hud?.AddLog("<color=#FFE838>★ PERFECT QTE! Critical Strike! ★</color>");
            }
            else if (qteResult == QTEOutcome.Good)
            {
                multiplier = 1.15f;
                _hud?.AddLog("<color=#66FF88>Good timing on strike.</color>");
            }
            else
            {
                multiplier = 0.85f;
                _hud?.AddLog("<color=#AAAAAA>Strike timing missed.</color>");
            }

            DamageResult result = DamageCalculator.CalculateDamage(_playerStats, _enemyStats, multiplier, isCrit);
            _enemyStats.ApplyDamage(result.MitigatedDamage);

            _enemyView.PlayHitReact();
            _enemyView.FlashColor(isCrit ? Color.yellow : Color.red, 0.2f);
            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(isCrit);
            _gameFeel?.TriggerCameraShake(isCrit ? 1.4f : 1f);

            FloatingCombatText.Spawn(
                _enemyView.transform.position,
                isCrit ? $"-{result.MitigatedDamage} [CRIT]" : $"-{result.MitigatedDamage}",
                isCrit ? Color.yellow : Color.white,
                isCrit ? 1.4f : 1f
            );

            _hud?.AddLog($"{_playerStats.Name} dealt {result.MitigatedDamage} damage! (+1 AP)");
            yield return new WaitForSeconds(0.4f);

            if (!_enemyStats.IsDefeated)
            {
                _enemyView.PlayIdle();
            }

            bool returned = false;
            _playerView.AnimateReturn(0.4f, () => returned = true);
            while (!returned)
                yield return null;
        }

        private IEnumerator ExecutePlayerSkill(SkillDefinitionSO skill)
        {
            _apPool.TrySpend(skill.APCost);
            _hud?.AddLog($"Gustave unleashes <color=#55AAFF>{skill.SkillName}</color> (-{skill.APCost} AP)!");

            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.38f, () => arrived = true);
            while (!arrived)
                yield return null;

            _playerView.PlayAttack();

            QTEOutcome qteResult = QTEOutcome.None;
            bool qteComplete = false;

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
                qteComplete = true;
            }

            while (!qteComplete)
                yield return null;

            float bonusMultiplier = 1f;
            bool isCrit = false;

            if (qteResult == QTEOutcome.Perfect)
            {
                bonusMultiplier = 1.4f;
                isCrit = true;
                _audioPlayer?.PlayQTESuccess();
                _hud?.AddLog("<color=#FFE838>★ PERFECT SKILL TIMING! ★</color>");
            }
            else if (qteResult == QTEOutcome.Good)
            {
                bonusMultiplier = 1.1f;
            }

            float finalMultiplier = skill.DamageMultiplier * bonusMultiplier;
            DamageResult result = DamageCalculator.CalculateDamage(_playerStats, _enemyStats, finalMultiplier, isCrit);
            _enemyStats.ApplyDamage(result.MitigatedDamage);

            _enemyView.PlayHitReact();
            _enemyView.FlashColor(Color.yellow, 0.25f);
            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(true);
            _gameFeel?.TriggerCameraShake(1.6f);

            FloatingCombatText.Spawn(
                _enemyView.transform.position,
                $"-{result.MitigatedDamage} [{skill.SkillName.ToUpper()}]",
                Color.yellow,
                1.5f
            );

            _hud?.AddLog($"{skill.SkillName} dealt {result.MitigatedDamage} damage to {_enemyStats.Name}!");
            yield return new WaitForSeconds(0.45f);

            if (!_enemyStats.IsDefeated)
            {
                _enemyView.PlayIdle();
            }

            bool returned = false;
            _playerView.AnimateReturn(0.38f, () => returned = true);
            while (!returned)
                yield return null;
        }

        private IEnumerator ExecutePlayerFreeAim()
        {
            _hud?.SetTurnBanner(string.Empty);
            _hud?.AddLog("<color=#FFE040>FREE AIM ACTIVATED!</color> Aim for weak points.");

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
                if (!_apPool.CanSpend(1) || _enemyStats.IsDefeated)
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
                        var (damage, isCrit) = FreeAimDamageCalculator.Calculate(_playerStats.AttackPower, hitbox.HitboxType, _enemyStats.Defense);
                        _enemyStats.ApplyDamage(damage);

                        _enemyView.PlayHitReact();
                        _enemyView.FlashColor(isCrit ? Color.yellow : Color.red, 0.2f);
                        _gameFeel?.TriggerHitStop(isCrit);
                        _gameFeel?.TriggerCameraShake(isCrit ? 1.5f : 1f);

                        if (isCrit)
                        {
                            _audioPlayer?.PlayWeakPointHit();
                            FloatingCombatText.Spawn(hit.point, $"-{damage} [WEAK POINT CRIT!]", Color.yellow, 1.6f);
                            _hud?.AddLog($"<color=#FFE838>★ WEAK POINT HIT! {damage} critical damage! ★</color>");
                        }
                        else
                        {
                            _audioPlayer?.PlayHit();
                            FloatingCombatText.Spawn(hit.point, $"-{damage}", Color.white, 1.2f);
                            _hud?.AddLog($"Gunshot dealt {damage} damage.");
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

                if (_freeAimHUD != null)
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

            bool camReturned = false;
            if (_cameraController != null)
            {
                _cameraController.BlendToDefault(0.35f, () => camReturned = true);
            }
            else
            {
                camReturned = true;
            }

            while (!camReturned)
                yield return null;

            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator EnemyTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.EnemyTurn);
            _hud?.SetTurnBanner("ENEMY TURN");

            EnemyAttackPatternSO pattern = _enemyAttackPatterns[_enemyTurnCounter % _enemyAttackPatterns.Length];
            _enemyTurnCounter++;

            _hud?.AddLog($"Incoming attack: <color=#FF6644>{pattern.AttackName}</color>!");
            yield return new WaitForSeconds(0.6f);

            _stateMachine.ChangeState(BattleState.ActionExecuting);
            yield return ExecuteEnemyAttackPatternWithActiveDefense(pattern);
        }

        private IEnumerator ExecuteEnemyAttackPatternWithActiveDefense(EnemyAttackPatternSO pattern)
        {
            bool arrived = false;
            _enemyView.AnimateApproach(_playerView.HomePosition, 0.42f, () => arrived = true);
            while (!arrived)
                yield return null;

            foreach (HitWindowDefinition strike in pattern.Strikes)
            {
                if (_playerStats.IsDefeated)
                    break;

                float totalStrikeDuration = strike.HitTimeOffset + 0.35f;
                float hitNormalized = strike.HitTimeOffset / totalStrikeDuration;
                float parryHalfNormalized = strike.ParryHalfWindow / totalStrikeDuration;
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

                _enemyView.PlayAttack();
                _audioPlayer?.PlayTelegraphWindup();

                if (_inputBuffer != null)
                {
                    _inputBuffer.Clear();
                    _inputBuffer.SetListening(true);
                }

                float elapsed = 0f;
                DefenseActionType capturedAction = DefenseActionType.None;
                float capturedTimestamp = -1f;

                while (elapsed < strike.HitTimeOffset)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / totalStrikeDuration);

                    if (_timingVisualizer != null)
                    {
                        _timingVisualizer.SetCursorProgress(progress);
                    }

                    if (_inputBuffer != null && capturedAction == DefenseActionType.None)
                    {
                        if (_inputBuffer.TryConsumeInput(elapsed, out DefenseActionType action, out float ts))
                        {
                            capturedAction = action;
                            capturedTimestamp = elapsed;
                        }
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
                    strike.HitTimeOffset,
                    strike
                );

                if (_timingVisualizer != null)
                {
                    _timingVisualizer.ShowResult(outcome);
                }

                yield return ResolveDefenseOutcome(outcome, strike);
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

        private IEnumerator ResolveDefenseOutcome(DefenseOutcome outcome, HitWindowDefinition strike)
        {
            switch (outcome)
            {
                case DefenseOutcome.ParrySuccess:
                    _apPool.Generate(2);
                    _audioPlayer?.PlayAPGain();
                    _audioPlayer?.PlayParrySuccess();
                    _gameFeel?.TriggerHitStop(true);
                    _gameFeel?.TriggerCameraShake(1.6f);

                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "PERFECT PARRY! (+2 AP)",
                        Color.green,
                        1.4f
                    );
                    _hud?.AddLog("<color=#00FF88>★ PERFECT PARRY! (+2 AP) Counter-strike triggered! ★</color>");

                    yield return new WaitForSeconds(0.2f);

                    bool counterComplete = false;
                    _playerView.PlayCounterAttack(_enemyView.transform.position, () =>
                    {
                        int counterDmg = Mathf.RoundToInt(_playerStats.AttackPower * 1.4f);
                        _enemyStats.ApplyDamage(counterDmg);
                        _enemyView.PlayHitReact();
                        _enemyView.FlashColor(Color.yellow, 0.2f);
                        _audioPlayer?.PlayHit();
                        _gameFeel?.TriggerHitStop(false);
                        _gameFeel?.TriggerCameraShake(1.1f);

                        FloatingCombatText.Spawn(
                            _enemyView.transform.position,
                            $"-{counterDmg} [COUNTER]",
                            Color.yellow,
                            1.3f
                        );
                        _hud?.AddLog($"Gustave counter-attacked for {counterDmg} damage!");
                    }, () => counterComplete = true);

                    while (!counterComplete)
                        yield return null;
                    break;

                case DefenseOutcome.DodgeSuccess:
                    _apPool.Generate(1);
                    _audioPlayer?.PlayAPGain();
                    _playerView.PlayDodge();
                    _audioPlayer?.PlayDodgeSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "DODGED! (+1 AP)",
                        new Color(1f, 0.9f, 0.2f),
                        1.2f
                    );
                    _hud?.AddLog("<color=#FFE040>Dodge successful! Damage evaded (+1 AP).</color>");
                    yield return new WaitForSeconds(0.5f);
                    break;

                case DefenseOutcome.JumpSuccess:
                    _apPool.Generate(1);
                    _audioPlayer?.PlayAPGain();
                    _playerView.PlayJump();
                    _audioPlayer?.PlayJumpSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "JUMPED! (+1 AP)",
                        new Color(0.3f, 0.8f, 1f),
                        1.2f
                    );
                    _hud?.AddLog("<color=#40C0FF>Jump successful! Low sweep evaded (+1 AP).</color>");
                    yield return new WaitForSeconds(0.6f);
                    break;

                default:
                    DamageResult result = DamageCalculator.CalculateDamage(_enemyStats, _playerStats, 1f, false);
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
                    _hud?.AddLog($"Gustave took {result.MitigatedDamage} damage!");
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
                _hud.OnPassSelected -= HandlePlayerPassSelected;
            }
        }
    }
}
