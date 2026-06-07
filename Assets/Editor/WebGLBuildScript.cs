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
        private const string TestOutputPath = "Builds/WebGL_Test";
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
            BuildWebGL(DefaultOutputPath, true);
        }

        [MenuItem("Tools/Push Notification God/Build WebGL Test")]
        public static void BuildWebGLTest()
        {
            BuildWebGL(TestOutputPath, false);
        }

        public static void BuildWebGLFromCommandLine()
        {
            BuildWebGL(GetCommandLineOutputPath(), true);
        }

        public static void BuildWebGLTestFromCommandLine()
        {
            BuildWebGL(GetCommandLineOutputPath(TestOutputPath), false);
        }

        private static void BuildWebGL(string outputPath, bool copyToDocs)
        {
            Debug.Log($"Build ID: {PushNotificationGod.Core.BuildInfo.BuildId}");
            if (copyToDocs)
            {
                CleanGitHubPagesOutput();
            }

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
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            BuildReport report = BuildPipeline.BuildPlayer(ScenePaths, outputPath, BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed: {report.summary.result}");
            }

            Debug.Log($"WebGL build succeeded: {outputPath}");
            if (copyToDocs)
            {
                CopyBuildToGitHubPagesDocs(outputPath);
            }
        }

        private static void CleanGitHubPagesOutput()
        {
            if (Directory.Exists(GitHubPagesOutputPath))
            {
                Directory.Delete(GitHubPagesOutputPath, true);
                Debug.Log($"Cleaned GitHub Pages output before build: {GitHubPagesOutputPath}");
            }

            Directory.CreateDirectory(GitHubPagesOutputPath);
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
            RemoveUnexpectedDocsArtifacts();
            string sourceIndex = Path.Combine(buildOutputPath, "index.html");
            string destinationIndex = Path.Combine(GitHubPagesOutputPath, "index.html");
            Debug.Log($"WebGL build copied to docs for GitHub Pages");
            Debug.Log($"Copied {sourceIndex} -> {destinationIndex}");
            Debug.Log($"docs/index.html updated at {File.GetLastWriteTime(destinationIndex):yyyy-MM-dd HH:mm:ss}");
        }

        private static void RemoveUnexpectedDocsArtifacts()
        {
            foreach (string file in Directory.GetFiles(GitHubPagesOutputPath))
            {
                string name = Path.GetFileName(file);
                if (name == "index.html")
                {
                    continue;
                }

                File.Delete(file);
                Debug.LogWarning($"Removed unexpected docs file: {file}");
            }

            foreach (string directory in Directory.GetDirectories(GitHubPagesOutputPath))
            {
                string name = Path.GetFileName(directory);
                if (name == "Build" || name == "TemplateData")
                {
                    continue;
                }

                Directory.Delete(directory, true);
                Debug.LogWarning($"Removed unexpected docs directory: {directory}");
            }

            foreach (string duplicateIndex in Directory.GetFiles(GitHubPagesOutputPath, "index *.html"))
            {
                File.Delete(duplicateIndex);
                Debug.LogWarning($"Removed duplicate docs index file: {duplicateIndex}");
            }
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

        private static string GetCommandLineOutputPath(string fallback = DefaultOutputPath)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildPath")
                {
                    return args[i + 1];
                }
            }

            return fallback;
        }
    }
}
