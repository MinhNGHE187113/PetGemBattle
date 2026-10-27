using System;
using System.Collections.Generic;

namespace PetGemBattle
{
    public static class BattleAI
    {
        private sealed class NoProc : IBattleRandom { public double Next()=>1; }
        public static CardKind? ChooseCard(PetState actor,PetState target,BattleRules rules)
        {
            if(CardSystem.CanUse(actor,CardKind.Evolution,rules))return CardKind.Evolution;
            if(CardSystem.CanUse(actor,CardKind.Signature,rules) && target.Hp>0)return CardKind.Signature;
            if((actor.EvolutionLevel==1 || (rules.specialManaCost>0 && actor.Mana<rules.specialManaCost)) && CardSystem.CanUse(actor,CardKind.Mana,rules))return CardKind.Mana;
            if(CardSystem.CanUse(actor,CardKind.Rage,rules) && actor.Rage<rules.specialRageCost)return CardKind.Rage;
            return null;
        }
        public static BoardMove ChooseMove(MatchBoard board,PetState actor,PetState target,BattleRules rules,int completedRounds=0,IBattleRandom tieRandom=null)
        {
            tieRandom=tieRandom??new SeededBattleRandom(0);int ties=0;
            var moves=board.LegalMoves();if(moves.Count==0)throw new InvalidOperationException("AI requires a playable board.");
            var best=moves[0];float bestScore=float.MinValue;
            foreach(var move in moves)
            {
                board.Swap(move);
                try
                {
                    var groups=board.FindMatches();var a=actor.Copy();var b=target.Copy();
                    var report=BattleCombat.ApplyMatches(a,b,groups,1,rules,new NoProc(),completedRounds);
                    bool danger=actor.Hp+actor.Shield<BattleCombat.SwordDamage(target,actor,3,1,rules,true,false,completedRounds)*2;
                    float score=(target.Hp-b.Hp)*3.2f+(target.Shield-b.Shield)*1.8f+
                        (a.Hp-actor.Hp)*(actor.Hp<actor.MaxHp*.4f ? 1.5f : .8f)+
                        (a.Shield-actor.Shield)*(danger ? 1.3f : .7f)+
                        (a.Mana-actor.Mana)*(actor.EvolutionLevel==1 ? 1.15f : rules.specialManaCost>0 ? .85f : .12f)+report.Rage*.65f;
                    if(b.Hp==0)score+=100000; // Only guaranteed damage qualifies as a lethal move.
                    var removed=new HashSet<Cell>();
                    foreach(var group in groups)
                    {
                        removed.UnionWith(group.Cells);
                        if(group.Kind==GemKind.Sword)
                        {
                            // Expected critical upside is separate from guaranteed lethal scoring.
                            bool special=report.Events.Exists(e=>e.gem==GemKind.Sword && e.count==group.Cells.Count && e.special);
                            int normal=BattleCombat.SwordDamage(actor,target,group.Cells.Count,1,rules,special,false,completedRounds);
                            int crit=BattleCombat.SwordDamage(actor,target,group.Cells.Count,1,rules,special,true,completedRounds);
                            score+=(crit-normal)*rules.CriticalChance(actor,group.Cells.Count)*2;
                        }
                        if(group.Kind!=GemKind.Drain)continue;
                        int n=group.Cells.Count;
                        float raw=Math.Min(target.Hp*rules.hpDrainPercent,target.MaxHp*rules.hpDrainMaxPercent);
                        float hp=Math.Max(0,raw-target.Shield)*rules.DrainChance(rules.hpDrainChance,rules.hpDrainChanceCap,n);
                        float rage=Math.Min(rules.maxRage-actor.Rage,target.Rage*rules.rageDrainPercent)*rules.DrainChance(rules.rageDrainChance,rules.resourceDrainChanceCap,n);
                        float armor=Math.Min(rules.ShieldCap(actor)-actor.Shield,target.Shield*rules.shieldDrainPercent)*rules.DrainChance(rules.shieldDrainChance,rules.resourceDrainChanceCap,n);
                        score+=hp*(actor.Hp<actor.MaxHp*.4f ? 4f : 2.4f)+rage*1.4f+armor*1.7f;
                    }
                    score+=VisibleCascadePotential(board,removed)*5f+(groups.Count-1)*4f;
                    if(score>bestScore+.0001f){bestScore=score;best=move;ties=1;}
                    else if(Math.Abs(score-bestScore)<=.0001f && tieRandom.Next()<1.0/++ties)best=move;
                }
                finally { board.Swap(move); }
            }
            return best;
        }
        public static SkillDefinition ChooseSkill(PetState actor,PetState target)
        {
            foreach(var skill in actor.Definition.skills)
                if(actor.CanUse(skill) && (skill.effect!=SkillEffect.Heal || actor.Hp<actor.MaxHp/2))return skill;
            return null;
        }
        private static int VisibleCascadePotential(MatchBoard board,HashSet<Cell> removed)
        {
            var compact=new int[MatchBoard.Size,MatchBoard.Size];
            for(int x=0;x<MatchBoard.Size;x++)
            {
                int target=0;
                for(int y=0;y<MatchBoard.Size;y++)if(!removed.Contains(new Cell(x,y)))compact[x,target++]=board[x,y];
                while(target<MatchBoard.Size)compact[x,target++]=-1;
            }
            int matches=0;
            for(int y=0;y<MatchBoard.Size;y++)for(int x=0;x<MatchBoard.Size;x++)
                if(compact[x,y]>=0 && ((x>=2 && compact[x,y]==compact[x-1,y] && compact[x,y]==compact[x-2,y]) ||
                    (y>=2 && compact[x,y]==compact[x,y-1] && compact[x,y]==compact[x,y-2])))matches++;
            return matches;
        }
    }
}
