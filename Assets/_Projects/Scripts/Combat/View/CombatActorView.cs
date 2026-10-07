using System;
using DG.Tweening;
using UnityEngine;

namespace Expedition33.Combat
{
    public class CombatActorView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Renderer[] _renderers;

        private CombatActorStats _stats;
        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private Tweener _moveTween;

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

            _homePosition = transform.position;
            _homeRotation = transform.rotation;
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
            if (_animator != null && !_stats.IsDefeated)
            {
                _animator.CrossFadeInFixedTime("Idle", 0.15f);
            }
        }

        public void PlayAttack()
        {
            if (_animator != null)
            {
                _animator.CrossFadeInFixedTime("Attack", 0.1f);
            }
        }

        public void PlayHitReact()
        {
            if (_animator != null && !_stats.IsDefeated)
            {
                _animator.CrossFadeInFixedTime("HitReact", 0.05f);
            }
        }

        public void PlayDodge()
        {
            if (_animator != null)
            {
                _animator.CrossFadeInFixedTime("Dodge", 0.08f);
            }
        }

        public void PlayJump()
        {
            if (_animator != null)
            {
                _animator.CrossFadeInFixedTime("Jump", 0.08f);
            }
        }

        public void PlayDeath()
        {
            if (_animator != null)
            {
                _animator.CrossFadeInFixedTime("Death", 0.1f);
            }
        }

        public void AnimateApproach(Vector3 targetPosition, float duration, Action onArrival)
        {
            _moveTween?.Kill();

            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 stopPoint = targetPosition - direction * 1.5f;
            stopPoint.y = _homePosition.y;

            transform.DOLookAt(stopPoint, 0.15f, AxisConstraint.Y);
            _moveTween = transform.DOMove(stopPoint, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onArrival?.Invoke());
        }

        public void AnimateReturn(float duration, Action onArrival)
        {
            _moveTween?.Kill();

            transform.DOLookAt(_homePosition, 0.15f, AxisConstraint.Y);
            _moveTween = transform.DOMove(_homePosition, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    transform.rotation = _homeRotation;
                    PlayIdle();
                    onArrival?.Invoke();
                });
        }

        public void PlayCounterAttack(Vector3 targetPosition, Action onHit, Action onComplete)
        {
            _moveTween?.Kill();

            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 strikePos = targetPosition - direction * 1.4f;
            strikePos.y = _homePosition.y;

            transform.DOLookAt(targetPosition, 0.08f, AxisConstraint.Y);
            _moveTween = transform.DOMove(strikePos, 0.16f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    PlayAttack();
                    DOVirtual.DelayedCall(0.35f, () => onHit?.Invoke());
                    DOVirtual.DelayedCall(0.7f, () =>
                    {
                        AnimateReturn(0.3f, onComplete);
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
                    Color orig = r.material.color;
                    r.material.DOColor(color, duration * 0.5f).OnComplete(() =>
                    {
                        r.material.DOColor(orig, duration * 0.5f);
                    });
                }
            }
        }

        private void ApplyTint(Color tint)
        {
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

        private void OnDestroy()
        {
            _moveTween?.Kill();
        }
    }
}
