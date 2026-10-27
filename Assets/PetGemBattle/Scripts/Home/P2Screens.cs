using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
namespace PetGemBattle
{
    public sealed partial class HomeController
    {
        private int islandIndex,petPage,levelFilter,formFilter,stoneGrade=1;
        private bool previewEvolution;
        private AudioSource clickSource;private AudioClip clickClip;
        private void P2Click()
        {
            if(catalog==null || !catalog.p2Enabled || PlayerPrefs.GetInt("BattleMuted",0)==1)return;
            if(clickSource==null){clickSource=gameObject.AddComponent<AudioSource>();clickSource.playOnAwake=false;var samples=new float[2205];for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(i*.2f)*(1-i/(float)samples.Length)*.09f;clickClip=AudioClip.Create("UI soft chime",samples.Length,1,22050,false);clickClip.SetData(samples,0);}
            clickSource.PlayOneShot(clickClip);
        }
        private Image Artwork(Transform parent,string sheet,Vector2 at,Vector2 size,int part=0,Color? color=null)
        {var image=Panel(parent,"Art_"+sheet,at,size,color??Color.white);image.sprite=P2ArtLibrary.Get(sheet,part);image.useSpriteMesh=true;image.preserveAspect=true;image.gameObject.AddComponent<P2SilhouetteMesh>();return image;}
        private Image StoneArtwork(Transform parent,Element element,int grade,Vector2 at,Vector2 size)
        {
            var image=Artwork(parent,"stones",at,size,grade-1);
            image.material=Resources.Load<Material>("P2StoneMaterials/"+element);return image;
        }
        private RectTransform Glass(Transform parent,string name,Vector2 at,Vector2 size)
        {
            var image=Panel(parent,name,at,size,new Color(.035f,.17f,.19f,.91f));var outline=image.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.72f,.65f,.4f,.7f);outline.effectDistance=new Vector2(1,-1);return image.rectTransform;
        }
        private bool DrawP2Screen()
        {
            if(CurrentScreen!="Home" && CurrentScreen!="Pets")Button(root,"CÀI ĐẶT",new Vector2(665,396),new Vector2(170,42),()=>Navigate("Settings"));
            switch(CurrentScreen)
            {
                case "Challenges":DrawChallenges();return true;
                case "IslandPreview":DrawIslandPreview();return true;
                case "Settings":DrawSettings();return true;
                case "ConfirmReset":DrawConfirmation("XÓA TIẾN TRÌNH DEMO?","Cấp Pet, đá và tiến trình đảo sẽ về mặc định.\nBản lưu hiện tại được giữ trong file dự phòng.",()=>{Progress.ResetDemo();selectedPet=Progress.Data.activePetId;message="Đã reset demo về Lv1.";Home();});return true;
                case "ConfirmSeed":DrawConfirmation("CẤP ĐÁ THỬ NGHIỆM?","Thêm "+Progress.Mode.DemoStoneAmount+" viên mỗi loại đá, một lần cho save demo.\nĐây là tiện ích thử nghiệm offline, không phải phần thưởng boss.",()=>{message=Progress.SeedDemoStones() ? "Đã cấp đá demo, không cộng lặp khi mở lại." : "Save này đã nhận đá demo.";CurrentScreen="Settings";Render();});return true;
                default:return false;
            }
        }
        private void DrawP2Lobby()
        {
            Action arena=()=>Navigate("Challenges");
            Action pets=()=>{selectedPet=Progress.Data.activePetId;petPage=0;Navigate("Pets");};
            Action upgrade=()=>{selectedPet=Progress.Data.activePetId;Navigate("Upgrade");};
            Action book=()=>Navigate("Book");
            Action practiceAction=()=>{practice=true;stage=1;Navigate("Prepare");};
            Action bag=()=>Navigate("Bag");
            Action cardsAction=()=>Navigate("Cards");
            // The painted island itself is the visual button; keep only its label
            // and a transparent hit target over each destination.
            WorldHotspot("SÁCH PET",new Vector2(-520,285),new Vector2(310,165),new Color(1,.87f,.55f),book);
            WorldHotspot("ĐẤU THỬ",new Vector2(520,285),new Vector2(310,165),new Color(.86f,.91f,1),practiceAction);
            WorldHotspot("KHO PET",new Vector2(-545,45),new Vector2(325,195),new Color(.48f,1,.64f),pets);
            WorldHotspot("ĐẤU TRƯỜNG",new Vector2(0,80),new Vector2(390,245),new Color(.52f,.92f,1),arena);
            WorldHotspot("NÂNG CẤP",new Vector2(545,45),new Vector2(325,195),new Color(1,.63f,.42f),upgrade);
            WorldHotspot("KHO ĐÁ",new Vector2(-520,-235),new Vector2(325,190),new Color(.65f,.82f,1),bag);
            WorldHotspot("THẺ PHÉP",new Vector2(520,-235),new Vector2(325,190),new Color(.84f,.64f,1),cardsAction);
            DrawLobbyNavbar();
        }
        private void DrawLobbyNavbar()
        {
            var bar=Panel(root,"LobbyNavbar",new Vector2(0,391),new Vector2(1510,112),Color.white);
            bar.sprite=LobbyNavbarSkin.Bar;
            bar.type=Image.Type.Sliced;
            var group=bar.gameObject.AddComponent<CanvasGroup>();
            var crest=Rect(bar.transform,"ArenaCrest",new Vector2(-655,2),new Vector2(82,82));
            var outer=Panel(crest,"GoldDiamond",Vector2.zero,new Vector2(52,52),new Color(1,.77f,.29f));
            outer.rectTransform.localEulerAngles=new Vector3(0,0,45);
            var inner=Panel(crest,"SapphireDiamond",Vector2.zero,new Vector2(43,43),new Color(.02f,.19f,.45f));
            inner.rectTransform.localEulerAngles=new Vector3(0,0,45);
            var gem=Panel(crest,"CyanGem",Vector2.zero,new Vector2(18,42),new Color(.44f,.9f,1));
            gem.rectTransform.localEulerAngles=new Vector3(0,0,45);
            var glint=Panel(crest,"GemGlint",new Vector2(-4,8),new Vector2(8,22),new Color(1,1,.93f,.9f));
            glint.rectTransform.localEulerAngles=new Vector3(0,0,45);
            var title=Text(bar.transform,"PET ARENA",new Vector2(-450,17),new Vector2(340,46),38,new Color(1,.84f,.42f));
            title.fontStyle=FontStyle.Bold;
            title.alignment=TextAnchor.MiddleLeft;
            var titleOutline=title.gameObject.AddComponent<Outline>();titleOutline.effectColor=new Color(.03f,.06f,.21f);titleOutline.effectDistance=new Vector2(2,-2);
            var subtitle=Text(bar.transform,"THIÊN CẢNH NGŨ HÀNH",new Vector2(-450,-26),new Vector2(365,32),17,new Color(.75f,.94f,1));
            subtitle.fontStyle=FontStyle.Bold;
            subtitle.alignment=TextAnchor.MiddleLeft;
            Panel(bar.transform,"GoldDivider",new Vector2(-250,0),new Vector2(2,82),new Color(1,.77f,.30f,.8f));
            var noticePanel=Panel(bar.transform,"NavbarNotice",new Vector2(0,-91),new Vector2(370,44),new Color(.01f,.06f,.13f,0));
            var noticeText=Text(noticePanel.transform,"",Vector2.zero,new Vector2(360,40),18,new Color(1,.92f,.60f));
            noticeText.fontStyle=FontStyle.Bold;
            string[] labels={"SỰ KIỆN","VÒNG QUAY","ĐIỂM DANH","NẠP LẦN ĐẦU","CÀI ĐẶT"};
            string[] icons={"events","wheel","checkin","first-recharge","settings"};
            for(int i=0;i<labels.Length;i++)
            {
                int index=i;
                var tile=Panel(bar.transform,"Nav_"+icons[i],new Vector2(-135+i*165,0),new Vector2(142,104),new Color(1,1,1,.001f));tile.raycastTarget=true;
                var sprite=P3ArtLibrary.NavIcon(icons[i]);
                if(sprite!=null)
                {
                    var icon=P1Art.Part(tile.transform,"Logo",sprite,new Vector2(0,14),new Vector2(69,69),Color.white);
                    icon.preserveAspect=true;icon.raycastTarget=false;
                }
                var caption=Text(tile.transform,labels[i],new Vector2(0,-36),new Vector2(142,28),15,Color.white);
                caption.fontStyle=FontStyle.Bold;
                var stroke=caption.gameObject.AddComponent<Outline>();stroke.effectColor=new Color(.01f,.04f,.11f,1);stroke.effectDistance=new Vector2(2,-2);
                var button=tile.gameObject.AddComponent<Button>();button.targetGraphic=tile;
                button.onClick.AddListener(()=>
                {
                    P2Click();
                    if(index==4){Navigate("Settings");return;}
                    noticePanel.color=new Color(.01f,.06f,.13f,.76f);
                    noticeText.text=labels[index]+" · Sắp mở";
                });
                var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;
            }
            var toggle=Panel(root,"NavbarToggle",new Vector2(713,392),new Vector2(72,72),Color.white);
            toggle.sprite=LobbyNavbarSkin.Toggle;
            toggle.raycastTarget=true;
            var arrow=Panel(toggle.transform,"Arrow",Vector2.zero,new Vector2(38,38),Color.white);
            arrow.sprite=LobbyNavbarSkin.Arrow;
            var toggleButton=toggle.gameObject.AddComponent<Button>();toggleButton.targetGraphic=toggle;
            var toggleMotion=toggle.gameObject.AddComponent<LobbyNavbarMotion>();
            toggleMotion.Configure(bar.rectTransform,group,arrow.rectTransform,lobbyNavExpanded);
            toggleButton.onClick.AddListener(()=>{P2Click();lobbyNavExpanded=!lobbyNavExpanded;toggleMotion.SetExpanded(lobbyNavExpanded);});
            var toggleNavigation=toggleButton.navigation;toggleNavigation.mode=Navigation.Mode.None;toggleButton.navigation=toggleNavigation;
        }
        private void WorldHotspot(string label,Vector2 at,Vector2 size,Color accent,Action action,bool available=true,int icon=-1)
        {
            var hit=Panel(content,"World_"+label,at,size,new Color(1,1,1,.008f));hit.sprite=P1Art.Circle;hit.raycastTarget=true;
            RectTransform emblem=null;
            if(icon>=0)
            {
                var sprite=P3ArtLibrary.LobbyIcon(icon);
                if(sprite!=null)
                {
                    var image=P1Art.Part(hit.transform,"LocationEmblem",sprite,new Vector2(0,18),new Vector2(116,116),Color.white);
                    image.preserveAspect=true;emblem=image.rectTransform;
                }
            }
            var caption=Text(hit.transform,label,new Vector2(0,-size.y*.32f),new Vector2(size.x+55,45),available ? 23 : 16,available ? Color.white : new Color(.83f,.89f,.94f));
            var stroke=caption.gameObject.AddComponent<Outline>();stroke.effectColor=new Color(.02f,.07f,.15f,1);stroke.effectDistance=new Vector2(2,-2);
            var button=hit.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.onClick.AddListener(()=>{P2Click();Safe(action);});
            var colors=button.colors;colors.normalColor=new Color(1,1,1,.05f);colors.highlightedColor=new Color(accent.r,accent.g,accent.b,.36f);colors.pressedColor=new Color(accent.r,accent.g,accent.b,.55f);button.colors=colors;
            var motion=hit.gameObject.AddComponent<WorldHotspotMotion>();motion.Configure(emblem,null,accent);
        }
        private void DrawChallenges()
        {
            Text(content,"QUẦN ĐẢO LINH THÚ · CHỌN ĐẢO",new Vector2(0,292),new Vector2(1100,55),32,new Color(1,.92f,.72f));
            Text(content,"Kim → Mộc → Thổ → Thủy → Hỏa → Kim  ·  Thuận +25% / ngược −15%",new Vector2(0,237),new Vector2(1400,35),18);
            for(int i=0;i<catalog.islands.Length;i++)
            {
                int n=i;var island=catalog.islands[i];float x=(i-2)*295;var card=Glass(content,"Island_"+island.id,new Vector2(x,-12),new Vector2(277,425));
                card.GetComponent<Image>().color=new Color(.035f,.12f,.16f,.88f);card.gameObject.GetComponent<Outline>().effectColor=P1Catalog.ElementColor(island.element);
                Artwork(card,island.art,new Vector2(0,73),new Vector2(255,215));
                P1Art.Part(card,"ElementBadge",P3ArtLibrary.Element(island.element),new Vector2(-102,-62),new Vector2(48,48),P1Catalog.ElementColor(island.element));
                Text(card,island.title,new Vector2(18,-64),new Vector2(215,45),24,P1Catalog.ElementColor(island.element));
                Text(card,island.description+"\nLv"+island.recommendedLevel+"+ · "+P1Catalog.ElementName(island.element),new Vector2(0,-121),new Vector2(255,75),18);
                Button(card,island.playable ? "KHÁM PHÁ · 7 ẢI" : "XEM TRƯỚC",new Vector2(0,-182),new Vector2(250,45),()=>{islandIndex=n;Navigate(island.playable ? "Island" : "IslandPreview");});
            }
        }
        private void DrawIslandPreview()
        {
            var island=catalog.islands[islandIndex];
            Artwork(content,island.art,new Vector2(-335,20),new Vector2(710,490));
            var card=Glass(content,"PreviewInfo",new Vector2(380,20),new Vector2(640,550));
            Text(card,island.title,new Vector2(0,228),new Vector2(610,60),34,P1Catalog.ElementColor(island.element));
            Pet(card,island.previewPetId,new Vector2(0,38),.85f);
            Text(card,island.description,new Vector2(0,-156),new Vector2(600,55),22);
            Button(card,"BOSS / ẢI — Coming Soon",new Vector2(0,-222),new Vector2(580,55),()=>{},false);
            for(int i=0;i<7;i++)Button(content,""+(i+1),new Vector2(-620+i*94,-286),new Vector2(73,48),()=>{},false);
        }
        private void DrawP2StageMap()
        {
            Artwork(content,"island-wood",new Vector2(0,0),new Vector2(1480,620),0,new Color(1,1,1,.45f));
            Text(content,"MỘC TINH · CON ĐƯỜNG RỄ CỔ",new Vector2(0,303),new Vector2(1100,47),29,new Color(1,.93f,.7f));
            Vector2[] points={new Vector2(-590,-176),new Vector2(-402,35),new Vector2(-172,-148),new Vector2(50,70),new Vector2(290,-143),new Vector2(532,31),new Vector2(390,233)};
            for(int i=1;i<points.Length;i++)
            {
                Vector2 delta=points[i]-points[i-1];var path=Panel(content,"Route",Vector2.Lerp(points[i],points[i-1],.5f),new Vector2(delta.magnitude,5),new Color(.92f,.83f,.45f,.8f));path.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            }
            for(int i=0;i<7;i++)
            {
                int n=i+1;var boss=catalog.stages[i];var pet=catalog.Pet(boss.petId);bool unlocked=n<=Progress.Data.unlockedStage,cleared=Progress.Data.clearedStages.Contains(n);
                var node=Button(content,n+". "+pet.title+"\n"+pet.rarity+" · "+(cleared ? "Đã chinh phục" : unlocked ? "Lv"+boss.encounter.recommendedLevel+"+" : "Khóa"),points[i]+new Vector2(0,-63),new Vector2(184,62),()=>{stage=n;practice=false;Navigate("Prepare");},unlocked);
                Artwork(content,pet.artSheet,points[i]+new Vector2(0,32),new Vector2(132,136),0,unlocked ? Color.white : new Color(.07f,.14f,.15f));
                P1Art.Part(content,"StagePedestal",P1Art.Circle,points[i]+new Vector2(0,-29),new Vector2(130,22),new Color(.85f,.77f,.48f,.75f));
            }
        }
        private void DrawP2Collection(bool book=false)
        {
            if(!book){DrawPetCollection();return;}
            string[] elements={"Tất cả","Kim","Mộc","Thủy","Hỏa","Thổ"};
            string[] rarities={"Tất cả","C","B","A","S","SS","SSS"};
            string[] sorts={"Lv","Bậc","HP","ATK"};
            string[] levels={"Tất cả","1","2","3","4","5","6","7"};
            string[] forms={"Tất cả","Gốc","Đã tiến hóa"};
            Panel(content,"FilterBackdrop",new Vector2(0,290),new Vector2(1490,62),new Color(.01f,.07f,.11f,.54f));
            DropFilter("Hệ",elements,elementFilter+1,new Vector2(-560,290),i=>{elementFilter=i-1;petPage=0;Render();});
            DropFilter("Bậc",rarities,rarityFilter+1,new Vector2(-325,290),i=>{rarityFilter=i-1;petPage=0;Render();});
            DropFilter("Sắp xếp",sorts,Array.IndexOf(sorts,sort),new Vector2(-90,290),i=>{sort=sorts[i];Render();});
            DropFilter("Lv",levels,levelFilter,new Vector2(145,290),i=>{levelFilter=i;petPage=0;Render();});
            DropFilter("Form",forms,formFilter,new Vector2(380,290),i=>{formFilter=i;petPage=0;Render();});
            Button(content,"XÓA LỌC",new Vector2(620,290),new Vector2(170,42),()=>{elementFilter=rarityFilter=-1;levelFilter=formFilter=petPage=0;sort="Lv";Render();});
            var owned=Progress.Filter(elementFilter,rarityFilter,sort).Where(p=>(levelFilter==0||p.level==levelFilter) && (formFilter==0 || (catalog.Forms(p.id,p.level).Length>0)==(formFilter==2))).ToArray();
            var bookPets=catalog.pets.Where(p=>(elementFilter<0 || (int)p.element==elementFilter) && (rarityFilter<0 || (int)p.rarity==rarityFilter))
                .Select(p=>catalog.Stats(p.id,Progress.Owned(p.id)?.level??1))
                .Where(p=>(levelFilter==0 || p.level==levelFilter) && (formFilter==0 || (p.permanentForm>0)==(formFilter==2)))
                .OrderByDescending(p=>sort=="Bậc" ? (int)p.rarity : sort=="HP" ? p.maxHp : sort=="ATK" ? p.attack : p.level).ThenBy(p=>p.id);
            var ids=book ? bookPets.Select(p=>p.id).ToArray() : owned.Select(p=>p.id).ToArray();
            int pages=Math.Max(1,(ids.Length+7)/8);petPage=Math.Min(petPage,pages-1);
            for(int slot=0;slot<8 && petPage*8+slot<ids.Length;slot++)
            {
                string id=ids[petPage*8+slot];var instance=Progress.Owned(id);var p=catalog.Pet(id);var stats=catalog.Stats(id,instance?.level??1);float x=(slot%4-1.5f)*355,y=114-(slot/4)*236;
                var tile=Glass(content,"Pet_"+id,new Vector2(x,y),new Vector2(330,216));
                Artwork(tile,catalog.Sheet(id,stats.level),new Vector2(-84,16),new Vector2(145,164),0,instance==null ? new Color(.06f,.12f,.12f) : Color.white);
                Text(tile,instance==null ? "Chưa sở hữu" : stats.name,new Vector2(68,61),new Vector2(184,54),19,new Color(1,.92f,.72f));
                Text(tile,p.rarity+" · "+P1Catalog.ElementName(p.element)+"\nLv"+stats.level+" · Form "+stats.permanentForm,new Vector2(70,-2),new Vector2(180,63),17);
                Button(tile,id==Progress.Data.activePetId ? "ĐANG XUẤT CHIẾN" : instance!=null ? "CHI TIẾT · OWNED" : "CHƯA THU PHỤC",new Vector2(0,-78),new Vector2(305,42),()=>{selectedPet=id;previewEvolution=false;Navigate("Detail");},instance!=null);
            }
            Text(content,(book ? "SÁCH PET" : "KHO PET")+" · "+ids.Length+" loài · Trang "+(petPage+1)+"/"+pages,new Vector2(0,-306),new Vector2(700,40),20);
            Button(content,"‹ TRƯỚC",new Vector2(-510,-310),new Vector2(210,43),()=>{petPage--;Render();},petPage>0);
            Button(content,"TIẾP ›",new Vector2(510,-310),new Vector2(210,43),()=>{petPage++;Render();},petPage+1<pages);
        }
        private void DropFilter(string title,string[] options,int selected,Vector2 at,Action<int> choose)
        {
            selected=Mathf.Clamp(selected,0,options.Length-1);
            Button(content,title+": "+options[selected]+"  ▾",at,new Vector2(220,42),()=>ShowDropFilter(title,options,at,choose));
        }
        private void ShowDropFilter(string title,string[] options,Vector2 at,Action<int> choose)
        {
            var blocker=Panel(root,"DropdownBlocker",Vector2.zero,new Vector2(1600,900),new Color(.01f,.04f,.09f,.33f));blocker.raycastTarget=true;
            var close=blocker.gameObject.AddComponent<Button>();close.targetGraphic=blocker;close.onClick.AddListener(()=>Destroy(blocker.gameObject));
            var popup=Glass(root,"Dropdown_"+title,new Vector2(at.x,at.y-15-options.Length*22),new Vector2(236,options.Length*44+12));
            popup.SetAsLastSibling();
            for(int i=0;i<options.Length;i++)
            {
                int index=i;
                Button(popup,options[i],new Vector2(0,(options.Length-1)*22-i*44),new Vector2(218,40),()=>choose(index));
            }
        }
        private void DrawP2Detail()
        {
            var owned=Progress.Owned(selectedPet);int level=previewEvolution ? 7 : owned.level;var p=catalog.Stats(selectedPet,level);var species=catalog.Pet(selectedPet);
            Pet(content,selectedPet,new Vector2(-390,20),1.3f,false,level);
            var panel=CollectionFrame(content,"DetailInfo",new Vector2(280,10),new Vector2(840,580),.96f).rectTransform;
            CollectionText(panel,p.name,new Vector2(0,232),new Vector2(790,58),30,new Color(1,.92f,.72f));
            CollectionText(panel,species.rarity+" · "+P1Catalog.ElementName(p.element)+" · Lv"+level+" · Form "+p.permanentForm,
                new Vector2(0,183),new Vector2(790,40),20,new Color(.8f,.94f,1));
            Text(panel,species.description,new Vector2(0,131),new Vector2(790,62),20);
            CollectionText(panel,Stats(p)+"   ·   Chí mạng "+Mathf.RoundToInt(p.baseCritChance*100)+"%",
                new Vector2(0,67),new Vector2(790,48),22,new Color(.75f,1,.84f));
            CollectionText(panel,p.signatureName,new Vector2(0,5),new Vector2(790,42),23,new Color(1,.82f,.49f));
            Text(panel,p.signatureDescription,new Vector2(0,-40),new Vector2(790,50),18);
            Text(panel,"Tốn "+p.signatureManaCost+" mana  ·  "+p.signatureRageCost+" nộ",
                new Vector2(0,-82),new Vector2(790,35),18,new Color(.74f,.92f,1));
            string milestones=species.forms==null || species.forms.Length==0 ? "Loài này không đổi form theo Lv." : string.Join(" · ",species.forms.Select(f=>"Lv"+f.minLevel+": "+f.displayName));
            Text(panel,milestones,new Vector2(0,-140),new Vector2(790,45),18,new Color(.73f,.95f,.78f));
            CollectionButton(panel,previewEvolution ? "XEM FORM ĐANG SỞ HỮU" : "XEM TRƯỚC LV7 / FORM",
                new Vector2(0,-222),new Vector2(690,48),()=>{previewEvolution=!previewEvolution;Render();},
                species.forms!=null && species.forms.Length>0 && owned.level<catalog.maxLevel);
            bool active=Progress.Data.activePetId==selectedPet;
            CollectionButton(content,active ? "ĐANG XUẤT CHIẾN" : "XUẤT CHIẾN",new Vector2(-230,-320),new Vector2(330,55),
                ()=>{Progress.Select(selectedPet);Home();},!active,new Color(.48f,1,.8f));
            CollectionButton(content,"NÂNG CẤP",new Vector2(230,-320),new Vector2(330,55),()=>Navigate("Upgrade"),true,new Color(1,.76f,.38f));
        }
        private void DrawP2Upgrade()
        {
            var owned=Progress.Owned(selectedPet);int next=Math.Min(7,owned.level+1);var before=catalog.Stats(selectedPet,owned.level);var after=catalog.Stats(selectedPet,next);bool evolution=before.permanentForm!=after.permanentForm,max=owned.level>=7;
            var species=catalog.Pet(selectedPet);
            var upcomingForm=species.forms==null ? null : species.forms.OrderBy(f=>f.minLevel).FirstOrDefault(f=>f.minLevel>owned.level);
            bool finalForm=species.forms!=null && species.forms.Length>0 && upcomingForm==null;
            var left=CollectionFrame(content,"UpgradePortrait",new Vector2(-410,15),new Vector2(620,600),.96f).rectTransform;
            Text(left,before.name,new Vector2(0,252),new Vector2(590,62),27,new Color(1,.92f,.73f));
            Pet(left,selectedPet,new Vector2(upcomingForm!=null ? -145 : 0,0),upcomingForm!=null ? .72f : 1.0f,false,owned.level);
            if(upcomingForm!=null)
            {
                Pet(left,selectedPet,new Vector2(151,20),.76f,false,upcomingForm.minLevel);
                Text(left,"HIỆN TẠI  →  "+upcomingForm.displayName+" (Lv"+upcomingForm.minLevel+")",new Vector2(0,-173),new Vector2(590,65),17,new Color(.7f,1,.82f));
            }
            else if(finalForm)Text(left,"ĐÃ TIẾN HÓA TỐI ĐA",new Vector2(0,-173),new Vector2(590,65),21,new Color(1,.87f,.54f));
            else Text(left,"KHÔNG CÓ HÌNH THÁI TIẾN HÓA",new Vector2(0,-173),new Vector2(590,65),17,new Color(.7f,1,.82f));
            for(int lv=1;lv<=7;lv++){var dot=P1Art.Part(left,"Level_"+lv,P1Art.Circle,new Vector2((lv-4)*73,-230),new Vector2(48,48),lv<=owned.level ? new Color(.8f,.76f,.4f) : new Color(.16f,.31f,.31f));Text(dot.transform,lv.ToString(),Vector2.zero,new Vector2(40,40),19);}
            var info=CollectionFrame(content,"UpgradeNumbers",new Vector2(335,15),new Vector2(760,600),.96f).rectTransform;
            Text(info,max ? "ĐÃ ĐẠT LV7" : "LV"+owned.level+" → LV"+next,new Vector2(0,245),new Vector2(700,55),31);
            Text(info,max ? "HP  "+before.maxHp+"    ATK  "+before.attack+"    DEF  "+before.defense+"\nĐã đạt cấp độ tối đa." :
                "HP    "+before.maxHp+" → "+after.maxHp+"   (+"+(after.maxHp-before.maxHp)+")\nATK  "+before.attack+" → "+after.attack+"   (+"+(after.attack-before.attack)+")\nDEF  "+before.defense+" → "+after.defense+"   (+"+(after.defense-before.defense)+")",new Vector2(0,99),new Vector2(710,160),27,new Color(.69f,.98f,.76f));
            StoneArtwork(info,before.element,next,new Vector2(-240,-80),new Vector2(105,100));
            int count=Progress.StoneCount(before.element,next);string required="Đá "+P1Catalog.ElementName(before.element)+" Lv"+next;
            Text(info,max ? "Không tiêu thêm đá" : "Cần 1 "+required+"\nĐang có "+count,new Vector2(70,-80),new Vector2(490,80),21);
            Text(info,finalForm ? "ĐÃ TIẾN HÓA TỐI ĐA" : upcomingForm!=null ? "Hình thái kế tiếp mở tại Lv"+upcomingForm.minLevel : "Cấp độ tăng chỉ số, không đổi hình thái",
                new Vector2(0,-160),new Vector2(690,40),19,new Color(.9f,.95f,1));
            int expected=owned.level;string tx=Guid.NewGuid().ToString("N");
            CollectionButton(info,max ? finalForm ? "ĐÃ TIẾN HÓA TỐI ĐA" : "ĐÃ ĐẠT LV7" : count<1 ? "Thiếu "+required : "XÁC NHẬN NÂNG CẤP",new Vector2(0,-222),new Vector2(690,65),()=>{if(Progress.Upgrade(selectedPet,expected,tx)){message=evolution ? "Đã mở "+after.name+"!" : "Đã nâng lên Lv"+next;Render();foreach(var rig in content.GetComponentsInChildren<ArticulatedPet>())StartCoroutine(UpgradeCelebration(rig,evolution));}},!max && count>0);
        }
        private IEnumerator UpgradeCelebration(ArticulatedPet rig,bool evolution)
        {
            rig.SetPose(evolution ? PetPose.Evolution : PetPose.Victory,1.6f);yield return new WaitForSeconds(1.65f);if(rig!=null)rig.ResetForm();
        }
        private void DrawP2Bag()
        {
            for(int i=0;i<5;i++){int e=i;Button(content,P1Catalog.ElementName((Element)i),new Vector2((i-2)*247,290),new Vector2(225,43),()=>{bagElement=e;Render();});}
            for(int g=1;g<=7;g++){int grade=g;float x=(g-4)*200;var tile=Glass(content,"Stone_"+g,new Vector2(x,100),new Vector2(182,230));StoneArtwork(tile,(Element)bagElement,g,new Vector2(0,38),new Vector2(130,140));Button(tile,"Lv"+g+" · ×"+Progress.StoneCount((Element)bagElement,g),new Vector2(0,-78),new Vector2(164,47),()=>{stoneGrade=grade;Render();});}
            var detail=Glass(content,"StoneInfo",new Vector2(-350,-182),new Vector2(740,250));StoneArtwork(detail,(Element)bagElement,stoneGrade,new Vector2(-242,0),new Vector2(185,185));
            Text(detail,"Đá "+P1Catalog.ElementName((Element)bagElement)+" Lv"+stoneGrade+"\n"+(stoneGrade==1 ? "Sưu tầm / nguyên liệu cho bản sau." : "Nâng Pet cùng hệ Lv"+(stoneGrade-1)+" → Lv"+stoneGrade)+"\n"+(bagElement==1 ? "Nguồn: Boss Mộc Tinh ải "+stoneGrade : "Chưa có nguồn farm P2."),new Vector2(90,0),new Vector2(510,210),21);
            Text(content,"THƯỞNG GẦN NHẤT\n"+string.Join("\n",Progress.Data.rewardHistory.Take(3)),new Vector2(405,-166),new Vector2(680,220),17);
            Button(content,"GHÉP / ĐỔI ĐÁ — Coming Soon",new Vector2(385,-312),new Vector2(600,45),()=>{},false);
        }
        private void DrawSettings()
        {
            Text(content,"CÀI ĐẶT · DEMO OFFLINE",new Vector2(0,260),new Vector2(1100,60),33,new Color(1,.93f,.7f));
            Button(content,PlayerPrefs.GetInt("BattleMuted",0)==0 ? "ÂM THANH: BẬT" : "ÂM THANH: TẮT",new Vector2(0,137),new Vector2(750,67),()=>{PlayerPrefs.SetInt("BattleMuted",1-PlayerPrefs.GetInt("BattleMuted",0));PlayerPrefs.Save();Render();});
            Button(content,PlayerPrefs.GetInt("P2ReducedVfx",0)==0 ? "VFX: ĐẦY ĐỦ" : "VFX: GIẢM CHUYỂN ĐỘNG",new Vector2(0,43),new Vector2(750,67),()=>{PlayerPrefs.SetInt("P2ReducedVfx",1-PlayerPrefs.GetInt("P2ReducedVfx",0));PlayerPrefs.Save();Render();});
            Button(content,"CẤP ĐÁ THỬ NGHIỆM · MỘT LẦN / SAVE",new Vector2(0,-65),new Vector2(750,64),()=>Navigate("ConfirmSeed"),Progress.Mode!=null && Progress.Mode.DemoTools);
            Button(content,"RESET SAVE DEMO…",new Vector2(0,-172),new Vector2(750,64),()=>Navigate("ConfirmReset"),Progress.Mode!=null && Progress.Mode.DemoTools);
            Text(content,"Reset cần xác nhận. Pet, Lv, form và đá được lưu vào JSON local.\nCác đảo khác hiện chỉ xem trước; không có trận giả hoặc giao dịch.",new Vector2(0,-280),new Vector2(1250,85),20);
        }
        private void DrawConfirmation(string title,string explanation,Action confirm)
        {
            var panel=Glass(content,"Confirmation",Vector2.zero,new Vector2(1120,450));
            Text(panel,title,new Vector2(0,130),new Vector2(1040,65),31,new Color(1,.85f,.55f));Text(panel,explanation,new Vector2(0,20),new Vector2(1040,120),25);
            Button(panel,"HỦY",new Vector2(-240,-125),new Vector2(360,65),Back);Button(panel,"XÁC NHẬN",new Vector2(240,-125),new Vector2(360,65),confirm);
        }
    }
    public sealed class LobbyParallax : MonoBehaviour
    {
        private RectTransform rect;private Vector2 start;
        private void Start(){rect=(RectTransform)transform;start=rect.anchoredPosition;}
        private void Update()
        {
            if(rect==null || PlayerPrefs.GetInt("P2ReducedVfx",0)==1)return;Vector2 mouse=Mouse.current==null ? Vector2.zero : Mouse.current.position.ReadValue();var at=new Vector2(mouse.x/Mathf.Max(1,Screen.width)-.5f,mouse.y/Mathf.Max(1,Screen.height)-.5f);
            rect.anchoredPosition=Vector2.Lerp(rect.anchoredPosition,start+at*9,Time.unscaledDeltaTime*3);
        }
    }
    public sealed class WorldHotspotMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform emblem;
        private Image halo;
        private Color accent;
        private bool hovered;
        public void Configure(RectTransform icon,Image glow,Color color){emblem=icon;halo=glow;accent=color;}
        public void OnPointerEnter(PointerEventData eventData){hovered=true;}
        public void OnPointerExit(PointerEventData eventData){hovered=false;}
        private void Update()
        {
            float wave=Mathf.Sin(Time.unscaledTime*2.4f+transform.GetSiblingIndex()*.7f);
            if(emblem!=null)
            {
                emblem.anchoredPosition=new Vector2(0,18+wave*3);
                emblem.localScale=Vector3.one*(hovered ? 1.12f : 1+wave*.025f);
            }
            if(halo!=null)halo.color=new Color(accent.r,accent.g,accent.b,hovered ? .29f : .12f+wave*.025f);
        }
    }
}
