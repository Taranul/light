using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    public class CombatHUD : MonoBehaviour
    {
        [Header("Player Status")]
        [SerializeField] private Slider _playerHpSlider;
        [SerializeField] private TMP_Text _playerHpText;
        [SerializeField] private TMP_Text _playerNameText;

        [Header("Enemy Status")]
        [SerializeField] private Slider _enemyHpSlider;
        [SerializeField] private TMP_Text _enemyHpText;
        [SerializeField] private TMP_Text _enemyNameText;

        [Header("Turn Timeline")]
        [SerializeField] private Transform _timelineContainer;
        [SerializeField] private GameObject _timelineBadgePrefab;
        [SerializeField] private TMP_Text _timelineFallbackText;

        [Header("Action Command Menu")]
        [SerializeField] private GameObject _commandMenuRoot;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _passButton;

        [Header("Banner & Feedback")]
        [SerializeField] private TMP_Text _turnBannerText;
        [SerializeField] private TMP_Text _battleLogText;

        public event Action OnAttackSelected;
        public event Action OnPassSelected;

        private void Awake()
        {
            if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(() => OnAttackSelected?.Invoke());
            }

            if (_passButton != null)
            {
                _passButton.onClick.AddListener(() => OnPassSelected?.Invoke());
            }

            SetCommandMenuVisible(false);
            SetTurnBanner(string.Empty);
        }

        public void SetupPlayerStatus(CombatActorStats stats)
        {
            if (stats == null)
                return;

            if (_playerNameText != null)
                _playerNameText.text = stats.Name;

            UpdatePlayerHealth(stats.CurrentHealth, stats.MaxHealth);
            stats.OnHealthChanged += UpdatePlayerHealth;
        }

        public void SetupEnemyStatus(CombatActorStats stats)
        {
            if (stats == null)
                return;

            if (_enemyNameText != null)
                _enemyNameText.text = stats.Name;

            UpdateEnemyHealth(stats.CurrentHealth, stats.MaxHealth);
            stats.OnHealthChanged += UpdateEnemyHealth;
        }

        public void UpdatePlayerHealth(int current, int max)
        {
            if (_playerHpSlider != null)
            {
                _playerHpSlider.maxValue = max;
                _playerHpSlider.value = current;
            }

            if (_playerHpText != null)
            {
                _playerHpText.text = $"HP: {current} / {max}";
            }
        }

        public void UpdateEnemyHealth(int current, int max)
        {
            if (_enemyHpSlider != null)
            {
                _enemyHpSlider.maxValue = max;
                _enemyHpSlider.value = current;
            }

            if (_enemyHpText != null)
            {
                _enemyHpText.text = $"HP: {current} / {max}";
            }
        }

        public void UpdateTimelinePreview(List<CombatActorStats> upcoming)
        {
            if (upcoming == null)
                return;

            if (_timelineFallbackText != null)
            {
                var names = new List<string>();
                for (int i = 0; i < Mathf.Min(6, upcoming.Count); i++)
                {
                    string colorTag = upcoming[i].IsPlayer ? "<color=#55AAFF>" : "<color=#FF5555>";
                    names.Add($"{colorTag}{upcoming[i].Name}</color>");
                }
                _timelineFallbackText.text = string.Join("  >  ", names);
            }
        }

        public void SetCommandMenuVisible(bool visible)
        {
            if (_commandMenuRoot != null)
            {
                _commandMenuRoot.SetActive(visible);
            }

            if (_attackButton != null)
                _attackButton.interactable = visible;

            if (_passButton != null)
                _passButton.interactable = visible;
        }

        public void SetTurnBanner(string message)
        {
            if (_turnBannerText != null)
            {
                _turnBannerText.text = message;
            }
        }

        public void AddLog(string message)
        {
            if (_battleLogText != null)
            {
                _battleLogText.text = message;
            }
        }
    }
}
