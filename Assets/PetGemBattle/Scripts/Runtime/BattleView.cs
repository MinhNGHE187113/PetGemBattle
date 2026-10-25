using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace PetGemBattle
{
    public sealed class BattleView : MonoBehaviour
    {
        private const float Pitch=61f;
        private static readonly Color Gold=new Color(.88f,.69f,.32f),Ink=new Color(.025f,.07f,.105f,.96f);
        private RectTransform root,boardRoot,effectRoot;
        private readonly RectTransform[,] tiles=new RectTransform[8,8];
        private Text status,detail,history,turnLabel,timerLabel,comboLabel,roundStatus,resultDetails;
        private PetPanel playerPanel,enemyPanel;
        private readonly Button[] cards=new Button[3];
        private readonly Text[] cardDetails=new Text[3];
        private GameObject resultPanel,pausePanel;
        private readonly List<CanvasGroup> boardGroups=new List<CanvasGroup>();
        private Text speedLabel,collectionLabel;
        private bool activePlayer;
        private P1Catalog elementTable;
        private bool combatStage;
        private Text resultText;
        private PlaceholderArt art;
        private BattleAudio audioFx;
        private Font font;
        private Action<Cell> click;
        private Action<Cell,Cell> drag;
        private readonly Queue<string> messages=new Queue<string>();
        private readonly List<BarState> bars=new List<BarState>();
        private readonly List<Floating> floating=new List<Floating>();
        private readonly Queue<Image> vfxPool=new Queue<Image>();
        private readonly List<Image> activeVfx=new List<Image>();
        private readonly List<Button> sessionButtons=new List<Button>();
        private readonly List<GameObject> transient=new List<GameObject>();
        private sealed class BarState { public Image Fill;public Text Text;public float Width,Target,Current; }
        private sealed class PetPanel
        {
            public Text Stats,Form,Special;
            public PetDefinition Definition;
            public BarState Hp,Mana,Rage,Shield;
            public PetPresentation Pet;
            public float X;
        }
        private sealed class Floating { public Text Text;public Vector2 Start;public float Age; }
        public void Build(PetDefinition player,PetDefinition enemy,Action<Cell> onClick,Action<Cell,Cell> onDrag,Action<int> onCard,Action restart,Action pause=null)
        {
            art=new PlaceholderArt();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");audioFx=gameObject.AddComponent<BattleAudio>();
            click=onClick;drag=onDrag;
            var canvasGO=new GameObject("BattleCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform,false);canvasGO.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;root=canvasGO.GetComponent<RectTransform>();
            if(FindFirstObjectByType<EventSystem>()==null)
            {
                var events=new GameObject("BattleEventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(transform,false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var bg=Panel(root,"TempleArena",Vector2.zero,new Vector2(1600,900),Color.white);bg.sprite=art.Background;Stretch(bg.rectTransform);
            var shade=Panel(root,"Atmosphere",Vector2.zero,new Vector2(4000,4000),new Color(.015f,.035f,.065f,.48f));shade.raycastTarget=false;
            Panel(root,"TopVignette",new Vector2(0,397),new Vector2(4000,115),new Color(.015f,.04f,.065f,.7f));
            Panel(root,"BottomVignette",new Vector2(0,-370),new Vector2(4000,160),new Color(.015f,.04f,.065f,.8f));
            // Pet art is below the board and HUD in the canvas draw order.
            playerPanel=BuildPet(player,true);enemyPanel=BuildPet(enemy,false);
            BuildBoardFrame();
            BuildHud(playerPanel,player,-425);BuildHud(enemyPanel,enemy,425);
            var clock=Panel(root,"TurnClock",new Vector2(0,373),new Vector2(122,122),Ink);clock.sprite=art.Diamond;
            var clockRing=Panel(root,"ClockRunes",new Vector2(0,373),new Vector2(124,124),Gold);clockRing.sprite=art.Ring;
            timerLabel=Label(root,"30",new Vector2(0,380),new Vector2(100,65),43,new Color(1,.88f,.51f));
            Label(root,"GIÂY",new Vector2(0,344),new Vector2(100,24),12,Gold);
            turnLabel=Label(root,"LƯỢT CỦA BẠN",new Vector2(0,304),new Vector2(510,30),18,Color.white);
            var cardDock=Border(root,"CardDock",new Vector2(0,-348),new Vector2(536,144),Ink,Gold);
            Label(cardDock,"THẺ CHIẾN THUẬT  ·  TỐI ĐA 1 THẺ / LƯỢT",new Vector2(0,59),new Vector2(510,24),12,Gold);
            string[] titles={"TIẾP MANA","KHƠI NỘ","TIẾN HÓA"};int[] iconKinds={(int)GemKind.Mana,(int)GemKind.Rage,(int)GemKind.Shield};
            for(int i=0;i<3;i++)
            {
                int index=i;Color tint=PlaceholderArt.Colors[iconKinds[i]];
                var card=Border(cardDock,"Card_"+(CardKind)i,new Vector2((i-1)*171,-10),new Vector2(158,107),new Color(.045f,.10f,.16f),tint);
                cards[i]=card.gameObject.AddComponent<Button>();cards[i].targetGraphic=card.GetComponent<Image>();
                card.GetComponent<Image>().raycastTarget=true;
                var colors=cards[i].colors;colors.highlightedColor=new Color(1,.9f,.65f);colors.disabledColor=new Color(.25f,.28f,.33f);cards[i].colors=colors;
                var nav=cards[i].navigation;nav.mode=Navigation.Mode.None;cards[i].navigation=nav;cards[i].onClick.AddListener(()=>onCard(index));
                var icon=Panel(card,"CardIcon",new Vector2(-48,22),new Vector2(30,30),tint);icon.sprite=i==2 ? art.Diamond : art.Icons[iconKinds[i]];
                Label(card,titles[i],new Vector2(12,24),new Vector2(115,26),15,new Color(1,.93f,.73f));
                cardDetails[i]=Label(card,"",new Vector2(0,-17),new Vector2(150,53),14,new Color(.79f,.88f,.95f));
            }
            status=Label(root,"",new Vector2(0,-253),new Vector2(700,29),16,new Color(1,.92f,.65f));
            detail=Label(root,"",new Vector2(550,-336),new Vector2(420,50),15,new Color(.85f,.94f,1));
            history=Label(root,"",new Vector2(550,-384),new Vector2(420,47),13,new Color(.66f,.8f,.87f));
            Label(root,"Kéo ngọc hoặc chọn 2 ô liền kề.\nGhép kiếm để đánh và tích Nộ.\nTuyệt kỹ: 60 Nộ + 30 Mana.",new Vector2(-550,-353),new Vector2(410,72),16,new Color(.8f,.88f,.92f));
            Label(root,"PET ARENA  /  CLASSIC",new Vector2(-599,-427),new Vector2(340,26),14,Gold);
            roundStatus=Label(root,"ĐẤU TRƯỜNG CỔ  ·  PvE",new Vector2(0,-431),new Vector2(530,24),12,new Color(.59f,.73f,.77f));
            MakeButton(root,"ÂM THANH",new Vector2(382,-426),new Vector2(122,32),()=>audioFx.ToggleMute());
            MakeButton(root,"TẠM DỪNG",new Vector2(512,-426),new Vector2(122,32),()=>pause?.Invoke());
            MakeButton(root,"CHƠI LẠI",new Vector2(669,-426),new Vector2(170,32),restart);
            effectRoot=NewRect(root,"BattleEffects",Vector2.zero,new Vector2(1600,900));
            comboLabel=Label(effectRoot,"",new Vector2(0,40),new Vector2(700,100),48,new Color(1,.84f,.3f));comboLabel.gameObject.SetActive(false);
            var overlay=Panel(root,"ResultOverlay",Vector2.zero,new Vector2(4000,4000),new Color(.015f,.03f,.06f,.87f));resultPanel=overlay.gameObject;
            overlay.raycastTarget=true;
            var resultBox=Border(overlay.rectTransform,"ResultFrame",Vector2.zero,new Vector2(670,300),Ink,Gold);
            resultText=Label(resultBox,"",new Vector2(0,65),new Vector2(620,75),48,new Color(1,.84f,.45f));
            resultDetails=Label(resultBox,"",new Vector2(0,0),new Vector2(620,60),20,Color.white);
            MakeButton(resultBox,"TÁI ĐẤU",new Vector2(0,-85),new Vector2(230,52),restart);resultPanel.SetActive(false);
            var pauseOverlay=Panel(root,"PauseOverlay",Vector2.zero,new Vector2(4000,4000),new Color(.015f,.03f,.06f,.92f));
            pausePanel=pauseOverlay.gameObject;pauseOverlay.raycastTarget=true;
            Label(pauseOverlay.rectTransform,"TẠM DỪNG",new Vector2(0,65),new Vector2(620,75),48,Gold);
            Label(pauseOverlay.rectTransform,"Đồng hồ, AI và hoạt ảnh đã dừng.",Vector2.zero,new Vector2(620,50),22,Color.white);
            MakeButton(pauseOverlay.rectTransform,"TIẾP TỤC",new Vector2(0,-85),new Vector2(230,52),()=>pause?.Invoke());pausePanel.SetActive(false);
            // Keep the authored composition while using the specification's 1920×1080 canvas.
            foreach(RectTransform child in root)
                if(child.anchorMin==child.anchorMax){child.anchoredPosition*=1.2f;child.localScale*=1.2f;}
        }
        public void SetPaused(bool paused){pausePanel.SetActive(paused);audioFx.SetPaused(paused);}
        public void AddSessionControls(Action exit,Action speed)
        {
            collectionLabel=Label(root,"",new Vector2(0,300),new Vector2(1200,55),23,Gold);collectionLabel.gameObject.SetActive(false);
            var button=MakeButton(root,"x1",new Vector2(-340,-515),new Vector2(90,32),speed);speedLabel=button.GetComponentInChildren<Text>();
            if(exit!=null)MakeButton(root,"VỀ ĐẢO",new Vector2(-520,-515),new Vector2(170,32),exit);
            foreach(RectTransform child in root)
                if(child.name=="BoardShadow" || child.name=="FantasyBoardFrame" || child.name=="BoardInlay" || child.name=="FrameGem" || child.name=="SharedBoard" || child.name=="CardDock")boardGroups.Add(child.gameObject.AddComponent<CanvasGroup>());
        }
        public void SetSpeedLabel(float speed){if(speedLabel!=null)speedLabel.text="x"+speed;}
        public IEnumerator SetCombatStage(bool hidden,float seconds)
        {
            combatStage=hidden;collectionLabel.gameObject.SetActive(hidden);
            foreach(var group in boardGroups)group.blocksRaycasts=false;
            float from=hidden ? 1 : 0,to=hidden ? 0 : 1;
            for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.01f,seconds))
            {
                float f=Mathf.SmoothStep(0,1,t),amount=Mathf.Lerp(1-from,1-to,f);
                foreach(var group in boardGroups)group.alpha=Mathf.Lerp(from,to,f);
                playerPanel.Pet.transform.localPosition=new Vector3(Mathf.Lerp(-660,-350,amount),-18,0);
                enemyPanel.Pet.transform.localPosition=new Vector3(Mathf.Lerp(660,350,amount),-18,0);
                playerPanel.Pet.transform.localScale=enemyPanel.Pet.transform.localScale=Vector3.one*Mathf.Lerp(1.2f,1.5f,amount);
                yield return null;
            }
            foreach(var group in boardGroups){group.alpha=to;group.blocksRaycasts=!hidden;}
            playerPanel.Pet.transform.localPosition=new Vector3(hidden ? -350 : -660,-18,0);enemyPanel.Pet.transform.localPosition=new Vector3(hidden ? 350 : 660,-18,0);
        }
        public IEnumerator AnimateImpact(bool playerActor,BattleReport report,float seconds,Action impact)
        {
            bool attack=report.NormalAttacks+report.SpecialAttacks>0;var actor=playerActor ? playerPanel : enemyPanel;var target=playerActor ? enemyPanel : playerPanel;
            if(attack)
            {
                bool impactSeen=false;var frameEvents=actor.Pet.AnimationEvents;Action signal=()=>impactSeen=true;if(frameEvents!=null)frameEvents.Impact+=signal;
                actor.Pet.SetPose(report.SpecialAttacks>0 ? PetPose.Special : PetPose.Attack,seconds);
                float from=actor.X*.53f,to=target.X*.53f;
                var bolt=EffectPanel("WindupProjectile",new Vector2(from,0),new Vector2(100,70),P1Catalog.ElementColor(actor.Definition.element));bolt.sprite=art.Glow;transient.Add(bolt.gameObject);
                var signature=P2ArtLibrary.Get(actor.Definition.artSheet,3);
                if(signature!=null){bolt.sprite=signature;bolt.color=Color.white;bolt.preserveAspect=true;bolt.rectTransform.sizeDelta=new Vector2(report.SpecialAttacks>0 ? 175 : 100,report.SpecialAttacks>0 ? 150 : 85);}
                float flight=0;while(flight<seconds*.4f || (frameEvents!=null && !impactSeen && flight<seconds*.65f)){float t=Mathf.Clamp01(flight/Mathf.Max(.01f,seconds*.4f));bolt.rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(from,to,t),Mathf.Sin(t*Mathf.PI)*85);flight+=Time.deltaTime;yield return null;}
                if(frameEvents!=null)frameEvents.Impact-=signal;
                ReleaseEffect(bolt);
                if(actor.Definition.rig==6 && report.SpecialAttacks>0)
                {
                    for(int i=0;i<7;i++){var vine=EffectPanel("BindingRoot",new Vector2(to+(i-3)*28,-45+i%2*25),new Vector2(32,180),new Color(.3f,.9f,.4f));vine.sprite=P1Art.Leaf;transient.Add(vine.gameObject);StartCoroutine(ReleaseLater(vine,.5f));}
                    Float("RỪNG TRÓI BUỘC",to,185,new Color(.5f,1,.6f),29);
                }
            }
            impact();yield return AnimateEffect(playerActor,report,attack ? seconds*.6f : seconds);
        }
        private PetPanel BuildPet(PetDefinition pet,bool left)
        {
            float x=left ? -550 : 550;var petRoot=NewRect(root,left ? "PlayerPet" : "EnemyPet",new Vector2(x,-15),new Vector2(450,500));
            var presentation=petRoot.gameObject.AddComponent<PetPresentation>();presentation.Build(art,pet,left);
            var plaque=Border(root,left ? "PlayerNameplate" : "EnemyNameplate",new Vector2(x,-278),new Vector2(420,58),Ink,Gold);
            Label(plaque,pet.name,new Vector2(0,12),new Vector2(400,28),22,new Color(1,.9f,.65f));
            var p=new PetPanel {Pet=presentation,X=x,Definition=pet};
            p.Form=Label(plaque,"DẠNG I",new Vector2(0,-14),new Vector2(400,23),12,new Color(.68f,.84f,.9f));
            p.Special=Label(root,"",new Vector2(x,218),new Vector2(440,30),14,new Color(.7f,.9f,1));
            p.Stats=Label(root,"",new Vector2(x,247),new Vector2(420,33),16,new Color(.88f,.94f,1));
            return p;
        }
        private void BuildHud(PetPanel p,PetDefinition definition,float x)
        {
            var hud=Border(root,x<0 ? "PlayerHUD" : "EnemyHUD",new Vector2(x,373),new Vector2(670,126),Ink,Gold);
            Label(hud,(x<0 ? "BẠN  /  " : "ĐỐI THỦ  /  ")+definition.name,new Vector2(0,44),new Vector2(640,30),19,new Color(1,.92f,.75f));
            p.Hp=Bar(hud,"HP",new Vector2(0,12),640,23,PlaceholderArt.Colors[(int)GemKind.Heart]);
            p.Mana=Bar(hud,"MANA",new Vector2(-164,-18),312,16,PlaceholderArt.Colors[(int)GemKind.Mana]);
            p.Rage=Bar(hud,"NỘ",new Vector2(164,-18),312,16,PlaceholderArt.Colors[(int)GemKind.Rage]);
            p.Shield=Bar(hud,"GIÁP",new Vector2(0,-44),640,13,PlaceholderArt.Colors[(int)GemKind.Shield]);
        }
        private void BuildBoardFrame()
        {
            Panel(root,"BoardShadow",new Vector2(0,17),new Vector2(550,550),new Color(0,0,0,.5f));
            Border(root,"FantasyBoardFrame",new Vector2(0,25),new Vector2(536,536),new Color(.13f,.18f,.20f),Gold);
            Border(root,"BoardInlay",new Vector2(0,25),new Vector2(520,520),new Color(.025f,.055f,.085f),new Color(.44f,.30f,.12f));
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
            {
                var corner=Panel(root,"FrameGem",new Vector2(x*263,25+y*263),new Vector2(25,25),Gold);corner.sprite=art.Diamond;
                var jewel=Panel(corner.rectTransform,"Jade",Vector2.zero,new Vector2(12,12),new Color(.1f,.8f,.72f));jewel.sprite=art.Diamond;
            }
            boardRoot=Panel(root,"SharedBoard",new Vector2(0,25),new Vector2(494,494),new Color(.018f,.04f,.07f)).rectTransform;
            var mask=NewRect(boardRoot,"BoardMask",Vector2.zero,new Vector2(494,494));mask.gameObject.AddComponent<RectMask2D>();boardRoot=mask;
        }
        private BarState Bar(RectTransform parent,string name,Vector2 at,float width,float height,Color tint)
        {
            var bg=Panel(parent,name+"Track",at,new Vector2(width+2,height+2),new Color(.005f,.015f,.025f));
            var fill=Panel(bg.rectTransform,name+"Fill",Vector2.zero,new Vector2(width,height),tint);
            fill.rectTransform.anchorMin=fill.rectTransform.anchorMax=new Vector2(0,.5f);fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchoredPosition=new Vector2(1,0);
            var gloss=Panel(fill.rectTransform,"Gloss",Vector2.zero,new Vector2(width,height*.3f),new Color(1,1,1,.13f));
            gloss.rectTransform.anchorMin=new Vector2(0,.6f);gloss.rectTransform.anchorMax=new Vector2(1,1);gloss.rectTransform.offsetMin=gloss.rectTransform.offsetMax=Vector2.zero;
            var state=new BarState {Fill=fill,Width=width,Text=Label(bg.rectTransform,"",Vector2.zero,new Vector2(width,height+9),height>20 ? 16 : 12,Color.white)};
            bars.Add(state);return state;
        }
        private static void SetBar(BarState bar,string title,int value,int maximum)
        {
            bar.Target=maximum>0 ? value/(float)maximum : 0;bar.Text.text=title+"  "+value+" / "+maximum;
        }
        public void Refresh(PetState player,PetState enemy,BattleRules rules,bool canAct,bool playerTurn,int turn,int completedRounds=0)
        {
            activePlayer=playerTurn;elementTable=rules.elements;RefreshPet(playerPanel,player,rules);RefreshPet(enemyPanel,enemy,rules);
            if(rules.specialManaCost>0)foreach(var button in sessionButtons)button.interactable=canAct;
            for(int i=0;i<3;i++)cards[i].interactable=canAct && CardSystem.CanUse(player,(CardKind)i,rules);
            cardDetails[0].text="+"+rules.manaCardGain+" Mana\nCòn "+Math.Max(0,rules.manaCardUses-player.ManaCardsUsed)+" / "+rules.manaCardUses;
            cardDetails[1].text="+"+rules.rageCardGain+" Nộ\nCòn "+Math.Max(0,rules.rageCardUses-player.RageCardsUsed)+" / "+rules.rageCardUses;
            cardDetails[2].text=player.EvolutionLevel>1 ? "ĐÃ TIẾN HÓA\nDạng II" : rules.evolutionManaCost+" Mana\nMột lần / trận";
            for(int i=0;i<3;i++)
            {
                string reason=!canAct ? "Chờ lượt / hiệu ứng" : player.SkillUsed ? "Đã dùng thẻ lượt này" :
                    i==2 && player.EvolutionLevel>1 ? "Đã tiến hóa" : i==2 && player.Mana<rules.evolutionManaCost ? "Thiếu "+(rules.evolutionManaCost-player.Mana)+" Mana" :
                    !cards[i].interactable ? "Đã đầy / hết lượt dùng" : "";
                if(reason.Length>0)cardDetails[i].text=cardDetails[i].text.Split('\n')[0]+"\n"+reason;
            }
            turnLabel.text=(playerTurn ? "LƯỢT CỦA BẠN" : "LƯỢT ĐỐI THỦ")+"  /  VÒNG "+Math.Min(completedRounds+1,rules.maxRounds)+" / "+rules.maxRounds;
            turnLabel.color=playerTurn ? new Color(1,.9f,.52f) : new Color(.5f,.84f,1);
            roundStatus.text=completedRounds>=rules.suddenDeathAfterRounds ? "SUDDEN DEATH  ·  SÁT THƯƠNG +"+BattleMath.Round((rules.SuddenDeathMultiplier(completedRounds)-1)*100)+"%" : "SUDDEN DEATH SAU VÒNG "+rules.suddenDeathAfterRounds+"  ·  GIỚI HẠN "+rules.maxRounds+" VÒNG";
            roundStatus.color=completedRounds>=rules.suddenDeathAfterRounds ? new Color(1,.55f,.2f) : Gold;
            resultDetails.text=(completedRounds>=rules.maxRounds ? "Hết "+rules.maxRounds+" vòng — xét tỷ lệ HP\n" : "HP còn lại\n")+
                "Bạn "+(100f*player.Hp/player.MaxHp).ToString("0.##")+"%  /  Đối thủ "+(100f*enemy.Hp/enemy.MaxHp).ToString("0.##")+"%";
        }
        private void RefreshPet(PetPanel p,PetState pet,BattleRules rules)
        {
            SetBar(p.Hp,"HP",pet.Hp,pet.MaxHp);SetBar(p.Mana,"MANA",pet.Mana,rules.maxMana);
            SetBar(p.Rage,"NỘ",pet.Rage,rules.maxRage);SetBar(p.Shield,"GIÁP",pet.Shield,rules.ShieldCap(pet));
            p.Stats.text="ATK "+pet.Attack.ToString("0.##")+" / DEF "+pet.Defense.ToString("0.##")+" / SPD "+pet.Definition.speed;
            p.Special.text=rules.specialManaCost>0 ? ((pet.Mana>=rules.specialManaCost ? "Đủ Mana" : "Thiếu Mana")+" · "+(pet.Rage>=rules.specialRageCost ? "Đủ Nộ" : "Thiếu Nộ")) : "";
            p.Form.text=(pet.BossAwakened ? "BOSS THỨC TỈNH" : pet.EvolutionLevel==1 ? "DẠNG I" : "DẠNG II  ·  ĐÃ THỨC TỈNH")+"   /   "+P1Catalog.ElementName(pet.Definition.element);
            if(!string.IsNullOrEmpty(pet.Definition.artSheet))p.Form.text="FORM "+pet.Definition.permanentForm+" · "+(pet.BossAwakened ? "BOSS THỨC TỈNH" : pet.EvolutionLevel>1 ? "THẺ THỨC TỈNH" : "THƯỜNG")+" / "+P1Catalog.ElementName(pet.Definition.element);
        }
        public void Timer(float remaining,bool ticking)
        {
            timerLabel.text=Mathf.CeilToInt(remaining).ToString();timerLabel.color=ticking && remaining<=5 ? new Color(1,.35f,.2f) : new Color(1,.9f,.58f);
        }
        private void Update()
        {
            foreach(var b in bars){b.Current=Mathf.Lerp(b.Current,b.Target,1-Mathf.Exp(-Time.unscaledDeltaTime*9));b.Fill.rectTransform.sizeDelta=new Vector2(b.Width*b.Current,b.Fill.rectTransform.sizeDelta.y);}
            for(int i=floating.Count-1;i>=0;i--)
            {
                var f=floating[i];f.Age+=Time.deltaTime;
                if(f.Age>1.2f){Destroy(f.Text.gameObject);floating.RemoveAt(i);continue;}
                f.Text.rectTransform.anchoredPosition=f.Start+Vector2.up*f.Age*65;
                var color=f.Text.color;color.a=Mathf.Clamp01((1.2f-f.Age)*2.5f);f.Text.color=color;
            }
        }
        public void Say(string message,bool log=true)
        {
            status.text=message;if(!log)return;messages.Enqueue(message);while(messages.Count>2)messages.Dequeue();history.text=string.Join("\n",messages);
        }
        public void SetDetail(string message){detail.text=message;}
        public void ShowResult(BattleResult result)
        {
            resultText.text=result==BattleResult.Victory ? "CHIẾN THẮNG" : result==BattleResult.Defeat ? "THẤT BẠI" : "HÒA";
            playerPanel.Pet.SetPose(result==BattleResult.Draw ? PetPose.Idle : result==BattleResult.Victory ? PetPose.Victory : PetPose.Defeat);
            enemyPanel.Pet.SetPose(result==BattleResult.Draw ? PetPose.Idle : result==BattleResult.Defeat ? PetPose.Victory : PetPose.Defeat);
            audioFx.PlayResult(result);
            // Let the outcome poses play visibly before fading in the result.
            StartCoroutine(ResultAfterPose());
        }
        private IEnumerator ResultAfterPose(){yield return new WaitForSeconds(.85f);resultPanel.SetActive(true);}
        public void HideResult()
        {
            combatStage=false;if(collectionLabel!=null)collectionLabel.gameObject.SetActive(false);foreach(var group in boardGroups){group.alpha=1;group.blocksRaycasts=true;}
            playerPanel.Pet.transform.localPosition=new Vector3(-660,-18,0);enemyPanel.Pet.transform.localPosition=new Vector3(660,-18,0);
            playerPanel.Pet.transform.localScale=enemyPanel.Pet.transform.localScale=Vector3.one*1.2f;
            StopAllCoroutines();audioFx.Stop();resultPanel.SetActive(false);messages.Clear();history.text="";comboLabel.gameObject.SetActive(false);
            playerPanel.Pet.ResetForm();enemyPanel.Pet.ResetForm();
            foreach(var effect in activeVfx.ToArray())ReleaseEffect(effect);
            foreach(var f in floating)if(f.Text!=null)Destroy(f.Text.gameObject);floating.Clear();
            foreach(var go in transient)if(go!=null)Destroy(go);transient.Clear();
            foreach(var bar in bars)bar.Current=0;
        }
        public void DrawBoard(MatchBoard board)
        {
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)
            {
                var cell=new Cell(x,y);int kind=board[x,y];
                if(tiles[x,y]!=null)
                {
                    var existing=tiles[x,y];existing.gameObject.SetActive(true);existing.anchoredPosition=Position(cell);existing.localScale=Vector3.one;existing.GetComponent<Image>().sprite=art.Gems[kind];existing.GetComponent<Image>().color=Color.white;
                    existing.Find("Symbol").GetComponent<Image>().sprite=art.Icons[kind];existing.Find("IconShadow").GetComponent<Image>().sprite=art.Icons[kind];continue;
                }
                var tile=Panel(boardRoot,"Gem_"+x+"_"+y,Position(cell),new Vector2(59,59),Color.white);tile.sprite=art.Gems[kind];
                var shadow=Panel(tile.rectTransform,"IconShadow",new Vector2(1,-2),new Vector2(47,47),new Color(.02f,.05f,.08f,.65f));shadow.sprite=art.Icons[kind];
                var icon=Panel(tile.rectTransform,"Symbol",Vector2.zero,new Vector2(47,47),Color.white);icon.sprite=art.Icons[kind];
                var glow=Panel(tile.rectTransform,"GemGlow",new Vector2(-8,12),new Vector2(48,32),new Color(1,1,1,.12f));glow.sprite=art.Glow;
                var input=tile.gameObject.AddComponent<GemInput>();input.Cell=cell;input.Click=click;input.Drag=drag;tile.raycastTarget=true;tiles[x,y]=tile.rectTransform;
            }
        }
        private void ClearTiles()
        {
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(tiles[x,y]!=null){tiles[x,y].gameObject.SetActive(false);Destroy(tiles[x,y].gameObject);tiles[x,y]=null;}
        }
        private static Vector2 Position(Cell c)=>new Vector2((c.X-3.5f)*Pitch,(c.Y-3.5f)*Pitch);
        public void Select(Cell? cell)
        {
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)if(tiles[x,y]!=null)tiles[x,y].localScale=cell.HasValue && cell.Value.Equals(new Cell(x,y)) ? Vector3.one*.84f : Vector3.one;
        }
        public IEnumerator AnimateSwap(BoardMove move,float seconds,bool reverse=false)
        {
            var a=tiles[move.A.X,move.A.Y];var b=tiles[move.B.X,move.B.Y];Vector2 pa=Position(move.A),pb=Position(move.B);
            if(reverse){var temp=pa;pa=pb;pb=temp;}
            for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.01f,seconds)){float f=Mathf.SmoothStep(0,1,t);a.anchoredPosition=Vector2.Lerp(pa,pb,f);b.anchoredPosition=Vector2.Lerp(pb,pa,f);yield return null;}
            a.anchoredPosition=pb;b.anchoredPosition=pa;
        }
        public void Collected(List<CollectedGroup> groups)
        {
            int[] counts=new int[6];foreach(var group in groups)counts[(int)group.Group.Kind]+=group.Group.Cells.Count;
            collectionLabel.text="KIẾM "+counts[0]+"  ·  GIÁP "+counts[1]+"  ·  HP "+counts[2]+"  ·  MANA "+counts[3]+"  ·  NỘ "+counts[4]+"  ·  HÚT "+counts[5];
        }
        public IEnumerator AnimateClear(List<MatchGroup> groups,float seconds)
        {
            var tokens=new List<Image>();var origins=new List<Vector2>();
            foreach(var group in groups)
            {
                Vector2 origin=Vector2.zero;foreach(var cell in group.Cells)origin+=Position(cell);origin/=group.Cells.Count;origin+=new Vector2(0,25);
                var token=EffectPanel("CollectedGem",origin,new Vector2(42,42),PlaceholderArt.Colors[(int)group.Kind]);token.sprite=art.Icons[(int)group.Kind];tokens.Add(token);origins.Add(origin);transient.Add(token.gameObject);
            }
            for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.01f,seconds))
            {
                for(int i=0;i<tokens.Count;i++)tokens[i].rectTransform.anchoredPosition=Vector2.Lerp(origins[i],new Vector2(activePlayer ? -550 : 550,340),t);
                foreach(var group in groups)foreach(var c in group.Cells){tiles[c.X,c.Y].localScale=Vector3.one*(1-t);tiles[c.X,c.Y].GetComponent<Image>().color=Color.Lerp(Color.white,PlaceholderArt.Colors[(int)group.Kind],t);}
                yield return null;
            }
            foreach(var token in tokens){ReleaseEffect(token);}
        }
        public IEnumerator AnimateFall(MatchBoard board,List<GemFall> falls,float seconds)
        {
            DrawBoard(board);
            for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.01f,seconds))
            {
                float f=1-(1-t)*(1-t);foreach(var fall in falls)tiles[fall.Target.X,fall.Target.Y].anchoredPosition=Vector2.Lerp(Position(new Cell(fall.Target.X,fall.SourceY)),Position(fall.Target),f);yield return null;
            }
            foreach(var fall in falls)tiles[fall.Target.X,fall.Target.Y].anchoredPosition=Position(fall.Target);
        }
        private void Float(string text,float x,float y,Color color,int size=29)
        {
            var label=Label(effectRoot,text,new Vector2(x,y),new Vector2(460,60),size,color);floating.Add(new Floating {Text=label,Start=new Vector2(x,y)});
        }
        public IEnumerator AnimateEffect(bool playerActor,BattleReport report,float seconds,int combo=1)
        {
            var actor=playerActor ? playerPanel : enemyPanel;var target=playerActor ? enemyPanel : playerPanel;
            float actorX=combatStage ? actor.X*.53f : actor.X,targetX=combatStage ? target.X*.53f : target.X;
            Color tint=playerActor ? new Color(1,.4f,.08f) : new Color(.2f,.8f,1);
            bool attack=report.NormalAttacks+report.SpecialAttacks>0;
            bool special=report.SpecialAttacks>0,critical=report.CriticalAttacks>0;audioFx.Play(report,playerActor);
            if(report.BossTransformation)actor.Pet.BossAwaken(seconds);
            else if(report.Evolved)actor.Pet.SetPose(PetPose.Evolution,seconds);
            else if(attack && !combatStage)actor.Pet.SetPose(special ? PetPose.Special : PetPose.Attack,seconds);
            if(report.HpDamage+report.Absorbed>0)target.Pet.SetPose(PetPose.Hit,seconds);
            if(report.HpDamage>0)Float("−"+report.HpDamage+" HP",targetX,80,critical ? new Color(1,.64f,.08f) : new Color(1,.95f,.72f),critical ? 49 : special ? 44 : 32);
            if(attack && elementTable!=null){float advantage=elementTable.Multiplier(actor.Definition.element,target.Definition.element);if(advantage!=1)Float(advantage>1 ? "KHẮC CHẾ +25%" : "BỊ KHẮC −15%",targetX,190,new Color(1,.85f,.45f),24);}
            if(critical)Float("CRITICAL!",targetX,145,new Color(1,.64f,.08f),42);
            if(report.Absorbed>0)Float("−"+report.Absorbed+" GIÁP",targetX,35,new Color(.83f,.6f,1),25);
            if(report.Healing>0)Float("+"+report.Healing+" HP",actorX,95,new Color(.4f,1,.57f));
            if(report.Mana>0)Float("+"+report.Mana+" MANA",actorX,190,PlaceholderArt.Colors[3],24);
            if(report.Rage>0)Float("+"+report.Rage+" NỘ",actorX,225,PlaceholderArt.Colors[4],24);
            if(report.Shield>0)Float("+"+report.Shield+" GIÁP",actorX,48,new Color(.83f,.6f,1),25);
            if(report.RageStolen+report.ShieldStolen>0 || report.HpStolen>0)Float("HÚT NĂNG LƯỢNG",actorX,145,new Color(.82f,.9f,1),24);
            if(report.BossTransformation)Float("BOSS THỨC TỈNH",actorX,190,new Color(1,.8f,.3f),32);
            if(report.Evolved)Float("THỨC TỈNH  /  DẠNG II",actorX,190,new Color(1,.89f,.45f),30);
            Image projectile=null,ring=null,slash=null;
            var petals=new List<Image>();var petalStarts=new List<Vector2>();
            var motif=special ? P2ArtLibrary.Get(actor.Definition.artSheet,3) : null;
            if(motif!=null && PlayerPrefs.GetInt("P2ReducedVfx",0)==0)
            {
                int count=actor.Definition.rig==6 ? 9 : 6;
                for(int i=0;i<count;i++)
                {
                    float angle=(i/(float)count*Mathf.PI*2)+actor.Definition.rig*.31f;
                    var ray=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    var petal=EffectPanel("Signature_"+actor.Definition.id,new Vector2(targetX,0),new Vector2(58,82),Color.white);
                    petal.sprite=motif;petal.preserveAspect=true;petals.Add(petal);petalStarts.Add(ray);
                }
            }
            if(attack)
            {
                slash=EffectPanel("SwordSlash",new Vector2(targetX,0),new Vector2(special ? 210 : 150,special ? 210 : 150),critical ? new Color(1,.65f,.1f) : new Color(1,.96f,.75f));
                slash.sprite=art.Icons[(int)GemKind.Sword];transient.Add(slash.gameObject);
            }
            if((attack && !combatStage) || report.DrainGroups>0)
            {
                projectile=EffectPanel("EnergyBolt",new Vector2(actorX,0),new Vector2(special ? 190 : 85,special ? 120 : 60),report.DrainGroups>0 && !attack ? new Color(.8f,.85f,1) : tint);
                projectile.sprite=report.DrainGroups>0 && !attack ? art.Icons[(int)GemKind.Drain] : art.Glow;transient.Add(projectile.gameObject);
            }
            if(report.BossTransformation || report.Evolved || special || report.Healing>0 || report.Shield>0)
            {
                ring=EffectPanel("EffectRing",new Vector2(actorX,0),new Vector2(280,280),(report.BossTransformation || report.Evolved) ? Gold : special ? tint : report.Healing>0 ? PlaceholderArt.Colors[2] : PlaceholderArt.Colors[1]);ring.sprite=art.Ring;transient.Add(ring.gameObject);
            }
            if(combo>=3 || special || critical){comboLabel.text=special ? (critical ? "ĐẶC BIỆT · CRITICAL!" : "ĐÒN ĐẶC BIỆT") : critical ? "CRITICAL!" : "COMBO "+combo;comboLabel.gameObject.SetActive(true);}
            for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.01f,seconds))
            {
                for(int i=0;i<petals.Count;i++)
                {
                    var petal=petals[i];float spread=actor.Definition.rig==6 ? 160*t : 80+125*t;
                    petal.rectTransform.anchoredPosition=new Vector2(targetX,0)+petalStarts[i]*spread+new Vector2(0,actor.Definition.element==Element.Water ? -t*50 : t*35);
                    petal.rectTransform.localRotation=Quaternion.Euler(0,0,(i*60+t*(actor.Definition.rig%2==0 ? 220 : -130)));
                    petal.rectTransform.localScale=Vector3.one*(.4f+Mathf.Sin(t*Mathf.PI)*.8f);petal.color=new Color(1,1,1,1-t);
                }
                if(slash!=null){slash.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-70,70,t));slash.rectTransform.localScale=Vector3.one*(.7f+Mathf.Sin(t*Mathf.PI));var sc=slash.color;sc.a=Mathf.Sin(t*Mathf.PI);slash.color=sc;}
                if(projectile!=null)
                {
                    float from=report.DrainGroups>0 && !attack ? targetX : actorX,to=report.DrainGroups>0 && !attack ? actorX : targetX;
                    projectile.rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(from,to,t),Mathf.Sin(t*Mathf.PI)*65);
                    projectile.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*(special ? 1.8f : .4f));
                    if(report.DrainGroups>0 && !attack)projectile.rectTransform.localRotation=Quaternion.Euler(0,0,t*540);
                }
                if(ring!=null){ring.rectTransform.localScale=Vector3.one*(1+t*1.5f);ring.rectTransform.localRotation=Quaternion.Euler(0,0,t*160);var color=ring.color;color.a=1-t;ring.color=color;}
                if(comboLabel.gameObject.activeSelf)comboLabel.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.08f);
                yield return null;
            }
            if(projectile!=null){ReleaseEffect(projectile);}
            if(slash!=null){ReleaseEffect(slash);}
            if(ring!=null){ReleaseEffect(ring);}comboLabel.gameObject.SetActive(false);
            foreach(var petal in petals)ReleaseEffect(petal);
        }
        public IEnumerator AnnounceSuddenDeath()
        {
            comboLabel.text="SUDDEN DEATH";comboLabel.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.4f);comboLabel.gameObject.SetActive(false);
        }
        private Image EffectPanel(string name,Vector2 at,Vector2 size,Color color)
        {
            Image effect=vfxPool.Count>0 ? vfxPool.Dequeue() : Panel(effectRoot,name,at,size,color);
            if(effect.GetComponent<P2SilhouetteMesh>()==null)effect.gameObject.AddComponent<P2SilhouetteMesh>();effect.name=name;effect.gameObject.SetActive(true);effect.sprite=null;effect.preserveAspect=false;effect.color=color;effect.rectTransform.anchoredPosition=at;effect.rectTransform.sizeDelta=size;effect.rectTransform.localScale=Vector3.one;effect.rectTransform.localRotation=Quaternion.identity;activeVfx.Add(effect);return effect;
        }
        private void ReleaseEffect(Image effect)
        {
            if(effect==null || !activeVfx.Remove(effect))return;transient.Remove(effect.gameObject);effect.gameObject.SetActive(false);vfxPool.Enqueue(effect);
        }
        private IEnumerator ReleaseLater(Image effect,float seconds){yield return new WaitForSeconds(seconds);ReleaseEffect(effect);}
        private static RectTransform NewRect(RectTransform parent,string name,Vector2 at,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=at;r.sizeDelta=size;return r;
        }
        private static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        private Image Panel(RectTransform parent,string name,Vector2 at,Vector2 size,Color color)
        {
            var r=NewRect(parent,name,at,size);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        private RectTransform Border(RectTransform parent,string name,Vector2 at,Vector2 size,Color fill,Color edge)
        {
            var panel=Panel(parent,name,at,size,fill);var r=panel.rectTransform;
            Panel(r,"TopEdge",new Vector2(0,size.y/2-1),new Vector2(size.x,2),edge);
            Panel(r,"BottomEdge",new Vector2(0,-size.y/2+1),new Vector2(size.x,2),edge*.7f);
            Panel(r,"LeftEdge",new Vector2(-size.x/2+1,0),new Vector2(2,size.y),edge);
            Panel(r,"RightEdge",new Vector2(size.x/2-1,0),new Vector2(2,size.y),edge*.7f);return r;
        }
        private Text Label(RectTransform parent,string text,Vector2 at,Vector2 size,int fontSize,Color color)
        {
            var r=NewRect(parent,"Text",at,size);var label=r.gameObject.AddComponent<Text>();label.font=font;label.text=text;label.fontSize=fontSize;label.color=color;
            label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;
            var shadow=r.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.9f);shadow.effectDistance=new Vector2(1,-1);return label;
        }
        private Button MakeButton(RectTransform parent,string text,Vector2 at,Vector2 size,Action action)
        {
            var rect=Border(parent,text,at,size,Ink,Gold);var image=rect.GetComponent<Image>();image.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;if(text=="CHƠI LẠI" || text=="VỀ ĐẢO" || text=="TẠM DỪNG" || text=="ÂM THANH")sessionButtons.Add(button);button.onClick.AddListener(()=>action());
            var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;Label(rect,text,Vector2.zero,size,16,new Color(1,.9f,.6f));return button;
        }
        private void OnDestroy(){art?.Dispose();}
    }
}
