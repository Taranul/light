using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public class BattleStateMachine
    {
        private BattleState _currentState;

        public BattleState CurrentState => _currentState;

        public event Action<BattleState, BattleState> OnStateChanged;

        public BattleStateMachine(BattleState initialState = BattleState.Intro)
        {
            _currentState = initialState;
        }

        public void ChangeState(BattleState newState)
        {
            if (_currentState == newState)
                return;

            BattleState previousState = _currentState;
            _currentState = newState;
            Debug.Log($"[BattleStateMachine] Transition: {previousState} -> {newState}");
            OnStateChanged?.Invoke(previousState, newState);
        }
    }
}
