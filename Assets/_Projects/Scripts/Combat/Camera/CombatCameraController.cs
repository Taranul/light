using System;
using DG.Tweening;
using UnityEngine;

namespace Expedition33.Combat
{
    public class CombatCameraController : MonoBehaviour
    {
        [Header("Camera Rig")]
        [SerializeField] private Camera _targetCamera;

        [Header("Default Battle Perspective")]
        [SerializeField] private Vector3 _defaultPosition = new Vector3(0f, 3.2f, -6f);
        [SerializeField] private Vector3 _defaultRotation = new Vector3(20f, 0f, 0f);
        [SerializeField] private float _defaultFov = 60f;

        [Header("Free Aim Perspective (Over-the-Shoulder)")]
        [SerializeField] private Vector3 _freeAimPosition = new Vector3(-2.6f, 1.85f, -1.8f);
        [SerializeField] private Vector3 _freeAimLookAt = new Vector3(2.2f, 1.3f, 0f);
        [SerializeField] private float _freeAimFov = 45f;

        private Tweener _posTween;
        private Tweener _rotTween;
        private Tweener _fovTween;

        public Camera TargetCamera => _targetCamera;

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = GetComponent<Camera>();
                if (_targetCamera == null)
                {
                    _targetCamera = Camera.main;
                }
            }
        }

        public void BlendToFreeAim(float duration, Action onComplete)
        {
            KillTweens();

            if (_targetCamera == null)
            {
                onComplete?.Invoke();
                return;
            }

            Quaternion targetRot = Quaternion.LookRotation(_freeAimLookAt - _freeAimPosition);

            _posTween = _targetCamera.transform.DOMove(_freeAimPosition, duration).SetEase(Ease.OutCubic);
            _rotTween = _targetCamera.transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.OutCubic);
            _fovTween = DOTween.To(() => _targetCamera.fieldOfView, f => _targetCamera.fieldOfView = f, _freeAimFov, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void BlendToDefault(float duration, Action onComplete)
        {
            KillTweens();

            if (_targetCamera == null)
            {
                onComplete?.Invoke();
                return;
            }

            Quaternion targetRot = Quaternion.Euler(_defaultRotation);

            _posTween = _targetCamera.transform.DOMove(_defaultPosition, duration).SetEase(Ease.InOutCubic);
            _rotTween = _targetCamera.transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.InOutCubic);
            _fovTween = DOTween.To(() => _targetCamera.fieldOfView, f => _targetCamera.fieldOfView = f, _defaultFov, duration)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void KillTweens()
        {
            _posTween?.Kill();
            _rotTween?.Kill();
            _fovTween?.Kill();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
