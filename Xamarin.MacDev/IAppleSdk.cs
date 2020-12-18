using System;
using System.Collections.Generic;

namespace Xamarin.MacDev {
	public interface IAppleSdk {
		bool IsInstalled { get; }
		string DeveloperRoot { get; }
		string GetPlatformPath (bool isSimulator);
		string GetSdkPath (string version, bool isSimulator);
		bool SdkIsInstalled (IAppleSdkVersion version, bool isSimulator);
		bool TryParseSdkVersion (string value, out IAppleSdkVersion version);
		IAppleSdkVersion GetClosestInstalledSdk (IAppleSdkVersion version, bool isSimulator);
		IList<IAppleSdkVersion> GetInstalledSdkVersions (bool isSimulator);
		AppleDTSettings GetAppleDTSettings ();
		AppleDTSdkSettings GetSdkSettings (IAppleSdkVersion version, bool isSimulator);
	}
}
