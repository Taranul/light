using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    /// <summary>
    /// In-game cheat / debug panel. Toggle with [F12].
    /// Provides: instant AP fill, force Overcharge, force Break, fill Gradient,
    /// toggle God Mode (player ignores all damage), and adjust timing windows.
    /// BattleManager must register itself via Register().
    /// </summary>
    public class DebugCheatPanel : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private GameObject _container;

        [Header("Status Labels")]
        [SerializeField] private TMP_Text _statusLabel;

        [Header("Buttons")]
        [SerializeField] private Button _fillAPButton;
        [SerializeField] private Button _fillOverchargeButton;
        [SerializeField] private Button _forceBreakButton;
        [SerializeField] private Button _fillGradientButton;
        [SerializeField] private Button _godModeButton;
        [SerializeField] private Button _widenParryButton;
        [SerializeField] private Button _normalParryButton;
        [SerializeField] private Button _closeButton;

        // Internal references injected by BattleManager
        private ActionPointPool _apPool;
        private OverchargeMeter _overchargeMeter;
        private BreakMeter _breakMeter;
        private GradientMeter _gradientMeter;
        private CombatActorStats _playerStats;

        private bool _godModeActive;
        private bool _widenedParry;
        private bool _isVisible;

        public bool GodModeActive => _godModeActive;

        private void Awake()
        {
            if (_container != null) _container.SetActive(false);

            if (_fillAPButton != null)
                _fillAPButton.onClick.AddListener(CheatFillAP);

            if (_fillOverchargeButton != null)
                _fillOverchargeButton.onClick.AddListener(CheatFillOvercharge);

            if (_forceBreakButton != null)
                _forceBreakButton.onClick.AddListener(CheatForceBreak);

            if (_fillGradientButton != null)
                _fillGradientButton.onClick.AddListener(CheatFillGradient);

            if (_godModeButton != null)
                _godModeButton.onClick.AddListener(CheatToggleGodMode);

            if (_widenParryButton != null)
                _widenParryButton.onClick.AddListener(CheatWidenParry);

            if (_normalParryButton != null)
                _normalParryButton.onClick.AddListener(CheatNormalParry);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(() => SetVisible(false));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                SetVisible(!_isVisible);
        }

        /// <summary>Register required combat systems from BattleManager.</summary>
        public void Register(
            ActionPointPool apPool,
            OverchargeMeter overchargeMeter,
            BreakMeter breakMeter,
            GradientMeter gradientMeter,
            CombatActorStats playerStats)
        {
            _apPool = apPool;
            _overchargeMeter = overchargeMeter;
            _breakMeter = breakMeter;
            _gradientMeter = gradientMeter;
            _playerStats = playerStats;
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (_container != null)
                _container.SetActive(visible);
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (_statusLabel == null) return;
            string godTag = _godModeActive ? "<color=#44FF44>ON</color>" : "<color=#FF4444>OFF</color>";
            string parryTag = _widenedParry ? "<color=#44FF44>WIDE</color>" : "<color=#CCCCCC>NORMAL</color>";
            _statusLabel.text =
                $"GOD MODE: {godTag}\n" +
                $"PARRY WINDOW: {parryTag}\n" +
                $"<size=13><color=#888888>[F12] toggle panel</color></size>";
        }

        private void CheatFillAP()
        {
            if (_apPool == null) return;
            _apPool.Generate(_apPool.MaxAP);
            Log("Cheat: AP filled to max.");
        }

        private void CheatFillOvercharge()
        {
            if (_overchargeMeter == null) return;
            _overchargeMeter.AddCharge(_overchargeMeter.MaxCharges);
            Log("Cheat: Overcharge filled.");
        }

        private void CheatForceBreak()
        {
            if (_breakMeter == null) return;
            _breakMeter.AddBreak(_breakMeter.MaxBreak);
            Log("Cheat: Enemy posture broken.");
        }

        private void CheatFillGradient()
        {
            if (_gradientMeter == null) return;
            _gradientMeter.AddCharge(_gradientMeter.MaxCharge);
            Log("Cheat: Gradient bar filled.");
        }

        private void CheatToggleGodMode()
        {
            _godModeActive = !_godModeActive;
            Log($"Cheat: God Mode {(_godModeActive ? "ENABLED" : "DISABLED")}.");
            RefreshStatus();
        }

        private void CheatWidenParry()
        {
            _widenedParry = true;
            Log("Cheat: Parry window widened (2×).");
            RefreshStatus();
        }

        private void CheatNormalParry()
        {
            _widenedParry = false;
            Log("Cheat: Parry window reset to normal.");
            RefreshStatus();
        }

        /// <summary>Returns the debug parry window multiplier (1.0 or 2.0).</summary>
        public float GetDebugParryWindowMultiplier() => _widenedParry ? 2.0f : 1.0f;

        private void Log(string msg)
        {
            Debug.Log($"[DebugCheatPanel] {msg}");
        }
    }
}
