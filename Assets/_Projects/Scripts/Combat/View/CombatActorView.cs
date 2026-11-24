using System;
using DG.Tweening;
using UnityEngine;

namespace Expedition33.Combat
{
    public class CombatActorView : MonoBehaviour
    {
        private const float DefaultApproachStopDistance = 1.65f;
        private const float CounterStrikeStopDistance = 1.55f;
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");

        [SerializeField] private Animator _animator;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Transform _hipsTransform;
        [SerializeField] private float _forwardHipsScale = 0.28f;
        [SerializeField] private float _maxForwardHipsOffset = 0.48f;
        [SerializeField] private float _lateralHipsScale = 0.45f;
        [SerializeField] private float _maxLateralHipsOffset = 0.85f;

        private CombatActorStats _stats;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private Color _baseTintColor = Color.white;
        private Tweener _moveTween;
        private Tweener _lookTween;
        private Action _pendingMoveCallback;
        private Sequence _counterSequence;
        private bool _hasAttackSpeedParam;

        public CombatActorStats Stats => _stats;
        public Vector3 HomePosition => _homePosition;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }

            if (_hipsTransform == null)
            {
                _hipsTransform = FindHipsTransform(transform);
            }

            CacheAnimatorParameters();

            _homePosition = transform.position;
            _homeRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            ClampHipsHorizontalMotion();
        }

        public void Initialize(CombatActorStats stats, Color tintColor)
        {
            _stats = stats;
            ApplyTint(tintColor);
        }

        public void SetHomePosition(Vector3 position, Quaternion rotation)
        {
            _homePosition = position;
            _homeRotation = rotation;
            transform.position = position;
            transform.rotation = rotation;
        }

        public void PlayIdle()
        {
            if (_animator != null && (_stats == null || !_stats.IsDefeated))
            {
                SetAttackSpeed(1f);
                _animator.CrossFadeInFixedTime("Idle", 0.15f);
            }
        }

        public void PlayAttack(float speed = 1f, float fixedTimeOffset = 0f)
        {
            if (_animator != null)
            {
                SetAttackSpeed(Mathf.Max(0.25f, speed));
                _animator.CrossFadeInFixedTime("Attack", 0.08f, 0, fixedTimeOffset);
            }
        }

        public void PlayHitReact()
        {
            if (_animator != null && (_stats == null || !_stats.IsDefeated))
            {
                SetAttackSpeed(1f);
                _animator.CrossFadeInFixedTime("HitReact", 0.05f, 0, 0f);
            }
        }

        public void PlayDodge(float fixedTimeOffset = 0f)
        {
            if (_animator != null)
            {
                SetAttackSpeed(1f);
                _animator.CrossFadeInFixedTime("Dodge", 0.06f, 0, fixedTimeOffset);
            }
        }

        public void PlayJump(float fixedTimeOffset = 0f)
        {
            if (_animator != null)
            {
                SetAttackSpeed(1f);
                _animator.CrossFadeInFixedTime("Jump", 0.06f, 0, fixedTimeOffset);
            }
        }

        public void PlayDeath()
        {
            if (_animator != null)
            {
                SetAttackSpeed(1f);
                _animator.CrossFadeInFixedTime("Death", 0.1f, 0, 0f);
            }
        }

        public void AnimateApproach(Vector3 targetPosition, float duration, Action onArrival, float stopDistance = DefaultApproachStopDistance)
        {
            CancelMovement(invokePendingCallback: true);

            Vector3 flatTarget = new Vector3(targetPosition.x, _homePosition.y, targetPosition.z);
            Vector3 direction = ResolveHorizontalDirection(transform.position, flatTarget);
            Vector3 stopPoint = flatTarget - direction * stopDistance;
            stopPoint.y = _homePosition.y;

            if (!isActiveAndEnabled || duration <= 0f)
            {
                transform.position = stopPoint;
                transform.rotation = Quaternion.LookRotation(direction);
                onArrival?.Invoke();
                return;
            }

            _pendingMoveCallback = onArrival;
            _lookTween = transform.DOLookAt(flatTarget, 0.15f, AxisConstraint.Y);
            _moveTween = transform.DOMove(stopPoint, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(InvokePendingMoveCallback);
        }

        public void AnimateReturn(float duration, Action onArrival)
        {
            CancelMovement(invokePendingCallback: true);

            if (!isActiveAndEnabled || duration <= 0f)
            {
                transform.position = _homePosition;
                transform.rotation = _homeRotation;
                PlayIdle();
                onArrival?.Invoke();
                return;
            }

            _pendingMoveCallback = onArrival;
            _lookTween = transform.DOLookAt(_homePosition, 0.15f, AxisConstraint.Y);
            _moveTween = transform.DOMove(_homePosition, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    _lookTween?.Kill();
                    _lookTween = null;
                    transform.rotation = _homeRotation;
                    PlayIdle();
                    InvokePendingMoveCallback();
                });
        }

        public void PlayCounterAttack(Vector3 targetPosition, Action onHit, Action onComplete)
        {
            CancelMovement(invokePendingCallback: true);

            Vector3 flatTarget = new Vector3(targetPosition.x, _homePosition.y, targetPosition.z);
            Vector3 direction = ResolveHorizontalDirection(transform.position, flatTarget);
            Vector3 strikePos = flatTarget - direction * CounterStrikeStopDistance;
            strikePos.y = _homePosition.y;

            if (!isActiveAndEnabled)
            {
                onHit?.Invoke();
                onComplete?.Invoke();
                return;
            }

            _pendingMoveCallback = onComplete;
            _lookTween = transform.DOLookAt(flatTarget, 0.08f, AxisConstraint.Y);
            _moveTween = transform.DOMove(strikePos, 0.18f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    PlayAttack(speed: 1.6f, fixedTimeOffset: 0.25f);
                    _counterSequence = DOTween.Sequence()
                        .AppendInterval(0.48f)
                        .AppendCallback(() => onHit?.Invoke())
                        .AppendInterval(0.35f)
                        .AppendCallback(() =>
                        {
                            Action pending = _pendingMoveCallback;
                            _pendingMoveCallback = null;
                            AnimateReturn(0.3f, pending);
                        });
                });
        }

        public void FlashColor(Color color, float duration = 0.18f)
        {
            if (_renderers == null)
                return;

            foreach (Renderer r in _renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.DOKill();
                    r.material.DOColor(color, duration * 0.5f).OnComplete(() =>
                    {
                        if (r != null && r.material != null)
                        {
                            r.material.DOColor(_baseTintColor, duration * 0.5f);
                        }
                    });
                }
            }
        }

        private void ApplyTint(Color tint)
        {
            _baseTintColor = tint;
            if (_renderers == null)
                return;

            foreach (Renderer r in _renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = tint;
                }
            }
        }

        private void InvokePendingMoveCallback()
        {
            Action cb = _pendingMoveCallback;
            _pendingMoveCallback = null;
            cb?.Invoke();
        }

        private void CancelMovement(bool invokePendingCallback)
        {
            _moveTween?.Kill();
            _moveTween = null;
            _lookTween?.Kill();
            _lookTween = null;
            _counterSequence?.Kill();
            _counterSequence = null;

            if (invokePendingCallback)
            {
                InvokePendingMoveCallback();
            }
            else
            {
                _pendingMoveCallback = null;
            }
        }

        private void ClampHipsHorizontalMotion()
        {
            if (_hipsTransform == null || (_stats != null && _stats.IsDefeated))
                return;

            // Mixamo clips encode large forward lunges (+1.7m) and sideways dodges (+1.9m) on Hips.
            // Scale & clamp horizontal root-relative offset so characters never pass through each other.
            Vector3 hipsInRoot = transform.InverseTransformPoint(_hipsTransform.position);
            if (hipsInRoot.z > 0f)
            {
                hipsInRoot.z = Mathf.Min(hipsInRoot.z * _forwardHipsScale, _maxForwardHipsOffset);
            }

            hipsInRoot.x = Mathf.Clamp(hipsInRoot.x * _lateralHipsScale, -_maxLateralHipsOffset, _maxLateralHipsOffset);
            _hipsTransform.position = transform.TransformPoint(hipsInRoot);
        }

        private Vector3 ResolveHorizontalDirection(Vector3 from, Vector3 to)
        {
            Vector3 delta = new Vector3(to.x - from.x, 0f, to.z - from.z);
            if (delta.sqrMagnitude > 0.0001f)
            {
                return delta.normalized;
            }

            Vector3 fallback = _homeRotation * Vector3.forward;
            fallback.y = 0f;
            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
        }

        private void CacheAnimatorParameters()
        {
            _hasAttackSpeedParam = false;
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.nameHash == AttackSpeedHash && param.type == AnimatorControllerParameterType.Float)
                {
                    _hasAttackSpeedParam = true;
                    break;
                }
            }
        }

        private void SetAttackSpeed(float speed)
        {
            if (_animator != null && _hasAttackSpeedParam)
            {
                _animator.SetFloat(AttackSpeedHash, speed);
            }
        }

        private static Transform FindHipsTransform(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.name.IndexOf("Hips", StringComparison.OrdinalIgnoreCase) >= 0)
                    return child;

                Transform nested = FindHipsTransform(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private void OnDestroy()
        {
            CancelMovement(invokePendingCallback: false);

            if (_renderers != null)
            {
                foreach (Renderer r in _renderers)
                {
                    if (r != null && r.material != null)
                    {
                        r.material.DOKill();
                    }
                }
            }
        }
    }
}
