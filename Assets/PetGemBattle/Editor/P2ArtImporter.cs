using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PetGemBattle.Editor
{
    public static class P2ArtImporter
    {
        private sealed class Region
        {
            public int area,minX,minY,maxX,maxY;public List<Vector2> edge=new List<Vector2>();
        }
        public static void Build()
        {
            const string path="Assets/PetGemBattle/Resources/P2ArtLibrary.asset";
            var old=AssetDatabase.LoadAssetAtPath<P2ArtLibrary>(path);if(old!=null){RepairGeometry(old);P2ArtLibrary.ClearCache();return;}
            var database=ScriptableObject.CreateInstance<P2ArtLibrary>();AssetDatabase.CreateAsset(database,path);var sheets=new List<PaintedSheet>();
            foreach(string png in Directory.GetFiles("Assets/PetGemBattle/Resources/P2Art","*.png"))
            {
                var importer=AssetImporter.GetAtPath(png) as TextureImporter;if(importer==null){AssetDatabase.ImportAsset(png);importer=AssetImporter.GetAtPath(png) as TextureImporter;}
                importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.isReadable=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(png);string key=Path.GetFileNameWithoutExtension(png);int parts=key=="stones" ? 7 : key=="lobby" || key.StartsWith("island-") ? 1 : 4;var regions=parts==1 ? new List<Region>() : Regions(texture);var sprites=new Sprite[parts];
                for(int i=0;i<parts;i++)
                {
                    Region region=null;
                    if(parts==1)region=new Region{minX=0,minY=0,maxX=texture.width-1,maxY=texture.height-1};
                    else if(parts==7)region=regions.Where(r=>Mathf.Clamp((int)((r.minX+r.maxX)*.5f/texture.width*7),0,6)==i).OrderByDescending(r=>r.area).FirstOrDefault();
                    else region=regions.Where(r=>((r.minX+r.maxX)*.5f<texture.width*.5f ? 0 : 1)+((r.minY+r.maxY)*.5f>texture.height*.5f ? 0 : 2)==i).OrderByDescending(r=>r.area).FirstOrDefault();
                    if(region==null)throw new InvalidOperationException("Missing separated sprite "+key+" part "+i);
                    var rect=new Rect(region.minX,region.minY,region.maxX-region.minX+1,region.maxY-region.minY+1);
                    var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=key+"_"+i;
                    AssetDatabase.AddObjectToAsset(sprite,database);sprites[i]=sprite;
                }
                sheets.Add(new PaintedSheet{key=key,parts=sprites});
                importer.isReadable=false;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            database.sheets=sheets.ToArray();EditorUtility.SetDirty(database);AssetDatabase.SaveAssets();RepairGeometry(database);P2ArtLibrary.ClearCache();
        }
        private static List<Region> Regions(Texture2D texture)
        {
            int w=texture.width,h=texture.height;var pixels=texture.GetPixels32();var seen=new bool[w*h];var queue=new int[w*h];var result=new List<Region>();
            for(int start=0;start<pixels.Length;start++)
            {
                if(seen[start] || pixels[start].a<48)continue;
                var region=new Region{minX=w,minY=h};int head=0,tail=1;queue[0]=start;seen[start]=true;
                while(head<tail)
                {
                    int p=queue[head++],x=p%w,y=p/w;region.area++;region.minX=Math.Min(region.minX,x);region.maxX=Math.Max(region.maxX,x);region.minY=Math.Min(region.minY,y);region.maxY=Math.Max(region.maxY,y);
                    bool border=x==0||x==w-1||y==0||y==h-1;
                    for(int direction=0;direction<4;direction++)
                    {
                        int next=direction==0 ? p-1 : direction==1 ? p+1 : direction==2 ? p-w : p+w;
                        if(next<0||next>=pixels.Length || Math.Abs(next%w-x)>1)continue;
                        if(pixels[next].a<48){border=true;continue;}if(!seen[next]){seen[next]=true;queue[tail++]=next;}
                    }
                    if(border && (x+y)%3==0)region.edge.Add(new Vector2(x,y));
                }
                if(region.area>300)result.Add(region);
            }
            return result;
        }
        private static void RepairGeometry(P2ArtLibrary database)
        {
            foreach(var sheet in database.sheets)
            {
                if(sheet.parts.Length!=4 || (sheet.geometry!=null && sheet.geometry.Length==4))continue;
                string path="Assets/PetGemBattle/Resources/P2Art/"+sheet.key+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.isReadable=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var regions=Regions(texture);sheet.geometry=new PaintedGeometry[4];
                for(int i=0;i<4;i++)
                {
                    var region=regions.Where(r=>((r.minX+r.maxX)*.5f<texture.width*.5f ? 0 : 1)+((r.minY+r.maxY)*.5f>texture.height*.5f ? 0 : 2)==i).OrderByDescending(r=>r.area).First();
                    var rect=sheet.parts[i].rect;var hull=Hull(region.edge);var vertices=hull.Select(p=>(p-new Vector2(rect.center.x,rect.center.y))/100).ToArray();
                    var triangles=new List<int>();for(int n=1;n<vertices.Length-1;n++){triangles.Add(0);triangles.Add(n);triangles.Add(n+1);}
                    sheet.geometry[i]=new PaintedGeometry{vertices=vertices,triangles=triangles.ToArray()};
                }
                importer.isReadable=false;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            EditorUtility.SetDirty(database);AssetDatabase.SaveAssets();
        }
        private static List<Vector2> Hull(List<Vector2> points)
        {
            points=points.Distinct().OrderBy(p=>p.x).ThenBy(p=>p.y).ToList();var hull=new List<Vector2>();
            foreach(var p in points){while(hull.Count>=2 && Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            int count=hull.Count;for(int i=points.Count-2;i>=0;i--){var p=points[i];while(hull.Count>count && Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}
            if(hull.Count>1)hull.RemoveAt(hull.Count-1);return hull;
        }
        private static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
    }
}
