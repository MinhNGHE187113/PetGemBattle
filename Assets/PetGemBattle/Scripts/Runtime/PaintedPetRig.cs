using UnityEngine;
using UnityEngine.UI;
namespace PetGemBattle
{
    public sealed class PaintedPetRig : MonoBehaviour
    {
        private RectTransform pivot,torso,head,accessory,orb,crown,eyeL,eyeR;
        private PetPose pose;private float elapsed,duration=1;private bool left,awakened,boss;
        private string sheet,baseSheet;private PetAnimationEvents events;
        private Image[] images;
        public bool Silhouette;
        public void Build(string key,bool isLeft,PetAnimationEvents frameEvents)
        {
            sheet=baseSheet=key;left=isLeft;events=frameEvents;
            pivot=new GameObject("PaintedRig",typeof(RectTransform)).GetComponent<RectTransform>();pivot.SetParent(transform,false);pivot.localScale=new Vector3(left ? 1 : -1,1,1);
            P1Art.Part(pivot,"ContactShadow",P1Art.Circle,new Vector2(0,-154),new Vector2(260,28),new Color(.03f,.10f,.11f,.28f));
            accessory=Part("TailOrMantle",3,new Vector2(-84,-10),new Vector2(130,175));
            torso=Part("PaintedTorso",2,new Vector2(0,-40),new Vector2(190,220));
            head=Part("PaintedHead",1,new Vector2(22,92),new Vector2(205,205));
            if(key.StartsWith("wood-3")){head.sizeDelta=new Vector2(255,205);head.anchoredPosition=new Vector2(0,105);torso.sizeDelta=new Vector2(175,190);}
            if(key.StartsWith("wood-6")){head.anchoredPosition=new Vector2(43,112);head.sizeDelta=new Vector2(190,205);torso.sizeDelta=new Vector2(205,230);accessory.sizeDelta=new Vector2(115,140);accessory.anchoredPosition=new Vector2(-30,90);}
            if(key.StartsWith("wood-7")){head.sizeDelta=new Vector2(230,200);head.anchoredPosition=new Vector2(0,108);torso.sizeDelta=new Vector2(215,220);accessory.anchoredPosition=new Vector2(118,0);accessory.sizeDelta=new Vector2(78,290);accessory.SetAsLastSibling();}
            if(key=="metal-bird"){torso.sizeDelta=new Vector2(160,205);head.sizeDelta=new Vector2(190,195);accessory.sizeDelta=new Vector2(170,210);}
            if(key=="earth-turtle"){torso.sizeDelta=new Vector2(245,175);head.sizeDelta=new Vector2(175,175);head.anchoredPosition=new Vector2(60,70);accessory.sizeDelta=new Vector2(180,175);accessory.anchoredPosition=new Vector2(-50,-10);}
            var glow=P1Art.Part(pivot,"AwakenedLight",P1Art.Circle,Vector2.zero,new Vector2(300,300),new Color(.8f,1,.55f,.12f));orb=glow.rectTransform;orb.SetAsFirstSibling();orb.gameObject.SetActive(false);
            var crownSprite=P2ArtLibrary.Get("stones",6)??P1Art.Stone(7);crown=P1Art.Part(head,"AwakenedCrest",crownSprite,new Vector2(0,95),new Vector2(95,65),Color.white).rectTransform;crown.gameObject.SetActive(false);
            Vector2 eyeAtL=new Vector2(0,-38),eyeAtR=new Vector2(60,-50),sizeL=new Vector2(29,34),sizeR=new Vector2(23,29);
            Color skin=new Color(.94f,.87f,.72f);
            if(key=="starter-metal"){eyeAtL=new Vector2(8,-30);eyeAtR=new Vector2(65,-43);}
            if(key.StartsWith("wood-2")){eyeAtL=new Vector2(1,-12);eyeAtR=new Vector2(72,-21);sizeL=new Vector2(35,40);sizeR=new Vector2(20,35);skin=new Color(.58f,.72f,.28f);}
            if(key.StartsWith("wood-3")){eyeAtL=new Vector2(6,-54);eyeAtR=new Vector2(49,-49);sizeL=new Vector2(31,23);sizeR=new Vector2(18,26);}
            if(key.StartsWith("wood-4"))skin=new Color(.32f,.61f,.31f);
            if(key.StartsWith("wood-6")){eyeAtL=new Vector2(-13,-46);eyeAtR=new Vector2(32,-60);sizeL=new Vector2(29,28);sizeR=new Vector2(17,28);skin=new Color(.76f,.49f,.28f);}
            if(key.StartsWith("wood-7")){eyeAtL=new Vector2(12,-64);eyeAtR=new Vector2(64,-77);sizeL=new Vector2(34,29);sizeR=new Vector2(20,27);skin=new Color(.61f,.38f,.14f);}
            if(key=="fire-lion"){eyeAtL=new Vector2(0,-34);eyeAtR=new Vector2(58,-43);skin=new Color(.97f,.55f,.22f);}
            if(key=="metal-bird")skin=new Color(.82f,.87f,.92f);
            if(key=="earth-turtle")skin=new Color(.8f,.64f,.36f);
            eyeL=Eyelid(eyeAtL,sizeL,skin);eyeR=Eyelid(eyeAtR,sizeR,skin);
            images=pivot.GetComponentsInChildren<Image>(true);
        }
        private RectTransform Eyelid(Vector2 at,Vector2 size,Color skin)
        {
            var eye=P1Art.Part(head,"BlinkLid",P1Art.Circle,at,size,skin);
            P1Art.Part(eye.transform,"ClosedLash",P1Art.Leaf,new Vector2(0,-3),new Vector2(3,size.x*.9f),new Color(.23f,.19f,.13f),90);eye.gameObject.SetActive(false);return eye.rectTransform;
        }
        private RectTransform Part(string name,int index,Vector2 at,Vector2 size){var image=P1Art.Part(pivot,name,P2ArtLibrary.Get(sheet,index),at,size,Color.white);image.preserveAspect=true;return image.rectTransform;}
        public void SetPose(PetPose value,float seconds){pose=value;elapsed=0;duration=Mathf.Max(.01f,seconds);}
        private void SwapSheet(string key){if(P2ArtLibrary.Get(key)==null)return;sheet=key;head.GetComponent<Image>().sprite=P2ArtLibrary.Get(key,1);torso.GetComponent<Image>().sprite=P2ArtLibrary.Get(key,2);accessory.GetComponent<Image>().sprite=P2ArtLibrary.Get(key,3);}
        public void BossAwaken(){SwapSheet(baseSheet+"-form1");boss=true;awakened=true;orb.gameObject.SetActive(true);crown.gameObject.SetActive(true);SetPose(PetPose.Evolution,1.2f);}
        public void ResetForm(){SwapSheet(baseSheet);boss=awakened=false;crown.gameObject.SetActive(false);orb.gameObject.SetActive(false);SetPose(PetPose.Idle,1);}
        private void Update()
        {
            if(pivot==null)return;elapsed+=Time.deltaTime;float t=Mathf.Clamp01(events!=null && pose!=PetPose.Idle ? events.phase : elapsed/duration);
            float idle=Mathf.Sin(Time.time*2.2f),wind=Mathf.Sin(Time.time*1.7f+.7f),strike=Mathf.Sin(t*Mathf.PI);
            if(pose==PetPose.Evolution && t>.5f){awakened=true;orb.gameObject.SetActive(true);crown.gameObject.SetActive(true);}
            bool attack=pose==PetPose.Attack||pose==PetPose.Special;
            float lunge=attack ? strike*(pose==PetPose.Special ? 112 : 78) : pose==PetPose.Hit ? -Mathf.Sin(t*Mathf.PI)*12 : 0;
            pivot.anchoredPosition=new Vector2((left ? 1 : -1)*lunge,idle*3+(pose==PetPose.Victory ? Mathf.Abs(Mathf.Sin(elapsed*5))*18 : 0)-(pose==PetPose.Defeat ? t*48 : 0));
            pivot.localRotation=Quaternion.Euler(0,0,pose==PetPose.Defeat ? (left ? -1 : 1)*t*48 : 0);
            float size=awakened ? 1.09f : 1;pivot.localScale=new Vector3((left ? 1 : -1)*size*(1+idle*.008f),size*(1-idle*.01f),1);
            head.localRotation=Quaternion.Euler(0,0,wind*2+(attack ? -strike*10 : 0));torso.localRotation=Quaternion.Euler(0,0,attack ? -strike*5 : idle*.8f);
            accessory.localRotation=Quaternion.Euler(0,0,wind*(sheet=="metal-bird" ? 14 : 6)+(attack ? strike*12 : 0));accessory.localScale=Vector3.one*(boss ? 1.18f : 1);
            bool blink=(Time.time+transform.GetInstanceID()%13)%4.6f<.10f || pose==PetPose.Defeat;eyeL.gameObject.SetActive(blink);eyeR.gameObject.SetActive(blink);
            foreach(var image in images)
            {
                if(image.name=="ContactShadow" || image.name=="AwakenedLight" || image.name.StartsWith("Blink") || image.name=="ClosedLash")continue;
                image.color=Silhouette ? new Color(.05f,.10f,.12f) : pose==PetPose.Hit && elapsed<.08f ? new Color(1,.65f,.55f) : pose==PetPose.Defeat ? Color.Lerp(Color.white,new Color(.5f,.55f,.57f,.6f),t) : Color.white;
            }
            orb.localScale=Vector3.one*(1+Mathf.Sin(Time.time*3)*.05f);crown.localRotation=Quaternion.Euler(0,0,wind*3);
            if(t>=1 && pose!=PetPose.Idle && pose!=PetPose.Victory && pose!=PetPose.Defeat)SetPose(PetPose.Idle,1);
        }
    }
}
