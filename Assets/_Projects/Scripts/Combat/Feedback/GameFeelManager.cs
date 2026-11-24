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

        public void TriggerHitStop(bool isCritical = false)
        {
            float duration = isCritical ? _criticalHitStopDuration : _defaultHitStopDuration;
            if (_activeHitStopRoutine != null)
            {
                StopCoroutine(_activeHitStopRoutine);
            }
            _activeHitStopRoutine = StartCoroutine(HitStopRoutine(duration));
        }

        public void TriggerCameraShake(float multiplier = 1f)
        {
            if (_cameraTransform == null && _cameraController == null)
                return;

            if (_activeShakeRoutine != null)
            {
                StopCoroutine(_activeShakeRoutine);
                ApplyShakeOffset(Vector3.zero);
            }

            _activeShakeRoutine = StartCoroutine(CameraShakeRoutine(_shakeDuration, _shakeMagnitude * multiplier));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
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
