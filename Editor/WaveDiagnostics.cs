#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class WaveDiagnostics
{
    private const string MenuPath = "LabFrame2023/Diagnostics/Print EyeTrack Wave Status";
    private const string WaveEssenceName = "com.htc.upm.wave.essence";
    private static ListRequest packageListRequest;

    [MenuItem(MenuPath)]
    public static void PrintEyeTrackWaveStatus()
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defineString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        var hasUseViveAndroid = HasDefine(defineString, "USE_VIVE_ANDROID");

        var activeWaveDefine = false;
#if LABFRAME_WAVE_ESSENCE
        activeWaveDefine = true;
#endif

        var activeViveWaveCompilePath = false;
#if USE_VIVE_ANDROID && LABFRAME_WAVE_ESSENCE
        activeViveWaveCompilePath = true;
#endif

        Debug.Log("[EyeTrackDiag] ------------------------------");
        Debug.Log("[EyeTrackDiag] BuildTargetGroup: " + buildTargetGroup);
        Debug.Log("[EyeTrackDiag] USE_VIVE_ANDROID (PlayerSettings): " + hasUseViveAndroid);
        Debug.Log("[EyeTrackDiag] LABFRAME_WAVE_ESSENCE (asmdef define): " + activeWaveDefine);
        Debug.Log("[EyeTrackDiag] Vive branch compiled: " + activeViveWaveCompilePath);

        var directPackage = PackageInfo.FindForAssetPath("Packages/" + WaveEssenceName);
        if (directPackage != null)
        {
            Debug.Log("[EyeTrackDiag] Wave package found via asset path: " + directPackage.name + "@" + directPackage.version);
            Debug.Log("[EyeTrackDiag] ------------------------------");
            return;
        }

        packageListRequest = Client.List(true);
        EditorApplication.update -= HandleListResult;
        EditorApplication.update += HandleListResult;
    }

    private static bool HasDefine(string defineSymbols, string target)
    {
        if (string.IsNullOrEmpty(defineSymbols) || string.IsNullOrEmpty(target))
        {
            return false;
        }

        return defineSymbols
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Any(x => x.Equals(target, StringComparison.Ordinal));
    }

    private static void HandleListResult()
    {
        if (packageListRequest == null || !packageListRequest.IsCompleted)
        {
            return;
        }

        EditorApplication.update -= HandleListResult;

        if (packageListRequest.Status == StatusCode.Failure)
        {
            Debug.LogWarning("[EyeTrackDiag] Cannot query package list: " + packageListRequest.Error.message);
            Debug.Log("[EyeTrackDiag] ------------------------------");
            packageListRequest = null;
            return;
        }

        var pkg = packageListRequest.Result.FirstOrDefault(p => p.name == WaveEssenceName);
        if (pkg == null)
        {
            Debug.LogWarning("[EyeTrackDiag] Wave package not found in Package Manager list.");
        }
        else
        {
            Debug.Log("[EyeTrackDiag] Wave package found via Package Manager list: " + pkg.name + "@" + pkg.version);
        }

        Debug.Log("[EyeTrackDiag] ------------------------------");
        packageListRequest = null;
    }
}
#endif