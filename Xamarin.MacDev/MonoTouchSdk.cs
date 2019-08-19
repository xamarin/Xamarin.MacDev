// 
// MonoTouchSdk.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc.
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
	public class MonoTouchSdk
	{
		static readonly IPhoneSdkVersion[] iOSSdkVersions = {
			IPhoneSdkVersion.V6_0,
			IPhoneSdkVersion.V6_1,
			IPhoneSdkVersion.V7_0,
			IPhoneSdkVersion.V7_1,
			IPhoneSdkVersion.V8_0,
			IPhoneSdkVersion.V8_1,
			IPhoneSdkVersion.V8_2,
			IPhoneSdkVersion.V8_3,
			IPhoneSdkVersion.V8_4,
			IPhoneSdkVersion.V9_0,
			IPhoneSdkVersion.V9_1,
			IPhoneSdkVersion.V9_2,
			IPhoneSdkVersion.V9_3,
			IPhoneSdkVersion.V10_0,
			IPhoneSdkVersion.V10_1
		};
		static readonly IPhoneSdkVersion[] watchOSSdkVersions = {
			IPhoneSdkVersion.V2_0,
			IPhoneSdkVersion.V2_1,
			IPhoneSdkVersion.V2_2,
			IPhoneSdkVersion.V3_0,
			IPhoneSdkVersion.V3_1
		};
		static readonly IPhoneSdkVersion[] tvOSSdkVersions = {
			IPhoneSdkVersion.V9_0,
			IPhoneSdkVersion.V9_1,
			IPhoneSdkVersion.V9_2,
			IPhoneSdkVersion.V10_0
		};

		public static readonly string[] DefaultLocations;

		// Note: We require Xamarin.iOS 10.4 because it is the first version to bundle mlaunch. Prior to that Xamarin Studio would ship it but that is no longer the case.
		// (We also need 10.4 for the Versions.plist, although we have a handy fallback if that file is missing)
		static IPhoneSdkVersion requiredXI = IPhoneSdkVersion.V10_4;

		DateTime lastMTExeWrite = DateTime.MinValue;
		PDictionary versions;
		bool hasUsrSubdir;

		static MonoTouchSdk ()
		{
			var locations = new List<string> ();
			var env = Environment.GetEnvironmentVariable ("MD_MTOUCH_SDK_ROOT");
			if (!string.IsNullOrEmpty (env))
				locations.Add (env);
			locations.Add ("/Library/Frameworks/Xamarin.iOS.framework/Versions/Current");
			locations.Add ("/Developer/MonoTouch");
			DefaultLocations = locations.ToArray ();
		}

		public MonoTouchSdk (string sdkDir)
		{
			SdkDir = sdkDir;
			Init ();
		}
		
		public string SdkDir { get; private set; }

		public string BinDir {
			get {
				return Path.Combine (SdkDir, hasUsrSubdir ? "usr/bin" : "bin");
			}
		}

		public string LibDir {
			get {
				return Path.Combine (SdkDir, hasUsrSubdir ? "usr/lib" : "lib");
			}
		}

		public string SdkNotInstalledReason { get; private set; }

		public event EventHandler Changed;

		static PArray CreateKnownSdkVersionsArray (IList<IPhoneSdkVersion> versions)
		{
			var array = new PArray ();

			for (int i = 0; i < versions.Count; i++)
				array.Add (new PString (versions[i].ToString ()));

			return array;
		}

		static PDictionary CreateKnownVersions ()
		{
			var knownVersions = new PDictionary ();

			knownVersions.Add ("iOS", CreateKnownSdkVersionsArray (iOSSdkVersions));
			knownVersions.Add ("tvOS", CreateKnownSdkVersionsArray (tvOSSdkVersions));
			knownVersions.Add ("watchOS", CreateKnownSdkVersionsArray (watchOSSdkVersions));

			return knownVersions;
		}

		static PDictionary CreateMinExtensionVersions ()
		{
			var minExtensionVersions = new PDictionary ();
			var watchos = new PDictionary ();
			var tvos = new PDictionary ();
			var ios = new PDictionary ();

			ios.Add ("com.apple.ui-services", new PString ("8.0"));
			ios.Add ("com.apple.services", new PString ("8.0"));
			ios.Add ("com.apple.keyboard-service", new PString ("8.0"));
			ios.Add ("com.apple.fileprovider-ui", new PString ("8.0"));
			ios.Add ("com.apple.fileprovider-nonui", new PString ("8.0"));
			ios.Add ("com.apple.photo-editing", new PString ("8.0"));
			ios.Add ("com.apple.share-services", new PString ("8.0"));
			ios.Add ("com.apple.widget-extension", new PString ("8.0"));
			ios.Add ("com.apple.watchkit", new PString ("8.0"));
			ios.Add ("com.apple.AudioUnit-UI", new PString ("8.0"));
			ios.Add ("com.apple.Safari.content-blocker", new PString ("9.0"));
			ios.Add ("com.apple.Safari.sharedlinks-service", new PString ("9.0"));
			ios.Add ("com.apple.spotlight.index", new PString ("9.0"));
			ios.Add ("com.apple.callkit.call-directory", new PString ("10.0"));
			ios.Add ("com.apple.intents-service", new PString ("10.0"));
			ios.Add ("com.apple.intents-ui-service", new PString ("10.0"));
			ios.Add ("com.apple.message-payload-provider", new PString ("10.0"));
			ios.Add ("com.apple.usernotifications.content-extension", new PString ("10.0"));
			ios.Add ("com.apple.usernotifications.service", new PString ("10.0"));

			tvos.Add ("com.apple.broadcast-services", new PString ("10.0"));
			tvos.Add ("com.apple.tv-services", new PString ("9.0"));

			minExtensionVersions.Add ("iOS", ios);
			minExtensionVersions.Add ("tvOS", tvos);
			minExtensionVersions.Add ("watchOS", watchos);

			return minExtensionVersions;
		}

		static PDictionary CreateDefaultVersionsPlist ()
		{
			var versions = new PDictionary ();

			versions.Add ("KnownVersions", CreateKnownVersions ());
			versions.Add ("RecommendedXcodeVersion", new PString ("8.0"));
			versions.Add ("MinExtensionVersion", CreateMinExtensionVersions ());

			return versions;
		}
		
		void Init ()
		{
			string currentLocation = IsInstalled ? Path.Combine (BinDir, "mtouch") : null;

			IsInstalled = false;
			versions = null;

			if (string.IsNullOrEmpty (SdkDir)) {
				foreach (var loc in DefaultLocations) {
					if (IsInstalled = ValidateSdkLocation (loc, out hasUsrSubdir)) {
						SdkDir = loc;
						break;
					}
				}
			} else {
				IsInstalled = ValidateSdkLocation (SdkDir, out hasUsrSubdir);
			}

			string mtouch = null;
			if (IsInstalled) {
				mtouch = Path.Combine (BinDir, "mtouch");
				lastMTExeWrite = File.GetLastWriteTimeUtc (mtouch);
				Version = ReadVersion ();

				if (Version.CompareTo (requiredXI) >= 0) {
					LoggingService.LogInfo ("Found Xamarin.iOS, version {0}.", Version);

					var path = Path.Combine (SdkDir, "Versions.plist");
					if (File.Exists (path)) {
						try {
							versions = PDictionary.FromFile (path);
						} catch {
							LoggingService.LogWarning ("Xamarin.iOS installation is corrupt: invalid Versions.plist at {0}.", path);
						}
					}

					if (versions == null)
						versions = CreateDefaultVersionsPlist ();
				} else {
					SdkNotInstalledReason = string.Format ("Found unsupported Xamarin.iOS, version {0}.\nYou need Xamarin.iOS {1} or above.", Version, requiredXI.ToString ());
					LoggingService.LogWarning (SdkNotInstalledReason);
					Version = new IPhoneSdkVersion ();
					versions = new PDictionary ();
					IsInstalled = false;
				}

				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.iOS.Version", Version.ToString ());
			} else {
				lastMTExeWrite = DateTime.MinValue;
				Version = new IPhoneSdkVersion ();
				versions = new PDictionary ();

				SdkNotInstalledReason = string.Format ("Xamarin.iOS not installed.\nCan't find mtouch or the Version file at {0}.", SdkDir);
				LoggingService.LogInfo (SdkNotInstalledReason);

				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.iOS.Version", string.Empty);
			}

			if (Changed != null && currentLocation != mtouch)
				Changed (this, EventArgs.Empty);
		}
		
		IPhoneSdkVersion ReadVersion ()
		{
			var versionFile = Path.Combine (SdkDir, "Version");

			if (File.Exists (versionFile)) {
				try {
					return IPhoneSdkVersion.Parse (File.ReadAllText (versionFile).Trim ());
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to read Xamarin.iOS version", ex);
				}
			}

			return new IPhoneSdkVersion ();
		}

		public static bool ValidateSdkLocation (string sdkDir)
		{
			bool hasUsrSubdir;

			return ValidateSdkLocation (sdkDir, out hasUsrSubdir);
		}

		public static bool ValidateSdkLocation (string sdkDir, out bool hasUsrSubdir)
		{
			hasUsrSubdir = false;

			if (!File.Exists (Path.Combine (sdkDir, "Version")))
				return false;

			if (File.Exists (Path.Combine (sdkDir, "bin", "mtouch")))
				return true;

			if (File.Exists (Path.Combine (sdkDir, "usr", "bin", "mtouch"))) {
				hasUsrSubdir = true;
				return true;
			}

			return false;
		}

		public bool IsInstalled { get; private set; }
		public IPhoneSdkVersion Version { get; private set; }

		ExtendedVersion extended_version;
		public ExtendedVersion ExtendedVersion {
			get {
				if (extended_version == null)
					extended_version = ExtendedVersion.Read (Path.Combine (SdkDir, "buildinfo"));
				return extended_version;
			}
		}

		static string GetPlatformKey (PlatformName platform)
		{
			switch (platform) {
			case PlatformName.iOS: return "iOS";
			case PlatformName.WatchOS: return "watchOS";
			case PlatformName.TvOS: return "tvOS";
			default: throw new ArgumentOutOfRangeException (nameof (platform));
			}
		}

		public Version RecommendedXcodeVersion {
			get {
				Version version;
				PString value;

				if (!versions.TryGetValue ("RecommendedXcodeVersion", out value) || !System.Version.TryParse (value.Value, out version))
					return new Version (8, 0);

				return version;
			}
		}

		public IList<IPhoneSdkVersion> GetKnownSdkVersions (PlatformName platform)
		{
			var list = new List<IPhoneSdkVersion> ();
			var key = GetPlatformKey (platform);
			PDictionary knownVersions;

			if (versions.TryGetValue ("KnownVersions", out knownVersions)) {
				PArray array;

				if (knownVersions.TryGetValue (key, out array)) {
					foreach (var knownVersion in array.OfType<PString> ()) {
						IPhoneSdkVersion version;

						if (IPhoneSdkVersion.TryParse (knownVersion.Value, out version))
							list.Add (version);
					}
				}
			}

			return list;
		}

		public IPhoneSdkVersion GetMinimumExtensionVersion (PlatformName platform, string extension)
		{
			var key = GetPlatformKey (platform);
			PDictionary minExtensionVersions;

			if (versions.TryGetValue ("MinExtensionVersion", out minExtensionVersions)) {
				PDictionary extensions;

				if (minExtensionVersions.TryGetValue (key, out extensions)) {
					PString value;

					if (extensions.TryGetValue (extension, out value)) {
						IPhoneSdkVersion version;

						if (IPhoneSdkVersion.TryParse (value.Value, out version))
							return version;
					}
				}
			}

			return IPhoneSdkVersion.V8_0;
		}
		
		public void CheckCaches ()
		{
			if (IsInstalled) {
				try {
					var lastWrite = File.GetLastWriteTimeUtc (Path.Combine (BinDir, "mtouch"));
					if (lastWrite == lastMTExeWrite)
						return;
				} catch (IOException) {
				}
			}
			
			Init ();
		}

		public bool SupportsFeature (string feature)
		{
			PArray features;

			if (!versions.TryGetValue ("Features", out features))
				return false;

			foreach (var item in features.OfType<PString> ().Select (x => x.Value)) {
				if (feature == item)
					return true;
			}

			return false;
		}

		public bool SupportsSGenConcurrentGC {
			get { return SupportsFeature ("sgen-concurrent-gc"); }
		}

		public bool SupportsLaunchDeviceBundleId {
			get { return SupportsFeature ("mlaunch-launchdevbundleid"); }
		}

		public bool SupportsObserveExtension {
			get { return SupportsFeature ("mlaunch-observe-extension"); }
		}

		public bool SupportsLaunchSimulator {
			get { return SupportsFeature ("mlaunch-launch-simulator"); }
		}

		public bool SupportsInstallProgress {
			get { return SupportsFeature ("mlaunch-install-progress"); }
		}

		public bool SupportsLaunchWatchOSComplications {
			get { return SupportsFeature ("mlaunch-watchos-complications"); }
		}

		public bool SupportsWirelessDevices {
			get { return SupportsFeature ("mlaunch-wireless-devices"); }
		}

		public bool SupportsSiriIntents {
			get { return SupportsFeature ("siri-intents"); }
		}

		public bool SupportsArm64_32 {
			get { return SupportsFeature ("arm64_32"); }
		}

		public bool SupportsAltool {
			get { return SupportsFeature ("altool"); }
		}
	}
}
