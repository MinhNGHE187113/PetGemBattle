using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace PetGemBattle.Tests
{
    public class P2BalanceTests
    {
        [Test] public void P2_30_BeforeAfterSevenHundredBattlesResolve()
        {
            var catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();P2Content.Upgrade(catalog);
            var rules=ScriptableObject.CreateInstance<BattleRules>();rules.elements=catalog;
            var lines=new List<string>{"# P2 balance before / after","",
                "50 seeds per stage per ruleset, same Metal Bird at recommended levels 1/1/2/3/4/5/6. Both use live AI, cards, seeded board and CombatTurnPlan. Before uses P1 encounters and formulas; after uses P2 encounters/forms/Mana special cost. Estimated times assume 12 s player thinking, not measured human play.",
                "","| Rules | Stage | Wins/50 | Median rounds | Median player sword turns | Median estimated min | Round limit |","|---|---:|---:|---:|---:|---:|---:|"};
            try
            {
                for(int version=1;version<=2;version++)
                {
                    catalog.p2Enabled=version==2;rules.attackMultiplier=version==2 ? 2.3f : 1.8f;rules.swordExtraMultiplier=version==2 ? .4f : .35f;rules.specialManaCost=version==2 ? 30 : 0;rules.finalMultiplierRounding=version==2;
                    for(int stage=1;stage<=7;stage++)
                    {
                        int wins=0,limits=0;var rounds=new List<int>();var swordTurns=new List<int>();var seconds=new List<double>();
                        for(int seed=0;seed<50;seed++)
                        {
                            int level=new[]{1,1,2,3,4,5,6}[stage-1];var a=new PetState(catalog.Stats("metal-bird",level));var b=new PetState(catalog.Stats("wood-"+stage,1,true,stage));
                            var board=new MatchBoard(seed);var rng=new SeededBattleRandom(seed^0x51A7D);var tie=new SeededBattleRandom(seed^0x3A91);bool player=a.Definition.speed>=b.Definition.speed;int turn=0,swords=0,playerTurns=0;double animation=0;
                            BattleResult result=BattleResult.Ongoing;
                            while(result==BattleResult.Ongoing && turn<60)
                            {
                                var actor=player ? a : b;var target=player ? b : a;actor.BeginTurn();if(player)playerTurns++;
                                var card=BattleAI.ChooseCard(actor,target,rules);if(card.HasValue){var report=CardSystem.Use(actor,card.Value,rules);animation+=rules.EffectDuration(report);}
                                if(board.LegalMoves().Count==0)board.ShufflePlayable();Assert.That(board.TrySwap(BattleAI.ChooseMove(board,actor,target,rules,turn/2,tie)),Is.True);
                                var collected=new List<CollectedGroup>();animation+=rules.swapSeconds+rules.boardTransitionSeconds*2;
                                for(int wave=1;wave<=rules.maxCascadeWaves;wave++)
                                {
                                    var groups=board.FindMatches();if(groups.Count==0)break;
                                    foreach(var g in groups)collected.Add(new CollectedGroup(g,wave));board.ClearAndRefill(groups);animation+=rules.clearSeconds+rules.fallSeconds;
                                    if(wave==rules.maxCascadeWaves)board.GeneratePlayable();
                                }
                                if(player && collected.Any(g=>g.Group.Kind==GemKind.Sword))swords++;
                                var plan=CombatTurnPlan.Prepare(actor,target,collected,rules,rng,turn/2);
                                foreach(var step in plan){step.Apply(actor,target);animation+=step.Report.BossTransformation ? target.Definition.bossProfile.awakenSeconds : rules.EffectDuration(step.Report);}
                                result=BattleCombat.Result(a,b);turn++;player=!player;
                            }
                            if(result==BattleResult.Ongoing){limits++;result=BattleCombat.ResultAtRoundLimit(a,b);}
                            Assert.That(result,Is.Not.EqualTo(BattleResult.Ongoing));Assert.That(turn,Is.LessThanOrEqualTo(60));if(result==BattleResult.Victory)wins++;
                            rounds.Add((turn+1)/2);swordTurns.Add(swords);seconds.Add(animation+playerTurns*12);
                        }
                        rounds.Sort();swordTurns.Sort();seconds.Sort();
                        lines.Add("| P"+version+" | "+stage+" | "+wins+" | "+rounds[25]+" | "+swordTurns[25]+" | "+(seconds[25]/60).ToString("0.00")+" | "+limits+" |");
                    }
                }
                Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/P2BalanceSimulation.md",lines);TestContext.WriteLine(string.Join("\n",lines));
            }
            finally{UnityEngine.Object.DestroyImmediate(catalog);UnityEngine.Object.DestroyImmediate(rules);}
        }
    }
}
