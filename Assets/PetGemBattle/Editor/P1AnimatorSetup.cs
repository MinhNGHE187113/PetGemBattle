using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace PetGemBattle.Editor
{
    public static class P1AnimatorSetup
    {
        public static void Ensure()
        {
            const string folder="Assets/PetGemBattle/Resources/P1Animations";
            const string path="Assets/PetGemBattle/Resources/P1PetAnimator.controller";
            Directory.CreateDirectory(folder);if(File.Exists(path))return;
            var controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach(PetPose pose in Enum.GetValues(typeof(PetPose)))
            {
                var clip=new AnimationClip{name=pose.ToString(),frameRate=30};
                clip.SetCurve("",typeof(PetAnimationEvents),"phase",AnimationCurve.Linear(0,0,1,1));
                var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=pose==PetPose.Idle;AnimationUtility.SetAnimationClipSettings(clip,settings);
                if(pose==PetPose.Attack || pose==PetPose.Special)AnimationUtility.SetAnimationEvents(clip,new[]{
                    new AnimationEvent{time=.4f,functionName="OnAttackImpact"},new AnimationEvent{time=.99f,functionName="OnAnimationComplete"}});
                else if(pose!=PetPose.Idle)AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{time=.99f,functionName="OnAnimationComplete"}});
                AssetDatabase.CreateAsset(clip,folder+"/"+pose+".anim");
                var state=controller.layers[0].stateMachine.AddState(pose.ToString());state.motion=clip;
                if(pose==PetPose.Idle)controller.layers[0].stateMachine.defaultState=state;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
