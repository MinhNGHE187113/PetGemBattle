using System;
using UnityEngine;
namespace PetGemBattle
{
    public sealed class PetAnimationEvents : MonoBehaviour
    {
        [HideInInspector] public float phase;
        public event Action Impact,Completed;
        public void OnAttackImpact(){Impact?.Invoke();}
        public void OnAnimationComplete(){Completed?.Invoke();}
    }
}
