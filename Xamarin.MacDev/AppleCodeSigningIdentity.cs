//
// AppleCodeSigningIdentity.cs
//
// Author:
//       Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
//

using System.Security.Cryptography.X509Certificates;

namespace Xamarin.MacDev {
	public class AppleCodeSigningIdentity {
		public string CommonName { get { return Keychain.GetCertificateCommonName (Certificate); } }
		public X509Certificate2 Certificate { get; private set; }
		public bool HasPrivateKey { get; private set; }

		public AppleCodeSigningIdentity (X509Certificate2 certificate, bool hasPrivateKey)
		{
			Certificate = certificate;
			HasPrivateKey = hasPrivateKey;
		}
	}
}
