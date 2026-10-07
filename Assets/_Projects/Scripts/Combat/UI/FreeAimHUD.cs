using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Expedition33.Combat
{
    public class FreeAimHUD : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _container;

        [Header("Crosshair Elements")]
        [SerializeField] private RectTransform _crosshairRect;
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private Image _crosshairCenterDot;

        [Header("Timer & Ammo Displays")]
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _ammoText;
        [SerializeField] private TMP_Text _weakPointPromptText;
        [SerializeField] private TMP_Text _controlsPromptText;

        public event Action<Vector2> OnFireRequested;
        public event Action OnCancelRequested;

        private bool _isActive;
        private Camera _aimCamera;

        public bool IsActive => _isActive;
        public Vector2 CrosshairScreenPosition => _crosshairRect != null ? _crosshairRect.position : (Vector2)Input.mousePosition;

        private void Awake()
        {
            if (_container == null)
            {
                _container = gameObject;
            }

            _aimCamera = Camera.main;
            if (!_isActive)
            {
                if (_container != null)
                    _container.SetActive(false);
            }
        }

        public void Show(Camera aimCamera)
        {
            _aimCamera = aimCamera != null ? aimCamera : Camera.main;
            _isActive = true;

            if (_container != null)
                _container.SetActive(true);

            if (_weakPointPromptText != null)
                _weakPointPromptText.text = string.Empty;

            if (_controlsPromptText != null)
                _controlsPromptText.text = "<b>[LEFT CLICK]</b> FIRE (1 AP)   |   <b>[RIGHT CLICK / ESC]</b> FINISH";

            // Center crosshair initially
            if (_crosshairRect != null)
            {
                _crosshairRect.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }
        }

        public void Hide()
        {
            _isActive = false;
            if (_container != null)
                _container.SetActive(false);
        }

        public void UpdateTimer(float remaining, float total)
        {
            float ratio = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;

            if (_timerFillImage != null)
            {
                _timerFillImage.fillAmount = ratio;
                _timerFillImage.color = remaining <= 1f ? new Color(1f, 0.2f, 0.2f) : new Color(0.2f, 0.8f, 1f);
            }

            if (_timerText != null)
            {
                _timerText.text = $"{remaining:F1}s";
                _timerText.color = remaining <= 1f ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        public void UpdateAmmo(int availableAP)
        {
            if (_ammoText != null)
            {
                _ammoText.text = $"SHOTS: <b>{availableAP}</b>  <color=#4DC4FF>[1 AP / SHOT]</color>";
            }
        }

        public void SetTargetState(HitboxType? targetType)
        {
            if (_crosshairImage == null)
                return;

            if (targetType.HasValue)
            {
                if (targetType.Value == HitboxType.WeakPoint)
                {
                    _crosshairImage.color = new Color(1f, 0.85f, 0.1f, 1f); // Vibrant Gold
                    if (_weakPointPromptText != null)
                        _weakPointPromptText.text = "<color=#FFE600><b>★ WEAK POINT TARGETED ★</b></color>";
                }
                else
                {
                    _crosshairImage.color = new Color(0.2f, 0.8f, 1f, 0.9f); // Cyan Body
                    if (_weakPointPromptText != null)
                        _weakPointPromptText.text = "<color=#4DC4FF>BODY TARGET</color>";
                }
            }
            else
            {
                _crosshairImage.color = new Color(1f, 1f, 1f, 0.6f); // Neutral White
                if (_weakPointPromptText != null)
                    _weakPointPromptText.text = string.Empty;
            }
        }

        private void Update()
        {
            if (!_isActive)
                return;

            // Follow mouse position smoothly
            if (_crosshairRect != null && Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                _crosshairRect.position = mousePos;
            }

            // Fire input
            bool firePressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);

            if (firePressed)
            {
                OnFireRequested?.Invoke(CrosshairScreenPosition);
            }

            // Cancel / exit input
            bool cancelPressed = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

            if (cancelPressed)
            {
                OnCancelRequested?.Invoke();
            }
        }
    }
}
