namespace Expedition33.Combat
{
    public enum DefenseActionType
    {
        None,
        Parry,
        Dodge,
        Jump
    }

    public enum DefenseOutcome
    {
        Miss,
        ParrySuccess,
        DodgeSuccess,
        JumpSuccess,
        InvalidAction
    }

    public enum AttackTelegraphType
    {
        Standard,
        GroundSweep,
        HeavyUnblockable
    }
}
