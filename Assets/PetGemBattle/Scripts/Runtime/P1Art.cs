using System;
using UnityEngine;
using UnityEngine.UI;

namespace PetGemBattle
{
    // Original geometric sprite vocabulary for the articulated temporary rigs and stone silhouettes.
    public static class P1Art
    {
        private static Sprite circle,leaf,triangle;
        private static readonly Sprite[] stones=new Sprite[7];
        public static Sprite Circle=>circle!=null ? circle : circle=Raster((x,y)=>x*x+y*y<.9f);
        public static Sprite Leaf=>leaf!=null ? leaf : leaf=Raster((x,y)=>Mathf.Abs(x)<.65f*(1-y*y) && Mathf.Abs(y)<.95f);
        public static Sprite Triangle=>triangle!=null ? triangle : triangle=Raster((x,y)=>y>-.8f && y<.9f && Mathf.Abs(x)<(.9f-y)*.5f);
        private static Sprite Raster(Func<float,float,bool> mask)
        {
            const int n=128;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Bilinear;var colors=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float px=(x+0.5f)/n*2-1,py=(y+.5f)/n*2-1;float light=.72f+.28f*(py+1)/2;colors[y*n+x]=mask(px,py) ? new Color(light,light,light,1) : Color.clear;}
            tex.SetPixels(colors);tex.Apply();return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100);
        }
        public static Sprite Stone(int grade)
        {
            if(grade<1||grade>7)throw new ArgumentOutOfRangeException(nameof(grade));if(stones[grade-1]!=null)return stones[grade-1];
            return stones[grade-1]=Raster((x,y)=>
            {
                float r=Mathf.Sqrt(x*x+y*y),a=Mathf.Atan2(y,x);
                switch(grade)
                {
                    case 1:return x*x+(y+.3f)*(y+.3f)<.23f || y>-.2f && y<.85f && Mathf.Abs(x)<(.85f-y)*.48f;
                    case 2:return (Mathf.Pow((x-.35f)/.3f,2)+Mathf.Pow((y-.22f)/.6f,2)<1) || (Mathf.Pow((x+.35f)/.3f,2)+Mathf.Pow((y-.22f)/.6f,2)<1) || Mathf.Abs(x)<.09f && y>-.9f && y<.4f;
                    case 3:return Mathf.Abs(x)<.8f && Mathf.Abs(y)<.86f-Mathf.Abs(x)*.5f;
                    case 4:return r<.58f+.28f*Mathf.Cos(a*4);
                    case 5:return r<.66f+.22f*Mathf.Cos(a*10);
                    case 6:return (Mathf.Abs(x)+Mathf.Abs(y)<1.2f && Mathf.Abs(x)+Mathf.Abs(y)>.98f) || (x*x+Mathf.Pow(y-Mathf.Sqrt(Mathf.Abs(x))*.65f,2)<.45f);
                    default:return y>-.65f && y<-.12f || y>=-.12f && y<.85f && (Mathf.Abs(x)<(.85f-y)*.25f || Mathf.Abs(x-.65f)<(.85f-y)*.3f || Mathf.Abs(x+.65f)<(.85f-y)*.3f);
                }
            });
        }
        public static Image Part(Transform parent,string name,Sprite sprite,Vector2 position,Vector2 size,Color color,float angle=0)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.useSpriteMesh=true;image.color=color;image.raycastTarget=false;if(P2ArtLibrary.Geometry(sprite)!=null)go.AddComponent<P2SilhouetteMesh>();image.rectTransform.anchoredPosition=position;image.rectTransform.sizeDelta=size;image.rectTransform.localRotation=Quaternion.Euler(0,0,angle);return image;
        }
    }
    public sealed class ArticulatedPet : MonoBehaviour
    {
        public PetAnimationEvents Events {get;private set;}
        private Animator animator;
        private PaintedPetRig painted;
        private P3PetRig p3;
        private RectTransform body,head,tail,earL,earR,armL,armR,footL,footR,eyeL,eyeR,crown;
        private Image[] coloredParts;private Color[] originalColors;
        private int species;private bool left,evolved;private PetPose pose;private float elapsed,duration=1;
        public void Build(int species,bool left,Color tint,string artSheet=null)
        {
            Events=gameObject.AddComponent<PetAnimationEvents>();
            var controller=Resources.Load<RuntimeAnimatorController>("P1PetAnimator");
            if(controller!=null){animator=gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;}
            if(P3ArtLibrary.Get(artSheet)!=null){p3=gameObject.AddComponent<P3PetRig>();p3.Build(artSheet,left,tint,Events);return;}
            if(P2ArtLibrary.Get(artSheet)!=null){painted=gameObject.AddComponent<PaintedPetRig>();painted.Build(artSheet,left,Events);return;}
            this.species=species;this.left=left;
            body=P1Art.Part(transform,"Body",P1Art.Circle,new Vector2(0,-25),new Vector2(species==3 ? 220 : 170,190),tint).rectTransform;
            tail=P1Art.Part(body,"Tail",P1Art.Leaf,new Vector2(-100,-25),new Vector2(species==3 ? 65 : 40,species==3 ? 180 : 110),tint*.8f,-65).rectTransform;tail.SetAsFirstSibling();
            footL=P1Art.Part(body,"FootL",P1Art.Circle,new Vector2(-58,-83),new Vector2(70,45),tint*.75f).rectTransform;
            footR=P1Art.Part(body,"FootR",P1Art.Circle,new Vector2(58,-83),new Vector2(70,45),tint*.75f).rectTransform;
            armL=P1Art.Part(body,"ArmL",P1Art.Leaf,new Vector2(-90,-3),new Vector2(45,105),tint*.85f,30).rectTransform;
            armR=P1Art.Part(body,"ArmR",P1Art.Leaf,new Vector2(90,-3),new Vector2(45,105),tint*.85f,-30).rectTransform;
            head=P1Art.Part(body,"Head",P1Art.Circle,new Vector2(0,90),new Vector2(species==6 ? 205 : 190,155),tint).rectTransform;
            earL=P1Art.Part(head,"EarL",species==0 || species==7 ? P1Art.Triangle : P1Art.Leaf,new Vector2(-65,65),new Vector2(58,100),tint*.85f,22).rectTransform;
            earR=P1Art.Part(head,"EarR",species==0 || species==7 ? P1Art.Triangle : P1Art.Leaf,new Vector2(65,65),new Vector2(58,100),tint*.85f,-22).rectTransform;
            if(species==1){earL.sizeDelta=earR.sizeDelta=new Vector2(70,70);earL.GetComponent<Image>().sprite=earR.GetComponent<Image>().sprite=P1Art.Circle;}
            eyeL=P1Art.Part(head,"EyeL",P1Art.Circle,new Vector2(-37,10),new Vector2(28,38),new Color(.045f,.10f,.13f)).rectTransform;
            eyeR=P1Art.Part(head,"EyeR",P1Art.Circle,new Vector2(37,10),new Vector2(28,38),new Color(.045f,.10f,.13f)).rectTransform;
            foreach(var eye in new[]{eyeL,eyeR})P1Art.Part(eye,"Highlight",P1Art.Circle,new Vector2(5,7),new Vector2(9,11),Color.white);
            P1Art.Part(head,"Muzzle",P1Art.Circle,new Vector2(0,-30),new Vector2(66,36),new Color(1,.93f,.72f));
            P1Art.Part(head,"Nose",P1Art.Triangle,new Vector2(0,-23),new Vector2(18,14),new Color(.2f,.19f,.2f),180);
            for(int side=-1;side<=1;side+=2)P1Art.Part(head,"Cheek",P1Art.Circle,new Vector2(side*66,-18),new Vector2(27,13),new Color(1,.65f,.55f,.7f));
            if(species==2)
            {
                earL.gameObject.SetActive(false);earR.gameObject.SetActive(false);
                var cap=P1Art.Part(head,"MushroomCap",P1Art.Circle,new Vector2(0,77),new Vector2(250,115),new Color(.9f,.36f,.35f));
                for(int i=0;i<5;i++)P1Art.Part(cap.transform,"CapSpot",P1Art.Circle,new Vector2((i-2)*39,(i%2)*22-5),new Vector2(23,18),new Color(1,.95f,.7f));
            }
            if(species==4)for(int i=0;i<10;i++)P1Art.Part(body,"LeafSpine",P1Art.Triangle,new Vector2(Mathf.Cos(i*.628f)*105,Mathf.Sin(i*.628f)*100),new Vector2(65,85),new Color(.2f,.62f,.45f),i*36-90).transform.SetAsFirstSibling();
            if(species==5 || species==6)for(int side=-1;side<=1;side+=2)
            {
                var antler=P1Art.Part(head,"BranchAntler",P1Art.Leaf,new Vector2(side*65,110),new Vector2(22,125),new Color(.55f,.33f,.15f),side*-16);
                for(int i=0;i<3;i++)P1Art.Part(antler.transform,"BranchLeaf",P1Art.Leaf,new Vector2(side*20,i*25-10),new Vector2(36,55),tint,side*55);
            }
            if(species==6){body.GetComponent<Image>().color=new Color(.52f,.33f,.17f);for(int i=0;i<5;i++)P1Art.Part(head,"TreeCanopy",P1Art.Circle,new Vector2((i-2)*41,88+Mathf.Sin(i)*15),new Vector2(92,84),new Color(.2f,.6f,.32f)).transform.SetAsFirstSibling();}
            if(species==0 || species==4 || species==7)P1Art.Part(head,"Sprout",P1Art.Leaf,new Vector2(0,95),new Vector2(38,80),species==7 ? new Color(1,.83f,.35f) : new Color(.6f,.95f,.35f),-25);
            crown=P1Art.Part(head,"EvolvedCrown",P1Art.Stone(7),new Vector2(0,118),new Vector2(180,100),new Color(1,.85f,.36f)).rectTransform;crown.gameObject.SetActive(false);
        }
        public void SetPose(PetPose value,float seconds){painted?.SetPose(value,seconds);p3?.SetPose(value,seconds);pose=value;duration=Mathf.Max(.01f,seconds);elapsed=0;if(Events!=null)Events.phase=0;if(animator!=null){animator.speed=1/duration;animator.Play(value.ToString(),0,0);}}
        public void SetBossAwaken(){painted?.BossAwaken();p3?.BossAwaken();SetPose(PetPose.Evolution,1.2f);}
        public void ResetForm(){painted?.ResetForm();p3?.ResetForm();evolved=false;if(crown!=null)crown.gameObject.SetActive(false);SetPose(PetPose.Idle,1);}
        private void Update()
        {
            if(body==null)return;if(coloredParts==null){coloredParts=body.GetComponentsInChildren<Image>(true);originalColors=new Color[coloredParts.Length];for(int i=0;i<coloredParts.Length;i++)originalColors[i]=coloredParts[i].color;}elapsed+=Time.deltaTime;float t=Mathf.Clamp01(animator!=null && pose!=PetPose.Idle ? Events.phase : elapsed/duration),idle=Mathf.Sin(Time.time*2.6f),strike=Mathf.Sin(t*Mathf.PI);bool attack=pose==PetPose.Attack||pose==PetPose.Special;
            if(pose==PetPose.Evolution && t>.5f){evolved=true;crown.gameObject.SetActive(true);}
            float size=evolved ? 1.18f : 1;
            body.localScale=new Vector3((left ? 1 : -1)*size*(1+idle*.025f),size*(1-idle*.02f),1);
            body.anchoredPosition=new Vector2(attack ? (left ? 1 : -1)*strike*(pose==PetPose.Special ? 145 : 100) : pose==PetPose.Hit ? Mathf.Sin(t*25)*18*(1-t) : 0,-25+idle*5+(pose==PetPose.Victory ? Mathf.Abs(Mathf.Sin(elapsed*6))*28 : 0)-(pose==PetPose.Defeat ? t*55 : 0));
            body.localRotation=Quaternion.Euler(0,0,pose==PetPose.Defeat ? (left ? -55 : 55)*t : 0);
            head.localRotation=Quaternion.Euler(0,0,idle*3+(attack ? -strike*15 : 0));tail.localRotation=Quaternion.Euler(0,0,-65+Mathf.Sin(Time.time*3)*18);
            earL.localRotation=Quaternion.Euler(0,0,22+Mathf.Sin(Time.time*3.2f)*10);earR.localRotation=Quaternion.Euler(0,0,-22-Mathf.Sin(Time.time*3.2f+.8f)*10);
            armL.localRotation=Quaternion.Euler(0,0,30+(attack ? strike*80 : idle*8));armR.localRotation=Quaternion.Euler(0,0,-30-(attack ? strike*100 : idle*8));
            footL.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*3)*8);footR.localRotation=Quaternion.Euler(0,0,-Mathf.Sin(Time.time*3)*8);
            float blink=Time.time%4.1f<.12f || pose==PetPose.Defeat ? .1f : 1;eyeL.localScale=eyeR.localScale=new Vector3(1,blink,1);
            for(int i=0;i<coloredParts.Length;i++){var c=pose==PetPose.Hit && elapsed<.08f ? Color.white : originalColors[i];c.a=pose==PetPose.Defeat ? 1-t*.45f : 1;coloredParts[i].color=c;}
            if(t>=1 && pose!=PetPose.Idle && pose!=PetPose.Victory && pose!=PetPose.Defeat)SetPose(PetPose.Idle,1);
        }
    }
}
