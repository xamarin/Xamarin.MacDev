//
// EntitlementExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (www.xamarin.com)
//

namespace Xamarin.MacDev
{
	public static class EntitlementKeys
	{
		public const string WirelessAccessoryConfiguration = "com.apple.external-accessory.wireless-configuration";
		public const string UbiquityKeyValueStore = "com.apple.developer.ubiquity-kvstore-identifier";
		public const string UbiquityContainers = "com.apple.developer.ubiquity-container-identifiers";
		public const string iCloudContainers = "com.apple.developer.icloud-container-identifiers";
		public const string iCloudServices = "com.apple.developer.icloud-services";
		public const string PassBookIdentifiers = "com.apple.developer.pass-type-identifiers";
		public const string AssociatedDomains = "com.apple.developer.associated-domains";
		public const string ApplicationGroups = "com.apple.security.application-groups";
		public const string NetworkingVpnApi = "com.apple.developer.networking.vpn.api";
		public const string NetworkExtensions = "com.apple.developer.networking.networkextension";
		public const string HotspotConfiguration = "com.apple.developer.networking.HotspotConfiguration";
		public const string Multipath = "com.apple.developer.networking.multipath";
		public const string InAppPayments = "com.apple.developer.in-app-payments";
		public const string KeychainAccessGroups = "keychain-access-groups";
		public const string HealthKit = "com.apple.developer.healthkit";
		public const string HomeKit = "com.apple.developer.homekit";
		public const string InterAppAudio = "inter-app-audio";
		public const string GetTaskAllow = "get-task-allow";
		public const string Siri = "com.apple.developer.siri";
		public const string APS = "aps-environment";
	}

	public static class EntitlementExtensions
	{
		public static PArray GetApplePayMerchants (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.InAppPayments);
		}

		public static void SetApplePayMerchants (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.InAppPayments);
			else
				dict[EntitlementKeys.InAppPayments] = value;
		}

		public static PArray GetApplicationGroups (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.ApplicationGroups);
		}

		public static void SetApplicationGroups (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.ApplicationGroups);
			else
				dict[EntitlementKeys.ApplicationGroups] = value;
		}

		public static PArray GetAssociatedDomains (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.AssociatedDomains);
		}

		public static void SetAssociatedDomains (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.AssociatedDomains);
			else
				dict[EntitlementKeys.AssociatedDomains] = value;
		}

		public static PArray GetiCloudContainers (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.iCloudContainers);
		}

		public static void SetiCloudContainers (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.iCloudContainers);
			else
				dict[EntitlementKeys.iCloudContainers] = value;
		}

		public static string GetUbiquityKeyValueStore (this PDictionary dict)
		{
			var str = dict.Get<PString> (EntitlementKeys.UbiquityKeyValueStore);
			return str == null ? null : str.Value;
		}

		public static void SetUbiquityKeyValueStore (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (EntitlementKeys.UbiquityKeyValueStore);
			else
				dict[EntitlementKeys.UbiquityKeyValueStore] = value;
		}

		public static PArray GetiCloudServices (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.iCloudServices);
		}

		public static void SetiCloudServices (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.iCloudServices);
			else
				dict[EntitlementKeys.iCloudServices] = value;
		}

		public static PArray GetKeychainAccessGroups (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.KeychainAccessGroups);
		}

		public static void SetKeychainAccessGroups (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.KeychainAccessGroups);
			else
				dict[EntitlementKeys.KeychainAccessGroups] = value;
		}

		public static PArray GetPassBookIdentifiers (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.PassBookIdentifiers);
		}

		public static void SetPassBookIdentifiers (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.PassBookIdentifiers);
			else
				dict[EntitlementKeys.PassBookIdentifiers] = value;
		}
	}
}
