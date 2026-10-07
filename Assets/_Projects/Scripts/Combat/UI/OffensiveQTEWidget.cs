using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Expedition33.Combat
{
    public class OffensiveQTEWidget : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _container;

        [Header("Rings")]
        [SerializeField] private RectTransform _targetRingRect;
        [SerializeField] private RectTransform _shrinkingRingRect;
        [SerializeField] private Image _targetRingImage;
        [SerializeField] private Image _shrinkingRingImage;

        [Header("Text & Labels")]
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private TMP_Text _resultText;

        [Header("Ring Size Settings")]
        [SerializeField] private float _targetDiameter = 90f;
        [SerializeField] private float _startDiameter = 280f;

        private QTEWindowDefinition _currentConfig;
        private bool _isActive;
        private float _elapsed;
        private bool _inputCaptured;
        private float _inputTimestamp;

        private void Awake()
        {
            if (_container == null)
            {
                _container = gameObject;
            }

            if (!_isActive)
            {
                if (_container != null)
                    _container.SetActive(false);
            }
        }

        public void StartQTE(QTEWindowDefinition config, Vector3 worldTargetPos, Action<QTEOutcome> onComplete)
        {
            _isActive = true;
            _currentConfig = config ?? new QTEWindowDefinition();
            _elapsed = 0f;
            _inputCaptured = false;
            _inputTimestamp = -1f;

            if (_container != null)
                _container.SetActive(true);

            if (_resultText != null)
                _resultText.text = string.Empty;

            if (_promptText != null)
                _promptText.text = "TIMED STRIKE!\n<b>[SPACE]</b> or <b>[CLICK]</b>";

            if (_targetRingRect != null)
                _targetRingRect.sizeDelta = new Vector2(_targetDiameter, _targetDiameter);

            if (_shrinkingRingRect != null)
                _shrinkingRingRect.sizeDelta = new Vector2(_startDiameter, _startDiameter);

            // Position over target in screen coordinates if camera exists
            Camera cam = Camera.main;
            if (cam != null && transform is RectTransform rt)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(worldTargetPos + Vector3.up * 1.2f);
                rt.position = screenPos;
            }

            StartCoroutine(QTERoutine(onComplete));
        }

        private void Update()
        {
            if (!_isActive || _inputCaptured)
                return;

            bool pressed = (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

            if (pressed)
            {
                _inputCaptured = true;
                _inputTimestamp = _elapsed;
            }
        }

        private IEnumerator QTERoutine(Action<QTEOutcome> onComplete)
        {
            _isActive = true;

            while (_elapsed < _currentConfig.Duration)
            {
                _elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(_elapsed / _currentConfig.SweetSpotTime);

                // Interpolate shrinking ring towards target ring
                float currentSize = Mathf.Lerp(_startDiameter, _targetDiameter, progress);
                if (_shrinkingRingRect != null)
                {
                    _shrinkingRingRect.sizeDelta = new Vector2(currentSize, currentSize);
                }

                if (_inputCaptured)
                {
                    break;
                }

                yield return null;
            }

            _isActive = false;

            QTEOutcome outcome = QTEEvaluator.Evaluate(_inputTimestamp, _currentConfig);
            ShowResult(outcome);

            yield return new WaitForSeconds(0.4f);
            Hide();

            onComplete?.Invoke(outcome);
        }

        private void ShowResult(QTEOutcome outcome)
        {
            if (_resultText == null)
                return;

            switch (outcome)
            {
                case QTEOutcome.Perfect:
                    _resultText.text = "<color=#FFE838>★ PERFECT! ★</color>";
                    break;
                case QTEOutcome.Good:
                    _resultText.text = "<color=#66FF88>GOOD!</color>";
                    break;
                default:
                    _resultText.text = "<color=#AAAAAA>MISS</color>";
                    break;
            }
        }

        public void Hide()
        {
            _isActive = false;
            if (_container != null)
                _container.SetActive(false);
        }
    }
}
