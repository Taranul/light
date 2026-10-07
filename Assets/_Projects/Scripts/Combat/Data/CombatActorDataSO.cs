using UnityEngine;

namespace Expedition33.Combat
{
    [CreateAssetMenu(fileName = "NewCombatActorData", menuName = "Expedition33/Combat/Actor Data")]
    public class CombatActorDataSO : ScriptableObject
    {
        [SerializeField] private string _actorName = "Actor";
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _attackPower = 20;
        [SerializeField] private int _defense = 5;
        [SerializeField] private int _agility = 10;
        [SerializeField] private bool _isPlayer;
        [SerializeField] private Color _tintColor = Color.white;

        public string ActorName => _actorName;
        public int MaxHealth => _maxHealth;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        public int Agility => _agility;
        public bool IsPlayer => _isPlayer;
        public Color TintColor => _tintColor;

        public CombatActorStats CreateStats()
        {
            return new CombatActorStats(_actorName, _maxHealth, _attackPower, _defense, _agility, _isPlayer);
        }
    }
}
