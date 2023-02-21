using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace HoloLab.UnityWebServer.Editor
{
    [InitializeOnLoad]
    internal sealed class CopyPlugins : IActiveBuildTargetChanged
    {
        private static readonly string SentinelFileName = "UnityWebServer.Plugins.sentinel";
        private static readonly string SentinelFileGuid = "60b11da8abf40f64cbe833e0e53f25cf";
        private static readonly string ImportTargetFolder = "Assets/UnityWebServer/Plugins";

        private static readonly string SessionKey = "_UnityWebServer_CopyPlugins";

        private const string SymbolCommonPluginsExist = "UNITYWEBSERVER_PLUGINS_EXIST";
        private static readonly object addSymbolLock = new object();

        // Returns true only the first time after the Unity Editor started
        private static bool IsNewSession
        {
            get
            {
                if (SessionState.GetBool(SessionKey, false))
                {
                    return false;
                }

                SessionState.SetBool(SessionKey, true);
                return true;
            }
        }

        public int callbackOrder => 0;

        static CopyPlugins()
        {
            if (!IsNewSession || Application.isPlaying)
            {
                return;
            }

            try
            {
                if (ExistsPluginsFolder())
                {
                    return;
                }

                var pluginsFolder = FindPluginsFolder();
                if (pluginsFolder == null)
                {
                    return;
                }

                ImportPlugins(pluginsFolder);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Check if the Plugins folder exists in the Assets folder
        /// </summary>
        /// <returns></returns>
        private static bool ExistsPluginsFolder()
        {
            var path = AssetDatabase.GUIDToAssetPath(SentinelFileGuid);
            return !string.IsNullOrEmpty(path);
        }

        private static DirectoryInfo FindPluginsFolder()
        {
            var packageCacheFolder = Path.GetFullPath(Path.Combine("Library", "PackageCache"));

            var sentinelFile = Directory.GetFiles(packageCacheFolder, SentinelFileName, SearchOption.AllDirectories)
                .FirstOrDefault();

            if (sentinelFile == null)
            {
                return null;
            }

            var pluginDirectory = Directory.GetParent(sentinelFile);
            return pluginDirectory;
        }

        private static void ImportPlugins(DirectoryInfo pluginsFolder)
        {
            var srcFolder = pluginsFolder.FullName;
            var targetFolder = Path.GetFullPath(ImportTargetFolder);
            DirectoryCopy(srcFolder, targetFolder, true);

            AddCommonPluginsSymbol(EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// Copy directory
        /// ref: https://docs.microsoft.com/ja-jp/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        private static void AddCommonPluginsSymbol(BuildTarget buildTarget)
        {
            lock (addSymbolLock)
            {
                var group = BuildPipeline.GetBuildTargetGroup(buildTarget);
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
                if (symbols.Contains(SymbolCommonPluginsExist))
                {
                    return;
                }
                symbols.Add(SymbolCommonPluginsExist);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
                AssetDatabase.SaveAssets();
            }
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            if (ExistsPluginsFolder())
            {
                AddCommonPluginsSymbol(newTarget);
            }
        }
    }
}