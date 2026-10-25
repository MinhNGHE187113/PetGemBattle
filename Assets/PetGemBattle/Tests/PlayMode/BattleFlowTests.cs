using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PetGemBattle.Tests
{
    public class BattleFlowTests
    {
        private GameObject host;private BattleController battle;private BattleRules rules;
        [UnitySetUp] public IEnumerator SetUp()
        {
            foreach(var existing in UnityEngine.Object.FindObjectsByType<BattleController>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(existing.gameObject);
            yield return null;
            rules=ScriptableObject.CreateInstance<BattleRules>();rules.swapSeconds=rules.clearSeconds=rules.fallSeconds=rules.effectSeconds=rules.aiDelay=.01f;
            rules.normalAttackSeconds=rules.specialAttackSeconds=.01f;rules.evolutionSeconds=.08f;rules.turnSeconds=100;
            host=new GameObject("FlowTestBattle");battle=host.AddComponent<BattleController>();
            typeof(BattleController).GetField("rules",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(battle,rules);
            typeof(BattleController).GetField("seed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(battle,42);
            yield return null;Assert.That(battle.Phase,Is.EqualTo(TurnPhase.PlayerInput));
        }
        [UnityTearDown] public IEnumerator TearDown(){UnityEngine.Object.Destroy(host);UnityEngine.Object.Destroy(rules);yield return null;}
        private GemInput Input(Cell c)=>host.GetComponentsInChildren<GemInput>().Single(g=>g.Cell.Equals(c));
        private Button Card(CardKind card)=>host.GetComponentsInChildren<Button>().Single(b=>b.name=="Card_"+card);
        private void ClickMove(BoardMove move){Input(move.A).Click(move.A);Input(move.B).Click(move.B);}
        private IEnumerator Until(Func<bool> predicate)
        {
            float deadline=Time.realtimeSinceStartup+8;while(!predicate() && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(predicate(),Is.True,"Controller failed to finish transition.");
        }
        [UnityTest] public IEnumerator PauseFreezesTimerRejectsActionsAndRestartResumes()
        {
            battle.TogglePause();float time=battle.RemainingSeconds;var board=battle.Board.Snapshot();
            battle.UseCard(0);ClickMove(battle.Board.LegalMoves()[0]);yield return new WaitForSecondsRealtime(.15f);
            Assert.That(battle.IsPaused,Is.True);Assert.That(battle.RemainingSeconds,Is.EqualTo(time));
            CollectionAssert.AreEqual(board,battle.Board.Snapshot());Assert.That(battle.Player.Mana,Is.Zero);
            battle.RestartBattle();yield return null;Assert.That(battle.IsPaused,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator PauseDuringResolutionResumesWithoutDuplicatingEvents()
        {
            ClickMove(battle.Board.LegalMoves()[0]);battle.TogglePause();int entries=battle.Journal.entries.Count;
            yield return new WaitForSecondsRealtime(.15f);Assert.That(battle.Journal.entries.Count,Is.EqualTo(entries));
            battle.TogglePause();yield return Until(()=>battle.Turn>=3);
            Assert.That(battle.Journal.entries.Count(e=>e.command.StartsWith("Swap")),Is.EqualTo(2));
            Assert.That(battle.Journal.entries.Any(e=>e.rolls.Count>0),Is.True);
        }
        [UnityTest] public IEnumerator CanvasUsesFullHdReferenceAndJournalRoundTrips()
        {
            var scaler=host.GetComponentInChildren<CanvasScaler>();Assert.That(scaler.referenceResolution,Is.EqualTo(new Vector2(1920,1080)));
            battle.UseCard(0);yield return Until(()=>battle.Phase==TurnPhase.PlayerInput);
            var restored=JsonUtility.FromJson<BattleJournal>(JsonUtility.ToJson(battle.Journal));
            Assert.That(restored.seed,Is.EqualTo(42));Assert.That(restored.entries.Count,Is.EqualTo(2));
            Assert.That(restored.entries[1].stats[2],Is.EqualTo(20));
        }
        [UnityTest] public IEnumerator InvalidMoveReturnsToSameTurnWithoutResettingTimer()
        {
            BoardMove? invalid=null;
            for(int y=0;y<8 && !invalid.HasValue;y++)for(int x=0;x<7;x++)
            {
                var move=new BoardMove(new Cell(x,y),new Cell(x+1,y));if(battle.Board.TrySwap(move))battle.Board.Swap(move);else{invalid=move;break;}
            }
            Assert.That(invalid.HasValue,Is.True);var snapshot=battle.Board.Snapshot();float remaining=battle.RemainingSeconds;
            ClickMove(invalid.Value);Assert.That(battle.Phase,Is.EqualTo(TurnPhase.Resolving));yield return Until(()=>battle.Phase==TurnPhase.PlayerInput);
            CollectionAssert.AreEqual(snapshot,battle.Board.Snapshot());Assert.That(battle.Turn,Is.EqualTo(1));Assert.That(battle.RemainingSeconds,Is.LessThanOrEqualTo(remaining));
        }
        [UnityTest] public IEnumerator DragAndComboLockInputAndAIHandsBackControl()
        {
            var move=battle.Board.LegalMoves()[0];var gem=Input(move.A);Canvas.ForceUpdateCanvases();
            var start=RectTransformUtility.WorldToScreenPoint(null,gem.transform.position);var end=RectTransformUtility.WorldToScreenPoint(null,Input(move.B).transform.position);
            var pointer=new PointerEventData(EventSystem.current){pressPosition=start,position=end};
            gem.OnPointerDown(pointer);gem.OnBeginDrag(pointer);gem.OnEndDrag(pointer);Assert.That(battle.Phase,Is.EqualTo(TurnPhase.Resolving));
            var during=battle.Board.Snapshot();Input(new Cell(0,0)).Click(new Cell(0,0));Input(new Cell(1,0)).Click(new Cell(1,0));battle.UseCard(0);
            CollectionAssert.AreEqual(during,battle.Board.Snapshot());Assert.That(battle.Player.ManaCardsUsed,Is.Zero);
            yield return Until(()=>battle.Phase==TurnPhase.PlayerInput || battle.Phase==TurnPhase.Ended);
            Assert.That(battle.Phase,Is.EqualTo(TurnPhase.PlayerInput));Assert.That(battle.Turn,Is.EqualTo(3));
            Assert.That(battle.Board.FindMatches(),Is.Empty);Assert.That(battle.Board.LegalMoves(),Is.Not.Empty);Assert.That(battle.Enemy.ManaCardsUsed,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator RestartDuringSwapCancelsPreviousResolution()
        {
            ClickMove(battle.Board.LegalMoves()[0]);battle.RestartBattle();var snapshot=battle.Board.Snapshot();yield return new WaitForSeconds(.4f);
            Assert.That(battle.Phase,Is.EqualTo(TurnPhase.PlayerInput));CollectionAssert.AreEqual(snapshot,battle.Board.Snapshot());
            Assert.That(battle.Player.Hp,Is.EqualTo(300));Assert.That(battle.Enemy.Hp,Is.EqualTo(300));Assert.That(battle.Player.ManaCardsUsed,Is.Zero);
        }
        [UnityTest] public IEnumerator EvolutionCardChangesStatsAndSpriteAndLocksOtherCards()
        {
            Assert.That(Card(CardKind.Evolution).interactable,Is.False);Card(CardKind.Evolution).onClick.Invoke();Assert.That(battle.Player.EvolutionLevel,Is.EqualTo(1));
            battle.Player.AddMana(100,rules);battle.Player.TakeDamage(100);battle.UseCard(2);
            Assert.That(battle.Phase,Is.EqualTo(TurnPhase.Resolving));yield return Until(()=>battle.Phase==TurnPhase.PlayerInput);yield return null;
            Assert.That(battle.Player.MaxHp,Is.EqualTo(420));Assert.That(battle.Player.Hp,Is.EqualTo(320));Assert.That(battle.Player.Mana,Is.EqualTo(20));
            var pet=host.GetComponentsInChildren<PetPresentation>().Single(p=>p.name=="PlayerPet");Assert.That(pet.IsEvolved,Is.True);Assert.That(pet.Body.sprite,Is.Not.Null);
            Assert.That(Card(CardKind.Mana).interactable,Is.False);Assert.That(Card(CardKind.Evolution).interactable,Is.False);
            battle.UseCard(2);Assert.That(battle.Player.EvolutionLevel,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator RestartDuringEvolutionResetsFormResourcesAndCharges()
        {
            battle.Player.AddMana(100,rules);battle.UseCard(2);battle.RestartBattle();yield return new WaitForSeconds(.3f);
            Assert.That(battle.Player.MaxHp,Is.EqualTo(300));Assert.That(battle.Player.EvolutionLevel,Is.EqualTo(1));Assert.That(battle.Player.Mana,Is.Zero);
            Assert.That(host.GetComponentsInChildren<PetPresentation>().All(p=>!p.IsEvolved),Is.True);Assert.That(Card(CardKind.Mana).interactable,Is.True);
        }
        [UnityTest] public IEnumerator LethalComboStopsBeforeAnotherWaveOrEnemyTurn()
        {
            BoardMove? lethal=null;
            for(int attempts=0;attempts<10 && !lethal.HasValue;attempts++)
            {
                foreach(var move in battle.Board.LegalMoves())
                {
                    battle.Board.Swap(move);bool sword=battle.Board.FindMatches().Any(g=>g.Kind==GemKind.Sword);battle.Board.Swap(move);if(sword){lethal=move;break;}
                }
                if(!lethal.HasValue)battle.Board.GeneratePlayable();
            }
            Assert.That(lethal.HasValue,Is.True);battle.Enemy.TakeDamage(299);ClickMove(lethal.Value);
            yield return Until(()=>battle.Phase==TurnPhase.Ended);var finalBoard=battle.Board.Snapshot();yield return new WaitForSeconds(1);
            CollectionAssert.AreEqual(finalBoard,battle.Board.Snapshot());Assert.That(battle.Player.Hp,Is.EqualTo(300));Assert.That(battle.Turn,Is.EqualTo(1));
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="CHIẾN THẮNG"),Is.True);
            battle.RestartBattle();yield return null;Assert.That(battle.Enemy.Hp,Is.EqualTo(300));Assert.That(battle.Phase,Is.EqualTo(TurnPhase.PlayerInput));
        }
        [UnityTest] public IEnumerator ResourceCardsWorkThroughUiAndBarsConvergeToData()
        {
            var mana=Card(CardKind.Mana);Assert.That(mana.GetComponent<Image>().raycastTarget,Is.True);mana.onClick.Invoke();
            yield return Until(()=>battle.Phase==TurnPhase.PlayerInput);Assert.That(battle.Player.Mana,Is.EqualTo(20));Assert.That(battle.Player.ManaCardsUsed,Is.EqualTo(1));
            Assert.That(mana.interactable,Is.False);Card(CardKind.Rage).onClick.Invoke();Assert.That(battle.Player.Rage,Is.Zero);
            yield return new WaitForSeconds(.6f);
            var fill=host.transform.Find("BattleCanvas/PlayerHUD/MANATrack/MANAFill").GetComponent<RectTransform>();
            Assert.That(fill.sizeDelta.x,Is.EqualTo(312*.2f).Within(1));
        }
        [UnityTest] public IEnumerator TurnTimeoutAutomaticallyMakesALegalMove()
        {
            rules.turnSeconds=.1f;battle.RestartBattle();yield return Until(()=>battle.Turn>=2 || battle.Phase==TurnPhase.Ended);
            Assert.That(battle.Turn,Is.GreaterThanOrEqualTo(2));
        }
        [UnityTest] public IEnumerator AiUsesEvolutionCardWhenItHasEnoughMana()
        {
            battle.Enemy.AddMana(80,rules);ClickMove(battle.Board.LegalMoves()[0]);
            yield return Until(()=>battle.Turn>=3 || battle.Phase==TurnPhase.Ended);
            Assert.That(battle.Enemy.EvolutionLevel,Is.EqualTo(2));Assert.That(battle.Enemy.Attack,Is.EqualTo(33.75f));
        }
        [UnityTest] public IEnumerator RoundLimitWaitsForBothTurnsThenStopsAndRestartClearsRoundState()
        {
            rules.maxRounds=1;ClickMove(battle.Board.LegalMoves()[0]);
            Assert.That(battle.CompletedRounds,Is.Zero);
            yield return Until(()=>battle.Phase==TurnPhase.Ended);
            Assert.That(battle.CompletedRounds,Is.EqualTo(1));Assert.That(battle.Turn,Is.EqualTo(2));
            Assert.That(battle.MatchResult,Is.EqualTo(BattleCombat.ResultAtRoundLimit(battle.Player,battle.Enemy)));
            var snapshot=battle.Board.Snapshot();yield return new WaitForSeconds(1);
            CollectionAssert.AreEqual(snapshot,battle.Board.Snapshot());
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Hết 1 vòng")),Is.True);
            battle.RestartBattle();Assert.That(battle.CompletedRounds,Is.Zero);Assert.That(battle.MatchResult,Is.EqualTo(BattleResult.Ongoing));
        }
        [UnityTest] public IEnumerator SuddenDeathAnnouncesAfterCompletedRoundAndKeepsCardsWorking()
        {
            rules.suddenDeathAfterRounds=1;rules.maxRounds=3;
            ClickMove(battle.Board.LegalMoves()[0]);
            yield return Until(()=>host.GetComponentsInChildren<Text>().Any(t=>t.text=="SUDDEN DEATH"));
            Assert.That(battle.CompletedRounds,Is.EqualTo(1));Assert.That(battle.Phase,Is.EqualTo(TurnPhase.Resolving));
            yield return Until(()=>battle.Phase==TurnPhase.PlayerInput);
            Assert.That(battle.Turn,Is.EqualTo(3));Assert.That(Card(CardKind.Mana).interactable,Is.True);
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("SÁT THƯƠNG +10%")),Is.True);
        }
        [UnityTest] public IEnumerator CriticalSpecialPresentationMatchesReportAndRestartClearsEffects()
        {
            var report=new BattleReport{SpecialAttacks=1,CriticalAttacks=1,HpDamage=122,Damage=122};
            var view=host.GetComponent<BattleView>();view.StartCoroutine(view.AnimateEffect(true,report,1));yield return null;
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="CRITICAL!"),Is.True);
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="−122 HP"),Is.True);
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="ĐẶC BIỆT · CRITICAL!"),Is.True);
            Assert.That(host.GetComponentsInChildren<PetPresentation>().Single(p=>p.name=="PlayerPet").Pose,Is.EqualTo(PetPose.Special));
            battle.RestartBattle();yield return null;
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="CRITICAL!"),Is.False);
        }
    }
}
