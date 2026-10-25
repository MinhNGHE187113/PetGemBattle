using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
            Button(root,"CÀI ĐẶT",new Vector2(-665,346),new Vector2(170,33),()=>Navigate("Settings"));
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
            var owned=Progress.Owned(Progress.Data.activePetId);var stats=catalog.Stats(owned.id,owned.level);
            Text(content,"HÀNH TRÌNH LINH THÚ",new Vector2(0,287),new Vector2(730,55),34,new Color(1,.91f,.66f));
            Pet(content,owned.id,new Vector2(0,20),1.18f);
            var plaque=Glass(content,"EquippedPet",new Vector2(0,-222),new Vector2(620,116));
            Text(plaque,stats.name,new Vector2(0,32),new Vector2(590,40),28,new Color(1,.93f,.72f));
            Text(plaque,P1Catalog.ElementName(stats.element)+" · Bậc "+stats.rarity+" · Lv"+stats.level+"/7 · Form "+stats.permanentForm,new Vector2(0,0),new Vector2(590,35),20);
            Text(plaque,Stats(stats),new Vector2(0,-34),new Vector2(590,32),19);
            string[] names={"KHO PET","KHO ĐỒ","NÂNG CẤP","SÁCH PET"};string[] pages={"Pets","Bag","Upgrade","Book"};
            for(int i=0;i<4;i++){int n=i;Button(content,names[i],new Vector2(-535,205-i*108),new Vector2(330,78),()=>{selectedPet=Progress.Data.activePetId;petPage=0;Navigate(pages[n]);});}
            var challenge=Button(content,"KHIÊU CHIẾN\nNăm thiên đảo đang chờ",new Vector2(530,174),new Vector2(380,126),()=>Navigate("Challenges"));challenge.GetComponentInChildren<Text>().fontSize=25;
            Button(content,"ĐẤU THỬ",new Vector2(530,32),new Vector2(330,74),()=>{practice=true;stage=1;Navigate("Prepare");});
            Button(content,"THẺ PHÉP · HƯỚNG DẪN",new Vector2(530,-70),new Vector2(330,74),()=>Navigate("Cards"));
            Text(content,Progress.Mode!=null && Progress.Mode.DemoFullCollection ? "DEMO OFFLINE · ĐÃ MỞ TOÀN BỘ "+Progress.Data.pets.Count+" PET" : "THU PHỤC LINH THÚ QUA CÁC ẢI",new Vector2(530,-155),new Vector2(410,70),19,new Color(.85f,.96f,.83f));
            string[] soon={"PvP","Cửa hàng / Triệu hồi","Bang hội","Nhiệm vụ / Xếp hạng"};
            for(int i=0;i<soon.Length;i++)Button(content,soon[i]+"\nComing Soon",new Vector2((i-1.5f)*345,-335),new Vector2(320,60),()=>{},false);
        }
        private void DrawChallenges()
        {
            Text(content,"KHIÊU CHIẾN · NGŨ HÀNH",new Vector2(0,292),new Vector2(1000,55),32,new Color(1,.92f,.72f));
            Text(content,"Kim → Mộc → Thổ → Thủy → Hỏa → Kim  ·  Thuận +25% / ngược −15%",new Vector2(0,237),new Vector2(1400,35),18);
            for(int i=0;i<catalog.islands.Length;i++)
            {
                int n=i;var island=catalog.islands[i];float x=(i-2)*295;var card=Glass(content,"Island_"+island.id,new Vector2(x,-12),new Vector2(277,425));
                Artwork(card,island.art,new Vector2(0,73),new Vector2(255,215));
                Text(card,island.title,new Vector2(0,-64),new Vector2(265,45),27,P1Catalog.ElementColor(island.element));
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
            Button(content,"Hệ: "+(elementFilter<0 ? "Tất cả" : P1Catalog.ElementName((Element)elementFilter)),new Vector2(-540,290),new Vector2(240,42),()=>{elementFilter=(elementFilter+2)%6-1;petPage=0;Render();});
            Button(content,"Bậc: "+(rarityFilter<0 ? "Tất cả" : ((PetRarity)rarityFilter).ToString()),new Vector2(-270,290),new Vector2(240,42),()=>{rarityFilter=(rarityFilter+2)%7-1;petPage=0;Render();});
            Button(content,"Sắp xếp: "+sort,new Vector2(0,290),new Vector2(240,42),()=>{string[] sorts={"Lv","Bậc","HP","ATK","SPD"};sort=sorts[(Array.IndexOf(sorts,sort)+1)%5];Render();});
            Button(content,"Lv: "+(levelFilter==0 ? "Tất cả" : levelFilter.ToString()),new Vector2(270,290),new Vector2(240,42),()=>{levelFilter=(levelFilter+1)%8;petPage=0;Render();});
            Button(content,"Form: "+new[]{"Tất cả","Gốc","Đã tiến hóa"}[formFilter],new Vector2(540,290),new Vector2(240,42),()=>{formFilter=(formFilter+1)%3;petPage=0;Render();});
            var owned=Progress.Filter(elementFilter,rarityFilter,sort).Where(p=>(levelFilter==0||p.level==levelFilter) && (formFilter==0 || (catalog.Forms(p.id,p.level).Length>0)==(formFilter==2))).ToArray();
            var bookPets=catalog.pets.Where(p=>(elementFilter<0 || (int)p.element==elementFilter) && (rarityFilter<0 || (int)p.rarity==rarityFilter))
                .Select(p=>catalog.Stats(p.id,Progress.Owned(p.id)?.level??1))
                .Where(p=>(levelFilter==0 || p.level==levelFilter) && (formFilter==0 || (p.permanentForm>0)==(formFilter==2)))
                .OrderByDescending(p=>sort=="Bậc" ? (int)p.rarity : sort=="HP" ? p.maxHp : sort=="ATK" ? p.attack : sort=="SPD" ? p.speed : p.level).ThenBy(p=>p.id);
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
        private void DrawP2Detail()
        {
            var owned=Progress.Owned(selectedPet);int level=previewEvolution ? 7 : owned.level;var p=catalog.Stats(selectedPet,level);var species=catalog.Pet(selectedPet);
            Pet(content,selectedPet,new Vector2(-390,20),1.3f,false,level);
            var panel=Glass(content,"DetailInfo",new Vector2(280,10),new Vector2(840,580));
            Text(panel,p.name,new Vector2(0,227),new Vector2(800,65),30,new Color(1,.92f,.72f));
            Text(panel,species.rarity+" · "+P1Catalog.ElementName(p.element)+" · Lv"+level+" · Form "+p.permanentForm+"\n"+species.description+"\n\n"+Stats(p)+"\nChí mạng "+Mathf.RoundToInt(p.baseCritChance*100)+"%",new Vector2(0,65),new Vector2(790,230),23);
            string milestones=species.forms==null || species.forms.Length==0 ? "Loài này không đổi form theo Lv." : string.Join(" · ",species.forms.Select(f=>"Lv"+f.minLevel+": "+f.displayName));
            Text(panel,milestones,new Vector2(0,-100),new Vector2(790,75),19,new Color(.73f,.95f,.78f));
            Button(panel,previewEvolution ? "XEM FORM ĐANG SỞ HỮU" : "XEM TRƯỚC LV7 / FORM",new Vector2(0,-178),new Vector2(720,44),()=>{previewEvolution=!previewEvolution;Render();});
            Button(content,"XUẤT CHIẾN",new Vector2(-230,-320),new Vector2(330,55),()=>{Progress.Select(selectedPet);Home();});
            Button(content,"NÂNG CẤP",new Vector2(230,-320),new Vector2(330,55),()=>Navigate("Upgrade"));
        }
        private void DrawP2Upgrade()
        {
            var owned=Progress.Owned(selectedPet);int next=Math.Min(7,owned.level+1);var before=catalog.Stats(selectedPet,owned.level);var after=catalog.Stats(selectedPet,next);bool evolution=before.permanentForm!=after.permanentForm,max=owned.level>=7;
            var left=Glass(content,"UpgradePortrait",new Vector2(-410,15),new Vector2(620,600));
            Text(left,before.name,new Vector2(0,252),new Vector2(590,62),27,new Color(1,.92f,.73f));
            Pet(left,selectedPet,new Vector2(evolution ? -110 : 0,0),evolution ? .8f : 1.0f,false,owned.level);
            if(evolution){Pet(left,selectedPet,new Vector2(150,27),.85f,false,next);Text(left,"FORM HIỆN TẠI    →    "+after.name,new Vector2(0,-173),new Vector2(590,65),17,new Color(.7f,1,.82f));}
            for(int lv=1;lv<=7;lv++){var dot=P1Art.Part(left,"Level_"+lv,P1Art.Circle,new Vector2((lv-4)*73,-230),new Vector2(48,48),lv<=owned.level ? new Color(.8f,.76f,.4f) : new Color(.16f,.31f,.31f));Text(dot.transform,lv.ToString(),Vector2.zero,new Vector2(40,40),19);}
            var info=Glass(content,"UpgradeNumbers",new Vector2(335,15),new Vector2(760,600));
            Text(info,max ? "ĐÃ ĐẠT LV7" : "LV"+owned.level+" → LV"+next,new Vector2(0,245),new Vector2(700,55),31);
            Text(info,"HP    "+before.maxHp+" → "+after.maxHp+"   (+"+(after.maxHp-before.maxHp)+")\nATK  "+before.attack+" → "+after.attack+"   (+"+(after.attack-before.attack)+")\nDEF  "+before.defense+" → "+after.defense+"   (+"+(after.defense-before.defense)+")\nSPD  "+before.speed+" → "+after.speed+"   (+"+(after.speed-before.speed)+")",new Vector2(0,99),new Vector2(710,206),27,new Color(.69f,.98f,.76f));
            StoneArtwork(info,before.element,next,new Vector2(-240,-80),new Vector2(105,100));
            int count=Progress.StoneCount(before.element,next);string required="Đá "+P1Catalog.ElementName(before.element)+" Lv"+next;
            Text(info,max ? "Không tiêu thêm đá" : "Cần 1 "+required+"\nĐang có "+count,new Vector2(70,-80),new Vector2(490,80),21);
            int expected=owned.level;string tx=Guid.NewGuid().ToString("N");
            Button(info,max ? "ĐÃ TỐI ĐA" : count<1 ? "Thiếu "+required : "XÁC NHẬN NÂNG CẤP",new Vector2(0,-222),new Vector2(690,65),()=>{if(Progress.Upgrade(selectedPet,expected,tx)){message=evolution ? "Đã mở "+after.name+"!" : "Đã nâng lên Lv"+next;Render();foreach(var rig in content.GetComponentsInChildren<ArticulatedPet>())StartCoroutine(UpgradeCelebration(rig,evolution));}},!max && count>0);
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
}
