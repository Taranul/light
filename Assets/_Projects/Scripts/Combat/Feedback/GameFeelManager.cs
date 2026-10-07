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
        [SerializeField] private float _shakeDuration = 0.15f;
        [SerializeField] private float _shakeMagnitude = 0.12f;

        private Vector3 _originalCameraLocalPos;
        private Coroutine _activeShakeRoutine;
        private Coroutine _activeHitStopRoutine;

        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_cameraTransform != null)
            {
                _originalCameraLocalPos = _cameraTransform.localPosition;
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
            if (_cameraTransform == null)
                return;

            if (_activeShakeRoutine != null)
            {
                StopCoroutine(_activeShakeRoutine);
                _cameraTransform.localPosition = _originalCameraLocalPos;
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
                _cameraTransform.localPosition = _originalCameraLocalPos + offset;
                yield return null;
            }

            _cameraTransform.localPosition = _originalCameraLocalPos;
            _activeShakeRoutine = null;
        }
    }
}
