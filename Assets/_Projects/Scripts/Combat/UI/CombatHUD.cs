using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    public class CombatHUD : MonoBehaviour
    {
        [Header("Player Status")]
        [SerializeField] private Image _playerHpFill;
        [SerializeField] private TMP_Text _playerHpText;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private Transform _playerApPipsContainer;
        [SerializeField] private TMP_Text _playerApText;

        [Header("Enemy Status")]
        [SerializeField] private Image _enemyHpFill;
        [SerializeField] private TMP_Text _enemyHpText;
        [SerializeField] private TMP_Text _enemyNameText;

        [Header("Turn Timeline")]
        [SerializeField] private TMP_Text _timelineText;

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
        private readonly List<Image> _apPipImages = new();

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

            CacheApPips();
            SetCommandMenuVisible(false);
            ShowSkillMenu(false);
            SetTurnBanner(string.Empty);
        }

        private void CacheApPips()
        {
            _apPipImages.Clear();
            if (_playerApPipsContainer != null)
            {
                foreach (Transform child in _playerApPipsContainer)
                {
                    var img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        _apPipImages.Add(img);
                    }
                }
            }
        }

        public void SetupPlayerStatus(CombatActorStats stats, ActionPointPool apPool, List<SkillDefinitionSO> skills)
        {
            if (stats != null)
            {
                if (_playerNameText != null)
                    _playerNameText.text = stats.Name.ToUpper();

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
                _enemyNameText.text = stats.Name.ToUpper();

            UpdateEnemyHealth(stats.CurrentHealth, stats.MaxHealth);
            stats.OnHealthChanged += UpdateEnemyHealth;
        }

        public void UpdatePlayerHealth(int current, int max)
        {
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            if (_playerHpFill != null)
            {
                _playerHpFill.DOKill();
                DOTween.To(() => _playerHpFill.fillAmount, x => _playerHpFill.fillAmount = x, ratio, 0.2f).SetEase(Ease.OutQuad);
            }

            if (_playerHpText != null)
            {
                _playerHpText.text = $"HP  <b>{current}</b> / {max}";
            }
        }

        public void UpdateEnemyHealth(int current, int max)
        {
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            if (_enemyHpFill != null)
            {
                _enemyHpFill.DOKill();
                DOTween.To(() => _enemyHpFill.fillAmount, x => _enemyHpFill.fillAmount = x, ratio, 0.2f).SetEase(Ease.OutQuad);
            }

            if (_enemyHpText != null)
            {
                _enemyHpText.text = $"HP  <b>{current}</b> / {max}";
            }
        }

        public void UpdatePlayerAP(int current, int max)
        {
            if (_playerApText != null)
            {
                _playerApText.text = $"AP  <b>{current}</b> / {max}";
            }

            for (int i = 0; i < _apPipImages.Count; i++)
            {
                bool isFilled = i < current;
                _apPipImages[i].color = isFilled ? new Color(0.1f, 0.8f, 1f, 1f) : new Color(0.15f, 0.22f, 0.3f, 0.5f);
            }

            PopulateSkillMenu();
        }

        public void UpdateTimelinePreview(List<CombatActorStats> upcoming)
        {
            if (upcoming == null || _timelineText == null)
                return;

            var badges = new List<string>();
            for (int i = 0; i < Mathf.Min(6, upcoming.Count); i++)
            {
                string tag = upcoming[i].IsPlayer ? "<color=#4DC4FF><b>" + upcoming[i].Name + "</b></color>" : "<color=#FF5566><b>" + upcoming[i].Name + "</b></color>";
                badges.Add(tag);
            }

            _timelineText.text = string.Join("  <color=#888888>→</color>  ", badges);
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

            for (int i = _skillButtonsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_skillButtonsContainer.GetChild(i).gameObject);
            }

            foreach (SkillDefinitionSO skill in _availableSkills)
            {
                GameObject btnObj = new GameObject($"Btn_{skill.SkillName}");
                btnObj.transform.SetParent(_skillButtonsContainer, false);

                var rect = btnObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(280f, 44f);

                var img = btnObj.AddComponent<Image>();
                img.color = new Color(0.18f, 0.16f, 0.28f, 0.95f);

                var btn = btnObj.AddComponent<Button>();
                bool canAfford = _trackedApPool != null && _trackedApPool.CanSpend(skill.APCost);
                btn.interactable = canAfford;

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;

                var txt = textObj.AddComponent<TextMeshProUGUI>();
                txt.fontSize = 17;
                txt.fontStyle = FontStyles.Bold;
                txt.alignment = TextAlignmentOptions.Center;
                txt.text = $"{skill.SkillName}  <color=#00D0FF>[{skill.APCost} AP]</color>";

                if (!canAfford)
                {
                    txt.color = new Color(0.6f, 0.6f, 0.6f, 0.4f);
                    img.color = new Color(0.12f, 0.12f, 0.16f, 0.7f);
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
