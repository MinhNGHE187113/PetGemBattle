using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PetGemBattle.Editor
{
    public static class BattleProjectSetup
    {
        public const string ScenePath="Assets/Scenes/PetGemBattle.unity";
        public const string RulesPath="Assets/PetGemBattle/Data/ClassicBattleRules.asset";
        [MenuItem("Tools/Pet Gem Battle/Open Battle Scene")]
        public static void OpenBattleScene()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EnsureAssets();
            EditorSceneManager.OpenScene(ScenePath);
        }
        public static void SetupBatch()
        {
            EnsureAssets();
            Debug.Log("PET_GEM_SETUP_OK: "+ScenePath);
        }
        private static void EnsureAssets()
        {
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>(RulesPath);
            if(rules==null)
            {
                rules=ScriptableObject.CreateInstance<BattleRules>();
                AssetDatabase.CreateAsset(rules,RulesPath);
            }
            if(!File.Exists(ScenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
                camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
                var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5;
                cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.035f,.055f,.1f);
                var battle=new GameObject("PetGemBattle");
                var controller=battle.AddComponent<BattleController>();
                var serialized=new SerializedObject(controller);
                serialized.FindProperty("rules").objectReferenceValue=rules;serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            var buildScenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if(!buildScenes.Exists(s=>s.path==ScenePath))buildScenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=buildScenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
