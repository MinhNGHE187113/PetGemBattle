using System;

namespace PetGemBattle
{
    public static class BattleMath
    {
        // Inspector floats can represent 22.5 as 22.499998 after multiplication.
        // Normalize sub-stat precision before applying the documented midpoint rule.
        public static int Round(float value) => Round(Math.Round((double)value,4));
        public static int Round(double value) => (int)Math.Round(value,MidpointRounding.AwayFromZero);
    }
    public interface IBattleRandom { double Next(); }
    public sealed class SeededBattleRandom : IBattleRandom
    {
        private readonly Random random;
        public SeededBattleRandom(int seed) { random=new Random(seed); }
        public double Next() => random.NextDouble();
    }
}
