using UnityEngine;
namespace PetGemBattle
{
    [CreateAssetMenu(menuName="Pet Gem Battle/Game Mode Config")]
    public sealed class GameModeConfig : ScriptableObject
    {
        public bool DemoFullCollection=true;
        public bool DemoTools=true;
        [Min(0)] public int DemoStoneAmount=2;
    }
}
