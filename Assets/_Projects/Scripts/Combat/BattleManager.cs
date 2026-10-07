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

        [Header("Enemy Attack Patterns")]
        [SerializeField] private EnemyAttackPatternSO[] _enemyAttackPatterns;

        private BattleStateMachine _stateMachine;
        private TurnTimeline _timeline;
        private CombatActorStats _playerStats;
        private CombatActorStats _enemyStats;

        private bool _playerActionChosen;
        private CombatActionType _selectedAction;
        private int _enemyTurnCounter;

        public CombatActorStats PlayerStats => _playerStats;
        public CombatActorStats EnemyStats => _enemyStats;
        public BattleStateMachine StateMachine => _stateMachine;

        private void Start()
        {
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            _stateMachine = new BattleStateMachine(BattleState.Intro);
            _timeline = new TurnTimeline();

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
                _hud.SetupPlayerStatus(_playerStats);
                _hud.SetupEnemyStatus(_enemyStats);
                _hud.OnAttackSelected += HandlePlayerAttackSelected;
                _hud.OnPassSelected += HandlePlayerPassSelected;
            }

            RefreshTimelineHUD();

            StartCoroutine(BattleRoutine());
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
            _playerActionChosen = true;
        }

        private void HandlePlayerPassSelected()
        {
            _audioPlayer?.PlayButtonClick();
            _selectedAction = CombatActionType.Pass;
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
            _hud?.AddLog("Choose your action.");
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
                yield return ExecutePlayerAttack();
            }
            else
            {
                _hud?.AddLog($"{_playerStats.Name} passed the turn.");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator ExecutePlayerAttack()
        {
            bool arrived = false;
            _playerView.AnimateApproach(_enemyView.HomePosition, 0.45f, () => arrived = true);
            while (!arrived)
                yield return null;

            _playerView.PlayAttack();
            yield return new WaitForSeconds(0.75f);

            DamageResult result = DamageCalculator.CalculateDamage(_playerStats, _enemyStats, 1f, false);
            _enemyStats.ApplyDamage(result.MitigatedDamage);

            _enemyView.PlayHitReact();
            _enemyView.FlashColor(Color.red, 0.2f);
            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(result.IsCritical);
            _gameFeel?.TriggerCameraShake(1f);

            FloatingCombatText.Spawn(
                _enemyView.transform.position,
                $"-{result.MitigatedDamage}",
                result.IsCritical ? Color.yellow : Color.white
            );

            _hud?.AddLog($"{_playerStats.Name} dealt {result.MitigatedDamage} damage to {_enemyStats.Name}!");
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

        private IEnumerator EnemyTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.EnemyTurn);
            _hud?.SetTurnBanner("ENEMY TURN");

            // Alternate between attack patterns
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

                // Setup visualizer
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

                // Telegraph windup loop
                while (elapsed < strike.HitTimeOffset)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / totalStrikeDuration);

                    if (_timingVisualizer != null)
                    {
                        _timingVisualizer.SetCursorProgress(progress);
                    }

                    // Continuously check for buffered defense input
                    if (_inputBuffer != null && capturedAction == DefenseActionType.None)
                    {
                        if (_inputBuffer.TryConsumeInput(elapsed, out DefenseActionType action, out float ts))
                        {
                            capturedAction = action;
                            capturedTimestamp = elapsed; // relative time of input
                        }
                    }

                    yield return null;
                }

                if (_inputBuffer != null)
                {
                    _inputBuffer.SetListening(false);
                }

                // Evaluate outcome
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

                // Resolve outcome
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
                    _audioPlayer?.PlayParrySuccess();
                    _gameFeel?.TriggerHitStop(true);
                    _gameFeel?.TriggerCameraShake(1.6f);

                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "PERFECT PARRY!",
                        Color.green,
                        1.4f
                    );
                    _hud?.AddLog("<color=#00FF88>★ PERFECT PARRY! Counter-strike triggered! ★</color>");

                    yield return new WaitForSeconds(0.2f);

                    // Immediate Counter-Attack
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
                    _playerView.PlayDodge();
                    _audioPlayer?.PlayDodgeSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "DODGED!",
                        new Color(1f, 0.9f, 0.2f),
                        1.2f
                    );
                    _hud?.AddLog("<color=#FFE040>Dodge successful! Damage completely evaded.</color>");
                    yield return new WaitForSeconds(0.5f);
                    break;

                case DefenseOutcome.JumpSuccess:
                    _playerView.PlayJump();
                    _audioPlayer?.PlayJumpSuccess();
                    FloatingCombatText.Spawn(
                        _playerView.transform.position,
                        "JUMPED!",
                        new Color(0.3f, 0.8f, 1f),
                        1.2f
                    );
                    _hud?.AddLog("<color=#40C0FF>Jump successful! Low sweep evaded.</color>");
                    yield return new WaitForSeconds(0.6f);
                    break;

                default: // Miss or InvalidAction
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
                _hud.OnPassSelected -= HandlePlayerPassSelected;
            }
        }
    }
}
