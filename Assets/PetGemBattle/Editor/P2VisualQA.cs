using System;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace PetGemBattle.Editor
{
    // Engine render smoke check; uses an isolated save and never drives the user's desktop.
    public static class P2VisualQA
    {
        [InitializeOnLoadMethod] private static void RestoreCallback()
        {
            if(SessionState.GetBool("P2VisualQA.Pending",false)){EditorApplication.playModeStateChanged-=Ready;EditorApplication.playModeStateChanged+=Ready;}
        }
        public static void Capture()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool("P2VisualQA.Pending",true);
            EditorApplication.playModeStateChanged+=Ready;EditorApplication.isPlaying=true;
        }
        private static void Ready(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode)return;
            SessionState.SetBool("P2VisualQA.Pending",false);
            EditorApplication.playModeStateChanged-=Ready;
            new GameObject("P2VisualQA").AddComponent<P2VisualRunner>();
        }
    }
    public sealed class P2VisualRunner : MonoBehaviour
    {
        private HomeController home;private Camera captureCamera;private RenderTexture target;
        private IEnumerator Start()
        {
            Application.runInBackground=true;
            var root=new GameObject("VisualHome");home=root.AddComponent<HomeController>();
            home.catalog=Resources.Load<P1Catalog>("P1Catalog");home.SavePathOverride=Path.GetFullPath("Logs/P2-Visual-QA-save.json");
            captureCamera=new GameObject("UICaptureCamera",typeof(Camera)).GetComponent<Camera>();
            captureCamera.gameObject.AddComponent<AudioListener>();
            captureCamera.clearFlags=CameraClearFlags.SolidColor;captureCamera.backgroundColor=new Color(.03f,.08f,.1f);
            captureCamera.orthographic=true;captureCamera.orthographicSize=450;captureCamera.nearClipPlane=.1f;captureCamera.farClipPlane=100;
            target=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);target.Create();captureCamera.targetTexture=target;
            yield return new WaitForSeconds(.3f);home.Progress.ResetDemo();home.Home();yield return new WaitForSeconds(.3f);
            yield return Capture("P2-Home");
            home.Navigate("Challenges");yield return Capture("P2-Islands");
            home.Navigate("Island");yield return Capture("P2-WoodMap");
            home.Home();home.Navigate("Challenges");yield return null;
            root.GetComponentsInChildren<Button>().Single(b=>b.transform.parent.name=="Island_fire" && b.name=="XEM TRƯỚC").onClick.Invoke();
            yield return Capture("P2-FirePreview");
            home.Home();home.Navigate("Pets");yield return Capture("P2-Collection");
            home.Home();home.Navigate("Bag");yield return null;
            root.GetComponentsInChildren<Button>().Single(b=>b.name=="Hỏa").onClick.Invoke();yield return Capture("P2-FireStones");
            home.Progress.Owned("wood-7").level=7;home.Progress.Select("wood-7");home.Home();yield return null;
            root.GetComponentsInChildren<Button>().Single(b=>b.name=="NÂNG CẤP").onClick.Invoke();yield return Capture("P2-Upgrade");
            home.Home();home.StartBattle();yield return new WaitForSeconds(.4f);yield return Capture("P2-Battle");
            var battle=FindFirstObjectByType<BattleController>();BoardMove? sword=null;
            for(int n=0;n<50 && !sword.HasValue;n++)
            {
                foreach(var move in battle.Board.LegalMoves())
                {
                    battle.Board.Swap(move);bool found=battle.Board.FindMatches().Any(g=>g.Kind==GemKind.Sword);battle.Board.Swap(move);
                    if(found){sword=move;break;}
                }
                if(!sword.HasValue)battle.Board.GeneratePlayable();
            }
            if(sword.HasValue)
            {
                battle.GetComponent<BattleView>().DrawBoard(battle.Board);
                var inputs=battle.GetComponentsInChildren<GemInput>();
                inputs.Single(g=>g.Cell.Equals(sword.Value.A)).Click(sword.Value.A);
                inputs.Single(g=>g.Cell.Equals(sword.Value.B)).Click(sword.Value.B);
                float stop=Time.realtimeSinceStartup+25;
                while(battle.Presentation!=PresentationPhase.Combat && Time.realtimeSinceStartup<stop)yield return null;
                yield return Capture("P2-Combat");
            }
            home.Progress.ResetDemo();home.Home();yield return new WaitForSeconds(.4f);yield return Capture("P2-Home");
            Debug.Log("P2_VISUAL_CAPTURE_DONE");EditorApplication.Exit(0);
        }
        private IEnumerator Capture(string name)
        {
            yield return null;
            foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=captureCamera;canvas.planeDistance=10;
                var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null){scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=Mathf.Min(1600f/scaler.referenceResolution.x,900f/scaler.referenceResolution.y);}
            }
            Canvas.ForceUpdateCanvases();yield return new WaitForSeconds(.15f);
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
            Directory.CreateDirectory("Docs");File.WriteAllBytes("Docs/"+name+".png",texture.EncodeToPNG());Destroy(texture);RenderTexture.active=previous;
        }
    }
}
