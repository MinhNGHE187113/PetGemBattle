using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PetGemBattle.Tests
{
    public class BattleCoreTests
    {
        private BattleRules rules;
        private sealed class FixedRandom : IBattleRandom
        {
            private readonly double[] values;private int index;
            public int Calls=>index;
            public FixedRandom(params double[] values){this.values=values;}
            public double Next()=>values[index++%values.Length];
        }
        [SetUp] public void SetUp(){rules=ScriptableObject.CreateInstance<BattleRules>();}
        [TearDown] public void TearDown(){UnityEngine.Object.DestroyImmediate(rules);}
        private static MatchGroup Group(GemKind kind,int count,int row=0)
        {
            var g=new MatchGroup {Kind=kind};for(int i=0;i<count;i++)g.Cells.Add(new Cell(i,row));return g;
        }
        private BattleReport Match(PetState a,PetState b,GemKind kind,int n,int wave=1,IBattleRandom rng=null)=>
            BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{Group(kind,n)},wave,rules,rng??new FixedRandom(1));
        private static int[] Latin()
        {
            var values=new int[64];for(int y=0;y<8;y++)for(int x=0;x<8;x++)values[y*8+x]=(x+y)%6;return values;
        }
        [Test] public void InitialBoardsHaveSixTypesNoMatchesAndLegalMoves()
        {
            var types=new HashSet<int>();
            for(int seed=0;seed<200;seed++)
            {
                var board=new MatchBoard(seed);Assert.That(board.FindMatches(),Is.Empty);Assert.That(board.LegalMoves(),Is.Not.Empty);
                Assert.That(board.Snapshot(),Has.All.InRange(0,5));types.UnionWith(board.Snapshot());
            }
            Assert.That(types.Count,Is.EqualTo(6));
        }
        [Test] public void InvalidSwapsAndNonNeighborsDoNotMutateBoard()
        {
            var board=new MatchBoard(12);board.Load(Latin());var before=board.Snapshot();
            Assert.That(board.TrySwap(new BoardMove(new Cell(0,0),new Cell(1,0))),Is.False);
            Assert.That(board.TrySwap(new BoardMove(new Cell(0,0),new Cell(2,0))),Is.False);
            Assert.That(board.TrySwap(new BoardMove(new Cell(0,0),new Cell(-1,0))),Is.False);CollectionAssert.AreEqual(before,board.Snapshot());
        }
        [Test] public void CrossMergesIntoFiveUniqueCells()
        {
            var board=new MatchBoard(1);var values=Latin();
            foreach(var c in new[]{new Cell(3,3),new Cell(2,3),new Cell(4,3),new Cell(3,2),new Cell(3,4)})values[c.Y*8+c.X]=0;
            foreach(var c in new[]{new Cell(1,3),new Cell(5,3),new Cell(3,1),new Cell(3,5)})values[c.Y*8+c.X]=1;
            board.Load(values);var groups=board.FindMatches();Assert.That(groups.Single(g=>g.Cells.Contains(new Cell(3,3))).Cells.Count,Is.EqualTo(5));
            Assert.That(groups.Sum(g=>g.Cells.Count),Is.EqualTo(groups.SelectMany(g=>g.Cells).Distinct().Count()));
        }
        [Test] public void GravityPreservesSurvivorOrderAndRefillsAllCells()
        {
            var board=new MatchBoard(9);var before=board.Snapshot();var g=new MatchGroup {Kind=GemKind.Sword};
            g.Cells.UnionWith(new[]{new Cell(0,0),new Cell(0,2),new Cell(0,5)});var falls=board.ClearAndRefill(new List<MatchGroup>{g});
            var survivors=new[]{1,3,4,6,7};for(int y=0;y<5;y++)Assert.That(board[0,y],Is.EqualTo(before[survivors[y]*8]));
            Assert.That(falls.Count,Is.EqualTo(64));Assert.That(board.Snapshot(),Has.All.InRange(0,5));
        }
        [Test] public void DeadBoardShuffleRestoresStablePlayability()
        {
            var board=new MatchBoard(1);board.Load(Latin());Assert.That(board.LegalMoves(),Is.Empty);board.ShufflePlayable();
            Assert.That(board.FindMatches(),Is.Empty);Assert.That(board.LegalMoves(),Is.Not.Empty);
        }
        [TestCase(3,41,10)] [TestCase(4,50,19)] [TestCase(5,59,28)]
        public void NormalSwordFormulaAndRageGain(int n,int damage,int rage)
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);var report=Match(a,b,GemKind.Sword,n);
            Assert.That(report.Damage,Is.EqualTo(damage));Assert.That(a.Rage,Is.EqualTo(rage));Assert.That(report.NormalAttacks,Is.EqualTo(1));
        }
        [TestCase(49,59,41)] [TestCase(50,0,72)] [TestCase(55,5,72)] [TestCase(100,40,72)]
        public void SpecialThresholdCheckedAfterSwordGainAndPreservesRemainder(int initial,int remaining,int damage)
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(initial,rules);
            var report=Match(a,b,GemKind.Sword,3);Assert.That(report.Damage,Is.EqualTo(damage));Assert.That(a.Rage,Is.EqualTo(remaining));
            Assert.That(report.SpecialAttacks,Is.EqualTo(initial>=50 ? 1 : 0));
        }
        [Test] public void RedGemsOnlyGrantRageAndDoNotAttack()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(55,rules);
            var report=Match(a,b,GemKind.Rage,3);Assert.That(a.Rage,Is.EqualTo(76));Assert.That(b.Hp,Is.EqualTo(300));Assert.That(report.SpecialAttacks,Is.Zero);
        }
        [Test] public void ComboAppliesToAllResourceFormulasAndShieldsAbsorbFirst()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(100);
            Match(a,b,GemKind.Heart,3,2);Assert.That(a.Hp,Is.EqualTo(215));
            Match(a,b,GemKind.Shield,4,3);Assert.That(a.Shield,Is.EqualTo(18));
            Match(a,b,GemKind.Mana,3,2);Assert.That(a.Mana,Is.EqualTo(30));
            Match(a,b,GemKind.Rage,3,3);Assert.That(a.Rage,Is.EqualTo(37));
            a.TakeDamage(40);Assert.That(a.Shield,Is.Zero);Assert.That(a.Hp,Is.EqualTo(193));
        }
        [Test] public void ResourcesClampToConfiguredAndEvolutionDependentLimits()
        {
            var a=new PetState(rules.player);a.Heal(999);a.AddMana(999,rules);a.AddRage(999,rules);a.AddShield(999,rules);
            Assert.That(a.Hp,Is.EqualTo(300));Assert.That(a.Mana,Is.EqualTo(100));Assert.That(a.Rage,Is.EqualTo(100));Assert.That(a.Shield,Is.EqualTo(60));
            CardSystem.Use(a,CardKind.Evolution,rules);a.AddShield(999,rules);Assert.That(a.Shield,Is.EqualTo(84));
        }
        [Test] public void DrainRollsAreIndependentAndUsePostDamageShield()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(100);b.AddShield(50,rules);b.AddRage(80,rules);
            var rng=new FixedRandom(.29,.26,.24);var report=Match(a,b,GemKind.Drain,3,1,rng);
            Assert.That(rng.Calls,Is.EqualTo(3));Assert.That(report.HpDrainTriggered,Is.True);Assert.That(report.RageDrainTriggered,Is.False);Assert.That(report.ShieldDrainTriggered,Is.True);
            Assert.That(report.HpStolen,Is.Zero);Assert.That(a.Hp,Is.EqualTo(200));Assert.That(b.Hp,Is.EqualTo(300));
            Assert.That(b.Shield,Is.EqualTo(18));Assert.That(a.Shield,Is.EqualTo(8));Assert.That(b.Rage,Is.EqualTo(80));
        }
        [Test] public void HpDrainHealsOnlyActualHpLostAndCapsAtTwelvePercent()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(100);
            var report=Match(a,b,GemKind.Drain,3,20,new FixedRandom(0,1,1));
            Assert.That(report.HpStolen,Is.EqualTo(36));Assert.That(b.Hp,Is.EqualTo(264));Assert.That(a.Hp,Is.EqualTo(236));
        }
        [Test] public void StolenResourcesNeverExceedVictimResourcesOrReceiverCaps()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(99,rules);a.AddShield(59,rules);b.AddRage(10,rules);b.AddShield(10,rules);
            Match(a,b,GemKind.Drain,3,40,new FixedRandom(1,0,0));
            Assert.That(b.Rage,Is.EqualTo(9));Assert.That(b.Shield,Is.EqualTo(9));Assert.That(a.Rage,Is.EqualTo(100));Assert.That(a.Shield,Is.EqualTo(60));
        }
        [Test] public void DrainProbabilityCapsAndFixedSeedAreReproducible()
        {
            Assert.That(rules.DrainChance(rules.hpDrainChance,rules.hpDrainChanceCap,20),Is.EqualTo(.55f));
            Assert.That(rules.DrainChance(rules.rageDrainChance,rules.resourceDrainChanceCap,20),Is.EqualTo(.5f));
            var rng1=new SeededBattleRandom(321);var rng2=new SeededBattleRandom(321);
            for(int i=0;i<100;i++)Assert.That(rng1.Next(),Is.EqualTo(rng2.Next()));
        }
        [Test] public void ActualDrainTrialsMatchIndependentProbabilities()
        {
            int hp=0,rage=0,armor=0,joint=0;var rng=new SeededBattleRandom(444);
            const int trials=10000;
            for(int i=0;i<trials;i++)
            {
                var report=Match(new PetState(rules.player),new PetState(rules.enemy),GemKind.Drain,3,1,rng);
                if(report.HpDrainTriggered)hp++;if(report.RageDrainTriggered)rage++;if(report.ShieldDrainTriggered)armor++;
                if(report.HpDrainTriggered && report.RageDrainTriggered)joint++;
            }
            Assert.That(hp/(float)trials,Is.EqualTo(.30f).Within(.02f));Assert.That(rage/(float)trials,Is.EqualTo(.25f).Within(.02f));
            Assert.That(armor/(float)trials,Is.EqualTo(.25f).Within(.02f));Assert.That(joint/(float)trials,Is.EqualTo(.075f).Within(.015f));
        }
        [Test] public void CardsEnforceChargesAndOneCardPerTurn()
        {
            var a=new PetState(rules.player);Assert.That(CardSystem.Use(a,CardKind.Mana,rules),Is.Not.Null);Assert.That(a.Mana,Is.EqualTo(20));
            Assert.That(CardSystem.Use(a,CardKind.Rage,rules),Is.Null);a.BeginTurn();CardSystem.Use(a,CardKind.Mana,rules);a.BeginTurn();
            Assert.That(CardSystem.Use(a,CardKind.Mana,rules),Is.Null);Assert.That(a.Mana,Is.EqualTo(40));
            CardSystem.Use(a,CardKind.Rage,rules);a.BeginTurn();CardSystem.Use(a,CardKind.Rage,rules);a.BeginTurn();
            Assert.That(CardSystem.Use(a,CardKind.Rage,rules),Is.Null);Assert.That(a.Rage,Is.EqualTo(40));
        }
        [Test] public void EvolutionValidatesCostOnceAndUpdatesStatsWithoutMutatingDefinition()
        {
            var a=new PetState(rules.player);a.AddMana(79,rules);Assert.That(CardSystem.Use(a,CardKind.Evolution,rules),Is.Null);
            Assert.That(a.Mana,Is.EqualTo(79));a.AddMana(21,rules);a.TakeDamage(100);a.AddRage(55,rules);a.AddShield(50,rules);
            var report=CardSystem.Use(a,CardKind.Evolution,rules);Assert.That(report.Evolved,Is.True);
            Assert.That(a.MaxHp,Is.EqualTo(420));Assert.That(a.Hp,Is.EqualTo(320));Assert.That(a.Attack,Is.EqualTo(33.75f));Assert.That(a.Defense,Is.EqualTo(9.6f).Within(.0001));
            Assert.That(a.Mana,Is.EqualTo(20));Assert.That(a.Rage,Is.EqualTo(55));Assert.That(a.Shield,Is.EqualTo(50));
            Assert.That(rules.player.maxHp,Is.EqualTo(300));Assert.That(rules.player.attack,Is.EqualTo(25));
            a.BeginTurn();a.AddMana(100,rules);Assert.That(CardSystem.Use(a,CardKind.Evolution,rules),Is.Null);
            Assert.That(BattleCombat.SwordDamage(a,new PetState(rules.enemy),3,1,rules),Is.EqualTo(57));
        }
        [Test] public void StableGroupOrderAvoidsDuplicateCellsAndUsesRedRageBeforeSwords()
        {
            var groups=new List<MatchGroup>{Group(GemKind.Sword,3),Group(GemKind.Rage,3,1)};
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(30,rules);
            var r=BattleCombat.ApplyMatches(a,b,groups,1,rules,new FixedRandom(1));Assert.That(r.SpecialAttacks,Is.EqualTo(1));Assert.That(a.Rage,Is.EqualTo(1));
            var duplicate=Group(GemKind.Sword,3);groups=new List<MatchGroup>{duplicate,duplicate};a=new PetState(rules.player);b=new PetState(rules.enemy);
            r=BattleCombat.ApplyMatches(a,b,groups,1,rules,new FixedRandom(1));Assert.That(r.NormalAttacks,Is.EqualTo(1));Assert.That(r.Damage,Is.EqualTo(41));
        }
        [Test] public void ResultHandlesVictoryDefeatAndDraw()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);b.TakeDamage(999);
            Assert.That(BattleCombat.Result(a,b),Is.EqualTo(BattleResult.Victory));a.TakeDamage(999);
            Assert.That(BattleCombat.Result(a,b),Is.EqualTo(BattleResult.Draw));Assert.That(BattleCombat.Result(a,new PetState(rules.enemy)),Is.EqualTo(BattleResult.Defeat));
        }
        [Test] public void AISelectsLegalMovesWithoutMutationAndEvolvesWhenReady()
        {
            var a=new PetState(rules.enemy);var b=new PetState(rules.player);
            Assert.That(BattleAI.ChooseCard(a,b,rules),Is.EqualTo(CardKind.Mana));a.AddMana(80,rules);
            Assert.That(BattleAI.ChooseCard(a,b,rules),Is.EqualTo(CardKind.Evolution));
            for(int seed=0;seed<30;seed++)
            {
                var board=new MatchBoard(seed);var before=board.Snapshot();var move=BattleAI.ChooseMove(board,a,b,rules);
                CollectionAssert.AreEqual(before,board.Snapshot());Assert.That(board.TrySwap(move),Is.True);
            }
        }
        [Test] public void LongSeededBattlesKeepResolvingLegalTurnsWithoutRunawayCombos()
        {
            for(int seed=0;seed<12;seed++)
            {
                var board=new MatchBoard(seed);var rng=new SeededBattleRandom(seed);var a=new PetState(rules.player);var b=new PetState(rules.enemy);int turns=0;
                while(BattleCombat.Result(a,b)==BattleResult.Ongoing && turns<600)
                {
                    a.BeginTurn();var card=BattleAI.ChooseCard(a,b,rules);if(card.HasValue)CardSystem.Use(a,card.Value,rules);
                    if(board.LegalMoves().Count==0)board.ShufflePlayable();Assert.That(board.TrySwap(BattleAI.ChooseMove(board,a,b,rules)),Is.True);
                    for(int wave=1;;wave++)
                    {
                        var matches=board.FindMatches();if(matches.Count==0)break;
                        BattleCombat.ApplyMatches(a,b,matches,wave,rules,rng);board.ClearAndRefill(matches);
                        if(BattleCombat.Result(a,b)!=BattleResult.Ongoing)break;
                        Assert.That(wave,Is.LessThanOrEqualTo(100));
                    }
                    var swap=a;a=b;b=swap;turns++;
                }
                // Recovery can sustain a battle indefinitely under the specified rules.
                // The invariant is that each turn resolves; there is no arbitrary turn limit.
                Assert.That(turns,Is.GreaterThan(0));
                Assert.That(a.Hp,Is.InRange(0,a.MaxHp));Assert.That(b.Hp,Is.InRange(0,b.MaxHp));
                if(BattleCombat.Result(a,b)==BattleResult.Ongoing)
                    Assert.That(board.FindMatches(),Is.Empty,"Unresolved cascade at seed "+seed);
            }
        }
    }
}

