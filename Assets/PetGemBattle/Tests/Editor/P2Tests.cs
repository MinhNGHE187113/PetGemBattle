using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace PetGemBattle.Tests
{
    public class P2Tests
    {
        private P1Catalog catalog;private BattleRules rules;private GameModeConfig mode;private string directory;private SaveGameService save;private ProgressionService progress;
        private sealed class Roll : IBattleRandom {public double Value=1;public int Calls;public double Next(){Calls++;return Value;}}
        [SetUp] public void Setup()
        {
            catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();P2Content.Upgrade(catalog);mode=ScriptableObject.CreateInstance<GameModeConfig>();
            rules=ScriptableObject.CreateInstance<BattleRules>();rules.elements=catalog;rules.attackMultiplier=2.3f;rules.swordExtraMultiplier=.4f;rules.specialManaCost=30;rules.finalMultiplierRounding=true;
            directory=Path.Combine(Path.GetTempPath(),"PetArenaP2Tests-"+Guid.NewGuid().ToString("N"));save=new SaveGameService(Path.Combine(directory,"save.json"),catalog);progress=new ProgressionService(catalog,save,mode);
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);UnityEngine.Object.DestroyImmediate(rules);UnityEngine.Object.DestroyImmediate(mode);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private MatchGroup Sword(int row=0){var g=new MatchGroup{Kind=GemKind.Sword};for(int x=0;x<3;x++)g.Cells.Add(new Cell(x,row));return g;}
        private PetState Actor(int atk=25)=>new PetState(new PetDefinition{attack=atk,element=Element.Wood});
        [Test] public void P2_02_FiveIslandsOnlyWoodPlayable(){Assert.That(catalog.islands.Length,Is.EqualTo(5));Assert.That(catalog.islands.Count(i=>i.playable),Is.EqualTo(1));Assert.That(catalog.islands.Single(i=>i.playable).element,Is.EqualTo(Element.Wood));}
        [Test] public void P2_06_FullCollectionHasAllElementsOnce(){Assert.That(progress.Data.pets.Count,Is.EqualTo(catalog.pets.Length));Assert.That(progress.Data.pets.Select(p=>catalog.Pet(p.id).element).Distinct().Count(),Is.EqualTo(5));Assert.That(progress.Data.pets.All(p=>p.level==1),Is.True);}
        [Test] public void P2_07_ReloadDoesNotDuplicateAndKeepsEquipped(){progress.Select("water-otter");var loaded=new ProgressionService(catalog,save,mode);Assert.That(loaded.Data.pets.Count,Is.EqualTo(12));Assert.That(loaded.Data.activePetId,Is.EqualTo("water-otter"));}
        [Test] public void P2_08_EquippedStatsAndFormComeFromSameDefinition(){progress.Owned("wood-2").level=4;progress.Select("wood-2");var stats=catalog.Stats(progress.Data.activePetId,progress.Owned(progress.Data.activePetId).level);Assert.That(stats.artSheet,Is.EqualTo("wood-2-form1"));Assert.That(stats.permanentForm,Is.EqualTo(1));}
        [Test] public void P2_09_MamMiuNeverChangesForm(){var p=catalog.Stats("wood-1",7);Assert.That(p.permanentForm,Is.Zero);Assert.That(p.maxHp,Is.EqualTo(392));Assert.That(p.artSheet,Is.EqualTo("wood-1"));}
        [Test] public void P2_10_FrogLvFourGetsSingleFormBonus(){var p=catalog.Stats("wood-2",4);Assert.That(p.maxHp,Is.EqualTo(379));Assert.That(p.attack,Is.EqualTo(35));Assert.That(p.defense,Is.EqualTo(9));Assert.That(p.speed,Is.EqualTo(25));Assert.That(p.name,Is.EqualTo("Ếch Lá Hiệp Sĩ"));}
        [Test] public void P2_11_TreeHasTwoMilestonesWithoutCompoundingOnReads(){var a=catalog.Stats("wood-7",4);Assert.That(a.permanentForm,Is.EqualTo(1));for(int i=0;i<20;i++){var p=catalog.Stats("wood-7",7);Assert.That(new[]{p.maxHp,p.attack,p.defense,p.speed},Is.EqualTo(new[]{887,78,24,43}));Assert.That(p.permanentForm,Is.EqualTo(2));}}
        [Test] public void P2_12_WrongElementStoneCannotUpgrade(){progress.Data.stones[ProgressionService.StoneIndex(Element.Fire,2)]=1;Assert.That(progress.Upgrade("wood-1",1,"wrong"),Is.False);Assert.That(progress.StoneCount(Element.Fire,2),Is.EqualTo(1));}
        [Test] public void P2_13_DoubleClickConsumesOneStone(){progress.SeedDemoStones();Assert.That(progress.Upgrade("wood-1",1,"upgrade"),Is.True);Assert.That(progress.Upgrade("wood-1",1,"upgrade"),Is.False);Assert.That(progress.StoneCount(Element.Wood,2),Is.EqualTo(1));}
        [Test] public void P2_14_BossHpIncreasingAndOwnedBaseUnaffected(){int[] expected={440,530,630,760,900,1070,1270};for(int i=1;i<=7;i++)Assert.That(catalog.Stats("wood-"+i,1,true,i).maxHp,Is.EqualTo(expected[i-1]));Assert.That(catalog.Stats("wood-7",1).maxHp,Is.EqualTo(450));}
        [Test] public void P2_15_ThreeSwordsBDealsSixtyTwo(){var a=Actor(29);var b=new PetState(new PetDefinition{defense=9,element=Element.Wood});Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules),Is.EqualTo(62));}
        [Test] public void P2_16_FirstBossDealsTwentyFiveNeutralDamage(){var a=new PetState(catalog.Stats("wood-1",1,true,1));var b=new PetState(new PetDefinition{maxHp=260,defense=6,element=Element.Wood});Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules),Is.EqualTo(25));}
        [Test] public void P2_InlineSerializedProfileDoesNotTurnOwnedPetIntoBoss()
        {
            var definition=catalog.Stats("wood-1",1);definition.bossProfile=new BossEncounterProfile();
            var a=new PetState(definition);var target=new PetState(catalog.Stats("wood-1",1));
            int damage=BattleCombat.SwordDamage(a,target,3,1,rules);definition.bossProfile=null;
            Assert.That(BattleCombat.SwordDamage(a,target,3,1,rules),Is.EqualTo(damage));
            definition.bossProfile=new BossEncounterProfile();a.TakeDamage(200);Assert.That(a.TryBossAwaken(rules),Is.False);
        }
        [TestCase(false)] [TestCase(true)] public void P2_BossDamageCapsPreventOneShot(bool special){var a=new PetState(catalog.Stats("wood-1",1,true,1));var b=new PetState(new PetDefinition{maxHp=260,element=Element.Wood});Assert.That(BattleCombat.SwordDamage(a,b,25,30,rules,special,true,30),Is.LessThanOrEqualTo(special ? 78 : 62));}
        [TestCase(29,false,72,29)] [TestCase(35,true,12,5)] public void P2_17_18_SpecialNeedsBothResources(int mana,bool special,int rageAfter,int manaAfter)
        {
            var a=Actor();a.AddRage(72,rules);a.AddMana(mana,rules);var report=BattleCombat.ApplyMatches(a,Actor(),new List<MatchGroup>{Sword()},1,rules,new Roll(),0,true);
            Assert.That(report.SpecialAttacks,Is.EqualTo(special ? 1 : 0));Assert.That(a.Rage,Is.EqualTo(rageAfter));Assert.That(a.Mana,Is.EqualTo(manaAfter));
        }
        [Test] public void P2_19_TwoSwordGroupsCheckRemainingManaEachTime(){var a=Actor();var b=Actor();a.AddRage(90,rules);a.AddMana(35,rules);var plan=CombatTurnPlan.Prepare(a,b,new List<CollectedGroup>{new CollectedGroup(Sword(),1),new CollectedGroup(Sword(1),1)},rules,new Roll(),0);Assert.That(plan.Sum(p=>p.Report.SpecialAttacks),Is.EqualTo(1));Assert.That(plan.Sum(p=>p.Report.NormalAttacks),Is.EqualTo(1));foreach(var p in plan)p.Apply(a,b);Assert.That(a.Mana,Is.EqualTo(5));}
        [Test] public void P2_20_BossAwakensOnceWithoutHealing(){var b=new PetState(catalog.Stats("wood-1",1,true,1));b.TakeDamage(317);int hp=b.Hp;Assert.That(b.TryBossAwaken(rules),Is.True);Assert.That(b.Hp,Is.EqualTo(hp));Assert.That(b.Attack,Is.EqualTo(22*1.18f).Within(.0001));Assert.That(b.Shield,Is.EqualTo(35));}
        [Test] public void P2_21_LethalDamageSkipsAwakening(){var b=new PetState(catalog.Stats("wood-1",1,true,1));b.TakeDamage(9999);Assert.That(b.TryBossAwaken(rules),Is.False);Assert.That(b.Shield,Is.Zero);}
        [Test] public void P2_22_ContinuedDamageDoesNotRepeatBuff(){var b=new PetState(catalog.Stats("wood-1",1,true,1));b.TakeDamage(320);b.TryBossAwaken(rules);float attack=b.Attack;int shield=b.Shield;b.TakeDamage(1);Assert.That(b.TryBossAwaken(rules),Is.False);Assert.That(b.Attack,Is.EqualTo(attack));Assert.That(b.Shield,Is.EqualTo(shield-1));}
        [Test] public void P2_23_ManaAndRageResolveBeforeSword(){var a=Actor();a.AddMana(6,rules);a.AddRage(29,rules);var mana=Sword(2);mana.Kind=GemKind.Mana;var rage=Sword(3);rage.Kind=GemKind.Rage;var plan=CombatTurnPlan.Prepare(a,Actor(),new List<CollectedGroup>{new CollectedGroup(Sword(),1),new CollectedGroup(mana,1),new CollectedGroup(rage,1)},rules,new Roll(),0);Assert.That(plan.Last().Report.SpecialAttacks,Is.EqualTo(1));Assert.That(a.Mana,Is.EqualTo(6));}
        [Test] public void P2_25_PlanDoesNotRerollWhenApplied(){var a=Actor();var b=Actor();var rng=new Roll{Value=0};var plan=CombatTurnPlan.Prepare(a,b,new List<CollectedGroup>{new CollectedGroup(Sword(),1)},rules,rng,0);int calls=rng.Calls;foreach(var step in plan){step.Apply(a,b);step.Apply(a,b);}Assert.That(rng.Calls,Is.EqualTo(calls));}
        [Test] public void P2_26_ArtLibraryHasEveryBaseAndForm(){foreach(var p in catalog.pets){Assert.That(P2ArtLibrary.Get(p.artSheet),Is.Not.Null,p.id);foreach(var form in p.forms??new EvolutionFormDefinition[0])Assert.That(P2ArtLibrary.Get(form.artSheet),Is.Not.Null,form.artSheet);}for(int i=0;i<7;i++)Assert.That(P2ArtLibrary.Get("stones",i),Is.Not.Null);}
        [Test] public void P2_SeparatedArtUsesPersistentSilhouetteGeometry()
        {
            foreach(var p in catalog.pets)for(int i=0;i<4;i++)Assert.That(P2ArtLibrary.Geometry(P2ArtLibrary.Get(p.artSheet,i)).vertices.Length,Is.GreaterThan(4),p.id+" part "+i);
            var go=new GameObject("ContourTest",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(P2SilhouetteMesh));
            try
            {
                go.GetComponent<UnityEngine.UI.Image>().sprite=P2ArtLibrary.Get("wood-1");
                using(var mesh=new UnityEngine.UI.VertexHelper()){go.GetComponent<P2SilhouetteMesh>().ModifyMesh(mesh);Assert.That(mesh.currentVertCount,Is.GreaterThan(4));}
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test] public void P2_27_DemoConquestNeverDuplicatesFinalBoss(){int count=progress.Data.pets.Count;for(int i=1;i<=7;i++)progress.Reward(i,BattleResult.Victory,"win-"+i,new Roll());Assert.That(progress.Data.pets.Count,Is.EqualTo(count));Assert.That(progress.StoneCount(Element.Wood,7),Is.EqualTo(1));Assert.That(progress.Data.rewardHistory[0],Does.Contain("Đã chinh phục"));}
        [Test] public void P2_28_LvSevenFormAndStonesSurviveReload(){progress.SeedDemoStones();for(int lv=1;lv<7;lv++)Assert.That(progress.Upgrade("wood-7",lv,"up-"+lv),Is.True);progress.Select("wood-7");var again=new ProgressionService(catalog,save,mode);Assert.That(again.Owned("wood-7").level,Is.EqualTo(7));Assert.That(catalog.Stats("wood-7",7).artSheet,Is.EqualTo("wood-7-form2"));Assert.That(again.StoneCount(Element.Wood,7),Is.EqualTo(1));}
        [Test] public void P2_29_BattleAwakeningDoesNotMutatePermanentForm(){var definition=catalog.Stats("wood-7",7);var pet=new PetState(definition);pet.AddMana(80,rules);CardSystem.Use(pet,CardKind.Evolution,rules);Assert.That(pet.MaxHp,Is.EqualTo(BattleMath.Round(887*1.4f)));Assert.That(definition.maxHp,Is.EqualTo(887));Assert.That(new PetState(catalog.Stats("wood-7",7)).MaxHp,Is.EqualTo(887));}
        [Test] public void P2_SeedAndResetRequireFreshTransaction(){Assert.That(progress.SeedDemoStones(),Is.True);Assert.That(progress.SeedDemoStones(),Is.False);progress.ResetDemo();Assert.That(progress.Data.pets.Count,Is.EqualTo(12));Assert.That(progress.Data.stones.Sum(),Is.Zero);}
        [Test] public void P2_MigrateP1KeepsLevelAndActivePet(){var old=PlayerSave.Fresh();old.pets.Add(new OwnedPet{id="wood-2",level=4});old.activePetId="wood-2";save.Write(old);var next=new ProgressionService(catalog,save,mode);Assert.That(next.Data.schemaVersion,Is.EqualTo(2));Assert.That(next.Data.activePetId,Is.EqualTo("wood-2"));Assert.That(next.Owned("wood-2").level,Is.EqualTo(4));}
        [Test] public void P2_NormalModeStartsWithStarterAndCaptures(){mode.DemoFullCollection=false;progress.ResetDemo();Assert.That(progress.Data.pets.Count,Is.EqualTo(1));progress.Reward(1,BattleResult.Victory,"campaign",new Roll());Assert.That(progress.Owned("wood-1"),Is.Not.Null);}
        [Test] public void P2_FinalMultipliersRoundOnlyAtEnd(){var a=Actor();var b=Actor();Assert.That(BattleCombat.SwordDamage(a,b,3,1,rules,true,true),Is.EqualTo(161));}
    }
}
