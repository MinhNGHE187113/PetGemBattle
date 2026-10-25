namespace PetGemBattle
{
    public enum CardKind { Mana, Rage, Evolution }
    public static class CardSystem
    {
        public static bool CanUse(PetState pet,CardKind card,BattleRules rules)
        {
            if(pet.Hp<=0 || pet.SkillUsed)return false;
            switch(card)
            {
                case CardKind.Mana:return pet.ManaCardsUsed<rules.manaCardUses && pet.Mana<rules.maxMana;
                case CardKind.Rage:return pet.RageCardsUsed<rules.rageCardUses && pet.Rage<rules.maxRage;
                case CardKind.Evolution:return pet.EvolutionLevel==1 && pet.Mana>=rules.evolutionManaCost;
                default:return false;
            }
        }
        public static BattleReport Use(PetState pet,CardKind card,BattleRules rules)
        {
            if(!CanUse(pet,card,rules))return null;
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
