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
        [SerializeField] private Slider _playerApSlider;
        [SerializeField] private TMP_Text _playerApText;

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
        [SerializeField] private Button _skillsButton;
        [SerializeField] private Button _passButton;

        [Header("Skill Submenu")]
        [SerializeField] private GameObject _skillMenuRoot;
        [SerializeField] private Transform _skillButtonsContainer;
        [SerializeField] private Button _skillBackBtn;

        [Header("Banner & Feedback")]
        [SerializeField] private TMP_Text _turnBannerText;
        [SerializeField] private TMP_Text _battleLogText;

        public event Action OnAttackSelected;
        public event Action OnSkillsMenuRequested;
        public event Action<SkillDefinitionSO> OnSkillSelected;
        public event Action OnPassSelected;

        private ActionPointPool _trackedApPool;
        private List<SkillDefinitionSO> _availableSkills;

        private void Awake()
        {
            if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(() => OnAttackSelected?.Invoke());
            }

            if (_skillsButton != null)
            {
                _skillsButton.onClick.AddListener(() =>
                {
                    OnSkillsMenuRequested?.Invoke();
                    ShowSkillMenu(true);
                });
            }

            if (_passButton != null)
            {
                _passButton.onClick.AddListener(() => OnPassSelected?.Invoke());
            }

            if (_skillBackBtn != null)
            {
                _skillBackBtn.onClick.AddListener(() => ShowSkillMenu(false));
            }

            SetCommandMenuVisible(false);
            ShowSkillMenu(false);
            SetTurnBanner(string.Empty);
        }

        public void SetupPlayerStatus(CombatActorStats stats, ActionPointPool apPool, List<SkillDefinitionSO> skills)
        {
            if (stats != null)
            {
                if (_playerNameText != null)
                    _playerNameText.text = stats.Name;

                UpdatePlayerHealth(stats.CurrentHealth, stats.MaxHealth);
                stats.OnHealthChanged += UpdatePlayerHealth;
            }

            if (apPool != null)
            {
                _trackedApPool = apPool;
                UpdatePlayerAP(apPool.CurrentAP, apPool.MaxAP);
                apPool.OnAPChanged += UpdatePlayerAP;
            }

            _availableSkills = skills;
            PopulateSkillMenu();
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

        public void UpdatePlayerAP(int current, int max)
        {
            if (_playerApSlider != null)
            {
                _playerApSlider.maxValue = max;
                _playerApSlider.value = current;
            }

            if (_playerApText != null)
            {
                _playerApText.text = $"AP: {current} / {max}";
            }

            // Refresh skill button interactivity
            PopulateSkillMenu();
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

            if (!visible)
            {
                ShowSkillMenu(false);
            }

            if (_attackButton != null)
                _attackButton.interactable = visible;

            if (_skillsButton != null)
                _skillsButton.interactable = visible;

            if (_passButton != null)
                _passButton.interactable = visible;
        }

        public void ShowSkillMenu(bool show)
        {
            if (_skillMenuRoot != null)
            {
                _skillMenuRoot.SetActive(show);
            }
        }

        private void PopulateSkillMenu()
        {
            if (_skillButtonsContainer == null || _availableSkills == null)
                return;

            // Clear existing buttons
            for (int i = _skillButtonsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_skillButtonsContainer.GetChild(i).gameObject);
            }

            foreach (SkillDefinitionSO skill in _availableSkills)
            {
                GameObject btnObj = new GameObject($"Btn_{skill.SkillName}");
                btnObj.transform.SetParent(_skillButtonsContainer, false);

                var rect = btnObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(260f, 48f);

                var img = btnObj.AddComponent<Image>();
                img.color = new Color(0.2f, 0.45f, 0.8f, 0.9f);

                var btn = btnObj.AddComponent<Button>();
                bool canAfford = _trackedApPool != null && _trackedApPool.CanSpend(skill.APCost);
                btn.interactable = canAfford;

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;

                var txt = textObj.AddComponent<TextMeshProUGUI>();
                txt.fontSize = 18;
                txt.fontStyle = FontStyles.Bold;
                txt.alignment = TextAlignmentOptions.Center;
                txt.text = $"{skill.SkillName} [{skill.APCost} AP]";

                if (!canAfford)
                {
                    txt.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                }

                SkillDefinitionSO capturedSkill = skill;
                btn.onClick.AddListener(() =>
                {
                    ShowSkillMenu(false);
                    OnSkillSelected?.Invoke(capturedSkill);
                });
            }
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
