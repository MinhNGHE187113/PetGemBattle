namespace PetGemBattle
{
    public enum CardKind { Mana, Rage, Evolution, Signature }
    public static class CardSystem
    {
        public static bool CanUse(PetState pet,CardKind card,BattleRules rules)
        {
            if(pet.Hp<=0 || pet.SkillUsed)return false;
            switch(card)
            {
                case CardKind.Mana:return pet.ManaCardsUsed<rules.manaCardUses && pet.Mana<rules.maxMana;
                case CardKind.Rage:return pet.RageCardsUsed<rules.rageCardUses && pet.Rage<rules.maxRage;
                case CardKind.Evolution:return pet.Definition.rarity!=PetRarity.SS && pet.EvolutionLevel==1 && pet.Mana>=rules.evolutionManaCost;
                case CardKind.Signature:return pet.Definition.signaturePower>0 && pet.SignatureCooldown==0 && pet.Mana>=pet.Definition.signatureManaCost && pet.Rage>=pet.Definition.signatureRageCost;
                default:return false;
            }
        }
        public static BattleReport Use(PetState pet,CardKind card,BattleRules rules,PetState target=null,float rhythmMultiplier=1f,int completedRounds=0)
        {
            if(!CanUse(pet,card,rules))return null;
            if(card==CardKind.Signature)
            {
                if(target==null || target.Hp<=0)return null;
                var result=new BattleReport{SpecialAttacks=1};
                result.ManaSpent=pet.RemoveMana(pet.Definition.signatureManaCost);
                result.RageSpent=pet.RemoveRage(pet.Definition.signatureRageCost);
                float advantage=rules.elements==null ? 1 : rules.elements.Multiplier(pet.Definition.element,target.Definition.element);
                int damage=System.Math.Max(1,BattleMath.Round(System.Math.Max(1,pet.Attack*pet.Definition.signaturePower-target.Defense*.5f)*advantage*rhythmMultiplier*rules.SuddenDeathMultiplier(completedRounds)));
                int shield=target.Shield;
                result.HpDamage=target.TakeDamage(damage);
                result.Absorbed=shield-target.Shield;
                result.Damage=damage;
                pet.RecordCard(card);
                return result;
            }
            var report=new BattleReport();
            if(card==CardKind.Mana)report.Mana=pet.AddMana(rules.manaCardGain,rules);
            else if(card==CardKind.Rage)report.Rage=pet.AddRage(rules.rageCardGain,rules);
            else
            {
                int oldHp=pet.Hp;
                EvolutionSystem.Apply(pet,rules);
                report.Healing=pet.Hp-oldHp;report.Evolved=true;
            }
            pet.RecordCard(card);
            return report;
        }
    }
    public static class EvolutionSystem
    {
        // All validation/spending is atomic; neither the UI nor AI can bypass the card rules.
        internal static void Apply(PetState pet,BattleRules rules) => pet.Evolve(rules);
    }
}
