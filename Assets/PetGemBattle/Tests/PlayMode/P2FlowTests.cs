using System;
using System.IO;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace PetGemBattle.Tests
{
    public class P2FlowTests
    {
        private P1Catalog catalog;private BattleRules rules;private GameObject host;private BattleController battle;private string temp;
        [UnitySetUp] public IEnumerator Setup()
        {
            foreach(var h in UnityEngine.Object.FindObjectsByType<HomeController>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(h.gameObject);
            foreach(var b in UnityEngine.Object.FindObjectsByType<BattleController>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(b.gameObject);
            yield return null;catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();P2Content.Upgrade(catalog);
            rules=ScriptableObject.CreateInstance<BattleRules>();rules.elements=catalog;rules.cinematicTurns=true;rules.finalMultiplierRounding=true;rules.specialManaCost=30;rules.attackMultiplier=2.3f;rules.swordExtraMultiplier=.4f;
            rules.player=catalog.Stats("metal-bird",1);rules.enemy=catalog.Stats("wood-1",1,true,1);rules.enemy.bossProfile.awakenSeconds=.12f;
            rules.swapSeconds=rules.clearSeconds=rules.fallSeconds=rules.effectSeconds=rules.aiDelay=.02f;rules.normalAttackSeconds=.15f;rules.specialAttackSeconds=.2f;rules.evolutionSeconds=.15f;rules.boardTransitionSeconds=.03f;rules.turnSeconds=100;
            host=new GameObject("P2Flow");battle=host.AddComponent<BattleController>();battle.Configure(rules);temp=Path.Combine(Path.GetTempPath(),"P2Flow-"+Guid.NewGuid().ToString("N"));yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(host!=null)UnityEngine.Object.Destroy(host);UnityEngine.Object.Destroy(rules);UnityEngine.Object.Destroy(catalog);Time.timeScale=1;yield return null;if(Directory.Exists(temp))Directory.Delete(temp,true);}
        private IEnumerator Until(Func<bool> predicate){float stop=Time.realtimeSinceStartup+20;while(!predicate() && Time.realtimeSinceStartup<stop)yield return null;Assert.That(predicate(),Is.True);}
        private BoardMove SwordMove()
        {
            for(int n=0;n<50;n++){foreach(var move in battle.Board.LegalMoves()){battle.Board.Swap(move);bool sword=battle.Board.FindMatches().Any(g=>g.Kind==GemKind.Sword);battle.Board.Swap(move);if(sword){host.GetComponent<BattleView>().DrawBoard(battle.Board);return move;}}battle.Board.GeneratePlayable();}
            throw new Exception("No sword move found.");
        }
        private void Move(BoardMove move){var gems=host.GetComponentsInChildren<GemInput>();gems.Single(g=>g.Cell.Equals(move.A)).Click(move.A);gems.Single(g=>g.Cell.Equals(move.B)).Click(move.B);}
        private IEnumerator HomeFixture()
        {
            UnityEngine.Object.Destroy(host);yield return null;host=new GameObject("P2Home");var home=host.AddComponent<HomeController>();home.catalog=catalog;home.SavePathOverride=Path.Combine(temp,"save.json");yield return null;
        }
        [UnityTest] public IEnumerator P2_01_02_03_LobbyChallengeAndWoodMap()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();Assert.That(home.CurrentScreen,Is.EqualTo("Home"));Assert.That(host.GetComponentsInChildren<GemInput>(),Is.Empty);
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="World_ĐẤU TRƯỜNG").onClick.Invoke();yield return null;
            Assert.That(home.CurrentScreen,Is.EqualTo("Challenges"));Assert.That(host.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Island_")),Is.EqualTo(5));
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="KHÁM PHÁ · 7 ẢI").onClick.Invoke();yield return null;Assert.That(home.CurrentScreen,Is.EqualTo("Island"));Assert.That(host.GetComponentsInChildren<RectTransform>().Count(r=>r.name=="StagePedestal"),Is.EqualTo(7));
        }
        [UnityTest] public IEnumerator P2_04_FourOtherIslandsArePreviewOnly()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();
            foreach(string id in new[]{"fire","metal","water","earth"})
            {
                home.Home();home.Navigate("Challenges");yield return null;
                host.GetComponentsInChildren<Button>().Single(b=>b.transform.parent.name=="Island_"+id && b.name=="XEM TRƯỚC").onClick.Invoke();yield return null;
                Assert.That(home.CurrentScreen,Is.EqualTo("IslandPreview"));var boss=host.GetComponentsInChildren<Button>().Single(b=>b.name=="BOSS / ẢI — Coming Soon");Assert.That(boss.interactable,Is.False);boss.onClick.Invoke();Assert.That(home.CurrentScreen,Is.EqualTo("IslandPreview"));
            }
        }
        [UnityTest] public IEnumerator P2_05_06_CollectionAndComingSoon()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();
            var future=new[]{"VÒNG QUAY","SỰ KIỆN","NẠP THẺ","ƯU ĐÃI ĐẦU"};
            foreach(string label in future)
            {
                var shortcut=host.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Shortcut_"+label);
                Assert.That(shortcut.GetComponent<Button>(),Is.Null);
            }
            var viewport=host.GetComponentsInChildren<ScrollRect>().Single(s=>s.name=="QuickShortcutViewport");
            Assert.That(viewport.content.rect.width,Is.GreaterThan(viewport.viewport.rect.width));
            Assert.That(home.CurrentScreen,Is.EqualTo("Home"));Assert.That(home.Progress.Data.pets.Count,Is.EqualTo(home.catalog.pets.Length));
            home.Navigate("Pets");yield return null;Assert.That(host.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Pet_")),Is.EqualTo(8));
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="TIẾP ›").onClick.Invoke();yield return null;Assert.That(host.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Pet_")),Is.EqualTo(Math.Min(8,home.catalog.pets.Length-8)));
        }
        [UnityTest] public IEnumerator P2_08_PermanentFormUsedInBattleAndHome()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();home.Progress.Owned("wood-7").level=7;home.Progress.Select("wood-7");home.Home();yield return null;
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Cổ Thụ Vương Miện")),Is.True);
            var companion=host.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Preview_wood-7");
            Assert.That(companion.GetComponent<P3PetRig>(),Is.Not.Null);
            Assert.That(companion.GetComponentsInChildren<Image>().Single(i=>i.name=="OriginalSprite").sprite.name,Is.EqualTo("p3-wood-7-form1"));
            home.StartBattle();yield return null;var session=UnityEngine.Object.FindFirstObjectByType<BattleController>();Assert.That(session.Player.Definition.artSheet,Is.EqualTo("p3-wood-7-form1"));Assert.That(session.Player.MaxHp,Is.EqualTo(887));home.Home();
        }
        [UnityTest] public IEnumerator P2_20_TransformationInsertedWithoutExtraTurn()
        {
            // Isolate the crossing hit: 154 -> 131 HP, without a lethal crit/cascade.
            rules.player.attack=9;rules.criticalChanceCap=0;rules.maxCascadeWaves=1;battle.RestartBattle();
            BoardMove? crossing=null;
            for(int attempt=0;attempt<100 && !crossing.HasValue;attempt++)
            {
                foreach(var move in battle.Board.LegalMoves())
                {
                    battle.Board.Swap(move);var groups=battle.Board.FindMatches();battle.Board.Swap(move);
                    if(groups.Count==1 && groups[0].Kind==GemKind.Sword && groups[0].Cells.Count==3){crossing=move;break;}
                }
                if(!crossing.HasValue)battle.Board.GeneratePlayable();
            }
            Assert.That(crossing.HasValue,Is.True);host.GetComponent<BattleView>().DrawBoard(battle.Board);
            battle.Enemy.TakeDamage(286);int turn=battle.Turn;Move(crossing.Value);
            yield return Until(()=>battle.Enemy.BossAwakened || battle.Phase==TurnPhase.Ended);
            Assert.That(battle.Enemy.BossAwakened,Is.True);Assert.That(battle.Turn,Is.EqualTo(turn));Assert.That(battle.Presentation,Is.EqualTo(PresentationPhase.Combat));
            Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("BOSS THỨC TỈNH")),Is.True);
        }
        [UnityTest] public IEnumerator P2_BossPreviewDoesNotUseOwnedPermanentForm()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();
            home.Progress.Owned("wood-2").level=4;home.Progress.Data.unlockedStage=2;home.Navigate("Island");yield return null;
            host.GetComponentsInChildren<Button>().Single(b=>b.name.StartsWith("2. Lá Lùn")).onClick.Invoke();yield return null;
            Assert.That(home.CurrentScreen,Is.EqualTo("Prepare"));
            var boss=host.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Preview_wood-2");
            Assert.That(boss.GetComponent<P3PetRig>(),Is.Not.Null);
            var sprite=boss.GetComponentsInChildren<Image>().Single(i=>i.name=="OriginalSprite").sprite;
            Assert.That(sprite,Is.Not.Null);Assert.That(sprite.name,Is.EqualTo("p3-wood-2"));
            Assert.That(home.Progress.Owned("wood-2").level,Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator P2_21_DeadBossNeverAwakens()
        {
            var move=SwordMove();battle.Enemy.TakeDamage(439);Move(move);yield return Until(()=>battle.Phase==TurnPhase.Ended);Assert.That(battle.Enemy.BossAwakened,Is.False);
            Assert.That(host.transform.Find("BattleCanvas/SharedBoard").GetComponent<CanvasGroup>().alpha,Is.Zero);
        }
        [UnityTest] public IEnumerator P2_23_24_CollectFirstAndHudAtImpact()
        {
            int hp=battle.Enemy.Hp;Move(SwordMove());yield return Until(()=>battle.Presentation==PresentationPhase.Collecting || battle.Presentation==PresentationPhase.BoardHiding);Assert.That(battle.Enemy.Hp,Is.EqualTo(hp));
            yield return Until(()=>battle.Presentation==PresentationPhase.Combat);Assert.That(host.transform.Find("BattleCanvas/SharedBoard").GetComponent<CanvasGroup>().alpha,Is.Zero);
            yield return Until(()=>battle.Enemy.Hp<hp);Assert.That(host.GetComponentsInChildren<Text>().Any(t=>t.text=="Thiếu Mana · Thiếu Nộ" || t.text.Contains("Đủ Mana")),Is.True);
        }
        [UnityTest] public IEnumerator P2_25_SpeedOnlyAffectsPresentationTimer()
        {
            battle.ToggleSpeed();float remaining=battle.RemainingSeconds;yield return new WaitForSecondsRealtime(.2f);Assert.That(remaining-battle.RemainingSeconds,Is.InRange(.1f,.36f));battle.ToggleSpeed();
        }
        [UnityTest] public IEnumerator P2_30_BoardTilesAreReusedAndResetCancelsEffects()
        {
            var ids=host.GetComponentsInChildren<GemInput>().Select(g=>g.GetInstanceID()).OrderBy(n=>n).ToArray();
            for(int i=0;i<15;i++){battle.Board.GeneratePlayable();host.GetComponent<BattleView>().DrawBoard(battle.Board);}
            Assert.That(host.GetComponentsInChildren<GemInput>().Select(g=>g.GetInstanceID()).OrderBy(n=>n),Is.EqualTo(ids));
            Move(SwordMove());yield return Until(()=>battle.Presentation==PresentationPhase.Combat);battle.RestartBattle();yield return new WaitForSeconds(.5f);Assert.That(battle.Enemy.Hp,Is.EqualTo(440));Assert.That(battle.Presentation,Is.EqualTo(PresentationPhase.Selecting));
        }
        [UnityTest] public IEnumerator P2_ResetRequiresConfirmationAndCancelKeepsItems()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();home.Progress.SeedDemoStones();int stones=home.Progress.Data.stones.Sum();home.Navigate("ConfirmReset");yield return null;
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="HỦY").onClick.Invoke();yield return null;Assert.That(home.Progress.Data.stones.Sum(),Is.EqualTo(stones));
            home.Navigate("ConfirmReset");yield return null;host.GetComponentsInChildren<Button>().Single(b=>b.name=="XÁC NHẬN").onClick.Invoke();yield return null;Assert.That(home.Progress.Data.stones.Sum(),Is.Zero);Assert.That(home.Progress.Data.pets.Count,Is.EqualTo(home.catalog.pets.Length));
        }
        [UnityTest] public IEnumerator P2_BestiaryLevelAndFormFiltersChangeResults()
        {
            yield return HomeFixture();var home=host.GetComponent<HomeController>();home.Progress.Owned("wood-7").level=7;home.Navigate("Book");yield return null;
            host.GetComponentsInChildren<Button>().Single(b=>b.name.StartsWith("Form:")).onClick.Invoke();yield return null;
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="Đã tiến hóa").onClick.Invoke();yield return null;
            Assert.That(host.GetComponentsInChildren<RectTransform>().Count(r=>r.name.StartsWith("Pet_")),Is.EqualTo(1));
            Assert.That(host.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Pet_wood-7"),Is.True);
            host.GetComponentsInChildren<Button>().Single(b=>b.name.StartsWith("Lv:")).onClick.Invoke();yield return null;
            host.GetComponentsInChildren<Button>().Single(b=>b.name=="1").onClick.Invoke();yield return null;
            Assert.That(host.GetComponentsInChildren<RectTransform>().Any(r=>r.name.StartsWith("Pet_")),Is.False);
        }
        [UnityTest] public IEnumerator P2_ArtworkHasIndependentPartsAndEvents()
        {
            Assert.That(host.GetComponentsInChildren<P3PetRig>().Length,Is.EqualTo(2));
            Assert.That(host.GetComponentsInChildren<P3SoftMotion>().Length,Is.EqualTo(2));
            var figure=host.GetComponentsInChildren<P3PetRig>().First().transform.Find("P3OriginalPet");
            Assert.That(figure,Is.Not.Null);var first=figure.localPosition;
            yield return new WaitForSeconds(.2f);Assert.That(figure.localPosition,Is.Not.EqualTo(first));
        }
    }
}
