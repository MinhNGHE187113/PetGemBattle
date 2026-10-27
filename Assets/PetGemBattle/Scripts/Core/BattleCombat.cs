using System;
using System.Collections.Generic;

namespace PetGemBattle
{
    public enum BattleResult { Ongoing, Victory, Defeat, Draw }
    public sealed class PetState
    {
        public readonly PetDefinition Definition;
        public int MaxHp { get; private set; }
        public float Attack { get; private set; }
        public float Defense { get; private set; }
        public int EvolutionLevel { get; private set; } = 1;
        public bool BossAwakened {get;private set;}
        public int Hp { get; private set; }
        public int Shield { get; private set; }
        public int Mana { get; private set; }
        public int Rage { get; private set; }
        public int ManaCardsUsed { get; private set; }
        public int RageCardsUsed { get; private set; }
        public bool SkillUsed { get; private set; }
        public int SignatureCooldown { get; private set; }
        public PetState(PetDefinition definition)
        {
            Definition=definition;MaxHp=Math.Max(1,definition.maxHp);Hp=MaxHp;
            Attack=definition.attack;Defense=definition.defense;
        }
        public PetState Copy() => (PetState)MemberwiseClone();
        public void ApplySnapshot(PetState state)
        {
            if(state.Definition!=Definition)throw new ArgumentException("Snapshot belongs to another pet.");
            MaxHp=state.MaxHp;Attack=state.Attack;Defense=state.Defense;EvolutionLevel=state.EvolutionLevel;
            Hp=state.Hp;Shield=state.Shield;Mana=state.Mana;Rage=state.Rage;ManaCardsUsed=state.ManaCardsUsed;RageCardsUsed=state.RageCardsUsed;SkillUsed=state.SkillUsed;
            BossAwakened=state.BossAwakened;SignatureCooldown=state.SignatureCooldown;
        }
        public int RemoveMana(int amount){int n=Math.Min(Mana,Math.Max(0,amount));Mana-=n;return n;}
        public bool TryBossAwaken(BattleRules rules)
        {
            var profile=Definition.bossProfile;
            if(!Definition.isBossEncounter || profile==null || BossAwakened || Hp<=0 || Hp>MaxHp*profile.awakenThreshold)return false;
            BossAwakened=true;Attack*=profile.awakenAttack;Defense*=profile.awakenDefense;
            AddShield(BattleMath.Round(MaxHp*profile.awakenShield),rules);return true;
        }
        public void BeginTurn() { SkillUsed=false;if(SignatureCooldown>0)SignatureCooldown--; }
        public int Heal(int amount) { int old=Hp;Hp=Math.Min(MaxHp,Hp+Math.Max(0,amount));return Hp-old; }
        public int AddShield(int amount,BattleRules rules) { int old=Shield;Shield=Math.Min(rules.ShieldCap(this),Shield+Math.Max(0,amount));return Shield-old; }
        public int AddMana(int amount,BattleRules rules) { int old=Mana;Mana=Math.Min(rules.maxMana,Mana+Math.Max(0,amount));return Mana-old; }
        public int AddRage(int amount,BattleRules rules) { int old=Rage;Rage=Math.Min(rules.maxRage,Rage+Math.Max(0,amount));return Rage-old; }
        public int RemoveRage(int amount) { int n=Math.Min(Rage,Math.Max(0,amount));Rage-=n;return n; }
        public int RemoveShield(int amount) { int n=Math.Min(Shield,Math.Max(0,amount));Shield-=n;return n; }
        public int TakeDamage(int amount)
        {
            amount=Math.Max(0,amount);int absorbed=RemoveShield(amount);
            int lost=Math.Min(Hp,amount-absorbed);Hp-=lost;return lost;
        }
        internal void RecordCard(CardKind card)
        {
            SkillUsed=true;
            if(card==CardKind.Mana)ManaCardsUsed++;
            if(card==CardKind.Rage)RageCardsUsed++;
            if(card==CardKind.Signature)SignatureCooldown=3;
        }
        internal void Evolve(BattleRules rules)
        {
            int oldMax=MaxHp;
            Mana-=rules.evolutionManaCost;
            MaxHp=BattleMath.Round(oldMax*rules.evolutionHpMultiplier);
            Attack*=rules.evolutionAttackMultiplier;
            Defense*=rules.evolutionDefenseMultiplier;
            Hp=Math.Min(MaxHp,Hp+BattleMath.Round(oldMax*rules.evolutionHealPercent));
            EvolutionLevel=2;
            Shield=Math.Min(Shield,rules.ShieldCap(this));Rage=Math.Min(Rage,rules.maxRage);
        }
        // Legacy spells remain available to future loadouts; cards share the same per-turn lock.
        public bool CanUse(SkillDefinition skill) => skill!=null && Hp>0 && !SkillUsed &&
            (skill.effect==SkillEffect.Ultimate ? Rage>=skill.cost : Mana>=skill.cost);
        public bool SpendSkill(SkillDefinition skill)
        {
            if(!CanUse(skill))return false;
            if(skill.effect==SkillEffect.Ultimate)Rage=0;else Mana-=skill.cost;
            SkillUsed=true;return true;
        }
    }
    [Serializable]
    public sealed class BattleReport
    {
        public readonly List<BattleEvent> Events=new List<BattleEvent>();
        public int Damage,HpDamage,Absorbed,Healing,Shield,Mana,Rage;
        public int NormalAttacks,SpecialAttacks,CriticalAttacks,RageSpent,ManaSpent,HpStolen,RageStolen,ShieldStolen,DrainGroups;
        public int PhysicalGems,EffectiveGems;
        public bool BossTransformation;
        public bool HpDrainTriggered,RageDrainTriggered,ShieldDrainTriggered,Evolved;
        public override string ToString()
        {
            var parts=new List<string>();
            if(BossTransformation)parts.Add("BOSS THỨC TỈNH");
            if(ManaSpent>0)parts.Add("Mana −"+ManaSpent);
            if(RageSpent>0)parts.Add("Nộ −"+RageSpent);
            if(PhysicalGems>0)parts.Add(PhysicalGems+" ô → "+EffectiveGems+" hiệu lực");
            if(SpecialAttacks>0)parts.Add("ĐẶC BIỆT");
            if(CriticalAttacks>0)parts.Add("CRITICAL! ×"+CriticalAttacks);
            if(HpDamage>0)parts.Add("-"+HpDamage+" HP");
            if(Absorbed>0)parts.Add("Giáp chặn "+Absorbed);
            if(Healing>0)parts.Add("HP +"+Healing);
            if(Shield>0)parts.Add("Giáp +"+Shield);
            if(Mana>0)parts.Add("Mana +"+Mana);
            if(Rage>0)parts.Add("Nộ +"+Rage);
            if(RageStolen>0)parts.Add("Hút "+RageStolen+" Nộ");
            if(ShieldStolen>0)parts.Add("Hút "+ShieldStolen+" giáp");
            if(DrainGroups>0 && !HpDrainTriggered && !RageDrainTriggered && !ShieldDrainTriggered)parts.Add("Hút không kích hoạt");
            return parts.Count==0 ? "Tài nguyên đã đầy" : string.Join("  ·  ",parts);
        }
    }
    public static class BattleCombat
    {
        public static BattleResult Result(PetState player,PetState enemy)
        {
            if(player.Hp<=0 && enemy.Hp<=0)return BattleResult.Draw;
            if(enemy.Hp<=0)return BattleResult.Victory;
            return player.Hp<=0 ? BattleResult.Defeat : BattleResult.Ongoing;
        }
        public static BattleResult ResultAtRoundLimit(PetState player,PetState enemy)
        {
            var result=Result(player,enemy);if(result!=BattleResult.Ongoing)return result;
            long left=(long)player.Hp*enemy.MaxHp,right=(long)enemy.Hp*player.MaxHp;
            return left==right ? BattleResult.Draw : left>right ? BattleResult.Victory : BattleResult.Defeat;
        }
        public static int SwordDamage(PetState actor,PetState target,int count,int wave,BattleRules rules,bool special=false,bool critical=false,int completedRounds=0)
        {
            float power=actor.Attack*(rules.attackMultiplier+rules.swordExtraMultiplier*Math.Max(0,count-3))*rules.ComboMultiplier(wave);
            int damage=Math.Max(1,BattleMath.Round(power-target.Defense*rules.defenseMultiplier));
            float unrounded=damage;
            if(special){unrounded*=rules.specialMultiplier;if(!rules.finalMultiplierRounding)unrounded=BattleMath.Round(unrounded);}
            if(critical){unrounded*=rules.criticalMultiplier;if(!rules.finalMultiplierRounding)unrounded=BattleMath.Round(unrounded);}
            float element=rules.elements==null ? 1 : rules.elements.Multiplier(actor.Definition.element,target.Definition.element);
            bool boss=actor.Definition.isBossEncounter && actor.Definition.bossProfile!=null;
            float scale=boss ? actor.Definition.bossProfile.outgoingScale : 1;
            damage=Math.Max(1,BattleMath.Round(unrounded*element*rules.SuddenDeathMultiplier(completedRounds)*scale));
            if(boss)damage=Math.Min(damage,Math.Max(1,BattleMath.Round(target.MaxHp*(special ? actor.Definition.bossProfile.specialCap : actor.Definition.bossProfile.normalCap))));
            return damage;
        }
        private static void Deal(PetState target,int damage,BattleReport report)
        {
            int shieldBefore=target.Shield;
            report.Damage+=damage;report.HpDamage+=target.TakeDamage(damage);
            report.Absorbed+=shieldBefore-target.Shield;
        }
        private static int Order(GemKind kind) => kind==GemKind.Sword ? 6 : kind==GemKind.Drain ? 5 : (int)kind;
        private static int FirstCell(MatchGroup group)
        {
            int key=int.MaxValue;foreach(var c in group.Cells)key=Math.Min(key,c.Y*MatchBoard.Size+c.X);return key;
        }
        public static BattleReport ApplyMatches(PetState actor,PetState target,List<MatchGroup> groups,int wave,BattleRules rules,IBattleRandom random=null,int completedRounds=0,bool swordRagePrepaid=false)
        {
            var report=new BattleReport();var counted=new HashSet<Cell>();
            var ordered=new List<MatchGroup>(groups);
            // Resources, drain, sword; ties resolve by the first board cell.
            ordered.Sort((a,b)=>{int order=Order(a.Kind).CompareTo(Order(b.Kind));return order!=0 ? order : FirstCell(a).CompareTo(FirstCell(b));});
            float combo=rules.ComboMultiplier(wave);
            foreach(var group in ordered)
            {
                if(actor.Hp<=0 || target.Hp<=0)break;
                int n=0;foreach(var cell in group.Cells)if(counted.Add(cell))n++;
                if(n<3)continue;
                int units=group.Units;
                report.PhysicalGems+=n;report.EffectiveGems+=units;
                var before=BattleEvent.Capture(actor,target);
                int bonus=rules.GroupRageBonus(units);
                if(!(swordRagePrepaid && group.Kind==GemKind.Sword))report.Rage+=actor.AddRage(bonus,rules);
                int eventStart=report.Events.Count;
                switch(group.Kind)
                {
                    case GemKind.Sword:
                        if(random==null)throw new ArgumentNullException(nameof(random),"Sword resolution requires the battle's seeded RNG.");
                        if(!swordRagePrepaid)report.Rage+=actor.AddRage(rules.swordBaseRage+rules.swordExtraRage*(units-3),rules);
                        bool special=actor.Rage>=rules.specialRageCost && actor.Mana>=rules.specialManaCost;
                        if(special)report.SpecialAttacks++;else report.NormalAttacks++;
                        double roll=random.Next();
                        bool critical=roll<rules.CriticalChance(actor,units);
                        if(critical)report.CriticalAttacks++;
                        Deal(target,SwordDamage(actor,target,units,wave,rules,special,critical,completedRounds),report);
                        if(special){report.RageSpent+=actor.RemoveRage(rules.specialRageCost);report.ManaSpent+=actor.RemoveMana(rules.specialManaCost);}
                        report.Events.Add(BattleEvent.Attack(group.Kind,n,wave,before,actor,target,roll,special,critical));
                        break;
                    case GemKind.Heart:
                        int heal=Math.Min(BattleMath.Round(actor.MaxHp*(rules.healBasePercent+rules.healExtraPercent*(units-3))*combo),BattleMath.Round(actor.MaxHp*rules.maxHealPercent));
                        report.Healing+=actor.Heal(heal);break;
                    case GemKind.Shield:
                        report.Shield+=actor.AddShield(BattleMath.Round(actor.MaxHp*(rules.shieldBasePercent+rules.shieldExtraPercent*(units-3))*combo),rules);break;
                    case GemKind.Mana:report.Mana+=actor.AddMana(BattleMath.Round(rules.manaPerGem*units*combo),rules);break;
                    case GemKind.Rage:report.Rage+=actor.AddRage(BattleMath.Round(rules.ragePerGem*units*combo),rules);break;
                    case GemKind.Drain:
                        if(random==null)throw new ArgumentNullException(nameof(random),"Drain resolution requires the battle's seeded RNG.");
                        Drain(actor,target,n,combo,rules,random,report);break;
                }
                if(report.Events.Count==eventStart)report.Events.Add(BattleEvent.Change(group.Kind,n,wave,before,actor,target));
            }
            return report;
        }
        private static void Drain(PetState actor,PetState target,int n,float combo,BattleRules rules,IBattleRandom random,BattleReport report)
        {
            report.DrainGroups++;
            // Always consume exactly three independent draws, even when the target resource is zero.
            bool hp=random.Next()<rules.DrainChance(rules.hpDrainChance,rules.hpDrainChanceCap,n);
            bool rage=random.Next()<rules.DrainChance(rules.rageDrainChance,rules.resourceDrainChanceCap,n);
            bool shield=random.Next()<rules.DrainChance(rules.shieldDrainChance,rules.resourceDrainChanceCap,n);
            report.HpDrainTriggered|=hp;report.RageDrainTriggered|=rage;report.ShieldDrainTriggered|=shield;
            if(hp)
            {
                int amount=Math.Min(BattleMath.Round(target.Hp*rules.hpDrainPercent*combo),BattleMath.Round(target.MaxHp*rules.hpDrainMaxPercent));
                int before=target.Hp;Deal(target,amount,report);int lost=before-target.Hp;
                report.HpStolen+=lost;report.Healing+=actor.Heal(lost);
            }
            if(rage)
            {
                int stolen=target.RemoveRage(Math.Min(rules.maxRage-actor.Rage,BattleMath.Round(target.Rage*rules.rageDrainPercent*combo)));
                report.RageStolen+=stolen;report.Rage+=actor.AddRage(stolen,rules);
            }
            if(shield)
            {
                int stolen=target.RemoveShield(Math.Min(rules.ShieldCap(actor)-actor.Shield,BattleMath.Round(target.Shield*rules.shieldDrainPercent*combo)));
                report.ShieldStolen+=stolen;report.Shield+=actor.AddShield(stolen,rules);
            }
        }
        public static BattleReport UseSkill(PetState actor,PetState target,SkillDefinition skill)
        {
            if(!actor.SpendSkill(skill))return null;var report=new BattleReport();
            if(skill.effect==SkillEffect.Heal)report.Healing=actor.Heal(skill.power);
            else Deal(target,Math.Max(1,BattleMath.Round(skill.power-target.Defense)),report);
            return report;
        }
    }
}
