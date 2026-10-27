using UnityEditor;
using UnityEngine;

namespace PetGemBattle.Editor
{
    public sealed class P3ArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.Contains("/Resources/PetRemaster/"))
            {
                var spriteImporter = (TextureImporter)assetImporter;
                spriteImporter.textureType = TextureImporterType.Sprite;
                spriteImporter.spriteImportMode = SpriteImportMode.Single;
                spriteImporter.alphaIsTransparency = true;
                spriteImporter.isReadable = false;
                spriteImporter.mipmapEnabled = false;
                spriteImporter.maxTextureSize = 4096;
                spriteImporter.textureCompression = TextureImporterCompression.Uncompressed;
                spriteImporter.filterMode = FilterMode.Bilinear;
                spriteImporter.wrapMode = TextureWrapMode.Clamp;
                return;
            }
            if (!assetPath.Contains("/Resources/P3Art/")) return;
            var importer = (TextureImporter)assetImporter;
            if (assetPath.Contains("/Resources/P3Art/NavIcons/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                return;
            }
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
