using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Builder
{
    private static readonly string _buildDir = "Builds/";
    
    private static string[] CheckFolders(string platform, bool delete = true)
    {
        if (!Directory.Exists(_buildDir))
        {
            Directory.CreateDirectory(_buildDir);
        }

        if (delete && Directory.Exists(_buildDir + platform))
            Directory.Delete(_buildDir + platform, true);

        return EditorBuildSettings.scenes.Select(x => x.path).ToArray();
    }

    [MenuItem("Automation/Standalone/Windows")]
    public static void BuildWindows()
    {
        BuildWindows(true);
    }
    public static void BuildWindows(bool delete)
    {
        Build(CheckFolders("Windows", delete), BuildTarget.StandaloneWindows64, Application.productName + "_Win.exe",
            BuildTargetGroup.Standalone, "", 5, true, false);
    }

    [MenuItem("Automation/Standalone/Mac")]
    public static void BuildMac()
    {
        BuildMac(true);
    }
    public static void BuildMac(bool delete)
    {
        Build(CheckFolders("Mac", delete), BuildTarget.StandaloneOSX, Application.productName + "_Mac",
            BuildTargetGroup.Standalone, "", 5, true, false);
    }

    [MenuItem("Automation/Standalone/Linux")]
    public static void BuildLinux()
    {
        BuildLinux(true);
    }
    public static void BuildLinux(bool delete)
    {
        Build(CheckFolders("Linux", delete), BuildTarget.StandaloneLinux64, Application.productName + "_Linux",
            BuildTargetGroup.Standalone, "", 5, true, false);
    }

    [MenuItem("Automation/Standalone/All")]
    public static void BuildAll()
    {
        BuildWindows();
        BuildMac(false);
        BuildLinux(false);
    }
    [MenuItem("Automation/Web")]
    public static void BuildWeb()
    {
        Build(CheckFolders("Web", true), BuildTarget.WebGL, Application.productName + "_Web",
            BuildTargetGroup.WebGL, "", 5, true, false);
    }

    static void Build(string[] scenes, BuildTarget target, string name, BuildTargetGroup group, string symbols,
        int qualityLevel, bool il2Cpp = true, bool dev = false)
    {
        AssetDatabase.Refresh();

        try
        {
            QualitySettings.SetQualityLevel(qualityLevel);

            if (group == BuildTargetGroup.Android)
            {
                if (!il2Cpp)
                {
                    PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.Mono2x);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                }
                else
                {
                    PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
                    AndroidArchitecture aac = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    PlayerSettings.Android.targetArchitectures = aac;
                }
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = _buildDir + target + "/" + name;
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = BuildOptions.None;
            buildPlayerOptions.targetGroup = group;

            if (il2Cpp && target == BuildTarget.Android)
                EditorUserBuildSettings.buildAppBundle = true;
            else //if (target == BuildTarget.Android)
                EditorUserBuildSettings.buildAppBundle = false;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            //throw;
        }

        AssetDatabase.Refresh();
    }
}