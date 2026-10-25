using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PetGemBattle
{
    [Serializable] public sealed class OwnedPet { public string id;public int level=1;public string acquiredAt; }
    [Serializable] public sealed class PlayerSave
    {
        public int schemaVersion=1,unlockedStage=1;
        public string activePetId="starter-metal";
        public List<OwnedPet> pets=new List<OwnedPet>();
        public int[] stones=new int[35];
        public List<int> clearedStages=new List<int>();
        public List<string> transactions=new List<string>(),rewardHistory=new List<string>();
        public List<string> demoGrants=new List<string>();
        public static PlayerSave Fresh(){var s=new PlayerSave();s.pets.Add(new OwnedPet{id="starter-metal",acquiredAt=DateTime.UtcNow.ToString("O")});return s;}
    }
    public sealed class SaveGameService
    {
        private readonly string path;
        private readonly P1Catalog catalog;
        public string Notice {get;private set;}
        public bool NeedsMigration {get;private set;}
        public SaveGameService(string path,P1Catalog catalog){this.path=path;this.catalog=catalog;}
        public void Validate(PlayerSave data)
        {
            if(data==null || (data.schemaVersion!=1 && data.schemaVersion!=2) || data.pets==null || data.pets.Count==0 || data.stones==null || data.stones.Length!=35 || data.stones.Any(n=>n<0) || data.pets.Any(p=>p==null || catalog.Pet(p.id)==null || p.level<1 || p.level>catalog.maxLevel) || data.pets.Select(p=>p.id).Distinct().Count()!=data.pets.Count || !data.pets.Any(p=>p.id==data.activePetId) || data.unlockedStage<1 || data.unlockedStage>7 || data.clearedStages==null || data.clearedStages.Any(n=>n<1||n>7) || data.transactions==null || data.rewardHistory==null)throw new InvalidDataException("Save data is invalid.");
        }
        public PlayerSave Load()
        {
            foreach(string source in new[]{path,path+".bak"})
            {
                if(!File.Exists(source))continue;
                try
                {
                    var data=JsonUtility.FromJson<PlayerSave>(File.ReadAllText(source));
                    // Version 0 had the same IDs but no transaction/history collections.
                    if(data!=null && data.schemaVersion==0){data.schemaVersion=1;data.transactions=data.transactions??new List<string>();data.rewardHistory=data.rewardHistory??new List<string>();}
                    if(catalog.p2Enabled && data!=null && data.schemaVersion>=0 && data.schemaVersion<=2){NeedsMigration=data.schemaVersion<2;data.schemaVersion=2;data.demoGrants=data.demoGrants??new List<string>();}
                    Validate(data);if(source!=path)Notice="Đã phục hồi bản lưu dự phòng.";return data;
                }
                catch(Exception ex) when(ex is IOException || ex is ArgumentException){Notice="Bản lưu lỗi; đã thử bản dự phòng.";}
            }
            return PlayerSave.Fresh();
        }
        public void Write(PlayerSave data)
        {
            Validate(data);Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";
            using(var file=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
            using(var writer=new StreamWriter(file)){writer.Write(JsonUtility.ToJson(data,true));writer.Flush();file.Flush(true);}
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
        }
    }
    public sealed class ProgressionService
    {
        public PlayerSave Data {get;private set;}
        public readonly P1Catalog Catalog;
        private readonly SaveGameService save;
        public readonly GameModeConfig Mode;
        public ProgressionService(P1Catalog catalog,SaveGameService save,GameModeConfig mode=null)
        {
            Catalog=catalog;this.save=save;Mode=mode;Data=save.Load();
            if(catalog.p2Enabled){var next=Copy();next.schemaVersion=2;bool changed=save.NeedsMigration || Data.schemaVersion!=2;
                if(mode!=null && mode.DemoFullCollection)foreach(var pet in catalog.pets)if(!next.pets.Any(p=>p.id==pet.id)){next.pets.Add(new OwnedPet{id=pet.id,acquiredAt=DateTime.UtcNow.ToString("O")});next.demoGrants.Add(pet.id);changed=true;}
                if(changed)Commit(next);
            }
        }
        public void ResetDemo()
        {
            var next=PlayerSave.Fresh();next.schemaVersion=2;
            if(Mode!=null && Mode.DemoFullCollection)foreach(var pet in Catalog.pets)if(!next.pets.Any(p=>p.id==pet.id))next.pets.Add(new OwnedPet{id=pet.id,acquiredAt=DateTime.UtcNow.ToString("O")});
            Commit(next);
        }
        public bool SeedDemoStones()
        {
            if(Mode==null || !Mode.DemoTools || Data.transactions.Contains("demo-stones-v1"))return false;
            var next=Copy();for(int i=0;i<35;i++)next.stones[i]+=Mode.DemoStoneAmount;next.transactions.Add("demo-stones-v1");Commit(next);return true;
        }
        public static int StoneIndex(Element e,int grade){if(grade<1||grade>7)throw new ArgumentOutOfRangeException(nameof(grade));return (int)e*7+grade-1;}
        public int StoneCount(Element e,int grade)=>Data.stones[StoneIndex(e,grade)];
        public OwnedPet Owned(string id)=>Data.pets.FirstOrDefault(p=>p.id==id);
        private PlayerSave Copy()=>JsonUtility.FromJson<PlayerSave>(JsonUtility.ToJson(Data));
        private void Commit(PlayerSave next){save.Write(next);Data=next;}
        public bool Select(string id)
        {
            if(Owned(id)==null)return false;var next=Copy();next.activePetId=id;Commit(next);return true;
        }
        public bool Upgrade(string id,int expectedLevel,string transaction)
        {
            var pet=Owned(id);if(pet==null || pet.level!=expectedLevel || pet.level>=Catalog.maxLevel || string.IsNullOrEmpty(transaction) || Data.transactions.Contains(transaction))return false;
            var definition=Catalog.Pet(id);int index=StoneIndex(definition.element,pet.level+1);if(Data.stones[index]<1)return false;
            var next=Copy();next.stones[index]--;next.pets.Single(p=>p.id==id).level++;next.transactions.Add(transaction);Commit(next);return true;
        }
        public string Reward(int stage,BattleResult result,string battleId,IBattleRandom rng)
        {
            if(result!=BattleResult.Victory || stage<1 || stage>Data.unlockedStage || string.IsNullOrEmpty(battleId) || Data.transactions.Contains(battleId))return "Không có thưởng mới.";
            var boss=Catalog.stages[stage-1];var next=Copy();bool first=!next.clearedStages.Contains(stage);bool captured=!next.pets.Any(p=>p.id==boss.petId);
            if(captured)next.pets.Add(new OwnedPet{id=boss.petId,acquiredAt=DateTime.UtcNow.ToString("O")});
            if(!next.clearedStages.Contains(stage))next.clearedStages.Add(stage);next.unlockedStage=Math.Max(next.unlockedStage,Math.Min(7,stage+1));
            next.stones[StoneIndex(Element.Wood,boss.stoneGrade)]++;bool bonus=rng.Next()<boss.bonusChance;
            if(bonus)next.stones[StoneIndex(Element.Wood,boss.bonusGrade)]++;
            string message=(Mode!=null && Mode.DemoFullCollection && first ? "Đã chinh phục "+Catalog.Pet(boss.petId).title+"!\n" : captured ? "Đã thu phục "+Catalog.Pet(boss.petId).title+"!\n" : "Đã sở hữu Pet — không tạo bản sao.\n")+"+1 Đá Mộc Lv"+boss.stoneGrade+(bonus ? " · Thưởng thêm +1 Lv"+boss.bonusGrade : "");
            next.transactions.Add(battleId);next.rewardHistory.Insert(0,"Ải "+stage+": "+message.Replace('\n',' '));if(next.rewardHistory.Count>8)next.rewardHistory.RemoveAt(8);Commit(next);return message;
        }
        public IEnumerable<OwnedPet> Filter(int element=-1,int rarity=-1,string sort="Lv",bool activeOnly=false)
        {
            var list=Data.pets.Where(p=>(element<0 || (int)Catalog.Pet(p.id).element==element) && (rarity<0 || (int)Catalog.Pet(p.id).rarity==rarity) && (!activeOnly || p.id==Data.activePetId));
            return list.OrderByDescending(p=>sort=="Bậc" ? (int)Catalog.Pet(p.id).rarity : sort=="HP" ? Catalog.Stats(p.id,p.level).maxHp : sort=="ATK" ? Catalog.Stats(p.id,p.level).attack : sort=="SPD" ? Catalog.Stats(p.id,p.level).speed : p.level).ThenBy(p=>p.id);
        }
    }
}
