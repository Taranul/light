using System;
using DG.Tweening;
using UnityEngine;

namespace Expedition33.Combat
{
    [RequireComponent(typeof(Camera))]
    public class CombatCameraController : MonoBehaviour
    {
        [Header("Camera Rig")]
        [SerializeField] private Camera _targetCamera;

        [Header("Default Battle Perspective")]
        private Vector3 _defaultPosition;
        private Quaternion _defaultRotation;
        [SerializeField] private float _defaultFov = 60f;

        [Header("Player Turn Perspective (Over-the-Shoulder Hero Angle)")]
        [SerializeField] private Vector3 _playerTurnPosition = new Vector3(-3.55f, 1.80f, -0.95f);
        [SerializeField] private Vector3 _playerTurnLookAt = new Vector3(1.60f, 1.25f, 0.10f);
        [SerializeField] private float _playerTurnFov = 55f;

        [Header("Player Action Perspective (Dynamic Behind-Player Follow)")]
        [SerializeField] private Vector3 _playerActionPosition = new Vector3(-1.65f, 2.05f, -1.25f);
        [SerializeField] private Vector3 _playerActionLookAt = new Vector3(1.90f, 1.20f, 0.0f);
        [SerializeField] private float _playerActionFov = 56f;

        [Header("Free Aim Perspective (Tight Over-the-Shoulder)")]
        [SerializeField] private Vector3 _freeAimPosition = new Vector3(-3.35f, 1.72f, -0.92f);
        [SerializeField] private Vector3 _freeAimLookAt = new Vector3(2.20f, 1.32f, 0.0f);
        [SerializeField] private float _freeAimFov = 42f;

        [Header("Enemy Turn Perspective (Behind-Player Defensive Ready)")]
        [SerializeField] private Vector3 _enemyTurnPosition = new Vector3(-4.55f, 2.25f, -1.35f);
        [SerializeField] private Vector3 _enemyTurnLookAt = new Vector3(1.10f, 1.15f, 0.0f);
        [SerializeField] private float _enemyTurnFov = 50f;

        [Header("Enemy Attack Perspective (Low Behind-Player Parry Framing)")]
        [SerializeField] private Vector3 _enemyAttackPosition = new Vector3(-3.95f, 1.70f, -1.45f);
        [SerializeField] private Vector3 _enemyAttackLookAt = new Vector3(-0.60f, 1.15f, 0.0f);
        [SerializeField] private float _enemyAttackFov = 46f;

        [Header("Idle Breathing & Turn Punch")]
        [SerializeField] private Vector2 _idleDriftPositionAmplitude = new Vector2(0.035f, 0.025f);
        [SerializeField] private float _idleDriftRotationAmplitude = 0.35f;
        [SerializeField] private float _idleDriftSpeed = 0.85f;
        [SerializeField] private float _turnPunchFovDelta = -2.5f;
        [SerializeField] private float _turnPunchDuration = 0.35f;

        private Vector3 _basePosition;
        private Quaternion _baseRotation;
        private float _baseFov;

        private Vector3 _shakeOffset;
        private float _fovPunchOffset;
        private float _driftWeight = 1f;

        private Tweener _positionTween;
        private Tweener _rotationTween;
        private Tweener _fovTween;
        private Tweener _punchTween;
        private Tweener _driftWeightTween;
        private Action _pendingOnComplete;

        public Camera TargetCamera => _targetCamera;

        private void Awake()
        {
            if (_targetCamera == null)
                _targetCamera = GetComponent<Camera>();

            _defaultPosition = _targetCamera.transform.position;
            _defaultRotation = _targetCamera.transform.rotation;

            _basePosition = _defaultPosition;
            _baseRotation = _defaultRotation;
            _baseFov = _defaultFov;
        }

        private void LateUpdate()
        {
            if (_targetCamera == null)
                return;

            float t = Time.time * _idleDriftSpeed;
            Vector3 localBreathing = new Vector3(
                Mathf.Sin(t) * _idleDriftPositionAmplitude.x,
                Mathf.Sin(t * 1.43f + 0.7f) * _idleDriftPositionAmplitude.y,
                0f
            ) * _driftWeight;

            Vector3 worldBreathing = _baseRotation * localBreathing;
            Quaternion rotBreathing = Quaternion.Euler(
                Mathf.Cos(t * 1.15f) * _idleDriftRotationAmplitude * _driftWeight,
                Mathf.Sin(t * 0.75f) * _idleDriftRotationAmplitude * _driftWeight,
                0f
            );

            _targetCamera.transform.position = _basePosition + worldBreathing + _shakeOffset;
            _targetCamera.transform.rotation = _baseRotation * rotBreathing;
            _targetCamera.fieldOfView = Mathf.Clamp(_baseFov + _fovPunchOffset, 20f, 110f);
        }

