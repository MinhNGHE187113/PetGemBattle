using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace PetGemBattle.Editor
{
    public static class P2PrefabSetup
    {
        public static void Ensure(P1Catalog catalog)
        {
            const string materials="Assets/PetGemBattle/Resources/P2StoneMaterials";
            Directory.CreateDirectory(materials);
            var shader=Shader.Find("PetArena/Painted Stone Core");
            if(shader==null)throw new System.InvalidOperationException("P2 stone shader missing");
            foreach(Element element in System.Enum.GetValues(typeof(Element)))
            {
                string path=materials+"/"+element+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
                material.SetColor("_CoreColor",P1Catalog.ElementColor(element));material.SetFloat("_Pattern",(int)element);EditorUtility.SetDirty(material);
            }
            const string folder="Assets/PetGemBattle/Resources/P2Pets";
            Directory.CreateDirectory(folder);
            foreach(var pet in catalog.pets)
            {
                Save(pet.artSheet,pet,folder);
                foreach(var form in pet.forms??new EvolutionFormDefinition[0])Save(form.artSheet,pet,folder);
            }
        }
        private static void Save(string key,PetSpecies pet,string folder)
        {
            string path=folder+"/"+key+".prefab";bool existed=File.Exists(path);
            var go=existed ? PrefabUtility.LoadPrefabContents(path) : new GameObject(key,typeof(RectTransform),typeof(P2PetPrefab));
            if(!existed)go.GetComponent<RectTransform>().sizeDelta=new Vector2(300,350);
            var profile=go.GetComponent<P2PetPrefab>()??go.AddComponent<P2PetPrefab>();profile.sheet=key;profile.species=pet.rig;profile.element=pet.element;
            var library=AssetDatabase.LoadAssetAtPath<P2ArtLibrary>("Assets/PetGemBattle/Resources/P2ArtLibrary.asset");
            var sheet=library==null || library.sheets==null ? null : System.Array.Find(library.sheets,s=>s.key==key);
            if(!key.StartsWith("p3-") && (sheet==null || sheet.parts==null || sheet.parts.Length==0 || sheet.parts[0]==null))
            {
                Object.DestroyImmediate(go);
                throw new System.InvalidOperationException("Missing painted Pet art sheet: "+key);
            }
            var source=key.StartsWith("p3-") ? P3ArtLibrary.Get(key) : sheet.parts[0];
            var preview=go.transform.Find("EditorPreview")?.GetComponent<Image>();
            if(preview==null)preview=P1Art.Part(go.transform,"EditorPreview",source,Vector2.zero,new Vector2(310,350),Color.white);
            preview.sprite=source;preview.preserveAspect=true;preview.color=Color.white;preview.rectTransform.anchoredPosition=Vector2.zero;preview.rectTransform.sizeDelta=new Vector2(310,350);
            PrefabUtility.SaveAsPrefabAsset(go,path);
            if(existed)PrefabUtility.UnloadPrefabContents(go);else Object.DestroyImmediate(go);
        }
    }
}
