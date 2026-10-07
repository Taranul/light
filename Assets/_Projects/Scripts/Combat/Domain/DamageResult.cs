namespace Expedition33.Combat
{
    public readonly struct DamageResult
    {
        public int RawDamage { get; }
        public int MitigatedDamage { get; }
        public bool IsCritical { get; }
        public bool IsDefeated { get; }

        public DamageResult(int rawDamage, int mitigatedDamage, bool isCritical, bool isDefeated)
        {
            RawDamage = rawDamage;
            MitigatedDamage = mitigatedDamage;
            IsCritical = isCritical;
            IsDefeated = isDefeated;
        }
    }
}
