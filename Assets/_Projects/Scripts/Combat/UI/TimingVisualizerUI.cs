using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    public class TimingVisualizerUI : MonoBehaviour
    {
        [Header("Root & Visibility")]
        [SerializeField] private GameObject _container;

        [Header("Timeline Bar")]
        [SerializeField] private RectTransform _barTrackRect;
        [SerializeField] private RectTransform _cursorRect;
        [SerializeField] private RectTransform _dodgeZoneRect;
        [SerializeField] private RectTransform _parryZoneRect;
        [SerializeField] private RectTransform _strikeLineRect;

        [Header("Colors & Sprites")]
        [SerializeField] private Image _parryZoneImage;
        [SerializeField] private Image _dodgeZoneImage;
        [SerializeField] private Image _cursorImage;

        [Header("Text Prompts")]
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private TMP_Text _resultText;

        private bool _isActive;

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

        public void Show(
            string attackName,
            AttackTelegraphType type,
            float hitNormalizedPos,
            float parryNormalizedHalfWidth,
            float dodgeNormalizedHalfWidth)
        {
            _isActive = true;
            if (_container != null)
                _container.SetActive(true);

            if (_resultText != null)
                _resultText.text = string.Empty;

            ConfigurePrompts(attackName, type);
            ConfigureZones(hitNormalizedPos, parryNormalizedHalfWidth, dodgeNormalizedHalfWidth, type);
            SetCursorProgress(0f);
        }

        public void SetCursorProgress(float progress)
        {
            if (_cursorRect == null || _barTrackRect == null)
                return;

            float trackWidth = _barTrackRect.rect.width;
            float targetX = (Mathf.Clamp01(progress) - 0.5f) * trackWidth;
            _cursorRect.anchoredPosition = new Vector2(targetX, 0f);
        }

        public void ShowResult(DefenseOutcome outcome)
        {
            if (_resultText == null)
                return;

            switch (outcome)
            {
                case DefenseOutcome.ParrySuccess:
                    _resultText.text = "<color=#00FF88>★ PERFECT PARRY! ★</color>";
                    break;
                case DefenseOutcome.DodgeSuccess:
                    _resultText.text = "<color=#FFE040>DODGED!</color>";
                    break;
                case DefenseOutcome.JumpSuccess:
                    _resultText.text = "<color=#40C0FF>JUMPED!</color>";
                    break;
                case DefenseOutcome.InvalidAction:
                    _resultText.text = "<color=#FF6644>WRONG DEFENSE!</color>";
                    break;
                default:
                    _resultText.text = "<color=#FF3333>HIT TAKEN!</color>";
                    break;
            }

            StartCoroutine(FadeResultRoutine());
        }

        public void Hide()
        {
            if (_container != null)
                _container.SetActive(false);
        }

        private void ConfigurePrompts(string attackName, AttackTelegraphType type)
        {
            if (_promptText == null)
                return;

            switch (type)
            {
                case AttackTelegraphType.GroundSweep:
                    _promptText.text = $"<color=#40C0FF><b>[SWEEP ATTACK]</b> {attackName}\nPress <b>[C] or [W] to JUMP</b></color>";
                    break;
                case AttackTelegraphType.HeavyUnblockable:
                    _promptText.text = $"<color=#FF4444><b>[UNBLOCKABLE]</b> {attackName}\nPress <b>[SPACE] to DODGE ONLY</b></color>";
                    break;
                default:
                    _promptText.text = $"<b>{attackName}</b>\n<b>[F]</b> PARRY  |  <b>[SPACE]</b> DODGE";
                    break;
            }
        }

        private void ConfigureZones(
            float hitPos,
            float parryHalfWidth,
            float dodgeHalfWidth,
            AttackTelegraphType type)
        {
            if (_barTrackRect == null)
                return;

            float trackWidth = _barTrackRect.rect.width;

            // Strike line
            if (_strikeLineRect != null)
            {
                float strikeX = (hitPos - 0.5f) * trackWidth;
                _strikeLineRect.anchoredPosition = new Vector2(strikeX, 0f);
            }

            // Dodge zone
            if (_dodgeZoneRect != null)
            {
                float dodgeWidth = (dodgeHalfWidth * 2f) * trackWidth;
                float strikeX = (hitPos - 0.5f) * trackWidth;
                _dodgeZoneRect.sizeDelta = new Vector2(dodgeWidth, _barTrackRect.rect.height);
                _dodgeZoneRect.anchoredPosition = new Vector2(strikeX, 0f);

                if (_dodgeZoneImage != null)
                {
                    _dodgeZoneImage.color = (type == AttackTelegraphType.GroundSweep)
                        ? new Color(0.2f, 0.2f, 0.2f, 0.3f)
                        : new Color(0.9f, 0.8f, 0.2f, 0.45f);
                }
            }

            // Parry / Jump zone
            if (_parryZoneRect != null)
            {
                float parryWidth = (parryHalfWidth * 2f) * trackWidth;
                float strikeX = (hitPos - 0.5f) * trackWidth;
                _parryZoneRect.sizeDelta = new Vector2(parryWidth, _barTrackRect.rect.height);
                _parryZoneRect.anchoredPosition = new Vector2(strikeX, 0f);

                if (_parryZoneImage != null)
                {
                    if (type == AttackTelegraphType.GroundSweep)
                    {
                        _parryZoneImage.color = new Color(0.2f, 0.7f, 1f, 0.7f);
                    }
                    else if (type == AttackTelegraphType.HeavyUnblockable)
                    {
                        _parryZoneImage.color = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                    }
                    else
                    {
                        _parryZoneImage.color = new Color(0f, 1f, 0.5f, 0.75f);
                    }
                }
            }
        }

        private IEnumerator FadeResultRoutine()
        {
            yield return new WaitForSeconds(0.8f);
            Hide();
        }
    }
}
