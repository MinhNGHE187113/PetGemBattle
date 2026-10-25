using UnityEditor;
using UnityEngine;

namespace PetGemBattle.Editor
{
    public sealed class BattleArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.Contains("PetGemBattle/Resources/BattleArt/"))return;
            Configure((TextureImporter)assetImporter);
        }
        private static void Configure(TextureImporter importer)
        {
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
        }
        [MenuItem("Tools/Pet Gem Battle/Prepare Fantasy Art")]
        public static void Prepare()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/PetGemBattle/Resources/BattleArt"}))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));Configure(importer);importer.SaveAndReimport();
            }
            Debug.Log("PET_GEM_ART_READY");
        }
    }
}
