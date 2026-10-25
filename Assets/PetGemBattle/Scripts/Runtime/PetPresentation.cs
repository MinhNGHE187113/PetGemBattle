using UnityEngine;
using UnityEngine.UI;

namespace PetGemBattle
{
    public enum PetPose { Idle,Attack,Special,Hit,Evolution,Victory,Defeat }
    public sealed class PetPresentation : MonoBehaviour
    {
        public bool IsEvolved { get; private set; }
        public PetPose Pose { get; private set; }
        public Image Body { get; private set; }
        private Sprite normal,evolved;
        private ArticulatedPet rig;
        public PetAnimationEvents AnimationEvents=>rig==null ? null : rig.Events;
        private RectTransform halo,ground;
        private readonly RectTransform[] sparks=new RectTransform[16];
        private Color tint;
        private float poseTime,poseDuration=1;
        private bool left;
        public void Build(PlaceholderArt art,PetDefinition pet,bool isLeft)
        {
            left=isLeft;tint=isLeft ? new Color(1,.42f,.08f) : new Color(.12f,.75f,1);
            normal=art.Load(pet.art);evolved=art.Load(pet.art+"Evolved");
            var glow=ImageChild("AuraGlow",art.Glow,new Vector2(440,470),tint*.65f);glow.rectTransform.anchoredPosition=new Vector2(0,10);
            halo=ImageChild("RunicAura",art.Ring,new Vector2(370,370),new Color(tint.r,tint.g,tint.b,.45f)).rectTransform;
            halo.anchoredPosition=new Vector2(0,35);
            ground=ImageChild("SummoningCircle",art.Ring,new Vector2(400,90),new Color(tint.r,tint.g,tint.b,.9f)).rectTransform;
            ground.anchoredPosition=new Vector2(0,-215);
            Body=ImageChild("PetSprite",normal,new Vector2(420,470),Color.white);Body.preserveAspect=true;
            if(pet.rig>=0){Body.enabled=false;rig=gameObject.AddComponent<ArticulatedPet>();rig.Build(pet.rig,isLeft,P1Catalog.ElementColor(pet.element),pet.artSheet);}
            for(int i=0;i<sparks.Length;i++)sparks[i]=ImageChild("AuraParticle",isLeft ? art.Glow : art.Diamond,new Vector2(7,12),tint).rectTransform;
        }
        private Image ImageChild(string name,Sprite sprite,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(transform,false);
            var img=go.GetComponent<Image>();img.sprite=sprite;img.color=color;img.raycastTarget=false;
            img.rectTransform.sizeDelta=size;return img;
        }
        public void SetPose(PetPose pose,float duration=1) { Pose=pose;poseTime=0;poseDuration=Mathf.Max(.01f,duration);rig?.SetPose(pose,duration); }
        public void BossAwaken(float duration){rig?.SetBossAwaken();SetPose(PetPose.Evolution,duration);}
        public void ResetForm()
        {
            rig?.ResetForm();IsEvolved=false;Body.sprite=normal;Body.color=Color.white;SetPose(PetPose.Idle);
            Body.rectTransform.localRotation=Quaternion.identity;Body.rectTransform.anchoredPosition=Vector2.zero;
        }
        private void Update()
        {
            if(Body==null)return;
            poseTime+=Time.deltaTime;float t=Mathf.Clamp01(poseTime/poseDuration);
            if(Pose==PetPose.Evolution && t>=.5f && !IsEvolved){IsEvolved=true;Body.sprite=evolved!=null ? evolved : normal;}
            float idle=Mathf.Sin(Time.time*1.8f+(left ? 0 : 1));
            Vector2 offset=new Vector2(0,idle*4);float scale=IsEvolved ? 1.08f : 1;float rotation=0;Color color=Color.white;
            if(Pose==PetPose.Attack || Pose==PetPose.Special)
            {
                float hit=Mathf.Sin(t*Mathf.PI);offset.x=(left ? 1 : -1)*hit*(Pose==PetPose.Special ? 72 : 30);
                rotation=(left ? -1 : 1)*hit*8;scale+=hit*(Pose==PetPose.Special ? .15f : .04f);
            }
            if(Pose==PetPose.Hit){offset.x=Mathf.Sin(t*32)*(1-t)*15;color=Color.Lerp(new Color(1,.4f,.35f),Color.white,t);}
            if(Pose==PetPose.Evolution){scale+=Mathf.Sin(t*Mathf.PI)*.16f;color=Color.Lerp(tint,Color.white,Mathf.Abs(t-.5f)*2);}
            if(Pose==PetPose.Victory){offset.y+=Mathf.Abs(Mathf.Sin(poseTime*3))*14;rotation=Mathf.Sin(poseTime*2)*3;}
            if(Pose==PetPose.Defeat){rotation=(left ? -1 : 1)*Mathf.SmoothStep(0,65,t);offset.y-=t*65;color=Color.Lerp(Color.white,new Color(.30f,.35f,.4f,.5f),t);}
            Body.rectTransform.anchoredPosition=offset;Body.rectTransform.localScale=Vector3.one*scale;
            Body.rectTransform.localRotation=Quaternion.Euler(0,0,rotation);Body.color=color;
            halo.localRotation=Quaternion.Euler(0,0,Time.time*(IsEvolved ? 15 : 7)*(left ? 1 : -1));
            halo.localScale=Vector3.one*((IsEvolved ? 1.2f : 1)+idle*.025f);
            bool reduced=PlayerPrefs.GetInt("P2ReducedVfx",0)==1;
            for(int i=0;i<sparks.Length;i++)
            {
                if(sparks[i].gameObject.activeSelf==(!reduced || i<4)){}else sparks[i].gameObject.SetActive(!reduced || i<4);if(reduced && i>=4)continue;
                float phase=(Time.time*(IsEvolved ? .32f : .18f)+i/(float)sparks.Length)%1;
                float angle=i*2.399f+Time.time*.3f;
                sparks[i].anchoredPosition=left ? new Vector2(Mathf.Sin(angle)*170,-190+phase*420) : new Vector2(Mathf.Cos(angle)*185,Mathf.Sin(angle)*195);
                sparks[i].localScale=Vector3.one*(IsEvolved ? 2 : 1)*(1+Mathf.Sin(phase*Mathf.PI));
                sparks[i].GetComponent<Image>().color=new Color(tint.r,tint.g,tint.b,Pose==PetPose.Defeat ? .1f : Mathf.Sin(phase*Mathf.PI)*.8f);
            }
            if(t>=1 && Pose!=PetPose.Idle && Pose!=PetPose.Victory && Pose!=PetPose.Defeat)SetPose(PetPose.Idle);
        }
    }
}
