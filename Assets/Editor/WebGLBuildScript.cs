using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PushNotificationGod.Editor
{
    public static class WebGLBuildScript
    {
        private const string DefaultOutputPath = "Builds/WebGL";
        private const string GitHubPagesOutputPath = "docs";

        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/TitleScene.unity",
            "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/ResultScene.unity"
        };

        [MenuItem("Tools/Push Notification God/Build WebGL")]
        public static void BuildWebGL()
        {
            BuildWebGL(DefaultOutputPath);
        }

        public static void BuildWebGLFromCommandLine()
        {
            BuildWebGL(GetCommandLineOutputPath());
        }

        private static void BuildWebGL(string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            Directory.CreateDirectory(outputPath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePaths[0], true),
                new EditorBuildSettingsScene(ScenePaths[1], true),
                new EditorBuildSettingsScene(ScenePaths[2], true)
            };

            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.WebGL.template = "PROJECT:FixedAspect";
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            BuildReport report = BuildPipeline.BuildPlayer(ScenePaths, outputPath, BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed: {report.summary.result}");
            }

            Debug.Log($"WebGL build succeeded: {outputPath}");
            CopyBuildToGitHubPagesDocs(outputPath);
        }

        private static void CopyBuildToGitHubPagesDocs(string buildOutputPath)
        {
            if (!Directory.Exists(buildOutputPath))
            {
                throw new DirectoryNotFoundException($"WebGL build output not found: {buildOutputPath}");
            }

            if (Directory.Exists(GitHubPagesOutputPath))
            {
                Directory.Delete(GitHubPagesOutputPath, true);
            }

            CopyDirectory(buildOutputPath, GitHubPagesOutputPath);
            Debug.Log("WebGL build copied to docs for GitHub Pages");
        }

        private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);

            foreach (string sourceFile in Directory.GetFiles(sourceDirectory))
            {
                string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, destinationFile, true);
            }

            foreach (string sourceSubDirectory in Directory.GetDirectories(sourceDirectory))
            {
                string destinationSubDirectory = Path.Combine(destinationDirectory, Path.GetFileName(sourceSubDirectory));
                CopyDirectory(sourceSubDirectory, destinationSubDirectory);
            }
        }

        private static string GetCommandLineOutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildPath")
                {
                    return args[i + 1];
                }
            }

            return DefaultOutputPath;
        }
    }
}
