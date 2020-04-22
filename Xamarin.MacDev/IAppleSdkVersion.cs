using System;
namespace Xamarin.MacDev {
	public interface IAppleSdkVersion : IEquatable<IAppleSdkVersion> {
		bool IsUseDefault { get; }
		void SetVersion (int [] version);
		IAppleSdkVersion GetUseDefault ();
	}

	public static class IAppleSdkVersion_Extensions {
		public static IAppleSdkVersion ResolveIfDefault (this IAppleSdkVersion @this, IAppleSdk sdk, bool sim)
		{
			return @this.IsUseDefault ? @this.GetDefault (sdk, sim) : @this;
		}

		public static IAppleSdkVersion GetDefault (this IAppleSdkVersion @this, IAppleSdk sdk, bool sim)
		{
			var v = sdk.GetInstalledSdkVersions (sim);
			return v.Count > 0 ? v [v.Count - 1] : @this.GetUseDefault ();
		}

		public static bool TryParse<T> (string s, out T result) where T : IAppleSdkVersion, new()
		{
			result = new T ();
			if (s == null)
				return false;

			var vstr = s.Split ('.');
			var vint = new int [vstr.Length];

			for (int j = 0; j < vstr.Length; j++) {
				int component;
				if (!int.TryParse (vstr [j], out component))
					return false;

				vint [j] = component;
			}

			Console.WriteLine ("V before: {0}", result);
			result.SetVersion (vint);
			Console.WriteLine ("V after: {0} {1}", result, string.Join (".", vint));
			return true;
		}

		public static bool TryParse<T> (string s, out IAppleSdkVersion result) where T : IAppleSdkVersion, new()
		{
			var rv = TryParse<T> (s, out T tmp);
			result = tmp;
			return rv;
		}
	}
}
