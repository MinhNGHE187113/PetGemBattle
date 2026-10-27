using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PetGemBattle
{
    public enum TurnPhase { PlayerInput, Resolving, AIThinking, Ended }
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleRules rules;
        [Tooltip("0 gives a fresh seed; fixed seeds reproduce the board and independent drain rolls.")]
        [SerializeField] private int seed;
        public TurnPhase Phase { get; private set; }
        public MatchBoard Board { get; private set; }
        public PetState Player { get; private set; }
        public PetState Enemy { get; private set; }
        public float RemainingSeconds { get; private set; }
        public int Turn { get; private set; }
        public int CompletedRounds { get; private set; }
        public BattleResult MatchResult { get; private set; }
        public PresentationPhase Presentation {get;private set;}
        public bool IsPlayerTurn=>playerTurn;
        public Action<BattleResult> Finished;
        public Action ExitRequested,RestartRequested;
        public void Configure(BattleRules settings){rules=settings;}
        private BattleView view;
        private RecordingBattleRandom combatRandom;
        private IBattleRandom aiRandom;
        public BattleJournal Journal { get; private set; }
        public bool IsPaused { get; private set; }
        private float previousTimeScale=1;
        private Cell? selected;
        private bool playerTurn,ownsRules;
        private void Start()
        {
            if(rules==null){rules=ScriptableObject.CreateInstance<BattleRules>();ownsRules=true;}
            rules.Validate();
            view=gameObject.AddComponent<BattleView>();view.Build(rules.player,rules.enemy,ClickGem,AttemptSwap,UseCard,()=>{if(RestartRequested!=null)RestartRequested();else RestartBattle();},TogglePause);
            view.AddSessionControls(ExitRequested,ToggleSpeed);RestartBattle();
        }
        public void RestartBattle()
        {
            SetPaused(false);StopAllCoroutines();int battleSeed=seed==0 ? Environment.TickCount : seed;
            Board=new MatchBoard(battleSeed);combatRandom=new RecordingBattleRandom(battleSeed^0x51A7D);aiRandom=new SeededBattleRandom(battleSeed^0x3A91);
            Journal=new BattleJournal{seed=battleSeed,balance=JsonUtility.ToJson(rules)};
            Player=new PetState(rules.player);Enemy=new PetState(rules.enemy);
            selected=null;playerTurn=true;Presentation=PresentationPhase.Selecting;Turn=1;CompletedRounds=0;MatchResult=BattleResult.Ongoing;RemainingSeconds=rules.turnSeconds;Phase=TurnPhase.PlayerInput;
            Journal.Record("Start",Turn,Board,Player,Enemy);
            Player.BeginTurn();view.HideResult();view.HideRhythm(null);view.DrawBoard(Board);view.SetDetail("Ghép kiếm để tích Nộ. Dùng thẻ tiến hóa khi đủ Mana.");
            view.Say("Chọn một thẻ hoặc bắt đầu xếp ngọc.");Refresh();
            if(!playerTurn){Enemy.BeginTurn();Phase=TurnPhase.AIThinking;Refresh();StartCoroutine(AITurn());}
        }
        private void Update()
        {
            if(view==null || IsPaused)return;
            bool ticking=Phase==TurnPhase.PlayerInput || Phase==TurnPhase.AIThinking;
            if(ticking)RemainingSeconds=Mathf.Max(0,RemainingSeconds-Time.unscaledDeltaTime);
            view.Timer(RemainingSeconds,ticking);
            if(Phase==TurnPhase.PlayerInput && RemainingSeconds<=0)
            {
                if(Board.LegalMoves().Count==0){Board.ShufflePlayable();view.DrawBoard(Board);}
                view.Say("Hết giờ — tự động chọn một nước hợp lệ.");
                var move=BattleAI.ChooseMove(Board,Player,Enemy,rules,CompletedRounds,aiRandom);AttemptSwap(move.A,move.B);
            }
        }
        private void Refresh()=>view.Refresh(Player,Enemy,rules,!IsPaused && Phase==TurnPhase.PlayerInput,playerTurn,Turn,CompletedRounds);
        private void ClickGem(Cell cell)
        {
            if(IsPaused || Phase!=TurnPhase.PlayerInput)return;
            if(selected.HasValue && selected.Value.Equals(cell)){selected=null;view.Select(null);return;}
            if(selected.HasValue && MatchBoard.Adjacent(selected.Value,cell)){AttemptSwap(selected.Value,cell);return;}
            selected=cell;view.Select(cell);
        }
        private void AttemptSwap(Cell a,Cell b)
        {
            if(IsPaused || Phase!=TurnPhase.PlayerInput || !MatchBoard.Adjacent(a,b))return;
            Phase=TurnPhase.Resolving;selected=null;view.Select(null);Refresh();StartCoroutine(ResolveMove(new BoardMove(a,b)));
        }
        public void UseCard(int index)
        {
            if(IsPaused || Phase!=TurnPhase.PlayerInput || index<0 || index>3 || !CardSystem.CanUse(Player,(CardKind)index,rules))return;
            Phase=TurnPhase.Resolving;selected=null;view.Select(null);Refresh();StartCoroutine(PlayerCard((CardKind)index));
        }
        private IEnumerator PlayerCard(CardKind card)
        {
            yield return CastCard(Player,card);
            if(FinishIfOver())yield break;
            Phase=TurnPhase.PlayerInput;view.Say("Đã dùng thẻ. Hãy thực hiện nước xếp ngọc.",false);Refresh();
        }
        private IEnumerator CastCard(PetState actor,CardKind card)
        {
            float rhythm=1;string grade=null;
            if(card==CardKind.Signature && actor==Player && actor.Definition.rarity==PetRarity.SS)
                yield return LegendaryRhythm((value,label)=>{rhythm=value;grade=label;});
            var target=actor==Player ? Enemy : Player;
            var report=CardSystem.Use(actor,card,rules,target,rhythm,CompletedRounds);if(report==null)yield break;
            Journal.Record("UseCard "+card,Turn,Board,Player,Enemy,report);
            string title=card==CardKind.Mana ? "Tiếp Mana" : card==CardKind.Rage ? "Khơi Nộ" : card==CardKind.Signature ? actor.Definition.signatureName : "Tiến Hóa";
            if(grade!=null)view.HideRhythm(grade);
            view.Say((playerTurn ? "Bạn" : "Đối thủ")+" dùng "+title);view.SetDetail(report.ToString());Refresh();
            yield return view.AnimateEffect(playerTurn,report,report.Evolved ? rules.evolutionSeconds : rules.effectSeconds);
        }
        private IEnumerator LegendaryRhythm(Action<float,string> done)
        {
            const string arrows="↑↓←→";
            int length=4+Mathf.FloorToInt((float)aiRandom.Next()*3);
            var pattern=new int[length];var signs=new string[length];
            for(int i=0;i<length;i++){pattern[i]=Mathf.Clamp(Mathf.FloorToInt((float)aiRandom.Next()*4),0,3);signs[i]=arrows[pattern[i]].ToString();}
            string sequence=string.Join(" ",signs);
            view.ShowRhythm(Player.Definition.signatureName,sequence,0,false);
            float intro=0;while(intro<.7f){if(!IsPaused)intro+=Time.unscaledDeltaTime;yield return null;}
            float inputTime=0;int entered=0;bool wrong=false;
            while(inputTime<6f && entered<length && !wrong)
            {
                if(!IsPaused)
                {
                    inputTime+=Time.unscaledDeltaTime;
                    int pressed=ReadArrow();
                    if(pressed>=0){if(pressed!=pattern[entered])wrong=true;else entered++;view.ShowRhythm(Player.Definition.signatureName,sequence,entered,false);}
                }
                yield return null;
            }
            if(wrong || entered<length){done(.70f,"MISS · ×0,70");yield break;}
            float timing=0;const float target=1.17f;
            while(timing<1.8f)
            {
                if(!IsPaused)
                {
                    timing+=Time.unscaledDeltaTime;
                    view.ShowRhythm(Player.Definition.signatureName,sequence,entered,true,Mathf.Clamp01(timing/1.8f));
                    if(Keyboard.current!=null && Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        float offset=Mathf.Abs(timing-target);
                        if(offset<=.08f)done(1.50f,"PERFECT · ×1,50");
                        else if(offset<=.25f)done(1.15f,"GOOD · ×1,15");
                        else done(.70f,"MISS · ×0,70");
                        yield break;
                    }
                }
                yield return null;
            }
            done(.70f,"MISS · ×0,70");
        }
        private static int ReadArrow()
        {
            var keys=Keyboard.current;if(keys==null)return -1;
            if(keys.upArrowKey.wasPressedThisFrame)return 0;
            if(keys.downArrowKey.wasPressedThisFrame)return 1;
            if(keys.leftArrowKey.wasPressedThisFrame)return 2;
            if(keys.rightArrowKey.wasPressedThisFrame)return 3;
            return -1;
        }
        private IEnumerator ResolveMove(BoardMove move)
        {
            Journal.Record("Swap "+move.A.X+","+move.A.Y+" > "+move.B.X+","+move.B.Y,Turn,Board,Player,Enemy);
            bool valid=Board.TrySwap(move);yield return view.AnimateSwap(move,rules.swapSeconds);
            if(!valid)
            {
                yield return view.AnimateSwap(move,rules.swapSeconds,true);view.DrawBoard(Board);view.Say("Chưa tạo tổ hợp — thử nước khác.",false);
                Phase=TurnPhase.PlayerInput;Refresh();yield break;
            }
            view.DrawBoard(Board);int wave=1;var collected=new List<CollectedGroup>();Presentation=PresentationPhase.Collecting;
            while(true)
            {
                var groups=Board.FindMatches();if(groups.Count==0)break;
                yield return view.AnimateClear(groups,rules.clearSeconds);
                foreach(var group in groups)collected.Add(new CollectedGroup(group,wave));
                if(!rules.cinematicTurns)
                {
                    var actor=playerTurn ? Player : Enemy;var target=playerTurn ? Enemy : Player;combatRandom.Rolls.Clear();
                    var report=BattleCombat.ApplyMatches(actor,target,groups,wave,rules,combatRandom,CompletedRounds);
                    Journal.Record("Resolve "+wave,Turn,Board,Player,Enemy,report,combatRandom.Rolls);
                    view.SetDetail(report.ToString());Refresh();yield return view.AnimateEffect(playerTurn,report,rules.EffectDuration(report),wave);
                }
                else view.Say("THU NGỌC · Combo "+wave+" · "+collected.Count+" nhóm — chưa gây sát thương",false);
                var falls=Board.ClearAndRefill(groups);yield return view.AnimateFall(Board,falls,rules.fallSeconds);
                if(!rules.cinematicTurns && FinishIfOver())yield break;
                if(wave++>=Math.Max(1,rules.maxCascadeWaves))
                {
                    Board.GeneratePlayable();view.DrawBoard(Board);view.Say("Đã đạt giới hạn combo — tạo bàn mới.");break;
                }
            }
            if(rules.cinematicTurns)
            {
                view.Collected(collected);Presentation=PresentationPhase.Prepared;var actor=playerTurn ? Player : Enemy;var target=playerTurn ? Enemy : Player;
                combatRandom.Rolls.Clear();var plan=CombatTurnPlan.Prepare(actor,target,collected,rules,combatRandom,CompletedRounds);
                Journal.Record("Turn prepared",Turn,Board,Player,Enemy,null,combatRandom.Rolls);
                Presentation=PresentationPhase.BoardHiding;yield return view.SetCombatStage(true,rules.boardTransitionSeconds);
                Presentation=PresentationPhase.Combat;view.Say("TRÌNH DIỄN CHIẾN ĐẤU · "+plan.Count+" hiệu ứng",false);
                foreach(var step in plan)
                {
                    var report=step.Report;view.SetDetail(report.ToString());
                    float duration=report.BossTransformation ? target.Definition.bossProfile.awakenSeconds : rules.EffectDuration(report);
                    yield return view.AnimateImpact(report.BossTransformation ? !playerTurn : playerTurn,report,duration,()=>{step.Apply(actor,target);Refresh();Journal.Record("Impact",Turn,Board,Player,Enemy,report);});
                    if(FinishIfOver())yield break;
                }
                Presentation=PresentationPhase.BoardShowing;yield return view.SetCombatStage(false,rules.boardTransitionSeconds);
            }
            Presentation=PresentationPhase.Selecting;
            if(Turn%2==0)
            {
                CompletedRounds++;
                if(CompletedRounds>=rules.maxRounds)
                {
                    EndBattle(BattleCombat.ResultAtRoundLimit(Player,Enemy));
                    view.SetDetail("Hết "+rules.maxRounds+" vòng — xét tỷ lệ HP còn lại.");yield break;
                }
                if(CompletedRounds==rules.suddenDeathAfterRounds)
                {
                    view.Say("SUDDEN DEATH — sát thương sẽ tăng mỗi "+rules.suddenDeathStepRounds+" vòng!");
                    yield return view.AnnounceSuddenDeath();
                }
            }
            if(Board.LegalMoves().Count==0)
            {
                view.Say("Hết nước đi — đang xáo ngọc.");yield return new WaitForSeconds(rules.effectSeconds);Board.ShufflePlayable();view.DrawBoard(Board);
            }
            playerTurn=!playerTurn;Turn++;RemainingSeconds=rules.turnSeconds;
            if(playerTurn){Player.BeginTurn();Phase=TurnPhase.PlayerInput;view.Say("Lượt của bạn — chọn thẻ hoặc xếp ngọc.");Refresh();}
            else{Enemy.BeginTurn();Phase=TurnPhase.AIThinking;Refresh();StartCoroutine(AITurn());}
        }
        private IEnumerator AITurn()
        {
            view.Say("Đối thủ đang chọn nước đi…",false);yield return new WaitForSeconds(Mathf.Min(rules.aiDelay,rules.turnSeconds));
            var card=BattleAI.ChooseCard(Enemy,Player,rules);
            if(card.HasValue){Phase=TurnPhase.Resolving;yield return CastCard(Enemy,card.Value);if(FinishIfOver())yield break;}
            if(Board.LegalMoves().Count==0){Board.ShufflePlayable();view.DrawBoard(Board);}
            var move=BattleAI.ChooseMove(Board,Enemy,Player,rules,CompletedRounds,aiRandom);Phase=TurnPhase.Resolving;Refresh();yield return ResolveMove(move);
        }
        private bool FinishIfOver()
        {
            var result=BattleCombat.Result(Player,Enemy);if(rules.cinematicTurns && result==BattleResult.Draw)result=playerTurn ? BattleResult.Victory : BattleResult.Defeat;if(result==BattleResult.Ongoing)return false;
            EndBattle(result);return true;
        }
        private void EndBattle(BattleResult result)
        {
            Journal.Record("Ended "+result,Turn,Board,Player,Enemy);
            try { Journal.Save(); } catch(System.IO.IOException ex) { Debug.LogWarning("Could not save battle log: "+ex.Message); }
            MatchResult=result;Phase=TurnPhase.Ended;selected=null;Refresh();view.ShowResult(result);Presentation=PresentationPhase.Result;Finished?.Invoke(result);
        }
        public void ToggleSpeed(){float speed=(IsPaused ? previousTimeScale : Time.timeScale)>1 ? 1 : 2;if(IsPaused)previousTimeScale=speed;else Time.timeScale=speed;view.SetSpeedLabel(speed);}
        public void TogglePause(){if(Phase!=TurnPhase.Ended)SetPaused(!IsPaused);}
        private void SetPaused(bool paused)
        {
            if(paused==IsPaused)return;
            if(paused){previousTimeScale=Time.timeScale;Time.timeScale=0;}else Time.timeScale=previousTimeScale;
            IsPaused=paused;if(view!=null){view.SetPaused(paused);Refresh();}
        }
        private void OnDestroy(){Time.timeScale=1;if(ownsRules)Destroy(rules);}
    }
}
