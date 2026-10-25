using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PetGemBattle.Editor
{
    public static class BattleBuild
    {
        [MenuItem("Tools/Pet Gem Battle/Build Windows")]
        public static void BuildWindows()
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("Exit Play Mode before building.");return;}
            P2ProjectSetup.Ensure();
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/PetGemBattle/Resources/P1BattleRules.asset");rules.Validate();
            Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{P1ProjectSetup.HomeScene,BattleProjectSetup.ScenePath},locationPathName="Builds/Windows/PetArenaClassic.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/WindowsBuild.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSize: "+report.summary.totalSize);
            Debug.Log("PET_ARENA_BUILD "+report.summary.result);
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors>0)throw new System.InvalidOperationException("Windows build contains errors. See Logs/WindowsBuild.txt");
        }
    }
}
