using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PetGemBattle.Tests
{
    public class SpecificationTests
    {
        private BattleRules rules;
        private sealed class Rolls : IBattleRandom { public int Calls;public double Next(){Calls++;return 0;} }
        [SetUp] public void Setup(){rules=ScriptableObject.CreateInstance<BattleRules>();}
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(rules);}
        private MatchGroup Group(GemKind kind,int count,int row=0)
        {
            var group=new MatchGroup{Kind=kind};for(int x=0;x<count;x++)group.Cells.Add(new Cell(x,row));return group;
        }
        [TestCase(3,0)] [TestCase(4,5)] [TestCase(5,10)] [TestCase(8,10)]
        public void GroupBonusIsOncePerGroupIndependentOfCombo(int count,int expected)
        {
            var a=new PetState(rules.player);var report=BattleCombat.ApplyMatches(a,new PetState(rules.enemy),new List<MatchGroup>{Group(GemKind.Mana,count)},7,rules);
            Assert.That(a.Rage,Is.EqualTo(expected));Assert.That(report.Events.Count,Is.EqualTo(1));
        }
        [Test] public void AdjacentMatchedRunsMergeEvenWithoutSharedCell()
        {
            var board=new MatchBoard(3);var cells=new int[64];for(int y=0;y<8;y++)for(int x=0;x<8;x++)cells[y*8+x]=(x+y)%5+1;
            for(int x=1;x<=3;x++)cells[2*8+x]=cells[3*8+x]=0;
            board.Load(cells);var swords=board.FindMatches().Where(g=>g.Kind==GemKind.Sword).ToList();
            Assert.That(swords.Count,Is.EqualTo(1));Assert.That(swords[0].Cells.Count,Is.EqualTo(6));
        }
        [Test] public void DrainBeforeSwordCanFundSpecialAndReportsInOrder()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(30,rules);b.AddRage(80,rules);
            var report=BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{Group(GemKind.Sword,3),Group(GemKind.Drain,3,1)},1,rules,new Rolls());
            Assert.That(report.SpecialAttacks,Is.EqualTo(1));Assert.That(a.Rage,Is.Zero);
            Assert.That(report.Events.Select(e=>e.gem),Is.EqualTo(new[]{GemKind.Drain,GemKind.Sword}));
            Assert.That(report.Events[1].before[3],Is.EqualTo(50));
        }
        [Test] public void DeathStopsLaterGroupsAndRandomDraws()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);b.TakeDamage(299);var rng=new Rolls();
            var report=BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{Group(GemKind.Sword,3),Group(GemKind.Sword,3,2)},1,rules,rng);
            Assert.That(rng.Calls,Is.EqualTo(1));Assert.That(report.Events.Count,Is.EqualTo(1));Assert.That(a.Rage,Is.EqualTo(10));
        }
        [Test] public void FullReceiverDoesNotDestroyVictimResources()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(100,rules);a.AddShield(60,rules);b.AddRage(80,rules);b.AddShield(60,rules);
            var report=BattleCombat.ApplyMatches(a,b,new List<MatchGroup>{Group(GemKind.Drain,3)},1,rules,new Rolls());
            Assert.That(b.Rage,Is.EqualTo(80));Assert.That(b.Shield,Is.EqualTo(36)); // HP drain absorbs 24 shield, no armor transfer.
            Assert.That(report.RageStolen,Is.Zero);Assert.That(report.ShieldStolen,Is.Zero);
        }
        [Test] public void EvolvedDefenseRetainsFractionUntilDamageRounding()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);b.AddMana(80,rules);CardSystem.Use(b,CardKind.Evolution,rules);
            Assert.That(b.Attack,Is.EqualTo(33.75f));Assert.That(b.Defense,Is.EqualTo(9.6f).Within(.00001));
            Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules),Is.EqualTo(40));
        }
        [Test] public void InvalidConfigAndUnknownCardsAreRejected()
        {
            rules.Validate();rules.criticalChance=float.NaN;Assert.Throws<InvalidOperationException>(()=>rules.Validate());
            Assert.That(CardSystem.CanUse(new PetState(rules.player),(CardKind)99,rules),Is.False);
        }
        [Test] public void AiTieRandomIsSeededAndNeverConsumesCombatRandom()
        {
            var a=new PetState(rules.player);var b=new PetState(rules.enemy);
            var r1=new SeededBattleRandom(12);var r2=new SeededBattleRandom(12);
            for(int i=0;i<30;i++){var board=new MatchBoard(i);var m1=BattleAI.ChooseMove(board,a,b,rules,0,r1);var m2=BattleAI.ChooseMove(board,a,b,rules,0,r2);Assert.That(m1.A,Is.EqualTo(m2.A));Assert.That(m1.B,Is.EqualTo(m2.B));}
        }
    }
}
