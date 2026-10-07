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

        private BattleStateMachine _stateMachine;
        private TurnTimeline _timeline;
        private CombatActorStats _playerStats;
        private CombatActorStats _enemyStats;

        private bool _playerActionChosen;
        private CombatActionType _selectedAction;

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

            // Setup views
            Color playerTint = _playerData != null ? _playerData.TintColor : new Color(0.85f, 0.95f, 1f);
            Color enemyTint = _enemyData != null ? _enemyData.TintColor : new Color(1f, 0.6f, 0.6f);

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
                yield return ExecuteActorAttack(_playerView, _enemyView, _playerStats, _enemyStats);
            }
            else
            {
                _hud?.AddLog($"{_playerStats.Name} passed the turn.");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator EnemyTurnRoutine()
        {
            _stateMachine.ChangeState(BattleState.EnemyTurn);
            _hud?.SetTurnBanner("ENEMY TURN");
            _hud?.AddLog($"{_enemyStats.Name} prepares to strike!");
            yield return new WaitForSeconds(0.8f);

            _stateMachine.ChangeState(BattleState.ActionExecuting);
            yield return ExecuteActorAttack(_enemyView, _playerView, _enemyStats, _playerStats);
        }

        private IEnumerator ExecuteActorAttack(
            CombatActorView attacker,
            CombatActorView defender,
            CombatActorStats attackerStats,
            CombatActorStats defenderStats)
        {
            bool arrived = false;
            attacker.AnimateApproach(defender.HomePosition, 0.45f, () => arrived = true);
            while (!arrived)
                yield return null;

            attacker.PlayAttack();

            // Wait for strike frame (roughly 0.75s in Surprise Uppercut clip)
            yield return new WaitForSeconds(0.75f);

            DamageResult result = DamageCalculator.CalculateDamage(attackerStats, defenderStats, 1f, false);
            defenderStats.ApplyDamage(result.MitigatedDamage);

            // Feedback
            defender.PlayHitReact();
            defender.FlashColor(Color.red, 0.2f);
            _audioPlayer?.PlayHit();
            _gameFeel?.TriggerHitStop(result.IsCritical);
            _gameFeel?.TriggerCameraShake(1f);

            FloatingCombatText.Spawn(
                defender.transform.position,
                $"-{result.MitigatedDamage}",
                result.IsCritical ? Color.yellow : Color.white,
                result.IsCritical ? 1.3f : 1f
            );

            _hud?.AddLog($"{attackerStats.Name} dealt {result.MitigatedDamage} damage to {defenderStats.Name}!");

            yield return new WaitForSeconds(0.4f);

            if (!defenderStats.IsDefeated)
            {
                defender.PlayIdle();
            }

            bool returned = false;
            attacker.AnimateReturn(0.4f, () => returned = true);
            while (!returned)
                yield return null;
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
