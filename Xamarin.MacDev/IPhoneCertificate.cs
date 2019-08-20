//
// IPhoneCertificate.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
//

using System;
using System.Security.Cryptography.X509Certificates;

namespace Xamarin.MacDev
{
	public static class IPhoneCertificate
	{
		public static readonly string[] DevelopmentPrefixes = { "iPhone Developer", "iOS Development", "Apple Development" };
		public static readonly string[] DistributionPrefixes = { "iPhone Distribution", "iOS Distribution", "Apple Distribution" };

		public static bool IsDevelopment (string name)
		{
			foreach (var prefix in DevelopmentPrefixes) {
				if (name.StartsWith (prefix, StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		public static bool IsDevelopment (X509Certificate2 cert)
		{
			return IsDevelopment (Keychain.GetCertificateCommonName (cert));
		}

		public static bool IsDistribution (string name)
		{
			foreach (var prefix in DistributionPrefixes) {
				if (name.StartsWith (prefix, StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		public static bool IsDistribution (X509Certificate2 cert)
		{
			return IsDistribution (Keychain.GetCertificateCommonName (cert));
		}
	}
}
