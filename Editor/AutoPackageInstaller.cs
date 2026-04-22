using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Practice.Package.Editor
{
	[InitializeOnLoad]
	public class AutoPackageInstaller
	{
		static AutoPackageInstaller()
		{
			InstallMissingPackages();
			EditorApplication.delayCall += EnsureVersionDefines;
		}

		private static void InstallMissingPackages()
		{
			PackageInstallerSettings aSettings = GetSettings();
			if (aSettings == null || aSettings.PackagesToInstall == null) return;

			ListRequest aRequest = Client.List(true);
			EditorApplication.update += Progress;

			void Progress()
			{
				if (aRequest.IsCompleted)
				{
					EditorApplication.update -= Progress;
					if (aRequest.Status == StatusCode.Success)
					{
						var aInstalledPackageIds = aRequest.Result.Select(p => p.name).ToList();
						foreach (var aRequirement in aSettings.PackagesToInstall)
						{
							bool aIsInstalled = aInstalledPackageIds.Contains(aRequirement.PackageId);
							if (!aIsInstalled)
							{
								Debug.Log($"[PracticePackage] Missing package [{aRequirement.PackageId}] detected, installing: {aRequirement.InstallSource}");
								Client.Add(aRequirement.InstallSource);
							}
						}
					}
				}
			}
		}

		private static void EnsureVersionDefines()
		{
			PackageInstallerSettings aSettings = GetSettings();
			if (aSettings == null || aSettings.VersionDefines == null) return;

			string[] aGuids = AssetDatabase.FindAssets($"{aSettings.TargetAsmdefName} t:AssemblyDefinitionAsset");
			if (aGuids.Length == 0) return;

			string aPath = AssetDatabase.GUIDToAssetPath(aGuids[0]);
			string aFullPath = Path.GetFullPath(aPath);
			string aContent = File.ReadAllText(aFullPath);

			bool aIsChanged = false;

			foreach (var aDefine in aSettings.VersionDefines)
			{
				if (!aContent.Contains(aDefine.Name))
				{
					aContent = InjectDefine(aContent, aDefine.Name, aDefine.Expression, aDefine.Symbol);
					aIsChanged = true;
				}
			}

			if (aIsChanged)
			{
				File.WriteAllText(aFullPath, aContent);
				AssetDatabase.ImportAsset(aPath);
				Debug.Log($"<color=#00FF00>[PracticePackage]</color> Automatically updated {aSettings.TargetAsmdefName}.asmdef with all required symbols.");
			}
		}

		private static string InjectDefine(string iJson, string iName, string iExpression, string iDefine)
		{
			string aEntry = $"{{\"name\": \"{iName}\", \"expression\": \"{iExpression}\", \"define\": \"{iDefine}\"}}";
			if (iJson.Contains("\"versionDefines\": []"))
				return iJson.Replace("\"versionDefines\": []", $"\"versionDefines\": [\n        {aEntry}\n    ]");
			else if (iJson.Contains("\"versionDefines\": ["))
				return iJson.Replace("\"versionDefines\": [", $"\"versionDefines\": [\n        {aEntry},");
			return iJson;
		}

		private static PackageInstallerSettings GetSettings()
		{
			string[] aGuids = AssetDatabase.FindAssets("t:PackageInstallerSettings");
			if (aGuids.Length == 0) return null;
			string aPath = AssetDatabase.GUIDToAssetPath(aGuids[0]);
			return AssetDatabase.LoadAssetAtPath<PackageInstallerSettings>(aPath);
		}
	}
}