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
		static readonly string IndexFileName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Xamarin", "Provisioning Profiles.index");
		static readonly string TempIndexFileName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Xamarin", "Provisioning Profiles.tmp");
		static readonly MobileProvisionCreationDateComparer CreationDateComparer = new MobileProvisionCreationDateComparer ();
		const int IndexVersion = 4;

		class DeveloperCertificate
		{
			public string Name;
			public string Thumbprint;

			DeveloperCertificate ()
			{
			}

			public DeveloperCertificate (X509Certificate2 certificate)
			{
				Name = Keychain.GetCertificateCommonName (certificate);
				Thumbprint = certificate.Thumbprint;
			}

			public static DeveloperCertificate Load (BinaryReader reader)
			{
				var certificate = new DeveloperCertificate ();
				certificate.Name = reader.ReadString ();
				certificate.Thumbprint = reader.ReadString ();
				return certificate;
			}

			public void Write (BinaryWriter writer)
			{
				writer.Write (Name);
				writer.Write (Thumbprint);
			}
		}

		class ProvisioningProfile
		{
			public string FileName;
			public DateTime LastModified;

			public string Name;
			public string Uuid;
			public MobileProvisionDistributionType Distribution;
			public DateTime CreationDate;
			public DateTime ExpirationDate;

			public List<MobileProvisionPlatform> Platforms;
			public string ApplicationIdentifier;
			public List<DeveloperCertificate> DeveloperCertificates;

			public ProvisioningProfile ()
			{
				Platforms = new List<MobileProvisionPlatform> ();
				DeveloperCertificates = new List<DeveloperCertificate> ();
			}

			public ProvisioningProfile (string fileName, MobileProvision provision) : this ()
			{
				FileName = fileName;
				LastModified = File.GetLastWriteTimeUtc (fileName);

				Name = provision.Name;
				Uuid = provision.Uuid;
				Distribution = provision.DistributionType;
				CreationDate = provision.CreationDate;
				ExpirationDate = provision.ExpirationDate;

				if (provision.Platforms != null) {
					for (int i = 0; i < provision.Platforms.Length; i++)
						Platforms.Add (provision.Platforms[i]);
				}

				PString identifier;
				if (provision.Entitlements.TryGetValue ("com.apple.application-identifier", out identifier))
					ApplicationIdentifier = identifier.Value;
				else if (provision.Entitlements.TryGetValue ("application-identifier", out identifier))
					ApplicationIdentifier = identifier.Value;
				else
					ApplicationIdentifier = string.Empty;

				for (int i = 0; i < provision.DeveloperCertificates.Count; i++)
					DeveloperCertificates.Add (new DeveloperCertificate (provision.DeveloperCertificates [i]));
			}

			public static ProvisioningProfile Load (BinaryReader reader)
			{
				var profile = new ProvisioningProfile ();
				int count;

				profile.FileName = reader.ReadString ();
				profile.LastModified = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

				profile.Name = reader.ReadString ();
				profile.Uuid = reader.ReadString ();
				Enum.TryParse (reader.ReadString (), out profile.Distribution);
				profile.CreationDate = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);
				profile.ExpirationDate = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

				count = reader.ReadInt32 ();
				for (int i = 0; i < count; i++) {
					MobileProvisionPlatform platform;

					Enum.TryParse (reader.ReadString (), out platform);
					profile.Platforms.Add (platform);
				}

				profile.ApplicationIdentifier = reader.ReadString ();

				count = reader.ReadInt32 ();
				for (int i = 0; i < count; i++)
					profile.DeveloperCertificates.Add (DeveloperCertificate.Load (reader));

				return profile;
			}

			public void Write (BinaryWriter writer)
			{
				writer.Write (FileName);
				writer.Write (LastModified.Ticks);

				writer.Write (Name);
				writer.Write (Uuid);
				writer.Write (Distribution.ToString ());
				writer.Write (CreationDate.Ticks);
				writer.Write (ExpirationDate.Ticks);

				writer.Write (Platforms.Count);
				for (int i = 0; i < Platforms.Count; i++)
					writer.Write (Platforms[i].ToString ());

				writer.Write (ApplicationIdentifier);

				writer.Write (DeveloperCertificates.Count);
				for (int i = 0; i < DeveloperCertificates.Count; i++)
					DeveloperCertificates[i].Write (writer);
			}
		}

		class MobileIndex
		{
			public int Version;
			public DateTime LastModified;
			public List<ProvisioningProfile> ProvisioningProfiles;

			public MobileIndex ()
			{
				ProvisioningProfiles = new List<ProvisioningProfile> ();
			}

			public static MobileIndex Load (string fileName)
			{
				var index = new MobileIndex ();

				using (var stream = File.OpenRead (fileName)) {
					using (var reader = new BinaryReader (stream)) {
						index.Version = reader.ReadInt32 ();
						index.LastModified = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

						int count = reader.ReadInt32 ();
						for (int i = 0; i < count; i++) {
							var profile = ProvisioningProfile.Load (reader);
							index.ProvisioningProfiles.Add (profile);
						}

						return index;
					}
				}
			}

			public void Save (string fileName, string tempFileName)
			{
				try {
					Directory.CreateDirectory (Path.GetDirectoryName (fileName));

					using (var stream = File.Create (tempFileName, 4096)) {
						using (var writer = new BinaryWriter (stream)) {
							writer.Write (Version);
							writer.Write (LastModified.Ticks);

							writer.Write (ProvisioningProfiles.Count);
							for (int i = 0; i < ProvisioningProfiles.Count; i++)
								ProvisioningProfiles[i].Write (writer);
						}
					}

					if (File.Exists (fileName))
						File.Replace (tempFileName, fileName, null, true);
					else
						File.Move (tempFileName, fileName);
				} catch (Exception ex) {
					LoggingService.LogWarning ("Failed to save '{0}': {1}", fileName, ex);
				}
			}
		}

		class MobileProvisionCreationDateComparer : IComparer<ProvisioningProfile>
		{
			public int Compare (ProvisioningProfile x, ProvisioningProfile y)
			{
				return x.CreationDate.CompareTo (y.CreationDate);
			}
		}

		static ProvisioningProfile LoadMobileProfile (string fileName)
		{
			var provision = MobileProvision.LoadFromFile (fileName);

			return new ProvisioningProfile (fileName, provision);
		}

		static MobileIndex CreateIndex ()
		{
			var index = new MobileIndex ();

			if (Directory.Exists (MobileProvision.ProfileDirectory)) {
				foreach (var fileName in Directory.EnumerateFiles (MobileProvision.ProfileDirectory)) {
					if (!fileName.EndsWith (".mobileprovision", StringComparison.Ordinal) && !fileName.EndsWith (".provisionprofile", StringComparison.Ordinal))
						continue;

					try {
						var profile = LoadMobileProfile (fileName);
						index.ProvisioningProfiles.Add (profile);
					} catch (Exception ex) {
						LoggingService.LogWarning ("Error reading provisioning profile '{0}': {1}", fileName, ex);
					}
				}

				index.ProvisioningProfiles.Sort (CreationDateComparer);
			} else {
				Directory.CreateDirectory (MobileProvision.ProfileDirectory);
			}

			index.Version = IndexVersion;
			index.LastModified = Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory);

			index.Save (IndexFileName, TempIndexFileName);

			return index;
		}

		static MobileIndex OpenIndex ()
		{
			MobileIndex index;

			try {
				index = MobileIndex.Load (IndexFileName);

				if (Directory.Exists (MobileProvision.ProfileDirectory)) {
					var mtime = Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory);

					if (index.Version != IndexVersion) {
						index = CreateIndex ();
					} else if (index.LastModified < mtime) {
						var table = new Dictionary<string, ProvisioningProfile> ();

						foreach (var profile in index.ProvisioningProfiles)
							table[profile.FileName] = profile;

						foreach (var fileName in Directory.EnumerateFiles (MobileProvision.ProfileDirectory)) {
							if (!fileName.EndsWith (".mobileprovision", StringComparison.Ordinal) && !fileName.EndsWith (".provisionprofile", StringComparison.Ordinal))
								continue;

							ProvisioningProfile profile;
							bool unknown = false;

							if (table.TryGetValue (Path.GetFileName (fileName), out profile)) {
								// remove from our lookup table (any leftover key/valie pairs will be used to determine deleted files)
								table.Remove (Path.GetFileName (fileName));

								// check if the file has changed since our last resync
								mtime = File.GetLastWriteTimeUtc (fileName);

								if (profile.LastModified < mtime) {
									// remove the old record
									index.ProvisioningProfiles.Remove (profile);

									// treat this provisioning profile as if it is unknown
									unknown = true;
								}
							} else {
								unknown = true;
							}

							if (unknown) {
								// unknown provisioning profile; add it to our ProvisioningProfiles array
								try {
									profile = LoadMobileProfile (fileName);
									index.ProvisioningProfiles.Add (profile);
								} catch (Exception ex) {
									LoggingService.LogWarning ("Error reading provisioning profile '{0}': {1}", fileName, ex);
								}
							}
						}

						// remove provisioning profiles which have been deleted from the file system
						foreach (var item in table)
							index.ProvisioningProfiles.Remove (item.Value);

						index.LastModified = Directory.GetLastWriteTimeUtc (MobileProvision.ProfileDirectory);
						index.Version = IndexVersion;

						index.ProvisioningProfiles.Sort (CreationDateComparer);

						index.Save (IndexFileName, TempIndexFileName);
					}
				} else {
					try {
						File.Delete (IndexFileName);
					} catch (Exception ex) {
						LoggingService.LogWarning ("Failed to delete stale index '{0}': {1}", IndexFileName, ex);
					}

					index.ProvisioningProfiles.Clear ();
				}
			} catch {
				index = CreateIndex ();
			}

			return index;
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

		public static MobileProvision GetMobileProvision (MobileProvisionPlatform platform, string name)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var path = Path.Combine (MobileProvision.ProfileDirectory, name + extension);

			if (File.Exists (path))
				return MobileProvision.LoadFromFile (path);

			var latestCreationDate = DateTime.MinValue;
			var index = OpenIndex ();

			path = null;

			foreach (var profile in index.ProvisioningProfiles) {
				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (name == profile.Name || name == profile.Uuid)
					return MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));

				if (profile.CreationDate > latestCreationDate) {
					path = Path.Combine (MobileProvision.ProfileDirectory, profile.FileName);
					latestCreationDate = profile.CreationDate;
				}
			}

			if (path == null)
				return null;

			return MobileProvision.LoadFromFile (path);
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, bool includeExpired = false, bool unique = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;
			var index = OpenIndex ();

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles[i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (!includeExpired && profile.ExpirationDate < now)
					continue;

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list[idx].CreationDate)
							list[idx] = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					} else {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, bool includeExpired = false, bool unique = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;
			var index = OpenIndex ();

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles[i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (!includeExpired && profile.ExpirationDate < now)
					continue;

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0)
					continue;

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list [idx].CreationDate)
							list [idx] = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					} else {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false, bool unique = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;
			var index = OpenIndex ();

			if (developerCertificates == null)
				throw new ArgumentNullException (nameof (developerCertificates));

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0)
				return list;

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles[i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (!includeExpired && profile.ExpirationDate < now)
					continue;

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0)
					continue;

				foreach (var cert in profile.DeveloperCertificates) {
					if (!thumbprints.Contains (cert.Thumbprint))
						continue;

					if (unique) {
						int idx;

						if (dictionary.TryGetValue (profile.Name, out idx)) {
							if (profile.CreationDate > list [idx].CreationDate)
								list [idx] = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						} else {
							var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
							dictionary.Add (profile.Name, list.Count);
							list.Add (provision);
						}
					} else {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						list.Add (provision);
					}
					break;
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, bool includeExpired = false, bool unique = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;
			var plist = OpenIndex ();

			if (bundleIdentifier == null)
				throw new ArgumentNullException (nameof (bundleIdentifier));

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = plist.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = plist.ProvisioningProfiles[i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (!includeExpired && profile.ExpirationDate < now)
					continue;

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0)
					continue;

				var id = profile.ApplicationIdentifier;
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

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list [idx].CreationDate)
							list [idx] = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					} else {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false, bool unique = false)
		{
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;
			var plist = OpenIndex ();

			if (bundleIdentifier == null)
				throw new ArgumentNullException (nameof (bundleIdentifier));

			if (developerCertificates == null)
				throw new ArgumentNullException (nameof (developerCertificates));

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0)
				return list;

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = plist.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = plist.ProvisioningProfiles[i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal))
					continue;

				if (!profile.Platforms.Contains (platform))
					continue;

				if (!includeExpired && profile.ExpirationDate < now)
					continue;

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0)
					continue;

				var id = profile.ApplicationIdentifier;
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

				foreach (var cert in profile.DeveloperCertificates) {
					if (!thumbprints.Contains (cert.Thumbprint))
						continue;

					if (unique) {
						int idx;

						if (dictionary.TryGetValue (profile.Name, out idx)) {
							if (profile.CreationDate > list [idx].CreationDate)
								list [idx] = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						} else {
							var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
							dictionary.Add (profile.Name, list.Count);
							list.Add (provision);
						}
					} else {
						var provision = MobileProvision.LoadFromFile (Path.Combine (MobileProvision.ProfileDirectory, profile.FileName));
						list.Add (provision);
					}
					break;
				}
			}

			return list;
		}
	}
}
