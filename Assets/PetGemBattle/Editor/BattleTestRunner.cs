using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace PetGemBattle.Editor
{
    public sealed class BattleTestRunner : ICallbacks
    {
        private static TestRunnerApi api;
        private const string PendingResultKey="PetGemBattle.PendingTestResult";
        private readonly string resultFile;
        private BattleTestRunner(string file) { resultFile=file; }
        [InitializeOnLoadMethod]
        private static void RestoreCallbackAfterReload()
        {
            string file=SessionState.GetString(PendingResultKey,"");
            if(!string.IsNullOrEmpty(file))TestRunnerApi.RegisterTestCallback(new BattleTestRunner(file));
        }
        [MenuItem("Tools/Pet Gem Battle/Run Core Tests")]
        public static void Run()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before running EditMode tests.");return; }
            Execute(TestMode.EditMode,"PetGemBattle.Tests","Logs/PetGemBattle-TestResults.xml");
        }
        [MenuItem("Tools/Pet Gem Battle/Run Gameplay Tests")]
        public static void RunGameplay()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before running tests.");return; }
            Execute(TestMode.PlayMode,"PetGemBattle.PlayModeTests","Logs/PetGemBattle-PlayModeResults.xml");
        }
        private static void Execute(TestMode mode,string assembly,string file)
        {
            SessionState.SetString(PendingResultKey,file);
            api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new BattleTestRunner(file));
            api.Execute(new ExecutionSettings(new Filter { testMode=mode, assemblyNames=new[]{assembly} }));
        }
        public void RunStarted(ITestAdaptor test) { Debug.Log("PET_GEM_TESTS_STARTED"); }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            Directory.CreateDirectory("Logs");
            TestRunnerApi.SaveResultToFile(result,resultFile);
            Debug.Log("PET_GEM_TESTS_FINISHED passed="+result.PassCount+" failed="+result.FailCount);
            SessionState.EraseString(PendingResultKey);
            TestRunnerApi.UnregisterTestCallback(this);
            Object.DestroyImmediate(api);
        }
    }
}
