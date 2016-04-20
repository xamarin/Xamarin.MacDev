namespace Xamarin.MacDev
{
	public interface IMonoMacSdk
	{
		string LegacyFrameworkAssembly { get; }
		string LegacyAppLauncherPath { get; }
	}
}
