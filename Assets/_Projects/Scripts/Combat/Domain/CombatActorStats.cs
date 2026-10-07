using System;
using UnityEngine;

namespace Expedition33.Combat
{
    [Serializable]
    public class CombatActorStats
    {
        [SerializeField] private string _name;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;
        [SerializeField] private int _attackPower = 20;
        [SerializeField] private int _defense = 5;
        [SerializeField] private int _agility = 10;
        [SerializeField] private bool _isPlayer;

        public string Name => _name;
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        public int Agility => _agility;
        public bool IsPlayer => _isPlayer;
        public bool IsDefeated => _currentHealth <= 0;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDefeated;

        public CombatActorStats(string name, int maxHealth, int attackPower, int defense, int agility, bool isPlayer)
        {
            _name = name;
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = _maxHealth;
            _attackPower = Mathf.Max(1, attackPower);
            _defense = Mathf.Max(0, defense);
            _agility = Mathf.Max(1, agility);
            _isPlayer = isPlayer;
        }

        public void ApplyDamage(int amount)
        {
            if (IsDefeated)
                return;

            int clampedDamage = Mathf.Max(1, amount);
            _currentHealth = Mathf.Max(0, _currentHealth - clampedDamage);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (IsDefeated)
            {
                OnDefeated?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDefeated)
                return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + Mathf.Max(0, amount));
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}
