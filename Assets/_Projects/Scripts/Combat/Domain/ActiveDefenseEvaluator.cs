using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public static class ActiveDefenseEvaluator
    {
        public static DefenseOutcome Evaluate(
            DefenseActionType action,
            float inputTime,
            float strikeTime,
            HitWindowDefinition window,
            float parryWindowMultiplier = 1.0f)
        {
            if (action == DefenseActionType.None)
                return DefenseOutcome.Miss;

            float timeDiff = Mathf.Abs(inputTime - strikeTime);

            switch (action)
            {
                case DefenseActionType.Parry:
                    if (window.TelegraphType == AttackTelegraphType.HeavyUnblockable)
                    {
                        return DefenseOutcome.InvalidAction;
                    }
                    if (window.TelegraphType == AttackTelegraphType.GroundSweep)
                    {
                        return DefenseOutcome.InvalidAction;
                    }
                    if (timeDiff <= window.ParryHalfWindow * parryWindowMultiplier)
                    {
                        return DefenseOutcome.ParrySuccess;
                    }
                    return DefenseOutcome.Miss;

                case DefenseActionType.Dodge:
                    if (window.TelegraphType == AttackTelegraphType.GroundSweep)
                    {
                        // Dodging a low sweeping strike fails; player must jump
                        return DefenseOutcome.InvalidAction;
                    }
                    if (timeDiff <= window.DodgeHalfWindow)
                    {
                        return DefenseOutcome.DodgeSuccess;
                    }
                    return DefenseOutcome.Miss;

                case DefenseActionType.Jump:
                    if (window.TelegraphType == AttackTelegraphType.GroundSweep)
                    {
                        if (timeDiff <= window.JumpHalfWindow)
                        {
                            return DefenseOutcome.JumpSuccess;
                        }
                    }
                    return DefenseOutcome.InvalidAction;

                default:
                    return DefenseOutcome.Miss;
            }
        }
    }
}
