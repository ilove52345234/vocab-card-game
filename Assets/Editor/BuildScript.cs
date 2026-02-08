using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace VocabCardGame.Editor
{
    /// <summary>
    /// CLI 建置腳本
    /// </summary>
    public static class BuildScript
    {
        private const string DefaultOutputDir = "Builds/WebGL";
        private const string DefaultScenePath = "Assets/Scenes/MvpScene.unity";

        /// <summary>
        /// 建置 WebGL（供 CLI -executeMethod 使用）
        /// </summary>
        public static void BuildWebGL()
        {
            EnsureScenes();

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new Exception("WebGL build support is not installed. Please add 'WebGL Build Support' in Unity Hub for this editor version.");
            }

            // 關閉壓縮，避免本機 server 缺少 gzip header 造成載入失敗
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;

            string outputDir = Environment.GetEnvironmentVariable("BUILD_OUTPUT");
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = DefaultOutputDir;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = outputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed: {summary.result}");
            }

            UnityEngine.Debug.Log("[BuildScript] WebGL build succeeded: " + outputDir);
        }

        private static void EnsureScenes()
        {
            // 確保至少有 MVP 場景
            if (!File.Exists(DefaultScenePath))
            {
                throw new FileNotFoundException("MVP scene not found: " + DefaultScenePath);
            }

            // 如果 Build Settings 沒有任何場景，就加入 MVP 場景
            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
            {
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(DefaultScenePath, true)
                };
            }
        }
    }
}
