//
// MobileProvisionIndex.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Xamarin.MacDev
{
	public static class MobileProvisionIndex
	{
		static readonly string IndexFileName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Xamarin", "Provisioning Profiles.plist");
		const int IndexVersion = 3;

		static PDictionary CreateIndexRecord (string fileName)
		{
			var provision = MobileProvision.LoadFromFile (fileName);
			var certificates = new PArray ();
			var record = new PDictionary ();
			PString identifier;

			record.Add ("FileName", new PString (Path.GetFileName (fileName)));
			record.Add ("LastModified", new PDate (File.GetLastWriteTimeUtc (fileName)));

			record.Add ("Name", new PString (provision.Name));
			record.Add ("Uuid", new PString (provision.Uuid));
			record.Add ("Distribution", new PString (provision.DistributionType.ToString ()));
			record.Add ("ExpirationDate", new PDate (provision.ExpirationDate));

			var platforms = new PArray ();
			if (provision.Platforms != null) {
				for (int i = 0; i < provision.Platforms.Length; i++)
					platforms.Add (new PString (provision.Platforms[i].ToString ()));
			}
			record.Add ("Platforms", platforms);

			if (provision.Entitlements.TryGetValue ("com.apple.application-identifier", out identifier))
				record.Add ("ApplicationIdentifier", new PString (identifier.Value));
			else if (provision.Entitlements.TryGetValue ("application-identifier", out identifier))
				record.Add ("ApplicationIdentifier", new PString (identifier.Value));
			else
				record.Add ("ApplicationIdentifier", new PString (string.Empty));

			foreach (var certificate in provision.DeveloperCertificates) {
				var info = new PDictionary ();

				info.Add ("Name", new PString (Keychain.GetCertificateCommonName (certificate)));
				info.Add ("Thumbprint", new PString (certificate.Thumbprint));

				certificates.Add (info);
			}

			record.Add ("DeveloperCertificates", certificates);

			return record;
		}

		static void Save (PDictionary plist)
		{
			try {
				Directory.CreateDirectory (Path.GetDirectoryName (IndexFileName));
				plist.Save (IndexFileName, true, true);
			} catch (Exception ex) {
				LoggingService.LogWarning ("Failed to save '{0}': {1}", IndexFileName, ex);
			}
		}

		static PDictionary CreateIndex ()
		{
			var plist = new PDictionary ();
			var profiles = new PArray ();

			foreach (var fileName in Directory.EnumerateFiles (MobileProvision.ProfileDirectory)) {
				if (!fileName.EndsWith (".mobileprovision", StringComparison.Ordinal) && !fileName.EndsWith (".provisionprofile", StringComparison.Ordinal))
					continue;

				try {
					var profile = CreateIndexRecord (fileName);
					profiles.Add (profile);
				} catch (Exception ex) {
					LoggingService.LogWarning ("Error reading provisioning profile '{0}': {1}", fileName, ex);
				}
			}

			plist.Add ("Version", new PNumber (IndexVersion));
			plist.Add ("LastModified", new PDate (Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory)));
			plist.Add ("ProvisioningProfiles", profiles);

			Save (plist);

			return plist;
		}

		static bool LastModifiedChanged (PDictionary plist, DateTime mtime)
		{
			PDate lastModified;

			if (!plist.TryGetValue ("LastModified", out lastModified))
				return true;

			return lastModified.Value < mtime;
		}

		static bool VersionChanged (PDictionary plist, int version)
		{
			PNumber value;

			if (!plist.TryGetValue ("Version", out value))
				return true;

			return value.Value != version;
		}

		static PDictionary OpenIndex ()
		{
			PDictionary plist;

			try {
				plist = PDictionary.FromFile (IndexFileName);

				if (Directory.Exists (MobileProvision.ProfileDirectory)) {
					var mtime = Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory);

					if (VersionChanged (plist, IndexVersion)) {
						plist = CreateIndex ();
					} else if (LastModifiedChanged (plist, mtime)) {
						var table = new Dictionary<string, PDictionary> ();
						PArray profiles;

						if (plist.TryGetValue ("ProvisioningProfiles", out profiles)) {
							foreach (var profile in profiles.OfType<PDictionary> ()) {
								PString fileName;

								if (!profile.TryGetValue ("FileName", out fileName))
									continue;

								table[fileName.Value] = profile;
							}
						} else {
							plist.Add ("ProvisioningProfiles", profiles = new PArray ());
						}

						foreach (var fileName in Directory.EnumerateFiles (MobileProvision.ProfileDirectory)) {
							if (!fileName.EndsWith (".mobileprovision", StringComparison.Ordinal) && !fileName.EndsWith (".provisionprofile", StringComparison.Ordinal))
								continue;

							bool unknown = false;
							PDictionary profile;

							if (table.TryGetValue (Path.GetFileName (fileName), out profile)) {
								// remove from our lookup table (any leftover key/valie pairs will be used to determine deleted files)
								table.Remove (Path.GetFileName (fileName));

								// check if the file has changed since our last resync
								mtime = File.GetLastWriteTimeUtc (fileName);

								if (LastModifiedChanged (profile, mtime)) {
									// remove the old record
									profile.Remove ();

									// treat this provisioning profile as if it is unknown
									unknown = true;
								}
							} else {
								unknown = true;
							}

							if (unknown) {
								// unknown provisioning profile; add it to our ProvisioningProfiles array
								try {
									profile = CreateIndexRecord (fileName);
									profiles.Add (profile);
								} catch (Exception ex) {
									LoggingService.LogWarning ("Error reading provisioning profile '{0}': {1}", fileName, ex);
								}
							}
						}

						// remove provisioning profiles which have been deleted from the file system
						foreach (var record in table)
							record.Value.Remove ();

						plist["LastModified"] = new PDate (Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory));
						plist["Version"] = new PNumber (IndexVersion);

						Save (plist);
					}
				} else {
					try {
						File.Delete (IndexFileName);
					} catch (Exception ex) {
						LoggingService.LogWarning ("Failed to delete stale index '{0}': {1}", IndexFileName, ex);
					}

					plist.Clear ();
				}
			} catch {
				plist = CreateIndex ();
			}

			return plist;
		}

		static bool Contains (PArray platforms, MobileProvisionPlatform platform)
		{
			var value = platform.ToString ();

			foreach (var item in platforms.OfType<PString> ()) {
				if (item.Value == value)
					return true;
			}

			return false;
		}

		public static MobileProvision GetMobileProvision (MobileProvisionPlatform platform, string uuid)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var path = Path.Combine (MobileProvision.ProfileDirectory, uuid + extension);

			if (File.Exists (path))
				return MobileProvision.LoadFromFile (path);

			var plist = OpenIndex ();
			PString fileName;
			PArray platforms;
			PArray profiles;
			PString value;

			if (plist.TryGetValue ("ProvisioningProfiles", out profiles)) {
				foreach (var profile in profiles.OfType<PDictionary> ()) {
					if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
						continue;

					if (!profile.TryGetValue ("Uuid", out value))
						continue;

					if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
						continue;

					return MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
				}
			}

			return null;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, bool includeExpired = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var list = new List<MobileProvision> ();
			var plist = OpenIndex ();
			var now = DateTime.Now;
			PString fileName;
			PArray platforms;
			PArray profiles;

			if (!plist.TryGetValue ("ProvisioningProfiles", out profiles))
				return list;

			foreach (var profile in profiles.OfType<PDictionary> ()) {
				if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
					continue;

				if (!includeExpired) {
					PDate expirationDate;

					if (!profile.TryGetValue ("ExpirationDate", out expirationDate))
						continue;

					if (expirationDate.Value < now)
						continue;
				}

				var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
				list.Add (provision);
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, bool includeExpired = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var list = new List<MobileProvision> ();
			MobileProvisionDistributionType dist;
			var plist = OpenIndex ();
			var now = DateTime.Now;
			PString fileName;
			PArray platforms;
			PArray profiles;

			if (!plist.TryGetValue ("ProvisioningProfiles", out profiles))
				return list;

			foreach (var profile in profiles.OfType<PDictionary> ()) {
				if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
					continue;

				if (!includeExpired) {
					PDate expirationDate;

					if (!profile.TryGetValue ("ExpirationDate", out expirationDate))
						continue;

					if (expirationDate.Value < now)
						continue;
				}

				if (type != MobileProvisionDistributionType.Any) {
					PString value;

					if (!profile.TryGetValue ("Distribution", out value) || !Enum.TryParse<MobileProvisionDistributionType> (value.Value, out dist))
						continue;

					if ((type & dist) == 0)
						continue;
				}

				var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
				list.Add (provision);
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			MobileProvisionDistributionType dist;
			var plist = OpenIndex ();
			var now = DateTime.Now;
			PString fileName;
			PArray platforms;
			PArray profiles;
			PArray array;

			if (developerCertificates == null)
				throw new ArgumentNullException ("developerCertificates");

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0)
				return list;

			if (!plist.TryGetValue ("ProvisioningProfiles", out profiles))
				return list;

			foreach (var profile in profiles.OfType<PDictionary> ()) {
				if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
					continue;

				if (!includeExpired) {
					PDate expirationDate;

					if (!profile.TryGetValue ("ExpirationDate", out expirationDate))
						continue;

					if (expirationDate.Value < now)
						continue;
				}

				if (type != MobileProvisionDistributionType.Any) {
					PString value;

					if (!profile.TryGetValue ("Distribution", out value) || !Enum.TryParse<MobileProvisionDistributionType> (value.Value, out dist))
						continue;

					if ((type & dist) == 0)
						continue;
				}

				if (!profile.TryGetValue ("DeveloperCertificates", out array))
					continue;

				foreach (var cert in array.OfType<PDictionary> ()) {
					PString thumbprint;

					if (!cert.TryGetValue ("Thumbprint", out thumbprint))
						continue;

					if (thumbprints.Contains (thumbprint.Value)) {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
						list.Add (provision);
						break;
					}
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, bool includeExpired = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			MobileProvisionDistributionType dist;
			var plist = OpenIndex ();
			var now = DateTime.Now;
			PString identifier;
			PString fileName;
			PArray platforms;
			PArray profiles;
			PArray array;

			if (bundleIdentifier == null)
				throw new ArgumentNullException ("bundleIdentifier");

			if (thumbprints.Count == 0)
				return list;

			if (!plist.TryGetValue ("ProvisioningProfiles", out profiles))
				return list;

			foreach (var profile in profiles.OfType<PDictionary> ()) {
				if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
					continue;

				if (!includeExpired) {
					PDate expirationDate;

					if (!profile.TryGetValue ("ExpirationDate", out expirationDate))
						continue;

					if (expirationDate.Value < now)
						continue;
				}

				if (type != MobileProvisionDistributionType.Any) {
					PString value;

					if (!profile.TryGetValue ("Distribution", out value) || !Enum.TryParse<MobileProvisionDistributionType> (value.Value, out dist))
						continue;

					if ((type & dist) == 0)
						continue;
				}

				if (!profile.TryGetValue ("ApplicationIdentifier", out identifier))
					continue;

				string id = identifier.Value;
				int dot;

				// Note: the ApplicationIdentifier will be in the form "7V723M9SQ5.com.xamarin.app-name", so we'll need to trim the leading TeamIdentifierPrefix
				if ((dot = id.IndexOf ('.')) != -1)
					id = id.Substring (dot + 1);

				if (id.Length > 0 && id[id.Length - 1] == '*') {
					// Note: this is a wildcard provisioning profile, which means we need to use a substring match
					id = id.TrimEnd ('*');

					if (!bundleIdentifier.StartsWith (id, StringComparison.Ordinal))
						continue;
				} else if (id != bundleIdentifier) {
					// the CFBundleIdentifier provided by our caller does not match this provisioning profile
					continue;
				}

				var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
				list.Add (provision);
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			MobileProvisionDistributionType dist;
			var plist = OpenIndex ();
			var now = DateTime.Now;
			PString identifier;
			PString fileName;
			PArray platforms;
			PArray profiles;
			PArray array;

			if (bundleIdentifier == null)
				throw new ArgumentNullException ("bundleIdentifier");

			if (developerCertificates == null)
				throw new ArgumentNullException ("developerCertificates");

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0)
				return list;

			if (!plist.TryGetValue ("ProvisioningProfiles", out profiles))
				return list;

			foreach (var profile in profiles.OfType<PDictionary> ()) {
				if (!profile.TryGetValue ("FileName", out fileName) || !fileName.Value.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.TryGetValue ("Platforms", out platforms) || !Contains (platforms, platform))
					continue;

				if (!includeExpired) {
					PDate expirationDate;

					if (!profile.TryGetValue ("ExpirationDate", out expirationDate))
						continue;

					if (expirationDate.Value < now)
						continue;
				}

				if (type != MobileProvisionDistributionType.Any) {
					PString value;

					if (!profile.TryGetValue ("Distribution", out value) || !Enum.TryParse<MobileProvisionDistributionType> (value.Value, out dist))
						continue;

					if ((type & dist) == 0)
						continue;
				}

				if (!profile.TryGetValue ("ApplicationIdentifier", out identifier))
					continue;

				string id = identifier.Value;
				int dot;

				// Note: the ApplicationIdentifier will be in the form "7V723M9SQ5.com.xamarin.app-name", so we'll need to trim the leading TeamIdentifierPrefix
				if ((dot = id.IndexOf ('.')) != -1)
					id = id.Substring (dot + 1);

				if (id.Length > 0 && id[id.Length - 1] == '*') {
					// Note: this is a wildcard provisioning profile, which means we need to use a substring match
					id = id.TrimEnd ('*');

					if (!bundleIdentifier.StartsWith (id, StringComparison.Ordinal))
						continue;
				} else if (id != bundleIdentifier) {
					// the CFBundleIdentifier provided by our caller does not match this provisioning profile
					continue;
				}

				if (!profile.TryGetValue ("DeveloperCertificates", out array))
					continue;

				foreach (var cert in array.OfType<PDictionary> ()) {
					PString thumbprint;

					if (!cert.TryGetValue ("Thumbprint", out thumbprint))
						continue;

					if (thumbprints.Contains (thumbprint.Value)) {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, fileName.Value));
						list.Add (provision);
						break;
					}
				}
			}

			return list;
		}
	}
}