        public void SetShakeOffset(Vector3 offset)
        {
            _shakeOffset = offset;
        }

        public void BlendToTurn(bool isPlayerTurn, float duration = 0.38f, Action onComplete = null)
        {
            if (isPlayerTurn)
                BlendToPlayerTurn(duration, onComplete);
            else
                BlendToEnemyTurn(duration, onComplete);
        }

        public void BlendToPlayerTurn(float duration = 0.38f, Action onComplete = null, bool triggerTurnPunch = true)
        {
            Quaternion targetRot = Quaternion.LookRotation(_playerTurnLookAt - _playerTurnPosition);
            BlendToPose(_playerTurnPosition, targetRot, _playerTurnFov, duration, Ease.OutCubic, driftWeight: 1f, triggerTurnPunch: triggerTurnPunch, onComplete);
        }

        public void BlendToPlayerAction(float duration = 0.35f, Action onComplete = null)
        {
            Quaternion targetRot = Quaternion.LookRotation(_playerActionLookAt - _playerActionPosition);
            BlendToPose(_playerActionPosition, targetRot, _playerActionFov, duration, Ease.OutCubic, driftWeight: 0.35f, triggerTurnPunch: false, onComplete);
        }

        public void BlendToEnemyTurn(float duration = 0.38f, Action onComplete = null)
        {
            Quaternion targetRot = Quaternion.LookRotation(_enemyTurnLookAt - _enemyTurnPosition);
            BlendToPose(_enemyTurnPosition, targetRot, _enemyTurnFov, duration, Ease.OutCubic, driftWeight: 1f, triggerTurnPunch: true, onComplete);
        }

        public void BlendToEnemyAttack(float duration = 0.35f, Action onComplete = null)
        {
            Quaternion targetRot = Quaternion.LookRotation(_enemyAttackLookAt - _enemyAttackPosition);
            BlendToPose(_enemyAttackPosition, targetRot, _enemyAttackFov, duration, Ease.OutCubic, driftWeight: 0.25f, triggerTurnPunch: false, onComplete);
        }

        public void BlendToFreeAim(float duration, Action onComplete)
        {
            Quaternion targetRot = Quaternion.LookRotation(_freeAimLookAt - _freeAimPosition);
            BlendToPose(_freeAimPosition, targetRot, _freeAimFov, duration, Ease.OutCubic, driftWeight: 0f, triggerTurnPunch: false, onComplete);
        }

        public void BlendToDefault(float duration, Action onComplete)
        {
            BlendToPose(_defaultPosition, _defaultRotation, _defaultFov, duration, Ease.InOutCubic, driftWeight: 1f, triggerTurnPunch: false, onComplete);
        }

        private void BlendToPose(
            Vector3 targetPos,
            Quaternion targetRot,
            float targetFov,
            float duration,
            Ease ease,
            float driftWeight,
            bool triggerTurnPunch,
            Action onComplete)
        {
            KillMovementTweens(invokePendingCallback: true);

            if (!isActiveAndEnabled || duration <= 0f)
            {
                _basePosition = targetPos;
                _baseRotation = targetRot;
                _baseFov = targetFov;
                _driftWeight = driftWeight;
                onComplete?.Invoke();
                return;
            }

            _pendingOnComplete = onComplete;

            Quaternion startRot = _baseRotation;
            _positionTween = DOTween.To(() => _basePosition, p => _basePosition = p, targetPos, duration).SetEase(ease);
            _rotationTween = DOTween.To(() => 0f, t => _baseRotation = Quaternion.Slerp(startRot, targetRot, t), 1f, duration).SetEase(ease);
            _fovTween = DOTween.To(() => _baseFov, f => _baseFov = f, targetFov, duration)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    Action cb = _pendingOnComplete;
                    _pendingOnComplete = null;
                    cb?.Invoke();
                });

            _driftWeightTween = DOTween.To(() => _driftWeight, w => _driftWeight = w, driftWeight, duration * 0.6f)
                .SetEase(Ease.OutQuad);

            if (triggerTurnPunch)
            {
                _punchTween?.Kill();
                _fovPunchOffset = _turnPunchFovDelta;
                _punchTween = DOTween.To(() => _fovPunchOffset, v => _fovPunchOffset = v, 0f, _turnPunchDuration)
                    .SetEase(Ease.OutCubic);
            }
        }

        private void KillMovementTweens(bool invokePendingCallback = false)
        {
            _positionTween?.Kill();
            _rotationTween?.Kill();
            _fovTween?.Kill();
            _driftWeightTween?.Kill();

            Action pending = _pendingOnComplete;
            _pendingOnComplete = null;
            if (invokePendingCallback)
            {
                pending?.Invoke();
            }
        }

        private void OnDestroy()
        {
            KillMovementTweens(invokePendingCallback: false);
            _punchTween?.Kill();
        }
    }
}
