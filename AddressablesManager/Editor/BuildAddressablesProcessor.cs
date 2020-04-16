/// <summary>
/// The script gives you choice to whether to build addressable bundles when clicking the build button.
/// For custom build script, call PreExport method yourself.
/// For cloud build, put BuildAddressablesProcessor.PreExport as PreExport command.
/// Discussion: https://forum.unity.com/threads/how-to-trigger-build-player-content-when-build-unity-project.689602/
/// Original source: https://gist.github.com/favoyang/cd2cf2ed9df7e2538f3630610c604c51
/// </summary>
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections;

class BuildAddressablesProcessor
{
    public static bool cancelBuild = false;
    public static bool showWarningBeforeBuild = true;

    /// <summary>
    /// Run a clean build before export.
    /// </summary>
    //[MenuItem("Addressables/Shortcuts/Build Addressables")] //uncomment for a shortcut to building addressables
    static public void PreExport()
    {
        Debug.Log("Building Addressables...");
        AddressableAssetSettings.CleanPlayerContent(
            AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("Finished building addressables.");
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
    }

    private static void BuildPlayerHandler(BuildPlayerOptions options)
    {            
        if (showWarningBeforeBuild)
        {
            BuildAddressablesWindow.Execute();
        }

        if (cancelBuild)
        {
            Debug.Log("Build has been cancelled.");
            cancelBuild = false;
            return;
        }        

        BuildReport report = BuildPipeline.BuildPlayer(options);

        Debug.LogFormat("{0} build finished with status of {4}.\nBuild took {1} minutes to complete.\nBuild started at {2}\nBuild ended at {3}\nBuild size: {5}MB", 
            report.summary.platformGroup, 
            report.summary.totalTime.TotalMinutes.ToString("F2"), 
            report.summary.buildStartedAt.ToString(),
            report.summary.buildEndedAt.ToString(),
            report.summary.result.ToString(),
            Mathf.RoundToInt(report.summary.totalSize / (1024 * 1024)));
    }

}

public class BuildAddressablesWindow : EditorWindow
{
    public static void Execute()
    {
        int option = EditorUtility.DisplayDialogComplex("Build with Addressables",
            "Do you want to build a clean addressables before export? This will clear any existing addressables files.",
            "Build with Addressables", 
            "Skip", 
            "Cancel");

        switch (option)
        {
            // Build with Addressables.
            case 0:
                BuildAddressablesProcessor.PreExport();
                break;

            // Skip.
            case 1:
                break;

            // Cancel.
            case 2:
                BuildAddressablesProcessor.cancelBuild = true;
                break;

            default:
                Debug.LogError("How did you get this.");
                break;
        }

        EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, "BuildAddressablesBeforeBuildPref");
        EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, "BuildAddressablesBeforeBuildPref", BuildAddressablesProcessor.showWarningBeforeBuild);
    }
}