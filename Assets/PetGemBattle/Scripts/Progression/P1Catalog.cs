using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PetGemBattle
{
    public enum PetRarity { C,B,A,S,SS,SSS }
    [Serializable] public sealed class PetSpecies
    {
        public string id,title,description;
        public Element element;
        public PetRarity rarity;
        public int hp,attack,defense,speed,rig;
        public string artSheet;
        public EvolutionFormDefinition[] forms=new EvolutionFormDefinition[0];
        [Range(0,1)] public float crit=.15f;
    }
    [Serializable] public sealed class BossStage
    {
        public int number,stoneGrade,bonusGrade;
        public string petId;
        [Range(0,1)] public float bonusChance;
        [Range(1,1.15f)] public float hpMultiplier=1;
        public BossEncounterProfile encounter;
    }
    [Serializable] public sealed class RarityBaseline
    {
        public PetRarity rarity;public int hp,attack,defense,speed;
    }
    [Serializable] public sealed class ElementEdge { public Element attacker,target; }
    [CreateAssetMenu(menuName="Pet Gem Battle/P1 Content Catalog")]
    public sealed class P1Catalog : ScriptableObject
    {
        public PetSpecies[] pets;
        public BossStage[] stages;
        public RarityBaseline[] rarities;
        public ElementEdge[] advantages;
        public float advantage=1.25f,disadvantage=.85f;
        public float hpGrowth=.10f,attackGrowth=.09f,defenseGrowth=.07f;
        public int speedGrowth=2,maxLevel=7;
        public bool p2Enabled;
        public ChallengeIslandDefinition[] islands;
        public EvolutionFormDefinition[] Forms(string id,int level)=>p2Enabled ? (Pet(id).forms??new EvolutionFormDefinition[0]).Where(f=>f.minLevel<=level).OrderBy(f=>f.minLevel).ToArray() : new EvolutionFormDefinition[0];
        public string Sheet(string id,int level){var forms=Forms(id,level);return forms.Length==0 ? Pet(id).artSheet : forms.Last().artSheet;}
        public PetSpecies Pet(string id)=>pets.FirstOrDefault(p=>p.id==id);
        public float Multiplier(Element attacker,Element target)
        {
            if(advantages.Any(e=>e.attacker==attacker && e.target==target))return advantage;
            return advantages.Any(e=>e.attacker==target && e.target==attacker) ? disadvantage : 1;
        }
        public PetDefinition Stats(string id,int level,bool boss=false,int stage=0)
        {
            var p=Pet(id)??throw new ArgumentException("Unknown species "+id);
            if(level<1 || level>maxLevel)throw new ArgumentOutOfRangeException(nameof(level));
            var result=new PetDefinition {id=id,name=p.title,art="Rig"+p.rig,rig=p.rig,rarity=p.rarity,element=p.element,level=level,
                maxHp=Math.Max(1,BattleMath.Round(p.hp*(1+hpGrowth*(level-1))*(boss ? stages[stage-1].hpMultiplier : 1))),
                attack=BattleMath.Round(p.attack*(1+attackGrowth*(level-1))),defense=BattleMath.Round(p.defense*(1+defenseGrowth*(level-1))),
                speed=p.speed+speedGrowth*(level-1),baseCritChance=p.crit};
            var forms=Forms(id,level);result.permanentForm=forms.Length;result.artSheet=Sheet(id,level);
            double hp=result.maxHp,attack=result.attack,defense=result.defense;
            foreach(var form in forms){hp*=form.hp;attack*=form.attack;defense*=form.defense;result.speed+=form.speed;result.name=form.displayName;}
            result.maxHp=BattleMath.Round(hp);result.attack=BattleMath.Round(attack);result.defense=BattleMath.Round(defense);
            if(p2Enabled && boss){result.isBossEncounter=true;result.bossProfile=stages[stage-1].encounter;result.maxHp=result.bossProfile.hp;}
            return result;
        }
        public void ValidateContent()
        {
            if(pets==null || pets.Length<8 || pets.Select(p=>p.id).Distinct().Count()!=pets.Length || stages==null || stages.Length!=7 || rarities==null || rarities.Length!=6 || advantages==null || advantages.Length!=5)throw new InvalidOperationException("Incomplete catalog");
            foreach(var p in pets)if(string.IsNullOrEmpty(p.id)||string.IsNullOrEmpty(p.title)||p.hp<1||p.attack<1||p.defense<0||p.speed<1||p.rig<0||p.rig>11||p.crit<0||p.crit>1)throw new InvalidOperationException("Invalid pet "+p.id);
            for(int i=0;i<7;i++){var s=stages[i];if(s.number!=i+1 || s.stoneGrade!=i+1 || Pet(s.petId)==null || Pet(s.petId).element!=Element.Wood || s.bonusGrade<1 || s.bonusGrade>7 || s.bonusChance<0 || s.bonusChance>1 || s.hpMultiplier<1 || s.hpMultiplier>1.15f)throw new InvalidOperationException("Invalid boss stage");}
        }
        public void SeedDefaults()
        {
            string[] names={"Mầm Miu","Lá Lùn","Nấm Nấp","Dây Dẻo","Hoa Gai","Linh Hươu","Cổ Thụ Vệ Thần","Kim Miêu"};
            int[,] stats={{245,22,6,18},{260,24,7,17},{280,26,8,19},{315,28,9,22},{330,31,10,21},{345,32,11,24},{450,38,14,27},{260,24,6,18}};
            string[] desc={"Mèo nhỏ nuôi mầm xanh trên đôi tai.","Ếch lá tròn, thích nhảy bên suối.","Nấm tinh nghịch ẩn dưới những tán cây.","Thằn lằn đuôi dây leo dẻo dai.","Nhím hoa bảo vệ rừng bằng gai ngọc.","Hươu non mang ánh sáng trên sừng lá.","Người bảo vệ cổ thụ, dùng rễ trói kẻ địch.","Bạn đồng hành bậc C, bộ giáp Kim khắc Mộc."};
            pets=new PetSpecies[8];stages=new BossStage[7];
            for(int i=0;i<8;i++)pets[i]=new PetSpecies{id=i==7 ? "starter-metal" : "wood-"+(i+1),title=names[i],description=desc[i],element=i==7 ? Element.Metal : Element.Wood,rarity=i==7 || i<3 ? PetRarity.C : i<6 ? PetRarity.B : PetRarity.A,hp=stats[i,0],attack=stats[i,1],defense=stats[i,2],speed=stats[i,3],rig=i};
            float[] chances={.25f,.25f,.2f,.2f,.15f,.15f,.1f};
            for(int i=0;i<7;i++)stages[i]=new BossStage{number=i+1,petId=pets[i].id,stoneGrade=i+1,bonusGrade=Math.Max(1,i),bonusChance=chances[i],hpMultiplier=1+i*.025f};
            int[,] baseline={{260,24,6,18},{320,29,9,22},{420,36,13,26},{540,44,18,31},{700,54,24,37},{920,68,31,44}};
            rarities=new RarityBaseline[6];for(int i=0;i<6;i++)rarities[i]=new RarityBaseline{rarity=(PetRarity)i,hp=baseline[i,0],attack=baseline[i,1],defense=baseline[i,2],speed=baseline[i,3]};
            Element[] cycle={Element.Metal,Element.Wood,Element.Earth,Element.Water,Element.Fire};
            advantages=new ElementEdge[5];for(int i=0;i<5;i++)advantages[i]=new ElementEdge{attacker=cycle[i],target=cycle[(i+1)%5]};
        }
        public static string ElementName(Element e)=>new[]{"Kim","Mộc","Thủy","Hỏa","Thổ"}[(int)e];
        public static Color ElementColor(Element e)=>new[]{new Color(.8f,.88f,1),new Color(.3f,.88f,.5f),new Color(.25f,.65f,1),new Color(1,.4f,.2f),new Color(.9f,.7f,.3f)}[(int)e];
    }
}
