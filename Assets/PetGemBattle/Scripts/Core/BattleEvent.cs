using System;

namespace PetGemBattle
{
    // Value snapshots only: presentation and logs never retain mutable PetState references.
    [Serializable]
    public sealed class BattleEvent
    {
        public GemKind gem;
        public int count,combo;
        public int[] before,after;
        public double criticalRoll=-1;
        public bool special,critical;
        public static int[] Capture(PetState actor,PetState target) => new[]{actor.Hp,actor.Shield,actor.Mana,actor.Rage,target.Hp,target.Shield,target.Mana,target.Rage};
        public static BattleEvent Change(GemKind kind,int count,int combo,int[] before,PetState actor,PetState target) =>
            new BattleEvent {gem=kind,count=count,combo=combo,before=before,after=Capture(actor,target)};
        public static BattleEvent Attack(GemKind kind,int count,int combo,int[] before,PetState actor,PetState target,double roll,bool special,bool critical)
        {
            var result=Change(kind,count,combo,before,actor,target);
            result.criticalRoll=roll;result.special=special;result.critical=critical;return result;
        }
    }
}
