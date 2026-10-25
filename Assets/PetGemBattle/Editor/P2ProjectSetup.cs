using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PetGemBattle.Editor
{
    public static class P2ProjectSetup
    {
        [MenuItem("Tools/Pet Gem Battle/Open P2 Home")]
        public static void Open()
        {
            if(EditorApplication.isPlaying)return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            Ensure();EditorSceneManager.OpenScene(P1ProjectSetup.HomeScene);
        }
        public static void Ensure()
        {
            P1ProjectSetup.Ensure();
            var catalog=AssetDatabase.LoadAssetAtPath<P1Catalog>("Assets/PetGemBattle/Resources/P1Catalog.asset");P2Content.Upgrade(catalog);catalog.ValidateContent();EditorUtility.SetDirty(catalog);
            const string modePath="Assets/PetGemBattle/Resources/GameModeConfig.asset";var mode=AssetDatabase.LoadAssetAtPath<GameModeConfig>(modePath);
            if(mode==null){mode=ScriptableObject.CreateInstance<GameModeConfig>();AssetDatabase.CreateAsset(mode,modePath);}
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/PetGemBattle/Resources/P1BattleRules.asset");
            rules.attackMultiplier=2.3f;rules.swordExtraMultiplier=.4f;rules.specialManaCost=30;rules.finalMultiplierRounding=true;rules.elements=catalog;rules.cinematicTurns=true;EditorUtility.SetDirty(rules);rules.Validate();
            P2ArtImporter.Build();
            P2PrefabSetup.Ensure(catalog);
            foreach(var pet in catalog.pets)
            {
                if(P2ArtLibrary.Get(pet.artSheet)==null)throw new System.InvalidOperationException("Missing art: "+pet.artSheet);
                foreach(var form in pet.forms??new EvolutionFormDefinition[0])if(P2ArtLibrary.Get(form.artSheet)==null)throw new System.InvalidOperationException("Missing form art: "+form.artSheet);
            }
            foreach(var island in catalog.islands)if(P2ArtLibrary.Get(island.art)==null)throw new System.InvalidOperationException("Missing island art "+island.art);
            AssetDatabase.SaveAssets();Debug.Log("P2_CONTENT_READY");
        }
    }
}
