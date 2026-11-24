using System.Collections;
using UnityEngine;

namespace Expedition33.Combat
{
    public class GameFeelManager : MonoBehaviour
    {
        [Header("Hit Stop Settings")]
        [SerializeField] private float _defaultHitStopDuration = 0.08f;
        [SerializeField] private float _criticalHitStopDuration = 0.16f;

        [Header("Camera Shake Settings")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CombatCameraController _cameraController;
        [SerializeField] private float _shakeDuration = 0.15f;
        [SerializeField] private float _shakeMagnitude = 0.12f;

        private Vector3 _currentShakeOffset;
        private Coroutine _activeShakeRoutine;
        private Coroutine _activeHitStopRoutine;
        private bool _isHitStopActive;
        private float _savedTimeScale = 1f;
        private float _hitStopEndTime;

        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_cameraController == null && _cameraTransform != null)
            {
                _cameraController = _cameraTransform.GetComponent<CombatCameraController>();
            }
        }

        private void OnDisable()
        {
            if (_activeHitStopRoutine != null)
            {
                StopCoroutine(_activeHitStopRoutine);
            }
            RestoreTimeScale();

            if (_activeShakeRoutine != null)
            {
                StopCoroutine(_activeShakeRoutine);
                _activeShakeRoutine = null;
                ApplyShakeOffset(Vector3.zero);
            }
        }

        public void TriggerHitStop(bool isCritical = false)
        {
            if (!isActiveAndEnabled)
                return;

            float duration = isCritical ? _criticalHitStopDuration : _defaultHitStopDuration;
            if (duration <= 0f)
                return;

            float targetEndTime = Time.unscaledTime + duration;

            if (_isHitStopActive)
            {
                // Keep the original pre-hitstop timeScale and simply extend the freeze window.
                _hitStopEndTime = Mathf.Max(_hitStopEndTime, targetEndTime);
                return;
            }

            _savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            _hitStopEndTime = targetEndTime;
            _isHitStopActive = true;
            _activeHitStopRoutine = StartCoroutine(HitStopRoutine());
        }

        public void TriggerCameraShake(float multiplier = 1f)
        {
            if (!isActiveAndEnabled || (_cameraTransform == null && _cameraController == null))
                return;

            if (_activeShakeRoutine != null)
            {
                StopCoroutine(_activeShakeRoutine);
                ApplyShakeOffset(Vector3.zero);
            }

            _activeShakeRoutine = StartCoroutine(CameraShakeRoutine(_shakeDuration, _shakeMagnitude * multiplier));
        }

        private IEnumerator HitStopRoutine()
        {
            Time.timeScale = 0f;

            while (Time.unscaledTime < _hitStopEndTime)
            {
                yield return null;
            }

            RestoreTimeScale();
        }

        private void RestoreTimeScale()
        {
            if (_isHitStopActive)
            {
                Time.timeScale = _savedTimeScale;
                _isHitStopActive = false;
            }
            _activeHitStopRoutine = null;
        }

        private IEnumerator CameraShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damp = 1f - (elapsed / duration);
                Vector3 offset = Random.insideUnitSphere * magnitude * damp;
                offset.z = 0f; // Maintain camera distance
                ApplyShakeOffset(offset);
                yield return null;
            }

            ApplyShakeOffset(Vector3.zero);
            _activeShakeRoutine = null;
        }

        private void ApplyShakeOffset(Vector3 newOffset)
        {
            if (_cameraController != null)
            {
                _currentShakeOffset = newOffset;
                _cameraController.SetShakeOffset(newOffset);
                return;
            }

            if (_cameraTransform != null)
            {
                _cameraTransform.localPosition += newOffset - _currentShakeOffset;
                _currentShakeOffset = newOffset;
            }
        }
    }
}
