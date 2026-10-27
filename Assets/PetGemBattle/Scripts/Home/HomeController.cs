using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace PetGemBattle
{
    public sealed partial class HomeController : MonoBehaviour
    {
        public P1Catalog catalog;
        [NonSerialized] public string SavePathOverride;
        public ProgressionService Progress {get;private set;}
        public string CurrentScreen {get;private set;}="Home";
        private RectTransform root,content;
        private Font font;
        private Text footer;
        private Sprite backdrop;
        private Texture2D backdropSource;
        private readonly Stack<string> screens=new Stack<string>();
        private string selectedPet="starter-metal",sort="Lv",message="",battleId,rewardText;
        private int elementFilter=-1,rarityFilter=-1,bagElement=1,stage=1,rewardSeed;
        private bool activeOnly,practice,lobbyNavExpanded=true;
        private GameObject battleObject;
        private BattleRules battleRules;
        private BattleResult lastResult;
        private void Start()
        {
            if(catalog==null)catalog=Resources.Load<P1Catalog>("P1Catalog");
            if(catalog==null){Debug.LogError("P1Catalog missing: run Tools/Pet Gem Battle/Open P1 Home");return;}
            if(catalog.p2Enabled)P2Content.Upgrade(catalog);
            catalog.ValidateContent();var save=new SaveGameService(SavePathOverride??Path.Combine(Application.persistentDataPath,"p1-save.json"),catalog);
            Progress=new ProgressionService(catalog,save,catalog.p2Enabled ? Resources.Load<GameModeConfig>("GameModeConfig") : null);selectedPet=Progress.Data.activePetId;message=save.Notice??"";
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=new GameObject("P1Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            root=canvas.GetComponent<RectTransform>();canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=100;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            if(FindFirstObjectByType<EventSystem>()==null){var events=new GameObject("HomeInput",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(transform,false);events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            Render();
        }
        public void Navigate(string page){screens.Push(CurrentScreen);CurrentScreen=page;Render();}
        public void Home(){screens.Clear();CurrentScreen="Home";LeaveBattle();Render();}
        private void Back(){CurrentScreen=screens.Count>0 ? screens.Pop() : "Home";Render();}
        private void Safe(Action action){try{action();}catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException){message="Không lưu được dữ liệu. "+ex.Message;Render();}}
        private RectTransform Rect(Transform parent,string name,Vector2 at,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=at;r.sizeDelta=size;return r;
        }
        private Image Panel(Transform parent,string name,Vector2 at,Vector2 size,Color color)
        {var r=Rect(parent,name,at,size);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        private Text Text(Transform parent,string value,Vector2 at,Vector2 size,int fontSize=22,Color? color=null)
        {
            var text=Rect(parent,"Text",at,size).gameObject.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=fontSize;text.color=color??new Color(.92f,.98f,.92f);text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
            var shadow=text.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,.04f,.04f,.8f);shadow.effectDistance=new Vector2(1,-1);return text;
        }
        private Button Button(Transform parent,string label,Vector2 at,Vector2 size,Action action,bool enabled=true)
        {
            bool textOnly=catalog!=null && catalog.p2Enabled;
            var p=Panel(parent,label,at,size,textOnly ? new Color(1,1,1,.001f) : enabled ? new Color(.12f,.32f,.28f,.97f) : new Color(.15f,.19f,.22f,.9f));p.raycastTarget=true;
            var outline=p.gameObject.AddComponent<Outline>();outline.effectColor=textOnly ? Color.clear : enabled ? new Color(.65f,.85f,.4f) : Color.gray;outline.effectDistance=new Vector2(1,-1);
            var b=p.gameObject.AddComponent<Button>();b.targetGraphic=p;b.interactable=enabled;b.onClick.AddListener(()=>{if(b.interactable){P2Click();Safe(action);}});var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            if(!enabled){var hint=p.gameObject.AddComponent<HomeHint>();hint.Show=()=>{if(footer!=null)footer.text=label.Contains("Coming Soon") ? label.Replace('\n',' ')+" · Chưa mở trong bản này." : label;};hint.Hide=()=>{if(footer!=null)footer.text=message;};}
            var labelText=Text(p.transform,label,Vector2.zero,size-Vector2.one*8,18,enabled ? Color.white : new Color(.6f,.65f,.65f));
            if(textOnly)
            {
                labelText.fontStyle=FontStyle.Bold;
                var stroke=labelText.gameObject.AddComponent<Outline>();stroke.effectColor=new Color(.01f,.06f,.10f,1);stroke.effectDistance=new Vector2(2,-2);
            }
            return b;
        }
        private ArticulatedPet Pet(Transform parent,string id,Vector2 at,float scale,bool silhouette=false,int previewLevel=0)
        {
            var definition=catalog.Pet(id);var rect=Rect(parent,"Preview_"+id,at,new Vector2(300,350));rect.localScale=Vector3.one*scale;
            string sheet=catalog.Sheet(id,previewLevel>0 ? previewLevel : Progress.Owned(id)?.level??1);
            var prefab=catalog.p2Enabled ? Resources.Load<GameObject>("P2Pets/"+sheet) : null;
            if(prefab!=null)
            {
                var model=Instantiate(prefab,rect,false).GetComponent<P2PetPrefab>();
                if(silhouette && model.GetComponent<PaintedPetRig>()!=null)model.GetComponent<PaintedPetRig>().Silhouette=true;
                return model.Rig;
            }
            var rig=rect.gameObject.AddComponent<ArticulatedPet>();rig.Build(definition.rig,true,silhouette ? new Color(.09f,.16f,.16f) : P1Catalog.ElementColor(definition.element),catalog.Sheet(id,previewLevel>0 ? previewLevel : Progress.Owned(id)?.level??1));
            if(silhouette && rect.GetComponent<PaintedPetRig>()!=null)rect.GetComponent<PaintedPetRig>().Silhouette=true;
            if(silhouette)foreach(var image in rect.GetComponentsInChildren<Image>())image.color=new Color(.055f,.12f,.12f);
            return rig;
        }
        private void Render()
        {
            if(root==null)return;root.gameObject.SetActive(true);foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var bg=Panel(root,"ForestBackdrop",Vector2.zero,new Vector2(1660,950),new Color(.12f,.28f,.22f));
            var texture=Resources.Load<Texture2D>(catalog.p2Enabled && CurrentScreen=="Home" ? "P3Art/lobby-map" : catalog.p2Enabled ? "P2Art/lobby" : "BattleArt/TempleArena");
            if(texture==null && catalog.p2Enabled && CurrentScreen=="Home")texture=Resources.Load<Texture2D>("P2Art/lobby");
            if(texture!=null){if(backdrop==null || backdropSource!=texture){if(backdrop!=null)Destroy(backdrop);backdrop=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));backdropSource=texture;}bg.sprite=backdrop;bg.color=catalog.p2Enabled ? Color.white : new Color(.8f,1,.8f,.5f);if(catalog.p2Enabled && CurrentScreen!="Home")bg.gameObject.AddComponent<LobbyParallax>();}
            Panel(root,"SafePanel",Vector2.zero,new Vector2(1560,860),catalog.p2Enabled ? new Color(.025f,.09f,.09f,CurrentScreen=="Home" ? .05f : .22f) : new Color(.025f,.09f,.09f,.91f));
            if(catalog.p2Enabled)
            {
                bool lobby=CurrentScreen=="Home";
                if(!lobby && CurrentScreen!="Pets")Panel(root,"HeaderGlass",new Vector2(0,378),new Vector2(1560,122),new Color(.02f,.09f,.17f,.87f));
                if(!lobby)Panel(root,"FooterGlass",new Vector2(0,-416),new Vector2(1560,40),new Color(.02f,.13f,.16f,.85f));
                if(!lobby && CurrentScreen!="Pets")Text(root,"PET ARENA  ·  THIÊN CẢNH NGŨ HÀNH",new Vector2(0,398),new Vector2(900,48),30,new Color(.9f,1,.6f));
                if(!lobby && CurrentScreen!="Pets")Text(root,"Nhà huấn luyện  ·  Ải "+Progress.Data.clearedStages.Count+"/7  ·  Tổng đá "+Progress.Data.stones.Sum(),new Vector2(0,352),new Vector2(1100,30),19);
                if(!lobby && CurrentScreen!="Result" && CurrentScreen!="Pets")Button(root,"‹ TRỞ LẠI",new Vector2(-665,396),new Vector2(170,42),Back);
            }
            else
            {
                Text(root,"PET ARENA CLASSIC  ·  RỪNG MẦM",new Vector2(0,398),new Vector2(1000,50),30,new Color(.9f,1,.6f));
                Text(root,"Nhà huấn luyện  ·  Ải "+Progress.Data.clearedStages.Count+"/7  ·  Tổng đá "+Progress.Data.stones.Sum(),new Vector2(0,352),new Vector2(1100,30),19);
                Button(root,"‹ QUAY LẠI",new Vector2(-665,396),new Vector2(170,42),Back,CurrentScreen!="Home" && CurrentScreen!="Result");
                Button(root,"SẢNH",new Vector2(665,396),new Vector2(170,42),Home);
            }
            content=Rect(root,"Screen_"+CurrentScreen,new Vector2(0,-15),new Vector2(1500,690));
            if(catalog.p2Enabled && DrawP2Screen()){footer=Text(root,message,new Vector2(0,-412),new Vector2(1430,36),16,new Color(1,.84f,.45f));return;}
            switch(CurrentScreen)
            {
                case "Home":DrawHome();break;case "Island":DrawIsland();break;case "Prepare":DrawPrepare();break;
                case "Pets":DrawPets();break;case "Detail":DrawDetail();break;case "Upgrade":DrawUpgrade();break;case "Bag":DrawBag();break;case "Book":DrawBook();break;case "Result":DrawResult();break;case "Cards":DrawCards();break;
            }
            footer=Text(root,message,new Vector2(0,catalog.p2Enabled && CurrentScreen=="Home" ? -439 : -412),new Vector2(1430,28),16,new Color(1,.84f,.45f));
        }
        private string Stats(PetDefinition p)=>"HP "+p.maxHp+"   ATK "+p.attack+"   DEF "+p.defense;
        private void DrawHome()
        {
            if(catalog.p2Enabled){DrawP2Lobby();return;}
            var owned=Progress.Owned(Progress.Data.activePetId);var pet=catalog.Pet(owned.id);var stats=catalog.Stats(owned.id,owned.level);
            Pet(content,owned.id,new Vector2(0,45),1.2f);Text(content,pet.title,new Vector2(0,280),new Vector2(600,45),32);
            Text(content,P1Catalog.ElementName(pet.element)+" · Bậc "+pet.rarity+" · Lv"+owned.level+"/7",new Vector2(0,-175),new Vector2(600,40),23);
            Text(content,Stats(stats),new Vector2(0,-220),new Vector2(650,40),22);
            string[] labels={"ĐẢO BOSS / PHIÊU LƯU","KHO PET","KHO ĐỒ","NÂNG CẤP PET","SÁCH PET","ĐẤU THỬ","THẺ PHÉP"};
            for(int i=0;i<labels.Length;i++){int index=i;float x=i<4 ? -525 : 525,y=240-(i<4 ? i : i-4)*100;Button(content,labels[i],new Vector2(x,y),new Vector2(320,72),()=>{selectedPet=Progress.Data.activePetId;if(index==5){practice=true;stage=1;Navigate("Prepare");}else Navigate(new[]{"Island","Pets","Bag","Upgrade","Book","Prepare","Cards"}[index]);});}
            string[] soon={"PvP","Bang hội / Bạn bè","Cửa hàng / Triệu hồi","Nhiệm vụ / Xếp hạng","Đảo mới"};
            for(int i=0;i<soon.Length;i++)Button(content,soon[i]+"\nComing Soon",new Vector2((i-2)*285,-315),new Vector2(270,62),()=>{},false);
        }
        private void DrawIsland()
        {
            if(catalog.p2Enabled){DrawP2StageMap();return;}
            Text(content,"ĐẢO RỪNG MẦM · 7 THỬ THÁCH",new Vector2(0,295),new Vector2(900,45),29);
            for(int i=0;i<7;i++)
            {
                int n=i+1;var boss=catalog.stages[i];var pet=catalog.Pet(boss.petId);float x=(i%4-1.5f)*360,y=i<4 ? 110 : -155;
                var tile=Panel(content,"Stage_"+n,new Vector2(x,y),new Vector2(330,240),new Color(.08f,.23f,.19f,.95f));bool unlocked=n<=Progress.Data.unlockedStage;
                Pet(tile.transform,boss.petId,new Vector2(-96,20),.38f,!Progress.Data.clearedStages.Contains(n));
                Text(tile.transform,n+". "+pet.title+"\nMộc · "+pet.rarity,new Vector2(45,64),new Vector2(230,72),19);
                Text(tile.transform,"Chắc chắn: Đá Lv"+boss.stoneGrade+"\n"+Mathf.RoundToInt(boss.bonusChance*100)+"% thêm Lv"+boss.bonusGrade,new Vector2(55,-8),new Vector2(220,62),17);
                Button(tile.transform,unlocked ? (Progress.Owned(boss.petId)!=null ? "ĐÃ THU PHỤC · ĐÁNH LẠI" : "CHỌN BOSS") : "KHÓA · Thắng ải "+i,new Vector2(0,-85),new Vector2(300,43),()=>{stage=n;practice=false;Navigate("Prepare");},unlocked);
            }
        }
        private void DrawPrepare()
        {
            var a=Progress.Owned(Progress.Data.activePetId);var player=catalog.Stats(a.id,a.level);var boss=catalog.Stats(catalog.stages[stage-1].petId,1,true,stage);
            Pet(content,a.id,new Vector2(-360,25),1.1f);Pet(content,boss.id,new Vector2(360,25),1.1f,false,1);
            Text(content,practice ? "ĐẤU THỬ · KHÔNG NHẬN THƯỞNG" : "CHUẨN BỊ ẢI "+stage,new Vector2(0,290),new Vector2(1100,45),30);
            Text(content,player.name+" · Lv"+a.level+"\n"+Stats(player),new Vector2(-360,-180),new Vector2(650,72),21);
            Text(content,boss.name+" · Mộc\n"+Stats(boss),new Vector2(360,-180),new Vector2(650,72),21);
            float multi=catalog.Multiplier(player.element,boss.element);
            Text(content,multi>1 ? "KHẮC CHẾ +25%" : multi<1 ? "BỊ KHẮC −15%" : "TƯƠNG KHẮC TRUNG TÍNH",new Vector2(0,160),new Vector2(450,65),24,new Color(1,.85f,.4f));
            Text(content,"BẠN ĐI TRƯỚC · LƯỢT LUÂN PHIÊN",new Vector2(0,85),new Vector2(520,45),24);
            if(catalog.p2Enabled)Text(content,"Khuyên dùng Lv"+catalog.stages[stage-1].encounter.recommendedLevel+"+\nBoss thức tỉnh khi HP ≤30%",new Vector2(0,-25),new Vector2(420,95),19);
            Button(content,"ĐỔI PET",new Vector2(-230,-290),new Vector2(300,58),()=>Navigate("Pets"));
            Button(content,"BẮT ĐẦU",new Vector2(230,-290),new Vector2(300,58),StartBattle);
        }
        private void DrawPets()
        {
            if(catalog.p2Enabled){DrawP2Collection();return;}
            Button(content,"Hệ: "+(elementFilter<0 ? "Tất cả" : P1Catalog.ElementName((Element)elementFilter)),new Vector2(-500,285),new Vector2(260,42),()=>{elementFilter=(elementFilter+2)%6-1;Render();});
            Button(content,"Bậc: "+(rarityFilter<0 ? "Tất cả" : ((PetRarity)rarityFilter).ToString()),new Vector2(-210,285),new Vector2(250,42),()=>{rarityFilter=(rarityFilter+2)%7-1;Render();});
            Button(content,"Sắp xếp: "+sort,new Vector2(75,285),new Vector2(250,42),()=>{string[] options={"Lv","Bậc","HP","ATK"};sort=options[(Array.IndexOf(options,sort)+1)%4];Render();});
            Button(content,activeOnly ? "Đang xuất chiến" : "Tất cả sở hữu",new Vector2(390,285),new Vector2(300,42),()=>{activeOnly=!activeOnly;Render();});
            int i=0;foreach(var owned in Progress.Filter(elementFilter,rarityFilter,sort,activeOnly))
            {
                var p=catalog.Pet(owned.id);string id=owned.id;float x=(i%4-1.5f)*355,y=95-(i/4)*245;var tile=Panel(content,"Owned_"+id,new Vector2(x,y),new Vector2(330,225),new Color(.09f,.24f,.21f,.95f));
                Pet(tile.transform,id,new Vector2(-88,20),.35f);Text(tile.transform,p.title+"\n"+p.rarity+" · "+P1Catalog.ElementName(p.element)+" · Lv"+owned.level,new Vector2(62,40),new Vector2(205,90),20);
                Button(tile.transform,id==Progress.Data.activePetId ? "ĐANG XUẤT CHIẾN · CHI TIẾT" : "XEM CHI TIẾT",new Vector2(0,-78),new Vector2(305,45),()=>{selectedPet=id;Navigate("Detail");});i++;
            }
            if(i==0)Text(content,"Không có Pet phù hợp bộ lọc.",Vector2.zero,new Vector2(900,80),26);
        }
        private void DrawDetail()
        {
            if(catalog.p2Enabled){DrawP2Detail();return;}
            var owned=Progress.Owned(selectedPet);var p=catalog.Pet(selectedPet);Pet(content,selectedPet,new Vector2(-370,0),1.35f);
            Text(content,p.title+" · "+p.rarity+" · "+P1Catalog.ElementName(p.element),new Vector2(250,240),new Vector2(850,55),30);
            Text(content,p.description+"\n\n"+Stats(catalog.Stats(selectedPet,owned.level))+"\nChí mạng cơ bản "+Mathf.RoundToInt(p.crit*100)+"%\nCấp "+owned.level+" / 7",new Vector2(250,95),new Vector2(850,210),24);
            Text(content,"Kim → Mộc → Thổ → Thủy → Hỏa → Kim\nMũi tên khắc +25%; chiều ngược −15%; còn lại trung tính.",new Vector2(200,-100),new Vector2(950,90),20);
            Button(content,"XUẤT CHIẾN",new Vector2(20,-255),new Vector2(300,60),()=>{Progress.Select(selectedPet);message="Đã chọn "+p.title;Home();});
            Button(content,"NÂNG CẤP",new Vector2(390,-255),new Vector2(300,60),()=>Navigate("Upgrade"));
        }
        private void DrawUpgrade()
        {
            if(catalog.p2Enabled){DrawP2Upgrade();return;}
            var owned=Progress.Owned(selectedPet);var p=catalog.Pet(selectedPet);var before=catalog.Stats(selectedPet,owned.level);bool max=owned.level>=7;int next=Math.Min(7,owned.level+1);var after=catalog.Stats(selectedPet,next);
            Pet(content,selectedPet,new Vector2(-380,10),1.25f);Text(content,p.title+" · Lv"+owned.level+" → "+next,new Vector2(245,250),new Vector2(900,60),31);
            Text(content,"HP  "+before.maxHp+" → "+after.maxHp+"\nATK  "+before.attack+" → "+after.attack+"\nDEF  "+before.defense+" → "+after.defense,new Vector2(245,85),new Vector2(700,180),29);
            int count=Progress.StoneCount(p.element,next);string cost="Đá "+P1Catalog.ElementName(p.element)+" Lv"+next;
            P1Art.Part(content,"UpgradeStone",P1Art.Stone(next),new Vector2(30,-100),new Vector2(80,80),P1Catalog.ElementColor(p.element));
            Text(content,max ? "ĐÃ ĐẠT CẤP TỐI ĐA" : "Cần 1 "+cost+" · Đang có "+count,new Vector2(300,-100),new Vector2(600,75),22);
            int expected=owned.level;string tx=Guid.NewGuid().ToString("N");
            Button(content,max ? "ĐÃ TỐI ĐA" : count<1 ? "Thiếu "+cost : "XÁC NHẬN · DÙNG 1 ĐÁ",new Vector2(245,-240),new Vector2(590,65),()=>{if(Progress.Upgrade(selectedPet,expected,tx)){message=p.title+" đã lên Lv"+(expected+1)+"!";Render();foreach(var rig in content.GetComponentsInChildren<ArticulatedPet>())rig.SetPose(PetPose.Victory,1);}},!max && count>0);
            Text(content,"Nâng cấp vĩnh viễn. Tiến hóa bằng thẻ chỉ có hiệu lực trong trận.",new Vector2(0,-335),new Vector2(1300,45),20);
        }
        private void DrawBag()
        {
            if(catalog.p2Enabled){DrawP2Bag();return;}
            for(int i=0;i<5;i++){int e=i;Button(content,P1Catalog.ElementName((Element)i),new Vector2((i-2)*245,290),new Vector2(225,45),()=>{bagElement=e;Render();});}
            Text(content,"ĐÁ "+P1Catalog.ElementName((Element)bagElement).ToUpper()+" · Chỉ nâng Pet cùng hệ",new Vector2(0,220),new Vector2(1100,45),27);
            for(int grade=1;grade<=7;grade++)
            {
                int g=grade;float x=(grade-4)*200;var tile=Panel(content,"Stone_"+grade,new Vector2(x,65),new Vector2(182,215),new Color(.09f,.23f,.23f));
                P1Art.Part(tile.transform,"Icon",P1Art.Stone(grade),new Vector2(0,35),new Vector2(95,95),P1Catalog.ElementColor((Element)bagElement));
                Button(tile.transform,"Lv"+grade+" · ×"+Progress.StoneCount((Element)bagElement,grade),new Vector2(0,-65),new Vector2(166,55),()=>{message=g==1 ? "Đá Lv1: sưu tầm, ghép/đổi sẽ có ở bản sau; không dùng nâng Lv1 → Lv2." : "Đá Lv"+g+": nâng Pet cùng hệ Lv"+(g-1)+" → Lv"+g+(bagElement==1 ? ". Rơi chắc chắn tại ải "+g : ". Chưa có nguồn farm P1.");Render();});
            }
            Text(content,"THƯỞNG GẦN NHẤT\n"+string.Join("\n",Progress.Data.rewardHistory.Take(3)),new Vector2(0,-160),new Vector2(1400,180),18);
            Button(content,"GHÉP ĐÁ — Coming Soon",new Vector2(0,-310),new Vector2(500,48),()=>{},false);
        }
        private void DrawBook()
        {
            if(catalog.p2Enabled){DrawP2Collection(true);return;}
            for(int i=0;i<7;i++){var p=catalog.pets[i];bool owned=Progress.Owned(p.id)!=null;float x=(i%4-1.5f)*355,y=140-(i/4)*280;Pet(content,p.id,new Vector2(x,y),.55f,!owned);Text(content,owned ? p.title+"\n"+p.rarity+" · Mộc · Lv"+Progress.Owned(p.id).level : "??? · Chưa thu phục\nBoss "+(i+1),new Vector2(x,y-120),new Vector2(335,65),20);}
        }
        private void DrawCards(){Text(content,"THẺ CHIẾN THUẬT\n\nTiếp Mana: +20, tối đa 2 lần/trận.\nKhơi Nộ: +20, tối đa 2 lần/trận.\nTiến hóa: tốn 80 Mana, một lần/trận.\n\nMỗi lượt dùng tối đa một thẻ trước khi swap.\nĐủ 60 Nộ + 30 Mana khi đánh kiếm: tự dùng tuyệt kỹ.\nTiến hóa không đổi Lv hoặc độ hiếm đã sở hữu.",Vector2.zero,new Vector2(1200,550),28);}
        public void StartBattle()
        {
            LeaveBattle();battleId=Guid.NewGuid().ToString("N");rewardSeed=Environment.TickCount;
            var template=Resources.Load<BattleRules>("P1BattleRules");battleRules=template!=null ? Instantiate(template) : ScriptableObject.CreateInstance<BattleRules>();
            var owned=Progress.Owned(Progress.Data.activePetId);battleRules.player=catalog.Stats(owned.id,owned.level);battleRules.enemy=catalog.Stats(catalog.stages[stage-1].petId,1,true,stage);battleRules.elements=catalog;battleRules.cinematicTurns=true;
            root.gameObject.SetActive(false);battleObject=new GameObject("P1BattleSession");var battle=battleObject.AddComponent<BattleController>();battle.Configure(battleRules);battle.RestartRequested=StartBattle;battle.Finished=OnBattleFinished;battle.ExitRequested=()=>{LeaveBattle();CurrentScreen="Island";Render();};
        }
        private void OnBattleFinished(BattleResult result)
        {
            lastResult=result;Award(result);StartCoroutine(ShowRewards(result));
        }
        private void Award(BattleResult result)
        {
            try{rewardText=practice ? "Đấu thử — không thưởng hoặc mở ải." : Progress.Reward(stage,result,battleId,new SeededBattleRandom(rewardSeed));}
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException){rewardText="Lưu thưởng chưa thành công. "+ex.Message;}
        }
        private IEnumerator ShowRewards(BattleResult result)
        {
            yield return new WaitForSeconds(1.05f);
            CurrentScreen="Result";Render();
            if(rewardText.StartsWith("Đã thu phục") || rewardText.StartsWith("Đã chinh phục"))yield return CaptureAnimation();
        }
        private IEnumerator CaptureAnimation()
        {
            var rig=content.GetComponentInChildren<ArticulatedPet>();if(rig==null)yield break;
            var orb=P1Art.Part(content,"CaptureOrb",P1Art.Circle,new Vector2(-400,40),new Vector2(130,130),new Color(.65f,1,.8f,.4f));
            for(float t=0;t<1;t+=Time.deltaTime){if(rig==null||orb==null)yield break;rig.transform.localScale=Vector3.one*Mathf.Lerp(1,.15f,t);orb.transform.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI));yield return null;}
            if(rig!=null)rig.transform.localScale=Vector3.one;if(orb!=null)Destroy(orb.gameObject);
        }
        private void DrawResult()
        {
            Text(content,lastResult==BattleResult.Victory ? "CHIẾN THẮNG · "+(practice ? "ĐẤU THỬ" : "ẢI "+stage) : lastResult==BattleResult.Draw ? "HÒA" : "THẤT BẠI",new Vector2(0,280),new Vector2(1300,65),38,new Color(1,.86f,.4f));
            Pet(content,catalog.stages[stage-1].petId,new Vector2(-400,35),1);Text(content,rewardText,new Vector2(240,65),new Vector2(850,220),27);
            Button(content,"ĐÁNH LẠI",new Vector2(-400,-260),new Vector2(310,65),StartBattle);
            Button(content,"VỀ ĐẢO",new Vector2(0,-260),new Vector2(310,65),()=>{LeaveBattle();CurrentScreen="Island";Render();});
            Button(content,"ẢI TIẾP",new Vector2(400,-260),new Vector2(310,65),()=>{LeaveBattle();stage++;practice=false;CurrentScreen="Prepare";Render();},!practice && lastResult==BattleResult.Victory && stage<7 && Progress.Data.unlockedStage>stage);
            if(rewardText.StartsWith("Lưu thưởng"))Button(content,"THỬ LƯU THƯỞNG LẠI",new Vector2(0,-345),new Vector2(500,45),()=>{StopAllCoroutines();Award(lastResult);StartCoroutine(ShowRewards(lastResult));});
        }
        private void LeaveBattle(){StopAllCoroutines();Time.timeScale=1;if(battleObject!=null){battleObject.SetActive(false);Destroy(battleObject);}if(battleRules!=null)Destroy(battleRules);}
        private void OnDestroy(){LeaveBattle();if(backdrop!=null)Destroy(backdrop);if(clickClip!=null)Destroy(clickClip);}
    }
    public sealed class HomeHint : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Action Show,Hide;
        public void OnPointerEnter(PointerEventData data){Show?.Invoke();}
        public void OnPointerExit(PointerEventData data){Hide?.Invoke();}
    }
}
