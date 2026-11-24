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
        private Vector3 _defaultRotation;
        [SerializeField] private float _defaultFov = 60f;

        [Header("Free Aim Perspective (Over-the-Shoulder)")]
        [SerializeField] private Vector3 _freeAimPosition = new Vector3(-2.6f, 1.85f, -1.8f);
        [SerializeField] private Vector3 _freeAimLookAt = new Vector3(2.2f, 1.3f, 0f);
        [SerializeField] private float _freeAimFov = 45f;

        private Tweener _positionTween;
        private Tweener _rotationTween;
        private Tweener _fovTween;

        public Camera TargetCamera => _targetCamera;

        private void Awake()
        {
            _targetCamera = GetComponent<Camera>();

            _defaultPosition = _targetCamera.transform.position;
            _defaultRotation = _targetCamera.transform.rotation.eulerAngles;
        }

        public void BlendToFreeAim(float duration, Action onComplete)
        {
            KillTweens();

            Quaternion targetRot = Quaternion.LookRotation(_freeAimLookAt - _freeAimPosition);

            _positionTween = _targetCamera.transform.DOMove(_freeAimPosition, duration).SetEase(Ease.OutCubic);
            _rotationTween = _targetCamera.transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.OutCubic);
            _fovTween = DOTween.To(() => _targetCamera.fieldOfView, f => _targetCamera.fieldOfView = f, _freeAimFov, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void BlendToDefault(float duration, Action onComplete)
        {
            KillTweens();

            Quaternion targetRot = Quaternion.Euler(_defaultRotation);

            _positionTween = _targetCamera.transform.DOMove(_defaultPosition, duration).SetEase(Ease.InOutCubic);
            _rotationTween = _targetCamera.transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.InOutCubic);
            _fovTween = DOTween.To(() => _targetCamera.fieldOfView, f => _targetCamera.fieldOfView = f, _defaultFov, duration)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void KillTweens()
        {
            _positionTween?.Kill();
            _rotationTween?.Kill();
            _fovTween?.Kill();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
