using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace PetGemBattle
{
    [Serializable] public sealed class PaintedSheet
    {
        public string key;
        public Sprite[] parts;
        public PaintedGeometry[] geometry;
    }
    [Serializable] public sealed class PaintedGeometry {public Vector2[] vertices;public int[] triangles;}
    public sealed class P2ArtLibrary : ScriptableObject
    {
        public PaintedSheet[] sheets;
        private static P2ArtLibrary loaded;
        private static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        private static readonly Dictionary<Sprite,PaintedGeometry> meshes=new Dictionary<Sprite,PaintedGeometry>();
        public static Sprite Get(string key,int part=0)
        {
            if(string.IsNullOrEmpty(key))return null;
            if(key.StartsWith("p3-"))return part==0 ? P3ArtLibrary.Get(key) : null;
            string id=key+":"+part;if(cache.TryGetValue(id,out var sprite))return sprite;
            if(loaded==null)loaded=Resources.Load<P2ArtLibrary>("P2ArtLibrary");
            var sheet=loaded==null ? null : loaded.sheets.FirstOrDefault(s=>s.key==key);
            sprite=sheet!=null && part<sheet.parts.Length ? sheet.parts[part] : null;
            if(sprite!=null && sheet.geometry!=null && part<sheet.geometry.Length)
            {
                var mesh=sheet.geometry[part];
                if(mesh!=null && mesh.vertices!=null && mesh.vertices.Length>=3)
                {
                    meshes[sprite]=mesh;
                }
            }
            cache[id]=sprite;return sprite;
        }
        public static void ClearCache()
        {
            meshes.Clear();cache.Clear();loaded=null;
        }
        public static PaintedGeometry Geometry(Sprite sprite)=>sprite!=null && meshes.TryGetValue(sprite,out var mesh) ? mesh : null;
    }
}
