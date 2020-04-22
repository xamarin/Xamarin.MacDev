using System;
using System.Collections.Generic;

namespace Xamarin.MacDev {
	public interface IAppleSdk {
		bool IsInstalled { get; }
		string DeveloperRoot { get; }
		string GetPlatformPath (bool is_simulator);
		string GetSdkPath (string version, bool is_simulator);
		bool SdkIsInstalled (IAppleSdkVersion version, bool is_simulator);
		bool TryParseSdkVersion (string value, out IAppleSdkVersion version);
		IAppleSdkVersion GetClosestInstalledSdk (IAppleSdkVersion version, bool is_simulator);
		IList<IAppleSdkVersion> GetInstalledSdkVersions (bool is_simulator);
	}
}
