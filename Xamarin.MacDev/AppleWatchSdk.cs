// 
// AppleIPhoneSdk.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2011-2013 Xamarin Inc.
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

using System.Collections.Generic;

namespace Xamarin.MacDev
{
	public class AppleWatchSdk : AppleSdk
	{
		static List<IPhoneSdkVersion> knownOSVersions = new List<IPhoneSdkVersion> {
			IPhoneSdkVersion.V1_0,
			IPhoneSdkVersion.V2_0,
			IPhoneSdkVersion.V2_1,
			IPhoneSdkVersion.V2_2,
			IPhoneSdkVersion.V3_0,
			IPhoneSdkVersion.V3_1,
		};

		protected override string SimulatorPlatformName {
			get {
				return "WatchSimulator";
			}
		}

		protected override string DevicePlatformName {
			get {
				return "WatchOS";
			}
		}

		protected override List<IPhoneSdkVersion> InitiallyKnownOSVersions {
			get {
				return knownOSVersions;
			}
		}

		public AppleWatchSdk (string sdkRoot, string versionPlist)
		{
			DeveloperRoot = sdkRoot;
			VersionPlist = versionPlist;
			Init ();
		}

//		public IEnumerable<WatchSimulatorTarget> GetSimulatorTargets (MonoTouchSdk monotouch, IPhoneSdkVersion minVersion,
//			IPhoneDeviceType supportedDevices, WatchSimulatorFeatures requiredFeatures = 0)
//		{
//			return GetSimulatorTargets (monotouch).Where (t => t.Supports (minVersion, supportedDevices, requiredFeatures));
//		}
//
//		readonly List<WatchSimulatorTarget> cachedSimulatorTargets = new List<WatchSimulatorTarget> ();
//		DateTime cachedDateTime = DateTime.MinValue;
//
//		bool QuerySimulators {
//			get {
//				var sdks = Path.Combine (AppleSdkSettings.DeveloperRoot, "Platforms", "WatchSimulator.platform", "Developer", "SDKs");
//
//				// Note: When Xcode downloads/installs a new iPhoneSimulator, it will be added in the SDKs directory which will bump the mtime.
//				if (Directory.Exists (sdks)) {
//					var mtime = Directory.GetLastWriteTime (sdks);
//
//					if (mtime > cachedDateTime) {
//						cachedDateTime = mtime;
//						return true;
//					}
//
//					return false;
//				}
//
//				return cachedSimulatorTargets.Count == 0;
//			}
//		}
//
//		public IEnumerable<WatchSimulatorTarget> GetSimulatorTargets (MonoTouchSdk monotouch)
//		{
//			if (monotouch.SupportsListingSimulators) {
//				if (QuerySimulators) {
//					var builder = new ProcessArgumentBuilder ();
//					var tmp = Path.GetTempFileName ();
//
//					builder.Add ("--sdkroot");
//					builder.AddQuoted (AppleSdkSettings.DeveloperRoot);
//					builder.Add ("--listsim");
//					builder.AddQuoted (tmp);
//
//					var startInfo = new ProcessStartInfo (Path.Combine (monotouch.BinDir, "mtouch"), builder.ToString ());
//					startInfo.RedirectStandardOutput = false;
//					startInfo.UseShellExecute = false;
//
//					cachedSimulatorTargets.Clear ();
//
//					try {
//						var process = new Process ();
//						process.StartInfo = startInfo;
//						process.Start ();
//						process.WaitForExit ();
//
//						using (var stream = File.OpenText (tmp)) {
//							var doc = XDocument.Load (stream);
//
//							var node = doc.Root.Element ("Simulator");
//							if (node != null) {
//								var available = node.Element ("AvailableDevices");
//
//								if (available != null) {
//									foreach (var device in available.Elements ("SimDevice")) {
//										var runtime = (string) device.Element ("SimRuntime");
//										if (runtime == null)
//											continue;
//
//										var type = (string) device.Element ("SimDeviceType");
//										if (type == null)
//											continue;
//
//										string name = (string) device.Attribute ("Name");
//										if (name == null)
//											continue;
//
//										try {
//											var sim = new WatchSimulatorTarget (name, runtime, type);
//											sim.Udid = (string) device.Attribute ("UDID");
//											sim.DataPath = (string) device.Element ("DataPath");
//
//											cachedSimulatorTargets.Add (sim);
//										} catch (FormatException ex) {
//											LoggingService.LogError ("Failed to add iOS Simulator.", ex);
//										}
//									}
//								}
//							}
//						}
//					} catch (Exception ex) {
//						LoggingService.LogError ("Failed to query list of simulators.", ex);
//						cachedDateTime = DateTime.MinValue;
//						cachedSimulatorTargets.Clear ();
//					}
//
//					if (File.Exists (tmp))
//						File.Delete (tmp);
//				}
//
//				if (cachedSimulatorTargets.Count > 0) {
//					foreach (var target in cachedSimulatorTargets)
//						yield return target;
//
//					yield break;
//				}
//
//				// else: fall back to the old way of enumerating a list of simulator targets...
//			}
//
//			foreach (var v in GetInstalledSdkVersions (true)) {
//				var settings = GetSdkSettings (v, true);
//
//				if (settings != null) {
//					if (settings.DeviceFamilies.HasFlag (IPhoneDeviceType.Watch)) {
//						yield return new WatchSimulatorTarget (WatchSimulatorType.AppleWatch38mm, v);
//						yield return new WatchSimulatorTarget (WatchSimulatorType.AppleWatch42mm, v);
//					}
//				}
//			}
//		}
	}
}
