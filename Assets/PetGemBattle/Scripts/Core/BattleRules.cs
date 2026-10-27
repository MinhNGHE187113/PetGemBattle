using System;
using UnityEngine;

namespace PetGemBattle
{
    // Preserve serialized values for the existing five types; Drain is the sixth.
    public enum GemKind { Sword, Shield, Heart, Mana, Rage, Drain }
    public enum Element { Metal, Wood, Water, Fire, Earth }
    public enum SkillEffect { Damage, Heal, Ultimate }

    [Serializable]
    public class SkillDefinition
    {
        public string title;
        public SkillEffect effect;
        [Min(0)] public int cost;
        [Min(0)] public int power;
        public SkillDefinition(string title, SkillEffect effect, int cost, int power)
        { this.title = title; this.effect = effect; this.cost = cost; this.power = power; }
    }

    [Serializable]
    public class PetDefinition
    {
        public string id;
        public string artSheet;
        public int permanentForm;
        public bool isBossEncounter;
        public BossEncounterProfile bossProfile;
        public int rig=-1;
        public PetRarity rarity;
        public string signatureName,signatureDescription;
        public int signatureManaCost,signatureRageCost;
        public float signaturePower;
        public float baseCritChance=.15f;
        public string name = "Hỏa Long Chiến Thần";
        public string art = "FireDragon";
        public Element element = Element.Fire;
        [Min(1)] public int level = 1;
        [Min(1)] public int maxHp = 300;
        [Min(1)] public int attack = 25;
        [Min(0)] public int defense = 8;
        public SkillDefinition[] skills = {
            new SkillDefinition("Fireball", SkillEffect.Damage, 30, 25),
            new SkillDefinition("Recovery", SkillEffect.Heal, 25, 25),
            new SkillDefinition("Pet Ultimate", SkillEffect.Ultimate, 100, 40)
        };
    }

