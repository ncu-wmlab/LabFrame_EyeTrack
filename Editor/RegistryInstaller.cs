#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class RegistryInstaller
{
    private const string ViveName = "VIVE";
    private const string ViveUrl = "https://npm-registry.vive.com/";
    private static readonly string[] ViveScopes = { "com.htc.upm" };

    private static readonly string[] WavePackages =
    {
        "com.htc.upm.wave.xrsdk@6.2.0-r9",
        "com.htc.upm.wave.native@6.2.0-r9",
        "com.htc.upm.wave.essence@6.2.0-r9"
    };

    private static readonly Queue<string> installQueue = new Queue<string>();
    private static AddRequest currentRequest;

        [MenuItem("LabFrame2023/Print EyeTrack Wave Status")]
        public static void PrintEyeTrackWaveStatus()
        {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defineString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        var hasUseViveAndroid = HasDefine(defineString, "USE_VIVE_ANDROID");

        var hasWaveDefine = false;
    #if LABFRAME_WAVE_ESSENCE
        hasWaveDefine = true;
    #endif

        var viveBranchCompiled = false;
    #if USE_VIVE_ANDROID && LABFRAME_WAVE_ESSENCE
        viveBranchCompiled = true;
    #endif

        var pkg = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.htc.upm.wave.essence");
        var pkgInfo = pkg == null ? "not found" : (pkg.name + "@" + pkg.version);

        Debug.Log("[EyeTrackDiag] BuildTargetGroup: " + buildTargetGroup);
        Debug.Log("[EyeTrackDiag] USE_VIVE_ANDROID: " + hasUseViveAndroid);
        Debug.Log("[EyeTrackDiag] LABFRAME_WAVE_ESSENCE: " + hasWaveDefine);
        Debug.Log("[EyeTrackDiag] Vive branch compiled: " + viveBranchCompiled);
        Debug.Log("[EyeTrackDiag] com.htc.upm.wave.essence: " + pkgInfo);
        }

    [MenuItem("LabFrame2023/Install Vive Wave Support")]
    public static void InstallWaveSupport()
    {
        if (!EnsureViveRegistry())
        {
            Debug.LogError("[LabFrame] Failed to update Packages/manifest.json for VIVE registry.");
            return;
        }

        installQueue.Clear();
        foreach (var pkg in WavePackages)
        {
            installQueue.Enqueue(pkg);
        }

        EditorApplication.update -= InstallNextPackage;
        EditorApplication.update += InstallNextPackage;
        Debug.Log("[LabFrame] Installing Wave packages...");
    }

    private static bool EnsureViveRegistry()
    {
        var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning("[LabFrame] Packages/manifest.json not found.");
            return false;
        }

        var text = File.ReadAllText(manifestPath, Encoding.UTF8);
        var json = JObject.Parse(text);

        if (json["scopedRegistries"] == null)
        {
            json["scopedRegistries"] = new JArray();
        }

        var registries = (JArray)json["scopedRegistries"];
        var registry = registries
            .OfType<JObject>()
            .FirstOrDefault(r =>
                string.Equals((string)r["name"], ViveName) ||
                string.Equals((string)r["url"], ViveUrl));

        if (registry == null)
        {
            registry = new JObject
            {
                ["name"] = ViveName,
                ["url"] = ViveUrl,
                ["scopes"] = new JArray(ViveScopes)
            };
            registries.Add(registry);
            File.WriteAllText(manifestPath, json.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[LabFrame] Added VIVE scoped registry.");
            return true;
        }

        var scopesArray = registry["scopes"] as JArray;
        if (scopesArray == null)
        {
            registry["scopes"] = new JArray(ViveScopes);
            File.WriteAllText(manifestPath, json.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[LabFrame] Updated VIVE scopes in scoped registry.");
            return true;
        }

        var existingScopes = new HashSet<string>(scopesArray.Values<string>());
        var changed = false;
        foreach (var scope in ViveScopes)
        {
            if (!existingScopes.Contains(scope))
            {
                scopesArray.Add(scope);
                changed = true;
            }
        }

        if (changed)
        {
            File.WriteAllText(manifestPath, json.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[LabFrame] Added missing VIVE scopes.");
        }

        return true;
    }

    private static void InstallNextPackage()
    {
        if (currentRequest != null)
        {
            if (!currentRequest.IsCompleted)
            {
                return;
            }

            if (currentRequest.Status == StatusCode.Failure)
            {
                Debug.LogError("[LabFrame] Wave install failed: " + currentRequest.Error.message);
                currentRequest = null;
                installQueue.Clear();
                EditorApplication.update -= InstallNextPackage;
                return;
            }

            Debug.Log("[LabFrame] Installed: " + currentRequest.Result.packageId);
            currentRequest = null;
        }

        if (installQueue.Count == 0)
        {
            EditorApplication.update -= InstallNextPackage;
            Debug.Log("[LabFrame] Wave installation complete.");
            return;
        }

        var nextPackage = installQueue.Dequeue();
        currentRequest = Client.Add(nextPackage);
    }

    private static bool HasDefine(string defineSymbols, string target)
    {
        if (string.IsNullOrEmpty(defineSymbols) || string.IsNullOrEmpty(target))
        {
            return false;
        }

        var symbols = defineSymbols.Split(';');
        foreach (var symbol in symbols)
        {
            if (symbol.Trim() == target)
            {
                return true;
            }
        }

        return false;
    }
}
#endif
