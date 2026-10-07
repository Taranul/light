using UnityEngine;
using UnityEngine.InputSystem;

namespace Expedition33.Combat
{
    public class InputBuffer : MonoBehaviour
    {
        [SerializeField] private float _bufferWindow = 0.12f;

        private DefenseActionType _bufferedAction = DefenseActionType.None;
        private float _bufferedTimestamp = -1f;
        private bool _isListening = false;

        public bool IsListening => _isListening;

        public void SetListening(bool listening)
        {
            _isListening = listening;
            if (!listening)
            {
                Clear();
            }
        }

        private void Update()
        {
            if (!_isListening)
                return;

            CheckInputs();
        }

        private void CheckInputs()
        {
            // Parry: F or E key, or Gamepad right shoulder / X button
            bool parryPressed = (Keyboard.current != null && (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                || (Gamepad.current != null && (Gamepad.current.rightShoulder.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame));

            // Dodge: Spacebar, or Gamepad B / buttonEast
            bool dodgePressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

            // Jump: C key or W key, or Gamepad A / buttonSouth
            bool jumpPressed = (Keyboard.current != null && (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame))
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

            float currentTime = Time.unscaledTime;

            if (parryPressed)
            {
                BufferAction(DefenseActionType.Parry, currentTime);
            }
            else if (dodgePressed)
            {
                BufferAction(DefenseActionType.Dodge, currentTime);
            }
            else if (jumpPressed)
            {
                BufferAction(DefenseActionType.Jump, currentTime);
            }
        }

        public void BufferAction(DefenseActionType action, float timestamp)
        {
            _bufferedAction = action;
            _bufferedTimestamp = timestamp;
        }

        public bool TryConsumeInput(float evaluationTime, out DefenseActionType action, out float timestamp)
        {
            if (_bufferedAction != DefenseActionType.None)
            {
                if (Mathf.Abs(evaluationTime - _bufferedTimestamp) <= _bufferWindow || _bufferedTimestamp >= evaluationTime - _bufferWindow)
                {
                    action = _bufferedAction;
                    timestamp = _bufferedTimestamp;
                    Clear();
                    return true;
                }
            }

            action = DefenseActionType.None;
            timestamp = -1f;
            return false;
        }

        public void Clear()
        {
            _bufferedAction = DefenseActionType.None;
            _bufferedTimestamp = -1f;
        }
    }
}
