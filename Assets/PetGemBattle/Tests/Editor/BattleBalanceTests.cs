using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PetGemBattle.Tests
{
    public class BattleBalanceTests
    {
        private BattleRules rules;
        private sealed class Roll : IBattleRandom
        {
            public int Calls;public double Value;
            public Roll(double value){Value=value;}
            public double Next(){Calls++;return Value;}
        }
        [SetUp] public void Setup(){rules=ScriptableObject.CreateInstance<BattleRules>();}
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(rules);}
        private static List<MatchGroup> Groups(GemKind kind,int n)
        {
            var g=new MatchGroup{Kind=kind};for(int i=0;i<n;i++)g.Cells.Add(new Cell(i%8,i/8));return new List<MatchGroup>{g};
        }
        [TestCase(3,12)] [TestCase(4,17)] [TestCase(5,21)]
        public void HeartActuallyHealsWoundedPet(int n,int expected)
        {
            var a=new PetState(rules.player);a.TakeDamage(150);
            var r=BattleCombat.ApplyMatches(a,new PetState(rules.enemy),Groups(GemKind.Heart,n),1,rules,new Roll(1));
            Assert.That(r.Healing,Is.EqualTo(expected));Assert.That(a.Hp,Is.EqualTo(150+expected));
        }
        [Test] public void HeartHasPerGroupCapEvenWithLargeComboAndNeverOverheals()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(150);
            var r=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Heart,8),20,rules,new Roll(1),27);
            Assert.That(r.Healing,Is.EqualTo(30));a.Heal(999);
            Assert.That(BattleCombat.ApplyMatches(a,b,Groups(GemKind.Heart,3),1,rules,new Roll(1)).Healing,Is.Zero);
            Assert.That(a.Hp,Is.EqualTo(300));
        }
        [TestCase(false,false,41)] [TestCase(false,true,70)] [TestCase(true,false,72)] [TestCase(true,true,122)]
        public void NormalSpecialCriticalRoundInSpecifiedOrder(bool special,bool critical,int damage)
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);if(special)a.AddRage(55,rules);
            var rng=new Roll(critical ? 0 : 1);
            var r=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Sword,3),1,rules,rng);
            Assert.That(r.Damage,Is.EqualTo(damage));Assert.That(rng.Calls,Is.EqualTo(1));
            Assert.That(r.CriticalAttacks,Is.EqualTo(critical ? 1 : 0));Assert.That(r.SpecialAttacks,Is.EqualTo(special ? 1 : 0));
            Assert.That(a.Rage,Is.EqualTo(special ? 5 : 10));Assert.That(r.RageSpent,Is.EqualTo(special ? 60 : 0));
        }
        [Test] public void CriticalHitsShieldBeforeHpAndUsesOneRollForLargeGroup()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);b.AddShield(60,rules);var rng=new Roll(0);
            var r=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Sword,5),1,rules,rng);
            Assert.That(rng.Calls,Is.EqualTo(1));Assert.That(r.Damage,Is.EqualTo(100));
            Assert.That(r.Absorbed,Is.EqualTo(60));Assert.That(r.HpDamage,Is.EqualTo(40));Assert.That(b.Hp,Is.EqualTo(260));
        }
        [Test] public void CriticalChanceIncludesEvolutionAndCapsAtFortyPercent()
        {
            var a=new PetState(rules.player);
            Assert.That(rules.CriticalChance(a,3),Is.EqualTo(.15f).Within(.00001));
            Assert.That(rules.CriticalChance(a,4),Is.EqualTo(.20f).Within(.00001));
            Assert.That(rules.CriticalChance(a,5),Is.EqualTo(.25f).Within(.00001));
            a.AddMana(100,rules);CardSystem.Use(a,CardKind.Evolution,rules);
            Assert.That(rules.CriticalChance(a,3),Is.EqualTo(.20f).Within(.00001));
            Assert.That(rules.CriticalChance(a,8),Is.EqualTo(.40f).Within(.00001));
        }
        [Test] public void CriticalSeedReproducesActualRollsAndFrequency()
        {
            var rng=new SeededBattleRandom(731);var copy=new SeededBattleRandom(731);int hits=0;
            for(int i=0;i<10000;i++)
            {
                var r=BattleCombat.ApplyMatches(new PetState(rules.player),new PetState(rules.enemy),Groups(GemKind.Sword,3),1,rules,rng);
                var other=BattleCombat.ApplyMatches(new PetState(rules.player),new PetState(rules.enemy),Groups(GemKind.Sword,3),1,rules,copy);
                Assert.That(r.Damage,Is.EqualTo(other.Damage));hits+=r.CriticalAttacks;
            }
            Assert.That(hits/10000f,Is.EqualTo(.15f).Within(.015));
        }
        [TestCase(14,1)] [TestCase(15,1.1f)] [TestCase(16,1.1f)] [TestCase(17,1.2f)] [TestCase(19,1.3f)] [TestCase(27,1.6f)] [TestCase(99,1.6f)]
        public void SuddenDeathUsesCompletedRoundsAndCaps(int completed,float expected)
        {Assert.That(rules.SuddenDeathMultiplier(completed),Is.EqualTo(expected).Within(.00001));}
        [Test] public void SuddenDeathBoostsSwordButNotHealingArmorOrDrain()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(150);
            Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules,false,false,27),Is.EqualTo(66));
            var heal=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Heart,3),1,rules,new Roll(1),27);
            var armor=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Shield,3),1,rules,new Roll(1),27);
            Assert.That(heal.Healing,Is.EqualTo(12));Assert.That(armor.Shield,Is.EqualTo(9));
            var drain=BattleCombat.ApplyMatches(a,b,Groups(GemKind.Drain,3),1,rules,new Roll(0),27);
            Assert.That(drain.HpStolen,Is.EqualTo(24));Assert.That(drain.Healing,Is.EqualTo(24));
        }
        [Test] public void RoundLimitComparesRatiosExactlyIgnoringShieldAndEvolutionMaxHp()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(150);
            b.AddMana(100,rules);CardSystem.Use(b,CardKind.Evolution,rules);b.TakeDamage(210);
            a.AddShield(60,rules);Assert.That(BattleCombat.ResultAtRoundLimit(a,b),Is.EqualTo(BattleResult.Draw));
            b.TakeDamage(1);Assert.That(BattleCombat.ResultAtRoundLimit(a,b),Is.EqualTo(BattleResult.Victory));
            b.Heal(2);Assert.That(BattleCombat.ResultAtRoundLimit(a,b),Is.EqualTo(BattleResult.Defeat));
        }
        [Test] public void SavedSceneRulesUseNewBalanceRatherThanOldSerializedValues()
        {
            var saved=UnityEditor.AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/PetGemBattle/Data/ClassicBattleRules.asset");
            Assert.That(saved,Is.Not.Null);var a=new PetState(saved.player);var b=new PetState(saved.enemy);
            Assert.That(BattleCombat.SwordDamage(a,b,3,1,saved),Is.EqualTo(41));
            Assert.That(saved.ShieldCap(a),Is.EqualTo(60));Assert.That(saved.healBasePercent,Is.EqualTo(.04f));
            Assert.That(saved.maxHealPercent,Is.EqualTo(.1f));Assert.That(saved.specialMultiplier,Is.EqualTo(1.75f));
            Assert.That(saved.criticalMultiplier,Is.EqualTo(1.7f));Assert.That(saved.maxRounds,Is.EqualTo(30));
        }
        [Test] public void ThousandSeededAiBattlesAlwaysConcludeWithinThirtyRounds()
        {
            const int count=1000;var rounds=new List<int>();var seconds=new List<double>();int wins=0,losses=0,draws=0,limits=0,sudden=0,criticals=0,attacks=0;
            long healed=0,damage=0,critDamage=0,specials=0,cards=0,drainProcs=0,shuffles=0;
            for(int seed=0;seed<count;seed++)
            {
                var board=new MatchBoard(seed);var rng=new SeededBattleRandom(seed^0x51A7D);
                var tieRng=new SeededBattleRandom(seed^0x3A91);
                var player=new PetState(rules.player);var enemy=new PetState(rules.enemy);int turns=0;double animation=0;
                var result=BattleResult.Ongoing;
                while(result==BattleResult.Ongoing && turns<rules.maxRounds*2)
                {
                    bool playerTurn=turns%2==0;var a=playerTurn ? player : enemy;var b=playerTurn ? enemy : player;int completed=turns/2;
                    a.BeginTurn();var card=BattleAI.ChooseCard(a,b,rules);
                    if(card.HasValue){cards++;var report=CardSystem.Use(a,card.Value,rules);animation+=report.Evolved ? rules.evolutionSeconds : rules.effectSeconds;}
                    if(board.LegalMoves().Count==0){shuffles++;board.ShufflePlayable();}
                    Assert.That(board.TrySwap(BattleAI.ChooseMove(board,a,b,rules,completed,tieRng)),Is.True);animation+=rules.swapSeconds;
                    for(int wave=1;;wave++)
                    {
                        var groups=board.FindMatches();if(groups.Count==0)break;
                        var report=BattleCombat.ApplyMatches(a,b,groups,wave,rules,rng,completed);
                        healed+=report.Healing;damage+=report.Damage;specials+=report.SpecialAttacks;
                        if(report.CriticalAttacks>0)critDamage+=report.Damage;
                        drainProcs+=(report.HpDrainTriggered ? 1 : 0)+(report.RageDrainTriggered ? 1 : 0)+(report.ShieldDrainTriggered ? 1 : 0);
                        criticals+=report.CriticalAttacks;attacks+=report.NormalAttacks+report.SpecialAttacks;
                        board.ClearAndRefill(groups);animation+=rules.clearSeconds+rules.EffectDuration(report)+rules.fallSeconds;
                        result=BattleCombat.Result(player,enemy);if(result!=BattleResult.Ongoing)break;
                        if(wave>=rules.maxCascadeWaves){board.GeneratePlayable();break;}
                    }
                    turns++;if(turns%2==0 && turns/2>=rules.maxRounds && result==BattleResult.Ongoing){limits++;result=BattleCombat.ResultAtRoundLimit(player,enemy);}
                }
                Assert.That(result,Is.Not.EqualTo(BattleResult.Ongoing));Assert.That(turns,Is.LessThanOrEqualTo(60));
                int round=(turns+1)/2;rounds.Add(round);if(turns>30)sudden++;
                if(result==BattleResult.Victory)wins++;else if(result==BattleResult.Defeat)losses++;else draws++;
                // Estimate human think time at 12 seconds per player turn; simulation itself has no waiting.
                seconds.Add(animation+round*12+(turns/2)*rules.aiDelay);
            }
            rounds.Sort();seconds.Sort();Directory.CreateDirectory("Logs");
            string summary=$"# Balance simulation\n\n1000 deterministic AI vs AI battles, seeds 0–999. Both sides share live rules and RNG implementation.\n\n"+
                $"- Mean rounds: {rounds.Average():0.00}\n- Median rounds: {rounds[count/2]}\n- P90 rounds: {rounds[(int)(count*.9)]}\n- Min/max rounds: {rounds[0]}/{rounds[count-1]}\n"+
                $"- Knockouts: {count-limits}; round-limit decisions: {limits}\n- Entered sudden death: {sudden}\n- First-side wins: {wins}; second-side wins: {losses}; draws: {draws}\n"+
                $"- Sword attacks: {attacks}; critical hits: {criticals} ({100.0*criticals/attacks:0.00}%)\n"+
                $"- Estimated duration with 12 s thinking/player turn: mean {seconds.Average()/60:0.00} min, median {seconds[count/2]/60:0.00} min, P90 {seconds[(int)(count*.9)]/60:0.00} min. Includes configured animations and AI delay; this is not measured human playtime.\n";
            summary+=$"\n- Healing: {healed}; total damage: {damage}; damage in critical waves: {critDamage}; specials: {specials}; cards: {cards}; drain proc flags: {drainProcs}; dead-board shuffles: {shuffles}\n";
            Assert.That(count-limits,Is.GreaterThan(count*.95));
            Assert.That(rounds[count/2],Is.InRange(8,16));
            File.WriteAllText("Logs/BalanceSimulation.md",summary);TestContext.WriteLine(summary);
        }
    }
}
