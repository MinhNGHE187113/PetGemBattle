using System.Collections.Generic;
using System.Linq;

namespace PetGemBattle
{
    public enum PresentationPhase { Selecting,Collecting,Prepared,BoardHiding,Combat,Result,BoardShowing }
    public sealed class CollectedGroup
    {
        public MatchGroup Group;public int Wave;
        public CollectedGroup(MatchGroup group,int wave){Group=new MatchGroup{Kind=group.Kind};Group.Cells.UnionWith(group.Cells);Wave=wave;}
    }
    public sealed class CombatStep
    {
        public PetState Actor,Target;public BattleReport Report;public int Wave;
        private bool applied;
        public bool Apply(PetState actor,PetState target){if(applied)return false;actor.ApplySnapshot(Actor);target.ApplySnapshot(Target);applied=true;return true;}
    }
    public static class CombatTurnPlan
    {
        public static List<CombatStep> Prepare(PetState actor,PetState target,List<CollectedGroup> groups,BattleRules rules,IBattleRandom random,int completedRounds)
        {
            var a=actor.Copy();var b=target.Copy();var result=new List<CombatStep>();
            foreach(var item in groups.Where(g=>g.Group.Kind!=GemKind.Sword && g.Group.Kind!=GemKind.Drain))
            {
                var report=BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{item.Group},item.Wave,rules,random,completedRounds);Add(result,a,b,report,item.Wave);
            }
            // Prepay all sword rage before any attacks, then spend 60 separately for each special.
            int gained=0;foreach(var item in groups.Where(g=>g.Group.Kind==GemKind.Sword))gained+=a.AddRage(rules.swordBaseRage+rules.swordExtraRage*(item.Group.Cells.Count-3)+rules.GroupRageBonus(item.Group.Cells.Count),rules);
            if(gained>0)Add(result,a,b,new BattleReport{Rage=gained},1);
            foreach(var item in groups.Where(g=>g.Group.Kind==GemKind.Drain).Concat(groups.Where(g=>g.Group.Kind==GemKind.Sword)))
            {
                if(a.Hp<=0 || b.Hp<=0)break;
                var report=BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{item.Group},item.Wave,rules,random,completedRounds,true);Add(result,a,b,report,item.Wave);
                int shieldBefore=b.Shield;
                if(b.TryBossAwaken(rules))Add(result,a,b,new BattleReport{BossTransformation=true,Shield=b.Shield-shieldBefore},item.Wave);
            }
            return result;
        }
        private static void Add(List<CombatStep> steps,PetState a,PetState b,BattleReport report,int wave)=>steps.Add(new CombatStep{Actor=a.Copy(),Target=b.Copy(),Report=report,Wave=wave});
    }
}