    [CreateAssetMenu(menuName = "Pet Gem Battle/Battle Rules")]
    public sealed class BattleRules : ScriptableObject
    {
        public P1Catalog elements;
        public bool cinematicTurns;
        public float boardTransitionSeconds=.28f;
        public const int BoardSize = 8;
        public const int GemKinds = 6;
        [Header("Sword / automatic special attack")]
        [Min(0)] public float attackMultiplier = 1.8f;
        [Min(0)] public float swordExtraMultiplier = .35f;
        [Range(0,1)] public float defenseMultiplier = .5f;
        [Min(0)] public int swordBaseRage = 10;
        [Min(0)] public int swordExtraRage = 4;
        [Min(1)] public int specialRageCost = 60;
        [Min(0)] public int specialManaCost;
        public bool finalMultiplierRounding;
        [Min(1)] public float specialMultiplier = 1.75f;
        [Header("Critical / one roll per sword group")]
        [Range(0,1)] public float criticalChance = .15f;
        [Range(0,1)] public float criticalExtraChance = .05f;
        [Range(0,1)] public float evolvedCriticalBonus = .05f;
        [Range(0,1)] public float criticalChanceCap = .4f;
        [Min(1)] public float criticalMultiplier = 1.7f;
        [Header("Percent-based recovery and armor")]
        [Range(0,1)] public float healBasePercent = .04f;
        [Range(0,1)] public float healExtraPercent = .015f;
        [Range(0,1)] public float maxHealPercent = .10f;
        [Range(0,1)] public float shieldBasePercent = .03f;
        [Range(0,1)] public float shieldExtraPercent = .01f;
        [Range(0,1)] public float maxShieldPercent = .2f;
        [Header("Round limit / sudden death")]
        [Min(1)] public int suddenDeathAfterRounds = 15;
        [Min(1)] public int suddenDeathStepRounds = 2;
        [Min(0)] public float suddenDeathStep = .1f;
        [Min(0)] public float suddenDeathMaxBonus = .6f;
        [Min(1)] public int maxRounds = 30;
        [Header("Resources and combo")]
        [Min(0)] public int manaPerGem = 8;
        [Min(0)] public int ragePerGem = 7;
        [Min(1)] public int maxMana = 100;
        [Min(1)] public int maxRage = 100;
        [Min(0)] public float comboStep = .25f;
        [Min(0)] public int fourGemRageBonus = 5;
        [Min(0)] public int fiveGemRageBonus = 10;
        [Header("Independent drain probabilities")]
        [Range(0,1)] public float hpDrainChance = .30f;
        [Range(0,1)] public float rageDrainChance = .25f;
        [Range(0,1)] public float shieldDrainChance = .25f;
        [Range(0,1)] public float drainExtraChance = .05f;
        [Range(0,1)] public float hpDrainChanceCap = .55f;
        [Range(0,1)] public float resourceDrainChanceCap = .50f;
        [Range(0,1)] public float hpDrainPercent = .08f;
        [Range(0,1)] public float hpDrainMaxPercent = .12f;
        [Range(0,1)] public float rageDrainPercent = .25f;
        [Range(0,1)] public float shieldDrainPercent = .30f;
        [Header("Cards / one shared use per turn")]
        [Min(0)] public int manaCardGain = 20;
        [Min(0)] public int rageCardGain = 20;
        [Min(0)] public int manaCardUses = 2;
        [Min(0)] public int rageCardUses = 2;
        [Min(0)] public int evolutionManaCost = 80;
        [Header("Evolution (round half away from zero)")]
        [Min(1)] public float evolutionHpMultiplier = 1.4f;
        [Min(1)] public float evolutionAttackMultiplier = 1.35f;
        [Min(1)] public float evolutionDefenseMultiplier = 1.2f;
        [Range(0,1)] public float evolutionHealPercent = .4f;
        [Header("Presentation and safety")]
        [Min(.01f)] public float swapSeconds = .24f;
        [Min(.01f)] public float clearSeconds = .34f;
        [Min(.01f)] public float fallSeconds = .42f;
        [Min(.01f)] public float effectSeconds = .9f;
        [Min(.01f)] public float normalAttackSeconds = 1.25f;
        [Min(.01f)] public float specialAttackSeconds = 2.35f;
        [Min(.01f)] public float evolutionSeconds = 2.4f;
        [Min(.01f)] public float aiDelay = 1.1f;
        [Min(1)] public float turnSeconds = 30;
        [Min(1)] public int maxCascadeWaves = 100;
        [Header("Pet loadouts (elements are metadata for now)")]
        public PetDefinition player = new PetDefinition();
        public PetDefinition enemy = new PetDefinition { name = "Lôi Băng Thần Thú", element = Element.Water, art = "ThunderFrost" };
        public float ComboMultiplier(int wave) => 1f + Math.Max(0, wave - 1) * comboStep;
        public int ShieldCap(PetState pet) => BattleMath.Round(pet.MaxHp * maxShieldPercent);
        public float CriticalChance(PetState pet,int count) => Mathf.Clamp01(Math.Min(criticalChanceCap,
            (elements!=null ? pet.Definition.baseCritChance : criticalChance)+Math.Max(0,count-3)*criticalExtraChance+(pet.EvolutionLevel>1 ? evolvedCriticalBonus : 0)));
        public float SuddenDeathMultiplier(int completedRounds) => completedRounds<suddenDeathAfterRounds ? 1 : 1+Math.Min(suddenDeathMaxBonus,
            (1+(completedRounds-suddenDeathAfterRounds)/Math.Max(1,suddenDeathStepRounds))*suddenDeathStep);
        public int GroupRageBonus(int count) => count>=5 ? fiveGemRageBonus : count==4 ? fourGemRageBonus : 0;
        public float EffectDuration(BattleReport report) => report.Evolved ? evolutionSeconds : report.SpecialAttacks>0 ? specialAttackSeconds : report.NormalAttacks>0 ? normalAttackSeconds : effectSeconds;
        public void Validate()
        {
            foreach(var field in typeof(BattleRules).GetFields())
            {
                if(field.IsStatic || (field.FieldType!=typeof(float) && field.FieldType!=typeof(int)))continue;
                double value=Convert.ToDouble(field.GetValue(this));
                var min=Attribute.GetCustomAttribute(field,typeof(MinAttribute)) as MinAttribute;
                var range=Attribute.GetCustomAttribute(field,typeof(RangeAttribute)) as RangeAttribute;
                if(double.IsNaN(value) || double.IsInfinity(value) || (min!=null && value<min.min) || (range!=null && (value<range.min || value>range.max)))
                    throw new InvalidOperationException("Invalid battle setting: "+field.Name);
            }
            if(player==null || enemy==null || player.maxHp<1 || enemy.maxHp<1 || player.attack<1 || enemy.attack<1 || player.defense<0 || enemy.defense<0 || specialRageCost>maxRage || evolutionManaCost>maxMana)
                throw new InvalidOperationException("Invalid pet or unreachable card/special cost.");
        }
        public float DrainChance(float basis, float cap, int count) => Mathf.Clamp01(Math.Min(cap,basis+Math.Max(0,count-3)*drainExtraChance));
    }
}
