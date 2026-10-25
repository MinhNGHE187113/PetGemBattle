using System;
using UnityEngine;

namespace PetGemBattle
{
    public sealed class BattleAudio : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip fire,ice,specialFire,specialIce,evolve,restore,drain;
        private AudioClip critical,shield,mana,victory,defeat,music;
        private AudioSource musicSource;
        public bool Muted { get; private set; }
        private void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.22f;
            fire=Tone("Flame strike",.25f,180,55,.22f);ice=Tone("Ice strike",.25f,650,180,.12f);
            specialFire=Tone("Dragon inferno",.65f,90,35,.55f);specialIce=Tone("Thunder storm",.65f,320,50,.50f);
            evolve=Tone("Ascension",1.6f,220,880,.03f);restore=Tone("Recovery",.35f,440,780,.02f);drain=Tone("Siphon",.45f,700,80,.13f);
            critical=Tone("Critical accent",.3f,1100,220,.15f);shield=Tone("Armor bell",.4f,330,660,.04f);mana=Tone("Mana sparkle",.3f,880,1320,0);
            victory=Tone("Victory",1.1f,330,990,.01f);defeat=Tone("Defeat",.9f,330,110,.02f);
            music=CreateMusic();musicSource=gameObject.AddComponent<AudioSource>();musicSource.clip=music;musicSource.loop=true;musicSource.volume=.06f;musicSource.spatialBlend=0;musicSource.Play();
            Muted=PlayerPrefs.GetInt("BattleMuted",0)==1;ApplyMute();
        }
        private AudioClip Tone(string title,float duration,float start,float end,float noise)
        {
            const int rate=22050;int n=(int)(duration*rate);var samples=new float[n];var random=new System.Random(17);double phase=0;
            for(int i=0;i<n;i++)
            {
                float t=i/(float)n,f=Mathf.Lerp(start,end,t);phase+=f*2*System.Math.PI/rate;
                float envelope=Mathf.Sin(Mathf.PI*t)*Mathf.Pow(1-t,.4f);
                samples[i]=envelope*((float)(System.Math.Sin(phase)+.3*System.Math.Sin(phase*1.5))*.35f+(float)(random.NextDouble()*2-1)*noise);
            }
            var clip=AudioClip.Create(title,n,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void Play(BattleReport report,bool player)
        {
            AudioClip clip=(report.Evolved || report.BossTransformation) ? evolve : report.SpecialAttacks>0 ? (player ? specialFire : specialIce) :
                report.NormalAttacks>0 ? (player ? fire : ice) : report.DrainGroups>0 ? drain : report.Shield>0 ? shield : report.Mana>0 ? mana : restore;
            source.PlayOneShot(clip);
            if(report.CriticalAttacks>0)source.PlayOneShot(critical,.65f);
        }
        public void ToggleMute(){Muted=!Muted;ApplyMute();PlayerPrefs.SetInt("BattleMuted",Muted ? 1 : 0);PlayerPrefs.Save();}
        private void ApplyMute(){source.mute=musicSource.mute=Muted;}
        public void SetPaused(bool paused){if(paused){source.Pause();musicSource.Pause();}else{source.UnPause();musicSource.UnPause();}}
        public void PlayResult(BattleResult result){source.PlayOneShot(result==BattleResult.Defeat ? defeat : victory);}
        private AudioClip CreateMusic()
        {
            // Original synthesized ambient motif; no sampled or third-party audio.
            const int rate=22050;const int seconds=16;var samples=new float[rate*seconds];
            double[] notes={220,261.6256,329.6276,293.6648,196,261.6256,246.9417,164.8138};
            for(int i=0;i<samples.Length;i++)
            {
                double t=i/(double)rate,local=t%2,f=notes[(int)(t/2)];
                double envelope=Math.Sin(Math.PI*local/2);envelope*=envelope;
                samples[i]=(float)(envelope*(Math.Sin(2*Math.PI*f*local)*.28+Math.Sin(2*Math.PI*f*.5*local)*.15));
            }
            var clip=AudioClip.Create("Temple ambient original",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void Stop() { source.Stop(); }
        private void OnDestroy(){foreach(var clip in new[]{fire,ice,specialFire,specialIce,evolve,restore,drain,critical,shield,mana,victory,defeat,music})Destroy(clip);}
    }
}
