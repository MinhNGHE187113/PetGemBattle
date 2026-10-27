using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace PetGemBattle
{
    [Serializable] public sealed class EvolutionFormDefinition
    {
        public int minLevel;
        public string displayName,artSheet;
        public float hp=1.12f,attack=1.18f,defense=1.1f;
    }
    [Serializable] public sealed class BossEncounterProfile
    {
        public int hp,recommendedLevel;
        public float outgoingScale=.52f,normalCap=.24f,specialCap=.30f;
        public float awakenThreshold=.30f,awakenAttack=1.18f,awakenDefense=1.08f,awakenShield=.08f,awakenSeconds=2f;
    }
    [Serializable] public sealed class ChallengeIslandDefinition
    {
        public string id,title,description,previewPetId,art;
        public Element element;
        public bool playable;
        public int recommendedLevel=1;
    }
    public static class P2Content
    {
        public static void Upgrade(P1Catalog catalog)
        {
            var list=catalog.pets.ToList();
            string[] ids={"fire-lion","metal-bird","water-otter","earth-turtle"};
            string[] titles={"Hỏa Lân Nhí","Thiết Điểu","Thủy Ly","Sa Quy"};
            Element[] types={Element.Fire,Element.Metal,Element.Water,Element.Earth};
            int[,] stats={{325,31,8},{310,30,10},{335,28,10},{350,28,12}};
            for(int i=0;i<4;i++)if(!list.Any(p=>p.id==ids[i]))list.Add(new PetSpecies{id=ids[i],title=titles[i],description="Linh thú "+P1Catalog.ElementName(types[i])+" · đồng hành thử nghiệm P2.",element=types[i],rarity=PetRarity.B,hp=stats[i,0],attack=stats[i,1],defense=stats[i,2],rig=8+i});
            AddLegendary(list,"wood-s","Bích Phong Kỳ Lân",Element.Wood,PetRarity.S,600,52,21,"Vạn Diệp Phong Ấn","Sừng quét lá tạo dấu ấn phong ấn.","p3-wood-s");
            AddLegendary(list,"water-s","Triều Ca Long Ngư",Element.Water,PetRarity.S,560,54,19,"Hải Triều Cửu Khúc","Lượn qua chín vòng sóng rồi đánh bật mục tiêu.","p3-water-s");
            AddLegendary(list,"earth-s","Sơn Nham Bệ Vệ",Element.Earth,PetRarity.S,650,45,28,"Sơn Mạch Quyền","Nắm đấm đá mở vết nứt dưới chân địch.","p3-earth-s");
            AddThreeFormS(list,"fire-lion-s","Viêm Tinh Sư",Element.Fire,605,57,20,"Viêm Cầu Bạo Kích","Quả cầu dung nham nổ tung ở điểm trúng.","p3-fire-lion-s");
            // Legendary bird trio: retain the eagle's established identity, turn the
            // former sea bird into a fire guardian, and add a golden metal bird.
            AddLegendary(list,"metal-eagle-s","Ngân Dực Thiên Ưng",Element.Metal,PetRarity.SS,900,82,34,"Thiên Ưng Phá Giới","Đôi cánh tinh thể xé toạc chiến tuyến.","p3-metal-eagle-s",60,65);
            AddLegendary(list,"water-bird-s","Xích Viêm Thần Điểu",Element.Fire,PetRarity.SS,875,84,30,"Xích Viêm Thiên Vũ","Lông vũ hóa thành mưa lửa quét khắp chiến trường.","p3-water-bird-s",60,65);
            AddLegendary(list,"golden-wing-ss","Kim Quang Thần Điểu",Element.Metal,PetRarity.SS,920,80,36,"Kim Vũ Thiên Phán","Triệu hồi vũ kiếm ánh vàng giáng xuống mục tiêu.","p3-golden-wing-ss",60,65);
            // SS are complete legendary forms at acquisition. They gain stats from levels,
            // but do not change their appearance or use the evolution card.
            AddLegendary(list,"metal-ss","Bạch Kim Cự Tê",Element.Metal,PetRarity.SS,820,66,30,"Thiên Kim Trấn Sát","Sừng pha lê tích điện xuyên qua chiến trường.","p3-metal-ss",60,65);
            AddLegendary(list,"fire-ss","Hỏa Vẫn Chimera",Element.Fire,PetRarity.SS,760,74,25,"Hỏa Vẫn Giáng Thế","Hóa thành thiên thạch lửa và giáng xuống.","p3-fire-ss",60,65);
            AddLegendary(list,"water-ss","Thiên Hải Cự Kình",Element.Water,PetRarity.SS,780,68,27,"Thiên Triều Lưu Quang","Cá đuối thần triệu hồi lưỡi sóng ánh sáng.","p3-water-ss",60,65);
            AddDiverse(list,"sun-rooster","Dương Kê Vệ Nhật",Element.Fire,PetRarity.A,510,46,17,"Nhật Luân Trảm","Quạt đuôi rực sáng lao qua mục tiêu.",.14f);
            AddDiverse(list,"moon-rabbit","Nguyệt Thỏ Tinh",Element.Water,PetRarity.S,610,53,21,"Trăng Khuyết Ảnh","Vẽ một cung trăng đẩy lùi đối thủ.",.18f);
            AddDiverse(list,"coral-crab","San Hô Cự Giáp",Element.Water,PetRarity.B,455,35,19,"Song Càng Sóng Vỗ","Hai càng san hô kẹp lấy mục tiêu.",.12f);
            AddDiverse(list,"jade-serpent","Thanh Long Lưu Giang",Element.Wood,PetRarity.SS,795,69,28,"Long Hà Phá Thiên","Thân rồng quấn nước và ngọc bích tung đòn.",.22f);
            AddDiverse(list,"fire-monkey","Xích Viên Hỏa Vũ",Element.Fire,PetRarity.A,485,49,15,"Hầu Vương Liên Kích","Liên hoàn quyền rực lửa bất ngờ.",.16f);
            AddDiverse(list,"star-pig","Tinh Trư Hân Hoan",Element.Earth,PetRarity.B,495,33,19,"Tinh Tú Bảo Hộ","Gọi vì sao nhỏ chặn một phần đòn đánh.",.1f);
            AddDiverse(list,"storm-tiger","Lôi Hổ Thiên Uy",Element.Metal,PetRarity.SS,765,74,25,"Thiên Lôi Hổ Phách","Tia chớp xanh xé ngang chiến trường.",.24f);
            AddDiverse(list,"abyss-angler","Đăng Ngư Vực Sâu",Element.Water,PetRarity.A,525,44,18,"Hải Đăng U Minh","Đèn câu dụ mục tiêu vào làn sóng tối.",.15f);
            catalog.pets=list.ToArray();catalog.p2Enabled=true;
            foreach(var pet in catalog.pets)if(string.IsNullOrEmpty(pet.artSheet))pet.artSheet=pet.id;
            var stages=new[]{440,530,630,760,900,1070,1270};var recommended=new[]{1,1,2,3,4,5,6};
            for(int i=0;i<7;i++)catalog.stages[i].encounter=new BossEncounterProfile{hp=stages[i],outgoingScale=i==6 ? .65f : .52f+i*.02f,recommendedLevel=recommended[i]};
            Form(catalog,"wood-2",4,"Ếch Lá Hiệp Sĩ","wood-2-form1");
            Form(catalog,"wood-3",7,"Nấm Ảo Ảnh","wood-3-form1");
            Form(catalog,"wood-4",4,"Long Đằng Non","wood-4-form1");
            Form(catalog,"wood-5",7,"Hoa Giáp Vệ","wood-5-form1");
            Form(catalog,"wood-6",4,"Hươu Linh Thủ Hộ","wood-6-form1");
            catalog.Pet("wood-7").forms=new[]{new EvolutionFormDefinition{minLevel=7,displayName="Cổ Thụ Vương Miện",artSheet="p3-wood-7-form1",hp=1.232f,attack=1.3216f,defense=1.188f}};
            foreach(var id in new[]{"wood-2","wood-3","wood-4","wood-5","wood-6","wood-7"})
            {
                var pet=catalog.Pet(id);pet.artSheet="p3-"+id;
                for(int i=0;i<pet.forms.Length;i++)pet.forms[i].artSheet="p3-"+id+"-form"+(i+1);
            }
            foreach(var id in new[]{"wood-1","starter-metal","metal-bird","fire-lion","water-otter","earth-turtle"})
                catalog.Pet(id).artSheet="p3-"+id;
            foreach(var id in new[]{"wood-s","water-s","earth-s","metal-ss","fire-ss","water-ss"})
            {
                var pet=catalog.Pet(id);
                var first=new EvolutionFormDefinition{minLevel=id=="water-s" ? 7 : 4,displayName=pet.title+" · Chiến Thể",artSheet="p3-"+id+"-form1"};
                pet.forms=pet.rarity==PetRarity.SS ? new EvolutionFormDefinition[0] : new[]{first};
            }
            catalog.islands=new[]{
                Island("wood","Mộc Tinh","Rễ cổ thụ · 7 boss đang chờ",Element.Wood,"wood-7",true),
                Island("fire","Hỏa Tinh","Núi lửa hổ phách · Coming Soon",Element.Fire,"fire-lion",false),
                Island("metal","Kim Tinh","Thành lũy tinh thạch · Coming Soon",Element.Metal,"metal-bird",false),
                Island("water","Thủy Tinh","San hô và thác nước · Coming Soon",Element.Water,"water-otter",false),
                Island("earth","Thổ Tinh","Tàn tích sa mạc · Coming Soon",Element.Earth,"earth-turtle",false)};
        }
        private static ChallengeIslandDefinition Island(string id,string title,string desc,Element type,string pet,bool playable)=>new ChallengeIslandDefinition{id=id,title=title,description=desc,element=type,previewPetId=pet,playable=playable,art="island-"+id};
        private static void AddLegendary(List<PetSpecies> list,string id,string title,Element element,PetRarity rarity,int hp,int attack,int defense,string skill,string skillDesc,string art,int mana=50,int rage=55)
        {
            var existing=list.FirstOrDefault(p=>p.id==id);
            if(existing!=null){existing.title=title;existing.description="Linh thú huyền thoại Pet Arena · hình thái tối thượng ngay từ đầu.";existing.element=element;existing.rarity=rarity;existing.hp=hp;existing.attack=attack;existing.defense=defense;existing.rig=0;existing.artSheet=art;existing.forms=Array.Empty<EvolutionFormDefinition>();existing.skillName=skill;existing.skillDescription=skillDesc;existing.skillManaCost=mana;existing.skillRageCost=rage;existing.crit=rarity==PetRarity.SS?.22f:.18f;return;}
            list.Add(new PetSpecies{id=id,title=title,description="Linh thú huyền thoại P3 · bản demo offline.",element=element,rarity=rarity,hp=hp,attack=attack,defense=defense,rig=0,artSheet=art,skillName=skill,skillDescription=skillDesc,skillManaCost=mana,skillRageCost=rage,crit=rarity==PetRarity.SS?.22f:.18f});
        }
        private static void AddThreeFormS(List<PetSpecies> list,string id,string title,Element element,int hp,int attack,int defense,string skill,string skillDesc,string art)
        {
            AddLegendary(list,id,title,element,PetRarity.S,hp,attack,defense,skill,skillDesc,art);
            var pet=list.First(p=>p.id==id);
            pet.description="Linh thú bậc S · tiến hóa từ hình thái đầu sang Chân Thể tại Lv7.";
            pet.forms=new[]{
                new EvolutionFormDefinition{minLevel=7,displayName=title+" · Chân Thể",artSheet=art+"-form1",hp=1.232f,attack=1.3216f,defense=1.188f}
            };
        }
        private static void AddDiverse(List<PetSpecies> list,string id,string title,Element element,PetRarity rarity,int hp,int attack,int defense,string skill,string skillDesc,float crit)
        {
            var existing=list.FirstOrDefault(p=>p.id==id);
            if(existing==null)
            {
                existing=new PetSpecies{id=id};list.Add(existing);
            }
            existing.title=title;existing.description="Linh thú nguyên bản từ những miền trời, rừng và biển của Pet Arena.";
            existing.element=element;existing.rarity=rarity;existing.hp=hp;existing.attack=attack;existing.defense=defense;
            existing.rig=0;existing.artSheet="p3-"+id;existing.forms=new EvolutionFormDefinition[0];existing.crit=crit;
            existing.skillName=skill;existing.skillDescription=skillDesc;
            existing.skillManaCost=rarity==PetRarity.SS ? 60 : rarity==PetRarity.S ? 50 : 40;
            existing.skillRageCost=rarity==PetRarity.SS ? 65 : rarity==PetRarity.S ? 55 : 45;
        }
        private static void Form(P1Catalog catalog,string id,int level,string title,string sheet)=>catalog.Pet(id).forms=new[]{new EvolutionFormDefinition{minLevel=level,displayName=title,artSheet=sheet}};
    }
}
