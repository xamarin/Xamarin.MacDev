//
// TestMobileProvisionIndex.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corp.
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

using NUnit.Framework;

using Xamarin.MacDev;

namespace UnitTests
{
	[TestFixture]
	public class TestMobileProvisionIndex
	{
		[Test]
		public void TestCreateIndex ()
		{
			var index = MobileProvisionIndex.CreateIndex ("../../TestData/Provisioning Profiles", "profiles.index");

			Assert.AreEqual (2, index.ProvisioningProfiles.Count);

			Assert.AreEqual ("YHT9CR87YA.com.companyname.*", index.ProvisioningProfiles[0].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[0].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[0].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[0].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[0].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[0].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[0].ExpirationDate);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[0].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("CompanyName Development Profile", index.ProvisioningProfiles[0].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[0].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[0].Platforms[0]);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e", index.ProvisioningProfiles[0].Uuid);

			Assert.AreEqual ("YHT9CR87YA.com.xamarin.*", index.ProvisioningProfiles[1].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[1].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[1].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[1].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[1].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[1].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[1].ExpirationDate);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[1].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("Xamarin Development Profile", index.ProvisioningProfiles[1].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[1].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[1].Platforms[0]);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616", index.ProvisioningProfiles[1].Uuid);
		}

		[Test]
		public void TestOpenIndex ()
		{
			var index = MobileProvisionIndex.OpenIndex ("../../TestData/Provisioning Profiles", "profiles.index");

			Assert.AreEqual (2, index.ProvisioningProfiles.Count);

			Assert.AreEqual ("YHT9CR87YA.com.companyname.*", index.ProvisioningProfiles[0].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[0].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[0].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[0].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[0].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[0].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[0].ExpirationDate);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[0].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("CompanyName Development Profile", index.ProvisioningProfiles[0].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[0].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[0].Platforms[0]);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e", index.ProvisioningProfiles[0].Uuid);

			Assert.AreEqual ("YHT9CR87YA.com.xamarin.*", index.ProvisioningProfiles[1].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[1].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[1].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[1].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[1].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[1].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[1].ExpirationDate);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[1].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("Xamarin Development Profile", index.ProvisioningProfiles[1].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[1].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[1].Platforms[0]);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616", index.ProvisioningProfiles[1].Uuid);
		}
	}
}
