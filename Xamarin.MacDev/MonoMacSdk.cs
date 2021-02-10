// 
// MonoMacSdk.cs
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

namespace Xamarin.MacDev
{
	public sealed class MonoMacSdk : IMonoMacSdk
	{
		static DateTime lastExeWrite = DateTime.MinValue;

		public string SdkDir {
			get; private set;
		}

		public string LegacyFrameworkAssembly {
			get; private set;
		}

		public string LegacyAppLauncherPath {
			get; private set;
		}

		public MonoMacSdk (string defaultLocation, string legacyFrameworkAssembly, string legacyAppLauncherPath)
		{
			LegacyFrameworkAssembly = legacyFrameworkAssembly;
			LegacyAppLauncherPath = legacyAppLauncherPath;
			SdkDir = defaultLocation;
			Init ();
		}

		void Init ()
		{
			IsInstalled = File.Exists (LegacyFrameworkAssembly);
			
			if (IsInstalled) {
				lastExeWrite = File.GetLastWriteTimeUtc (LegacyFrameworkAssembly);
				Version = ReadVersion ();
				if (Version.IsUseDefault)
					LoggingService.LogInfo ("Found MonoMac.");
				else
					LoggingService.LogInfo ("Found MonoMac, version {0}.", Version);
			} else {
				lastExeWrite = DateTime.MinValue;
				Version = new AppleSdkVersion ();
				LoggingService.LogInfo ("MonoMac not installed. Can't find {0}.", LegacyFrameworkAssembly);
			}
		}
		
		AppleSdkVersion ReadVersion ()
		{
			var versionFile = Path.Combine (SdkDir, "Version");
			if (File.Exists (versionFile)) {
				try {
					return AppleSdkVersion.Parse (File.ReadAllText (versionFile).Trim ());
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to read MonoMac version", ex);
				}
			}
			
			return new AppleSdkVersion ();
		}
		
		public bool IsInstalled { get; private set; }
		public AppleSdkVersion Version { get; private set; }
		
		public void CheckCaches ()
		{
			DateTime lastWrite = DateTime.MinValue;
			try {
				lastWrite = File.GetLastWriteTimeUtc (LegacyFrameworkAssembly);
				if (lastWrite == lastExeWrite)
					return;
			} catch (IOException) {
			}
			
			Init ();
		}
	}
}
