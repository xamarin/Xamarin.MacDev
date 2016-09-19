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
using System.Collections.Generic;

namespace Xamarin.MacDev
{
	public class MonoTouchSdk
	{
		public static readonly string[] DefaultLocations;

		DateTime lastMTExeWrite = DateTime.MinValue;
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

		public event EventHandler Changed;
		
		void Init ()
		{
			string currentLocation = IsInstalled ? Path.Combine (BinDir, "mtouch") : null;

			IsInstalled = false;
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
				lastMTExeWrite = File.GetLastWriteTime (mtouch);
				Version = ReadVersion ();

				if (Version.CompareTo (IPhoneSdkVersion.V6_2) >= 0) {
					LoggingService.LogInfo ("Found Xamarin.iOS, version {0}.", Version);
				} else {
					LoggingService.LogInfo ("Found unsupported Xamarin.iOS, version {0}.", Version);
					Version = new IPhoneSdkVersion ();
					IsInstalled = false;
				}

				AnalyticsService.ReportContextProperty ("XS.Core.SDK.iOS.Version", Version.ToString ());
			} else {
				lastMTExeWrite = DateTime.MinValue;
				Version = new IPhoneSdkVersion ();
				LoggingService.LogInfo ("Xamarin.iOS not installed. Can't find mtouch.");

				AnalyticsService.ReportContextProperty ("XS.Core.SDK.iOS.Version", string.Empty);
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
		
		public void CheckCaches ()
		{
			if (IsInstalled) {
				try {
					var lastWrite = File.GetLastWriteTime (Path.Combine (BinDir, "mtouch"));
					if (lastWrite == lastMTExeWrite)
						return;
				} catch (IOException) {
				}
			}
			
			Init ();
		}

		public bool SupportsFastDev {
			get { return Version >= new IPhoneSdkVersion (6, 3, 7); }
		}

		public bool ExperimentalSgen {
			get { return Version < new IPhoneSdkVersion (7, 2, 1); }
		}

		public bool SupportsRefCountingWithBoehm {
			get { return Version >= new IPhoneSdkVersion (7, 2, 1); }
		}

		public bool RefCountingDeprecated {
			get { return Version >= new IPhoneSdkVersion (9, 3, 0); }
		}

		public bool SupportsGenericValueTypeSharing {
			get { return Version < new IPhoneSdkVersion (8, 9, 0); }
		}

		public bool SupportsLicenseSync {
			get { return Version >= new IPhoneSdkVersion (6, 4, 3); }
		}

		public bool SupportsBackgroundFetch {
			get { return Version >= new IPhoneSdkVersion (7, 1); }
		}

		public bool SupportsXamarinIOS {
			get { return File.Exists (Path.Combine (LibDir, "64bits", "Xamarin.iOS.dll")); }
		}

		public bool SupportsExecutableNameOverride {
			get { return Version >= new IPhoneSdkVersion (7, 9, 1); }
		}

		public bool SupportsMSBuild {
			get { return Version >= new IPhoneSdkVersion (7, 4, 0); }
		}

		public bool SupportsListingSimulators {
			get { return AppleSdkSettings.XcodeVersion.Major >= 6 && Version >= new IPhoneSdkVersion (8, 0, 0); }
		}

		public bool SupportsWatchApps {
			get { return Version >= new IPhoneSdkVersion (8, 7, 0); }
		}

		public bool SupportsNoSymbolStrip {
			get { return Version >= new IPhoneSdkVersion (8, 11, 0); }
		}

		public bool SupportsDeviceSpecificBuilds {
			get { return Version >= new IPhoneSdkVersion (8, 11, 0); }
		}

		public bool DefaultsTo32BitFloat {
			get { return Version >= new IPhoneSdkVersion (8, 11, 0); }
		}

		public bool SupportsWatchOS2 {
			get { return Version >= new IPhoneSdkVersion (9, 0); }
		}

		public bool SupportsProvideAssets {
			get { return Version >= new IPhoneSdkVersion (9, 3, 0, 267); }
		}
		
		public bool SupportsModernHttpClient {
			get { return Version >= new IPhoneSdkVersion (9, 5, 0); }
		}

		public bool SupportsTlsProvider {
			get { return Version >= new IPhoneSdkVersion (9, 5, 0); }
		}

		public bool LaunchUsingAppBundle {
			get { return Version >= new IPhoneSdkVersion (9, 5, 0); }
		}

		public bool SupportsMonoSymbolArchive {
			get { return Version >= new IPhoneSdkVersion (9, 10, 0); }
		}
	}
}
