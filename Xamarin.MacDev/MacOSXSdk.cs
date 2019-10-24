// 
// MacOSXSdk.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Xamarin.MacDev
{
	public class MacOSXSdk
	{
		List<MacOSXSdkVersion> knownOSVersions = new List<MacOSXSdkVersion> {
			MacOSXSdkVersion.V10_7,
			MacOSXSdkVersion.V10_8,
			MacOSXSdkVersion.V10_9,
			MacOSXSdkVersion.V10_10,
			MacOSXSdkVersion.V10_11,
			MacOSXSdkVersion.V10_12,
			MacOSXSdkVersion.V10_13,
            		MacOSXSdkVersion.V10_14
        	};

		static readonly Dictionary<string, DTSdkSettings> sdkSettingsCache = new Dictionary<string, DTSdkSettings> ();
		static DTSettings dtSettings;

		public string DeveloperRoot { get; private set; }
		public string VersionPlist { get; private set; }
		public string DesktopPlatform { get; private set; }
		public string SdkDeveloperRoot { get; private set; }
		
		const string PLATFORM_VERSION_PLIST = "version.plist";
		const string SYSTEM_VERSION_PLIST = "/System/Library/CoreServices/SystemVersion.plist";
		
		public MacOSXSdk (string developerRoot, string versionPlist)
		{
			var platformDir = Path.Combine (developerRoot, "Platforms", "MacOSX.platform");
			var sdkDir = Path.Combine (platformDir, "Developer", "SDKs");

			if (Directory.Exists (sdkDir))
				SdkDeveloperRoot = Path.Combine (platformDir, "Developer");
			else
				SdkDeveloperRoot = developerRoot;

			DesktopPlatform = platformDir;
			DeveloperRoot = developerRoot;
			VersionPlist = versionPlist;

			Init ();

			if (InstalledSdkVersions.Length > 0) {
				// Enumerable.Union is there to ensure we add the latest installed sdk to the list (even if it's not in knownOSVersions)
				knownOSVersions = knownOSVersions.Where (x => x < InstalledSdkVersions[0]).Union (InstalledSdkVersions).ToList ();
			}
		}
		
		void Init ()
		{
			IsInstalled = File.Exists (Path.Combine (DesktopPlatform, "Info.plist"));
			if (IsInstalled) {
				InstalledSdkVersions = EnumerateSdks (Path.Combine (SdkDeveloperRoot, "SDKs"), "MacOSX");
			} else {
				InstalledSdkVersions = new MacOSXSdkVersion[0];
			}
		}

		public bool IsInstalled { get; private set; }
		public MacOSXSdkVersion[] InstalledSdkVersions { get; private set; }

		public IList<MacOSXSdkVersion> KnownOSVersions { get { return knownOSVersions; } }
		
		static MacOSXSdkVersion[] EnumerateSdks (string sdkDir, string name)
		{
			if (!Directory.Exists (sdkDir))
				return new MacOSXSdkVersion[0];
			
			var sdks = new List<string> ();
			
			foreach (var dir in Directory.GetDirectories (sdkDir)) {
				string dirName = Path.GetFileName (dir);
				if (!dirName.StartsWith (name, StringComparison.Ordinal) || !dirName.EndsWith (".sdk", StringComparison.Ordinal))
					continue;
				
				if (!File.Exists (Path.Combine (dir, "SDKSettings.plist")))
					continue;
				
				int verLength = dirName.Length - (name.Length + ".sdk".Length);
				if (verLength == 0)
					continue;
				
				dirName = dirName.Substring (name.Length, verLength);
				sdks.Add (dirName);
			}
			
			var vs = new List<MacOSXSdkVersion> ();
			foreach (var s in sdks) {
				try {
					vs.Add (MacOSXSdkVersion.Parse (s));
				} catch (Exception ex) {
					LoggingService.LogError ("Could not parse {0} SDK version '{1}':\n{2}", name, s, ex);
				}
			}
			
			var versions = vs.ToArray ();
			Array.Sort (versions);
			
			return versions;
		}
		
		public string GetPlatformPath ()
		{
			return DesktopPlatform;
		}
		
		public string GetSdkPath (MacOSXSdkVersion version)
		{
			return GetSdkPath (version.ToString ());
		}
		
		public string GetSdkPath (string version)
		{
			return Path.Combine (SdkDeveloperRoot, "SDKs", "MacOSX" + version + ".sdk");
		}
		
		string GetSdkPlistFilename (string version)
		{
			return Path.Combine (GetSdkPath (version), "SDKSettings.plist");
		}
		
		public bool SdkIsInstalled (MacOSXSdkVersion version)
		{
			foreach (var v in InstalledSdkVersions) {
				if (v.Equals (version))
					return true;
			}
			
			return false;
		}
		
		public DTSdkSettings GetSdkSettings (MacOSXSdkVersion sdk)
		{
			Dictionary<string, DTSdkSettings> cache = sdkSettingsCache;
			
			DTSdkSettings settings;
			if (cache.TryGetValue (sdk.ToString (), out settings))
				return settings;
			
			try {
				settings = LoadSdkSettings (sdk);
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Error loading settings for SDK MacOSX {0}", sdk), ex);
			}
			
			cache[sdk.ToString ()] = settings;
			
			return settings;
		}
		
		DTSdkSettings LoadSdkSettings (MacOSXSdkVersion sdk)
		{
			string filename = GetSdkPlistFilename (sdk.ToString ());

			if (!File.Exists (filename))
				return null;

			var plist = PDictionary.FromFile (filename);
			var settings = new DTSdkSettings ();

			settings.CanonicalName = plist.GetString ("CanonicalName").Value;

			var props = plist.Get<PDictionary> ("DefaultProperties");

			PString gcc;
			if (!props.TryGetValue<PString> ("GCC_VERSION", out gcc))
				settings.DTCompiler = "com.apple.compilers.llvm.clang.1_0";
			else
				settings.DTCompiler = gcc.Value;

			settings.DTSDKBuild = GrabRootString (Path.Combine (DeveloperRoot, SYSTEM_VERSION_PLIST), "ProductBuildVersion");

			return settings;
		}
		
		public DTSettings GetDTSettings ()
		{
			if (dtSettings != null)
				return dtSettings;

			var dict = PDictionary.FromFile (Path.Combine (DesktopPlatform, "Info.plist"));
			var infos = dict.Get<PDictionary> ("AdditionalInfo");
			var systemVersionPlist = Path.Combine (DeveloperRoot, SYSTEM_VERSION_PLIST);

			return (dtSettings = new DTSettings {
				DTPlatformVersion = infos.Get<PString> ("DTPlatformVersion").Value,
				DTPlatformBuild = GrabRootString (Path.Combine (DesktopPlatform, "version.plist"), "ProductBuildVersion") ?? GrabRootString (VersionPlist, "ProductBuildVersion"),
				DTXcodeBuild = GrabRootString (VersionPlist, "ProductBuildVersion"),
				BuildMachineOSBuild = GrabRootString (systemVersionPlist, "ProductBuildVersion"),
			});
		}
		
		static string GrabRootString (string file, string key)
		{
			var dict = PDictionary.FromFile (file);
			PString value;

			if (dict.TryGetValue<PString> (key, out value))
				return value.Value;

			return null;
		}
			
		public MacOSXSdkVersion GetClosestInstalledSdk (MacOSXSdkVersion v)
		{
			// sorted low to high, so get first that's >= requested version
			foreach (var i in GetInstalledSdkVersions ()) {
				if (i.CompareTo (v) >= 0)
					return i;
			}
			return MacOSXSdkVersion.UseDefault;
		}
		
		public IList<MacOSXSdkVersion> GetInstalledSdkVersions ()
		{
			return InstalledSdkVersions;
		}
		
		public class DTSettings
		{
			public string DTXcodeBuild { get; set; }
			public string DTPlatformVersion { get; set; }
			public string DTPlatformBuild { get; set; }
			public string BuildMachineOSBuild { get; set; }
		}

		public class DTSdkSettings
		{
			public string CanonicalName { get; set; }
			public string AlternateSDK { get; set; }
			public string DTCompiler { get; set; }
			public string DTSDKBuild { get; set; }
		}
	}
}
