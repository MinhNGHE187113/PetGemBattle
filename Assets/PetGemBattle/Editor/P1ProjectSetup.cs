using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PetGemBattle.Editor
{
    public static class P1ProjectSetup
    {
        public const string HomeScene="Assets/Scenes/P1Home.unity";
        [MenuItem("Tools/Pet Gem Battle/Open P1 Home")]
        public static void Open()
        {
            if(EditorApplication.isPlaying)return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            Ensure();EditorSceneManager.OpenScene(HomeScene);
        }
        public static void Ensure()
        {
            P1AnimatorSetup.Ensure();
            const string path="Assets/PetGemBattle/Resources/P1Catalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<P1Catalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<P1Catalog>();catalog.SeedDefaults();AssetDatabase.CreateAsset(catalog,path);}
            catalog.ValidateContent();
            const string rulesPath="Assets/PetGemBattle/Resources/P1BattleRules.asset";
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>(rulesPath);
            if(rules==null){rules=Object.Instantiate(AssetDatabase.LoadAssetAtPath<BattleRules>(BattleProjectSetup.RulesPath));rules.elements=catalog;rules.cinematicTurns=true;AssetDatabase.CreateAsset(rules,rulesPath);}
            Directory.CreateDirectory("Assets/PetGemBattle/Resources/P1Stones");
            for(int grade=1;grade<=7;grade++)
            {
                string png="Assets/PetGemBattle/Resources/P1Stones/Stone"+grade+".png";
                if(!File.Exists(png))File.WriteAllBytes(png,P1Art.Stone(grade).texture.EncodeToPNG());
            }
            if(!File.Exists(HomeScene))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;
                var home=new GameObject("P1Home",typeof(HomeController));home.GetComponent<HomeController>().catalog=catalog;
                EditorSceneManager.SaveScene(scene,HomeScene);
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(HomeScene,true),new EditorBuildSettingsScene(BattleProjectSetup.ScenePath,true)};
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("P1_CONTENT_READY");
        }
    }
}
