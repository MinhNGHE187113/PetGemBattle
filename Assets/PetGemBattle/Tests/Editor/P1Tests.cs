using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PetGemBattle.Tests
{
    public class P1Tests
    {
        private P1Catalog catalog;private BattleRules rules;private string directory;private SaveGameService save;private ProgressionService progress;
        private sealed class Roll : IBattleRandom {public int Calls;public double Value=1;public double Next(){Calls++;return Value;}}
        [SetUp] public void Setup()
        {
            catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();rules=ScriptableObject.CreateInstance<BattleRules>();rules.elements=catalog;
            directory=Path.Combine(Path.GetTempPath(),"PetArenaP1Tests-"+Guid.NewGuid().ToString("N"));save=new SaveGameService(Path.Combine(directory,"save.json"),catalog);progress=new ProgressionService(catalog,save);
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);UnityEngine.Object.DestroyImmediate(rules);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private void Win(int stage,string id=null)=>progress.Reward(stage,BattleResult.Victory,id??Guid.NewGuid().ToString("N"),new Roll());
        private List<CollectedGroup> Group(params GemKind[] kinds)
        {
            var result=new List<CollectedGroup>();for(int i=0;i<kinds.Length;i++){var g=new MatchGroup{Kind=kinds[i]};for(int x=0;x<3;x++)g.Cells.Add(new Cell(x,i%8));result.Add(new CollectedGroup(g,i+1));}return result;
        }
        [TestCase(Element.Metal,Element.Wood,1.25f)] [TestCase(Element.Wood,Element.Metal,.85f)] [TestCase(Element.Fire,Element.Wood,1f)]
        [TestCase(Element.Water,Element.Fire,1.25f)] [TestCase(Element.Earth,Element.Water,1.25f)] [TestCase(Element.Fire,Element.Metal,1.25f)] [TestCase(Element.Wood,Element.Earth,1.25f)]
        public void P1_ElementPairs(Element a,Element b,float expected){Assert.That(catalog.Multiplier(a,b),Is.EqualTo(expected));}
        [Test] public void P1_T04_AllSixRaritiesAndSpeciesValidate(){catalog.ValidateContent();for(int i=1;i<6;i++){Assert.That(catalog.rarities[i].hp,Is.GreaterThan(catalog.rarities[i-1].hp));Assert.That(catalog.rarities[i].attack,Is.GreaterThan(catalog.rarities[i-1].attack));}}
        [Test] public void P1_LevelSevenUsesBaseStats(){var p=catalog.pets[3];p.hp=320;p.attack=29;p.defense=9;var s=catalog.Stats(p.id,7);Assert.That(new[]{s.maxHp,s.attack,s.defense},Is.EqualTo(new[]{512,45,13}));}
        [Test] public void P1_T05_WrongElementCannotUpgrade(){Win(1);progress.Data.stones[ProgressionService.StoneIndex(Element.Fire,2)]=1;Assert.That(progress.Upgrade("wood-1",1,"up"),Is.False);Assert.That(progress.StoneCount(Element.Fire,2),Is.EqualTo(1));}
        [Test] public void P1_T06_UpgradeConsumesCorrectStoneAndSaves(){Win(1);Win(2);Assert.That(progress.Upgrade("wood-1",1,"up"),Is.True);Assert.That(progress.Owned("wood-1").level,Is.EqualTo(2));Assert.That(progress.StoneCount(Element.Wood,2),Is.Zero);Assert.That(save.Load().pets.Single(p=>p.id=="wood-1").level,Is.EqualTo(2));}
        [Test] public void P1_T07_LevelSevenNeverConsumes(){Win(1);progress.Owned("wood-1").level=7;progress.Data.stones[13]=2;Assert.That(progress.Upgrade("wood-1",7,"up"),Is.False);Assert.That(progress.Data.stones[13],Is.EqualTo(2));}
        [Test] public void P1_T08_RestartPreservesSelectionAndLevel(){Win(1);Win(2);progress.Upgrade("wood-1",1,"up");progress.Select("wood-1");var loaded=new ProgressionService(catalog,save);Assert.That(loaded.Data.activePetId,Is.EqualTo("wood-1"));Assert.That(loaded.Owned("wood-1").level,Is.EqualTo(2));Assert.That(loaded.StoneCount(Element.Wood,2),Is.Zero);}
        [Test] public void P1_T09_FirstClearCapturesAndUnlocks(){Win(1);Win(2);Win(3);Assert.That(progress.Owned("wood-3").level,Is.EqualTo(1));Assert.That(progress.Data.unlockedStage,Is.EqualTo(4));Assert.That(progress.StoneCount(Element.Wood,3),Is.EqualTo(1));}
        [Test] public void P1_T10_FarmDoesNotDuplicatePet(){Win(1);Win(2);Win(3);Win(3);Assert.That(progress.Data.pets.Count(p=>p.id=="wood-3"),Is.EqualTo(1));Assert.That(progress.StoneCount(Element.Wood,3),Is.EqualTo(2));}
        [Test] public void P1_T11_LossDoesNotReward(){for(int i=1;i<5;i++)Win(i);progress.Reward(5,BattleResult.Defeat,"loss",new Roll());Assert.That(progress.Data.unlockedStage,Is.EqualTo(5));Assert.That(progress.Owned("wood-5"),Is.Null);}
        [Test] public void P1_T12_FinalBossIsAAndBuffNotCaptured(){Assert.That(catalog.Pet("wood-7").rarity,Is.EqualTo(PetRarity.A));for(int i=1;i<=7;i++)Win(i);Assert.That(catalog.Stats("wood-7",progress.Owned("wood-7").level).maxHp,Is.EqualTo(450));Assert.That(progress.Data.pets.Count,Is.EqualTo(8));}
        [Test] public void P1_T14_PrepareCollectsWithoutMutatingLiveState(){var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.TakeDamage(100);var plan=CombatTurnPlan.Prepare(a,b,Group(GemKind.Sword,GemKind.Rage,GemKind.Heart),rules,new Roll(),0);Assert.That(a.Hp,Is.EqualTo(200));Assert.That(b.Hp,Is.EqualTo(300));Assert.That(plan.Last().Report.NormalAttacks+plan.Last().Report.SpecialAttacks,Is.EqualTo(1));}
        [Test] public void P1_T15_TwoAttacksSpendRageSeparately(){var a=new PetState(rules.player);var b=new PetState(rules.enemy);a.AddRage(45,rules);var plan=CombatTurnPlan.Prepare(a,b,Group(GemKind.Sword,GemKind.Sword),rules,new Roll(),0);Assert.That(plan.Sum(s=>s.Report.SpecialAttacks),Is.EqualTo(1));Assert.That(plan.Sum(s=>s.Report.NormalAttacks),Is.EqualTo(1));foreach(var step in plan)step.Apply(a,b);Assert.That(a.Rage,Is.EqualTo(5));}
        [Test] public void P1_T16_ImpactCannotApplyTwiceOrReroll(){var a=new PetState(rules.player);var b=new PetState(rules.enemy);var rng=new Roll{Value=0};var plan=CombatTurnPlan.Prepare(a,b,Group(GemKind.Drain,GemKind.Sword),rules,rng,0);int calls=rng.Calls;foreach(var step in plan){Assert.That(step.Apply(a,b),Is.True);Assert.That(step.Apply(a,b),Is.False);}Assert.That(rng.Calls,Is.EqualTo(calls));Assert.That(calls,Is.EqualTo(4));}
        [Test] public void P1_T17_SpecialCritElementOrder(){var a=new PetState(new PetDefinition{element=Element.Metal});var b=new PetState(new PetDefinition{element=Element.Wood});Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules,true,true),Is.EqualTo(153));}
        [Test] public void P1_T20_PresentationSpeedCannotAffectPreparedDamage(){var g=Group(GemKind.Sword,GemKind.Drain);var first=CombatTurnPlan.Prepare(new PetState(rules.player),new PetState(rules.enemy),g,rules,new SeededBattleRandom(42),0);rules.effectSeconds/=2;var second=CombatTurnPlan.Prepare(new PetState(rules.player),new PetState(rules.enemy),g,rules,new SeededBattleRandom(42),0);Assert.That(first.Last().Target.Hp,Is.EqualTo(second.Last().Target.Hp));}
        [Test] public void P1_T21_SevenDistinctStoneSilhouettes(){var masks=new HashSet<string>();for(int grade=1;grade<=7;grade++)masks.Add(string.Concat(P1Art.Stone(grade).texture.GetPixels32().Select(p=>p.a>0 ? "1":"0")));Assert.That(masks.Count,Is.EqualTo(7));}
        [Test] public void P1_T23_DoubleClickAndDuplicateRewardAreIdempotent(){Win(1,"battle");Win(1,"battle");Assert.That(progress.StoneCount(Element.Wood,1),Is.EqualTo(1));Win(2);Assert.That(progress.Upgrade("wood-1",1,"up"),Is.True);Assert.That(progress.Upgrade("wood-1",1,"up"),Is.False);Assert.That(progress.Owned("wood-1").level,Is.EqualTo(2));}
        [Test] public void P1_T24_FilterOwnedWoodC(){for(int i=1;i<=4;i++)Win(i);Assert.That(progress.Filter((int)Element.Wood,(int)PetRarity.C).Select(p=>p.id),Is.EqualTo(new[]{"wood-1","wood-2","wood-3"}));}
        [Test] public void CorruptSaveRestoresBackup(){Win(1);Win(2);File.WriteAllText(Path.Combine(directory,"save.json"),"{broken");var restored=save.Load();Assert.That(restored.unlockedStage,Is.EqualTo(2));Assert.That(save.Notice,Does.Contain("dự phòng"));}
        [Test] public void InvalidActivePetIsRejected(){var s=PlayerSave.Fresh();s.activePetId="missing";Assert.Throws<InvalidDataException>(()=>save.Write(s));}
        [Test] public void FailedSaveDoesNotConsumeStoneOrLevel(){Win(1);Win(2);File.Delete(Path.Combine(directory,"save.json"));Directory.CreateDirectory(Path.Combine(directory,"save.json"));Assert.Catch(()=>progress.Upgrade("wood-1",1,"up"));Assert.That(progress.Owned("wood-1").level,Is.EqualTo(1));Assert.That(progress.StoneCount(Element.Wood,2),Is.EqualTo(1));}
        [Test] public void RareDropRollsOnceAndIsSeedReproducible(){Win(1);var rng=new Roll{Value=0};progress.Reward(1,BattleResult.Victory,"bonus",rng);progress.Reward(1,BattleResult.Victory,"bonus",rng);Assert.That(rng.Calls,Is.EqualTo(1));Assert.That(progress.StoneCount(Element.Wood,1),Is.EqualTo(3));}
        [Test] public void P1_T25_TwoHundredTenCinematicBattlesConclude()
        {
            var lines=new List<string>{"# P1 seeded cinematic simulation",""};int count=0;
            for(int stage=1;stage<=7;stage++){int wins=0;double rounds=0;
                for(int seed=0;seed<30;seed++)
                {
                    var board=new MatchBoard(seed);var rng=new SeededBattleRandom(seed);var a=new PetState(catalog.Stats(stage==1 ? "starter-metal" : "wood-"+Math.Max(1,stage-1),Math.Min(7,Math.Max(1,stage-1))));var b=new PetState(catalog.Stats("wood-"+stage,1,true,stage));
                    bool player=true;int turn=0;BattleResult result=BattleResult.Ongoing;
                    while(result==BattleResult.Ongoing && turn<60)
                    {
                        var actor=player ? a : b;var target=player ? b : a;actor.BeginTurn();var card=BattleAI.ChooseCard(actor,target,rules);if(card.HasValue)CardSystem.Use(actor,card.Value,rules,target);
                        if(board.LegalMoves().Count==0)board.ShufflePlayable();Assert.That(board.TrySwap(BattleAI.ChooseMove(board,actor,target,rules,turn/2)),Is.True);
                        var groups=new List<CollectedGroup>();for(int wave=1;wave<=100;wave++){var found=board.FindMatches();if(found.Count==0)break;foreach(var group in found)groups.Add(new CollectedGroup(group,wave));board.ClearAndRefill(found);if(wave==100)board.GeneratePlayable();}
                        foreach(var step in CombatTurnPlan.Prepare(actor,target,groups,rules,rng,turn/2))step.Apply(actor,target);
                        result=BattleCombat.Result(a,b);turn++;player=!player;
                    }
                    if(result==BattleResult.Ongoing)result=BattleCombat.ResultAtRoundLimit(a,b);Assert.That(result,Is.Not.EqualTo(BattleResult.Ongoing));if(result==BattleResult.Victory)wins++;rounds+=(turn+1)/2;count++;
                }
                lines.Add("Stage "+stage+": "+wins+"/30 wins; mean rounds "+(rounds/30).ToString("0.0"));
            }
            Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/P1Simulation.md",lines);Assert.That(count,Is.EqualTo(210));
        }
    }
}
