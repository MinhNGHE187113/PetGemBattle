using System;
using System.Collections;
using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
namespace PetGemBattle.Tests
{
    public class P1FlowTests
    {
        private GameObject host;private BattleRules rules;private P1Catalog catalog;private BattleController battle;
        [UnitySetUp] public IEnumerator Setup()
        {
            foreach(var b in UnityEngine.Object.FindObjectsByType<BattleController>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(b.gameObject);
            foreach(var h in UnityEngine.Object.FindObjectsByType<HomeController>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(h.gameObject);
            yield return null;catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();rules=ScriptableObject.CreateInstance<BattleRules>();
            rules.elements=catalog;rules.cinematicTurns=true;rules.player=catalog.Stats("starter-metal",1);rules.enemy=catalog.Stats("wood-1",1);
            rules.swapSeconds=rules.clearSeconds=rules.fallSeconds=.02f;rules.normalAttackSeconds=rules.specialAttackSeconds=.12f;rules.effectSeconds=.02f;rules.evolutionSeconds=.12f;rules.aiDelay=.02f;rules.boardTransitionSeconds=.03f;rules.turnSeconds=100;
            host=new GameObject("P1Flow");battle=host.AddComponent<BattleController>();battle.Configure(rules);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(host!=null)UnityEngine.Object.Destroy(host);UnityEngine.Object.Destroy(rules);UnityEngine.Object.Destroy(catalog);Time.timeScale=1;yield return null;}
        private IEnumerator Until(Func<bool> test){float end=Time.realtimeSinceStartup+15;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(test(),Is.True);}
        private BoardMove SwordMove()
        {
            for(int i=0;i<100;i++){foreach(var m in battle.Board.LegalMoves()){battle.Board.Swap(m);bool found=battle.Board.FindMatches().Any(g=>g.Kind==GemKind.Sword);battle.Board.Swap(m);if(found){host.GetComponent<BattleView>().DrawBoard(battle.Board);return m;}}battle.Board.GeneratePlayable();}
            throw new Exception("No sword move");
        }
        private void Move(BoardMove m){var inputs=host.GetComponentsInChildren<GemInput>();inputs.Single(g=>g.Cell.Equals(m.A)).Click(m.A);inputs.Single(g=>g.Cell.Equals(m.B)).Click(m.B);}
        [UnityTest] public IEnumerator P1_T13_FasterEnemyStartsAndAlternates()
        {
            rules.enemy.speed=50;battle.RestartBattle();Assert.That(battle.IsPlayerTurn,Is.False);
            yield return Until(()=>battle.Turn==2);Assert.That(battle.IsPlayerTurn,Is.True);Assert.That(battle.CompletedRounds,Is.Zero);
        }
        [UnityTest] public IEnumerator P1_T14_CollectionHasNoDamageAndBoardHidesBeforeImpact()
        {
            int hp=battle.Enemy.Hp;Move(SwordMove());
            yield return Until(()=>battle.Presentation==PresentationPhase.Collecting || battle.Presentation==PresentationPhase.BoardHiding);
            Assert.That(battle.Enemy.Hp,Is.EqualTo(hp));
            yield return Until(()=>battle.Presentation==PresentationPhase.Combat);
            Assert.That(host.transform.Find("BattleCanvas/SharedBoard").GetComponent<CanvasGroup>().alpha,Is.Zero);
            yield return Until(()=>battle.Enemy.Hp<hp);Assert.That(battle.Presentation,Is.EqualTo(PresentationPhase.Combat));
        }
        [UnityTest] public IEnumerator P1_T18_LethalImpactNeverShowsBoardAgain()
        {
            var move=SwordMove();battle.Enemy.TakeDamage(battle.Enemy.Hp-1);Move(move);
            yield return Until(()=>battle.Phase==TurnPhase.Ended);Assert.That(battle.Presentation,Is.EqualTo(PresentationPhase.Result));
            Assert.That(host.transform.Find("BattleCanvas/SharedBoard").GetComponent<CanvasGroup>().alpha,Is.Zero);
        }
        [UnityTest] public IEnumerator P1_T20_SpeedDoesNotAccelerateDecisionTimer()
        {
            battle.ToggleSpeed();float timer=battle.RemainingSeconds;yield return new WaitForSecondsRealtime(.2f);
            Assert.That(timer-battle.RemainingSeconds,Is.InRange(.1f,.35f));Assert.That(Time.timeScale,Is.EqualTo(2));battle.ToggleSpeed();
        }
        [UnityTest] public IEnumerator P1_T22_UnloadDuringCombatResetsTimeAndNewSession()
        {
            Move(SwordMove());yield return Until(()=>battle.Presentation==PresentationPhase.Combat);battle.ToggleSpeed();UnityEngine.Object.Destroy(host);yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(1));host=new GameObject("NewSession");battle=host.AddComponent<BattleController>();battle.Configure(rules);yield return null;
            Assert.That(battle.Phase,Is.EqualTo(TurnPhase.PlayerInput));Assert.That(battle.Player.Hp,Is.EqualTo(260));
        }
        [UnityTest] public IEnumerator P1_T19_HomeNavigationAndComingSoonAreReal()
        {
            UnityEngine.Object.Destroy(host);yield return null;host=new GameObject("TestHome");var home=host.AddComponent<HomeController>();home.catalog=catalog;
            home.SavePathOverride=Path.Combine(Path.GetTempPath(),"P1Home-"+Guid.NewGuid().ToString("N"),"save.json");yield return null;
            var soon=host.GetComponentsInChildren<Button>().Where(b=>b.name.Contains("Coming Soon")).ToArray();Assert.That(soon.Length,Is.EqualTo(5));
            foreach(var b in soon){Assert.That(b.interactable,Is.False);b.onClick.Invoke();}Assert.That(home.CurrentScreen,Is.EqualTo("Home"));
            home.Navigate("Island");yield return null;Assert.That(host.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("KHÓA")),Is.EqualTo(6));
            home.Navigate("Bag");yield return null;Assert.That(host.GetComponentsInChildren<Image>().Count(i=>i.name=="Icon"),Is.EqualTo(7));
            home.Home();yield return null;Assert.That(home.CurrentScreen,Is.EqualTo("Home"));
        }
        [UnityTest] public IEnumerator P1_RigsHaveIndependentMovingParts()
        {
            var pet=host.GetComponentsInChildren<ArticulatedPet>().First();var ear=pet.GetComponentsInChildren<RectTransform>().First(r=>r.name=="EarL");var rotation=ear.localRotation;
            yield return new WaitForSeconds(.2f);Assert.That(ear.localRotation,Is.Not.EqualTo(rotation));pet.SetPose(PetPose.Evolution,.1f);yield return new WaitForSeconds(.15f);
            Assert.That(pet.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="EvolvedCrown"),Is.True);
        }
        [UnityTest] public IEnumerator P1_FullHomeBattleRewardAndReload()
        {
            UnityEngine.Object.Destroy(host);yield return null;host=new GameObject("IntegrationHome");var home=host.AddComponent<HomeController>();home.catalog=catalog;
            string path=Path.Combine(Path.GetTempPath(),"P1Integration-"+Guid.NewGuid().ToString("N"),"save.json");home.SavePathOverride=path;yield return null;
            home.StartBattle();yield return null;var session=UnityEngine.Object.FindFirstObjectByType<BattleController>();
            var settings=(BattleRules)typeof(BattleController).GetField("rules",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(session);
            settings.swapSeconds=settings.clearSeconds=settings.fallSeconds=settings.effectSeconds=settings.normalAttackSeconds=settings.specialAttackSeconds=settings.boardTransitionSeconds=.01f;
            settings.aiDelay=.01f;session.Enemy.TakeDamage(session.Enemy.Hp-1);
            BoardMove? found=null;
            for(int tries=0;tries<50 && !found.HasValue;tries++)
            {
                foreach(var move in session.Board.LegalMoves()){session.Board.Swap(move);bool sword=session.Board.FindMatches().Any(g=>g.Kind==GemKind.Sword);session.Board.Swap(move);if(sword){found=move;break;}}
                if(!found.HasValue)session.Board.GeneratePlayable();
            }
            Assert.That(found.HasValue,Is.True);session.gameObject.GetComponent<BattleView>().DrawBoard(session.Board);
            var inputs=session.GetComponentsInChildren<GemInput>();inputs.Single(g=>g.Cell.Equals(found.Value.A)).Click(found.Value.A);inputs.Single(g=>g.Cell.Equals(found.Value.B)).Click(found.Value.B);
            yield return Until(()=>home.CurrentScreen=="Result");
            Assert.That(home.Progress.Owned("wood-1"),Is.Not.Null);Assert.That(home.Progress.StoneCount(Element.Wood,1),Is.GreaterThanOrEqualTo(1));Assert.That(home.Progress.Data.unlockedStage,Is.EqualTo(2));
            var restored=new ProgressionService(catalog,new SaveGameService(path,catalog));Assert.That(restored.Data.pets.Count,Is.EqualTo(2));Assert.That(restored.Data.unlockedStage,Is.EqualTo(2));
            home.Home();yield return null;Assert.That(UnityEngine.Object.FindFirstObjectByType<BattleController>(),Is.Null);
            Directory.Delete(Path.GetDirectoryName(path),true);
        }
        [UnityTest] public IEnumerator P1_AnimatorEmitsImpactAndCompletionFrames()
        {
            var rig=host.GetComponentsInChildren<ArticulatedPet>().First();Assert.That(rig.GetComponent<Animator>(),Is.Not.Null);
            bool impact=false,complete=false;rig.Events.Impact+=()=>impact=true;rig.Events.Completed+=()=>complete=true;
            rig.SetPose(PetPose.Special,.25f);yield return new WaitForSeconds(.13f);
            Assert.That(impact,Is.True);Assert.That(complete,Is.False);
            yield return new WaitForSeconds(.2f);Assert.That(complete,Is.True);
        }
    }
}
