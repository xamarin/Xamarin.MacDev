//
// XamMacSdk.cs
//
// Authors:
//   Aaron Bockover <abock@xamarin.com>
//   Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.MacDev
{
	public sealed class XamMacSdk : IMonoMacSdk
	{
		readonly Dictionary<string, DateTime> lastWriteTimes = new Dictionary<string, DateTime> ();

		string monoMacAppLauncherPath;

		public bool IsInstalled { get; private set; }
		public bool SupportsMSBuild { get; private set; }
		public bool SupportsV2Features { get; private set; }
		public bool SupportsNewStyleActivation { get; private set; }
		public bool SupportsRefCounting { get; private set; }
		public bool SupportsModernHttpClient { get; private set; }
		public bool SupportsTlsProvider { get; private set; }
		public bool RefCountingDeprecated { get; private set; }
		public bool SupportsFullProfile { get; private set; }
		public bool SupportsMonoSymbolArchive { get; private set; }
		public MacOSXSdkVersion Version { get; private set; }

		public string FrameworkDirectory { get; private set; }
		public string MmpPath { get; private set; }
		public string BmacPath { get; private set; }
		public string UnifiedFullProfileFrameworkAssembly { get; private set; }

		#region Compat paths

		public string LegacyFrameworkAssembly { get; private set; }
		public string LegacyAppLauncherPath { get; private set; }

		#endregion

		public XamMacSdk (string monoMacAppLauncherPath)
		{
			this.monoMacAppLauncherPath = monoMacAppLauncherPath;
			Init ();
		}

		public event EventHandler Changed;

		void Init ()
		{
			string currentLocation = IsInstalled ? MmpPath : null;

			IsInstalled = false;

			FrameworkDirectory = "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current";
			var envFrameworkDir = Environment.GetEnvironmentVariable ("XAMMAC_FRAMEWORK_PATH");
			if (envFrameworkDir != null && Directory.Exists (envFrameworkDir))
				FrameworkDirectory = envFrameworkDir;

			var versionPath = Path.Combine (FrameworkDirectory, "Version");
			if (File.Exists (versionPath)) {
				Version = ReadVersion (versionPath);
				lastWriteTimes [versionPath] = File.GetLastWriteTimeUtc (versionPath);
			} else {
				NotInstalled (versionPath, error: false);
				AnalyticsService.IdentifyTrait ("Xamarin.Mac", string.Empty);
				return;
			}

			var paths = Version >= new MacOSXSdkVersion (1, 9, 0)
				? Detect2x ()
				: Detect1x ();

			foreach (var path in paths) {
				if (!File.Exists (path)) {
					NotInstalled (path);
					return;
				}

				lastWriteTimes [path] = File.GetLastWriteTimeUtc (path);
			}

			IsInstalled = true;
			LoggingService.LogInfo ("Found Xamarin.Mac, version {0}.", Version);
			AnalyticsService.IdentifyTrait ("Xamarin.Mac", Version.ToString ());

			if (Changed != null && currentLocation != MmpPath)
				Changed (this, EventArgs.Empty);
		}

		IEnumerable<string> Detect2x ()
		{
			SupportsMSBuild = true;
			SupportsV2Features = true;

			SupportsNewStyleActivation = Version >= new MacOSXSdkVersion (1, 9, 0);
			SupportsRefCounting = Version >= new MacOSXSdkVersion (1, 11, 0);
			RefCountingDeprecated = Version >= new MacOSXSdkVersion (2, 5, 0);
			SupportsModernHttpClient = Version >= new MacOSXSdkVersion (2, 5, 0);
			SupportsTlsProvider = Version >= new MacOSXSdkVersion (2, 5, 0);
			SupportsMonoSymbolArchive = Version >= new MacOSXSdkVersion (2, 10, 0);

			yield return MmpPath = Path.Combine (FrameworkDirectory, "bin", "mmp");
			yield return BmacPath = Path.Combine (FrameworkDirectory, "bin", "bmac");
			yield return LegacyFrameworkAssembly = Path.Combine (FrameworkDirectory, "lib", "mono", "XamMac.dll");
			yield return LegacyAppLauncherPath = Path.Combine (FrameworkDirectory, "lib", "XamMacLauncher");
			yield return UnifiedFullProfileFrameworkAssembly = Path.Combine (FrameworkDirectory, "lib", "reference", "full", "Xamarin.Mac.dll");

			SupportsFullProfile =
				File.Exists (UnifiedFullProfileFrameworkAssembly) &&
				Version >= new MacOSXSdkVersion (1, 11, 2, 3);
		}

		IEnumerable<string> Detect1x ()
		{
			SupportsMSBuild = false;
			SupportsV2Features = false;

			var usrPrefix = string.Empty;
			var appDriverPath = monoMacAppLauncherPath;

			// 1.2.13 began bundling its own launcher
			if (Version >= new MacOSXSdkVersion (1, 2, 13))
				appDriverPath = Path.Combine (FrameworkDirectory, "lib", "mono", "XamMacLauncher");

			if (Version < new MacOSXSdkVersion (1, 4, 0))
				usrPrefix = "usr";

			yield return MmpPath = Path.Combine (FrameworkDirectory, usrPrefix, "bin", "mmp");
			yield return BmacPath = Path.Combine (FrameworkDirectory, usrPrefix, "bin", "bmac");
			yield return LegacyFrameworkAssembly = Path.Combine (FrameworkDirectory, usrPrefix, "lib", "mono", "XamMac.dll");
			LegacyAppLauncherPath = appDriverPath;

			if (LegacyAppLauncherPath != null)
				yield return LegacyAppLauncherPath;

			yield break;
		}

		void NotInstalled (string pathMissing, bool error = true)
		{
			lastWriteTimes.Clear ();
			IsInstalled = false;
			Version = new MacOSXSdkVersion ();

			if (error)
				LoggingService.LogError ("Xamarin.Mac not installed. Can't find {0}.", pathMissing);
			else
				LoggingService.LogInfo ("Xamarin.Mac not installed. Can't find {0}.", pathMissing);
		}

		MacOSXSdkVersion ReadVersion (string versionPath)
		{
			try {
				return MacOSXSdkVersion.Parse (File.ReadAllText (versionPath).Trim ());
			} catch (Exception ex) {
				LoggingService.LogError ("Failed to read Xamarin.Mac version", ex);
				return new MacOSXSdkVersion ();
			}
		}

		public void CheckCaches ()
		{
			try {
				foreach (var path in lastWriteTimes) {
					if (File.GetLastWriteTimeUtc (path.Key) != path.Value) {
						Init ();
						return;
					}
				}
			} catch (IOException) {
				Init ();
			}
		}
	}
}
