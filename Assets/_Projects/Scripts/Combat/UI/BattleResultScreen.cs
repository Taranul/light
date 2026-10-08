using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    /// <summary>
    /// Shows the Victory or Defeat result screen at the end of a battle.
    /// Supports a Retry button that reloads the scene and a Quit button.
    /// Appears on top of all other UI via a semi-opaque overlay.
    /// </summary>
    public class BattleResultScreen : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private GameObject _container;

        [Header("Labels")]
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _subtitleText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitButton;

        [Header("Background")]
        [SerializeField] private Image _overlayImage;

        public event Action OnRetryRequested;

        private bool _isActive;

        private void Awake()
        {
            if (_container != null)
                _container.SetActive(false);

            if (_retryButton != null)
                _retryButton.onClick.AddListener(HandleRetry);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(HandleQuit);
        }

        /// <summary>Show the result screen as Victory.</summary>
        public void ShowVictory()
        {
            Show(
                "<color=#55FF88><b>VICTORY!</b></color>",
                "The expedition survives another step forward.",
                new Color(0.04f, 0.22f, 0.10f, 0.88f)
            );
        }

        /// <summary>Show the result screen as Defeat.</summary>
        public void ShowDefeat()
        {
            Show(
                "<color=#FF5555><b>DEFEAT</b></color>",
                "The party has fallen. Try again?",
                new Color(0.20f, 0.04f, 0.04f, 0.88f)
            );
        }

        private void Show(string headline, string subtitle, Color overlayColor)
        {
            if (_isActive) return;
            _isActive = true;

            if (_overlayImage != null)
            {
                _overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            }

            if (_container != null)
                _container.SetActive(true);

            if (_resultText != null)
                _resultText.text = headline;

            if (_subtitleText != null)
                _subtitleText.text = subtitle;

            // Fade overlay in
            if (_overlayImage != null)
            {
                DOTween.To(() => _overlayImage.color, c => _overlayImage.color = c, overlayColor, 0.6f).SetEase(Ease.InOutQuad);
            }

            // Punch-scale headline
            if (_resultText != null)
            {
                _resultText.transform.localScale = Vector3.zero;
                _resultText.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.3f).SetEase(Ease.OutBack);
            }
        }

        private void HandleRetry()
        {
            OnRetryRequested?.Invoke();
        }

        private void HandleQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
