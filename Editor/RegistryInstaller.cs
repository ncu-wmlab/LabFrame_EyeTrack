#if UNITY_EDITOR && USE_VIVE_ANDROID
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Newtonsoft.Json.Linq; // 建議在你的 Editor 目錄自帶一個簡單 JSON 解析（或改用 MiniJSON）

[InitializeOnLoad]
public static class RegistryInstaller
{
    const string ViveName = "VIVE";
    const string ViveUrl  = "https://npm-registry.vive.com/";
    static readonly string[] ViveScopes = { "com.htc.upm" };
    const string EssencePkg = "com.htc.upm.wave.essence";
    const string EssenceVer = "6.2.0"; // 與你的 package.json 一致

    static RegistryInstaller()
    {
        // 延遲一點，避免阻塞第一次導入
        EditorApplication.delayCall += EnsureViveRegistryAndEssence;
    }

    [MenuItem("LabFrame2023/Install Wave XR Support")]
    public static void EnsureViveRegistryAndEssence()
    {
        var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning("[LabFrame] manifest.json not found.");
            return;
        }

        var text = File.ReadAllText(manifestPath, Encoding.UTF8);
        var json = JObject.Parse(text);

        // 確保 scopedRegistries 存在
        if (json["scopedRegistries"] == null)
            json["scopedRegistries"] = new JArray();

        var regs = (JArray)json["scopedRegistries"];
        bool hasVive = regs.Any(r =>
            (string)r["name"] == ViveName ||
            (string)r["url"] == ViveUrl
        );

        if (!hasVive)
        {
            var reg = new JObject
            {
                ["name"] = ViveName,
                ["url"] = ViveUrl,
                ["scopes"] = new JArray(ViveScopes)
            };
            regs.Add(reg);
            File.WriteAllText(manifestPath, json.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[LabFrame] Added VIVE scoped registry to Packages/manifest.json");
        }

        Client.Add($"{EssencePkg}@{EssenceVer}");
    }
}
#endif
