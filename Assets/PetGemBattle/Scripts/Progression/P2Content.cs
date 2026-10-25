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
        public int speed=2;
    }
    [Serializable] public sealed class BossEncounterProfile
    {
        public int hp,recommendedLevel;
        public float outgoingScale=.52f,normalCap=.24f,specialCap=.30f;
        public float awakenThreshold=.30f,awakenAttack=1.18f,awakenDefense=1.08f,awakenShield=.08f,awakenSeconds=1.2f;
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
            int[,] stats={{325,31,8,23},{310,30,10,25},{335,28,10,24},{350,28,12,20}};
            for(int i=0;i<4;i++)if(!list.Any(p=>p.id==ids[i]))list.Add(new PetSpecies{id=ids[i],title=titles[i],description="Linh thú "+P1Catalog.ElementName(types[i])+" · đồng hành thử nghiệm P2.",element=types[i],rarity=PetRarity.B,hp=stats[i,0],attack=stats[i,1],defense=stats[i,2],speed=stats[i,3],rig=8+i});
            catalog.pets=list.ToArray();catalog.p2Enabled=true;
            foreach(var pet in catalog.pets)pet.artSheet=pet.id;
            var stages=new[]{440,530,630,760,900,1070,1270};var recommended=new[]{1,1,2,3,4,5,6};
            for(int i=0;i<7;i++)catalog.stages[i].encounter=new BossEncounterProfile{hp=stages[i],outgoingScale=i==6 ? .65f : .52f+i*.02f,recommendedLevel=recommended[i]};
            Form(catalog,"wood-2",4,"Ếch Lá Hiệp Sĩ","wood-2-form1");
            Form(catalog,"wood-3",7,"Nấm Ảo Ảnh","wood-3-form1");
            Form(catalog,"wood-4",4,"Long Đằng Non","wood-4-form1");
            Form(catalog,"wood-5",7,"Hoa Giáp Vệ","wood-5-form1");
            Form(catalog,"wood-6",4,"Hươu Linh Thủ Hộ","wood-6-form1");
            Form(catalog,"wood-7",4,"Cổ Thụ Giáp Vệ","wood-7-form1");
            catalog.Pet("wood-7").forms=new[]{catalog.Pet("wood-7").forms[0],new EvolutionFormDefinition{minLevel=7,displayName="Cổ Thụ Vương Miện",artSheet="wood-7-form2",hp=1.1f,attack=1.12f,defense=1.08f,speed=2}};
            catalog.islands=new[]{
                Island("wood","Mộc Tinh","Rễ cổ thụ · 7 boss đang chờ",Element.Wood,"wood-7",true),
                Island("fire","Hỏa Tinh","Núi lửa hổ phách · Coming Soon",Element.Fire,"fire-lion",false),
                Island("metal","Kim Tinh","Thành lũy tinh thạch · Coming Soon",Element.Metal,"metal-bird",false),
                Island("water","Thủy Tinh","San hô và thác nước · Coming Soon",Element.Water,"water-otter",false),
                Island("earth","Thổ Tinh","Tàn tích sa mạc · Coming Soon",Element.Earth,"earth-turtle",false)};
        }
        private static ChallengeIslandDefinition Island(string id,string title,string desc,Element type,string pet,bool playable)=>new ChallengeIslandDefinition{id=id,title=title,description=desc,element=type,previewPetId=pet,playable=playable,art="island-"+id};
        private static void Form(P1Catalog catalog,string id,int level,string title,string sheet)=>catalog.Pet(id).forms=new[]{new EvolutionFormDefinition{minLevel=level,displayName=title,artSheet=sheet}};
    }
}
