//
// MacCatalystSupport.cs
//
// Author: Rolf Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2021 Microsoft Corp.
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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.MacDev {
	public static class MacCatalystSupport {
		// Versions for Mac Catalyst are complicated. In some cases we have to use the
		// corresponding iOS version of the SDK, and in some cases we have to use the
		// macOS version that iOS version correspond to. In Xcode, when you select the
		// deployment target, you select a macOS version in the UI, but the corresponding
		// iOS version is written to the project file. This means that there's a mapping
		// between the two, and it turns out Apple stores it in the SDKSettings.plist in
		// the macOS SDK. Here we provide a method that loads Apple's mapping from the
		// SDKSettings.plist.
		public static void LoadVersionMaps (string sdkDirectory, out Dictionary<string, string> map_ios_to_macos, out Dictionary<string, string> map_macos_to_ios)
		{
			map_ios_to_macos = new Dictionary<string, string> ();
			map_macos_to_ios = new Dictionary<string, string> ();

			var fn = Path.Combine (sdkDirectory, "SDKSettings.plist");
			var plist = PDictionary.FromFile (fn);
#if NET
			if (plist?.TryGetValue ("VersionMap", out PDictionary? versionMap) == true) {
#else
			// Earlier versions of the C# compiler aren't able to understand the syntax above, and complains about "Use of unassigned local variable 'versionMap'", so do something else here.
			if (plist!.TryGetValue ("VersionMap", out PDictionary? versionMap)) {
#endif
				if (versionMap.TryGetValue ("iOSMac_macOS", out PDictionary? versionMapiOSToMac)) {
					foreach (var kvp in versionMapiOSToMac)
						map_ios_to_macos [kvp.Key!] = ((PString) kvp.Value).Value;
				}
				if (versionMap.TryGetValue ("macOS_iOSMac", out PDictionary? versionMapMacToiOS)) {
					foreach (var kvp in versionMapMacToiOS)
						map_macos_to_ios [kvp.Key!] = ((PString) kvp.Value).Value;
				}
			}
		}

		public static bool TryGetMacOSVersion (string sdkDirectory, Version iOSVersion, [NotNullWhen (true)] out Version? macOSVersion, out ICollection<string> knowniOSVersions)
		{
			macOSVersion = null;

			if (!TryGetMacOSVersion (sdkDirectory, iOSVersion.ToString (), out var strValue, out knowniOSVersions))
				return false;

			return Version.TryParse (strValue, out macOSVersion);
		}

		public static bool TryGetMacOSVersion (string sdkDirectory, string iOSVersion, [NotNullWhen (true)] out string? macOSVersion, out ICollection<string> knowniOSVersions)
		{
			LoadVersionMaps (sdkDirectory, out var map, out var _);

			knowniOSVersions = map.Keys;

			return map.TryGetValue (iOSVersion.ToString (), out macOSVersion);
		}

		public static bool TryGetiOSVersion (string sdkDirectory, Version macOSVersion, [NotNullWhen (true)] out Version? iOSVersion, out ICollection<string> knownMacOSVersions)
		{
			iOSVersion = null;

			if (!TryGetiOSVersion (sdkDirectory, macOSVersion.ToString (), out var strValue, out knownMacOSVersions))
				return false;

			return Version.TryParse (strValue, out iOSVersion);
		}

		public static bool TryGetiOSVersion (string sdkDirectory, string macOSVersion, [NotNullWhen (true)] out string? iOSVersion, out ICollection<string> knownMacOSVersions)
		{
			LoadVersionMaps (sdkDirectory, out var _, out var map);

			knownMacOSVersions = map.Keys;

			return map.TryGetValue (macOSVersion.ToString (), out iOSVersion);
		}
	}
}

#if !NET
namespace System.Diagnostics.CodeAnalysis {
	// from: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs
	[AttributeUsage (AttributeTargets.Parameter, Inherited = false)]
	internal sealed class NotNullWhenAttribute : Attribute {
		public NotNullWhenAttribute (bool returnValue) => ReturnValue = returnValue;
		public bool ReturnValue { get; }
	}
}
#endif // !NET
