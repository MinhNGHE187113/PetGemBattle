using System;
using System.Collections.Generic;
using UnityEngine;

namespace PetGemBattle
{
    public sealed class PlaceholderArt
    {
        public readonly Sprite[] Gems=new Sprite[BattleRules.GemKinds];
        public readonly Sprite[] Icons=new Sprite[BattleRules.GemKinds];
        public readonly Sprite Ring,Glow,Diamond,Background;
        private readonly List<Sprite> sprites=new List<Sprite>();
        private readonly List<Texture2D> ownedTextures=new List<Texture2D>();
        // Enum order: sword, shield, heart, mana, rage, drain. No legacy palette.
        public static readonly Color[] Colors={
            new Color(1f,.73f,.12f),new Color(.65f,.32f,.92f),new Color(.17f,.82f,.39f),
            new Color(.12f,.63f,1f),new Color(1f,.23f,.12f),new Color(.65f,.72f,.79f)
        };
        public PlaceholderArt()
        {
            for(int i=0;i<Gems.Length;i++)
            {
                int kind=i;var tint=Colors[i];
                Gems[i]=Make(128,(x,y)=>
                {
                    float edge=Math.Max(Math.Max(Math.Abs(x),Math.Abs(y)),(Math.Abs(x)+Math.Abs(y))/.1f*.0715f);
                    if(edge>.96f)return Color.clear;
                    float light=Mathf.Clamp01(.46f+y*.25f-x*.12f);
                    Color c=Color.Lerp(tint*.45f,Color.Lerp(tint,Color.white,.38f),light);
                    if(edge>.84f)c=Color.Lerp(tint*.30f,Color.white,y>0 ? .75f : .12f);
                    if(edge>.93f)c=Color.Lerp(tint,Color.white,.75f);
                    if(y>.55f && x<.55f)c=Color.Lerp(c,Color.white,.15f);
                    c.a=1;return c;
                });
                Icons[i]=Make(96,(x,y)=>Icon(kind,x,y) ? new Color(1,.98f,.84f) : Color.clear);
            }
            Ring=Make(256,(x,y)=>
            {
                float r=Mathf.Sqrt(x*x+y*y),angle=Mathf.Atan2(y,x);
                bool line=Mathf.Abs(r-.83f)<.012f || Mathf.Abs(r-.72f)<.006f;
                bool rune=r>.73f && r<.82f && Mathf.Cos(angle*20)>.87f;
                return line||rune ? Color.white : Color.clear;
            });
            Glow=Make(128,(x,y)=>new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-Mathf.Sqrt(x*x+y*y)),2)));
            Diamond=Make(64,(x,y)=>Math.Abs(x)+Math.Abs(y)<.9f ? Color.white : Color.clear);
            Background=Load("TempleArena");
        }
        public Sprite Load(string name)
        {
            var texture=Resources.Load<Texture2D>("BattleArt/"+name);
            if(texture==null)return null;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);
            sprites.Add(sprite);return sprite;
        }
        private static bool Icon(int k,float x,float y)
        {
            switch((GemKind)k)
            {
                case GemKind.Sword:
                    float u=(x+y)*.707f,v=(y-x)*.707f;
                    return (Math.Abs(u)<.10f && v>-.26f && v<.72f-Math.Abs(u)) ||
                        (Math.Abs(v+.28f)<.075f && Math.Abs(u)<.32f) || (Math.Abs(u)<.07f && v>-.65f && v<-.28f);
                case GemKind.Shield:return Math.Abs(x)<.51f && y<.6f && y>-.68f+Math.Abs(x)*.85f &&
                    (Math.Abs(x)>.36f || y>.45f || y<-.48f+Math.Abs(x)*.85f || Math.Abs(x)<.05f);
                case GemKind.Heart:return ((x+.25f)*(x+.25f)+(y-.2f)*(y-.2f)<.29f*.29f) ||
                    ((x-.25f)*(x-.25f)+(y-.2f)*(y-.2f)<.29f*.29f) || (y<.23f && y>-.60f && Math.Abs(x)<(.60f+y)*.7f);
                case GemKind.Mana:return (y>-.67f && y<.7f && (y>0 ? x>-.12f && x<.4f-y*.45f : x<.12f && x>-.4f-y*.45f));
                case GemKind.Rage:
                    return (x*x/.21f+(y+.17f)*(y+.17f)/.26f<1 && !(x*x/.035f+(y+.32f)*(y+.32f)/.08f<1)) ||
                        (y>.05f && y<.72f && x>-.40f+y*.48f && x<.4f-y*.55f);
                default:
                    float radius=Mathf.Sqrt(x*x+y*y),a=Mathf.Atan2(y,x);
                    for(int i=0;i<3;i++)if(Mathf.Abs(radius-(.09f+.085f*(a+Mathf.PI+i*Mathf.PI*2)))<.055f && radius<.65f)return true;
                    return radius<.10f;
            }
        }
        private Sprite Make(int size,Func<float,float,Color> pixel)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            var data=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)data[y*size+x]=pixel((x+.5f)/size*2-1,(y+.5f)/size*2-1);
            texture.SetPixels(data);texture.Apply();ownedTextures.Add(texture);
            var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100);sprites.Add(sprite);return sprite;
        }
        public void Dispose()
        {
            foreach(var sprite in sprites)UnityEngine.Object.Destroy(sprite);
            foreach(var texture in ownedTextures)UnityEngine.Object.Destroy(texture);
        }
    }
}
