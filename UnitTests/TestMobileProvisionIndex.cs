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

			var idCompanyName = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("companyname"));
			var idXamarin = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("xamarin"));
			Assert.AreNotEqual (-1, idCompanyName, "Company Name Index");
			Assert.AreNotEqual (-1, idXamarin, "Xamarin Index");
			Assert.AreNotEqual (idXamarin, idCompanyName, "Indices");
			Assert.AreEqual ("YHT9CR87YA.com.companyname.*", index.ProvisioningProfiles[idCompanyName].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[idCompanyName].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[idCompanyName].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[idCompanyName].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[idCompanyName].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[idCompanyName].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[idCompanyName].ExpirationDate);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[idCompanyName].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("CompanyName Development Profile", index.ProvisioningProfiles[idCompanyName].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[0].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[idCompanyName].Platforms[0]);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e", index.ProvisioningProfiles[idCompanyName].Uuid);

			Assert.AreEqual ("YHT9CR87YA.com.xamarin.*", index.ProvisioningProfiles[idXamarin].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[idXamarin].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[idXamarin].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[idXamarin].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[idXamarin].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[idXamarin].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[idXamarin].ExpirationDate);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[idXamarin].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("Xamarin Development Profile", index.ProvisioningProfiles[idXamarin].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[idXamarin].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[idXamarin].Platforms[0]);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616", index.ProvisioningProfiles[idXamarin].Uuid);
		}

		[Test]
		public void TestOpenIndex ()
		{
			var index = MobileProvisionIndex.OpenIndex ("../../TestData/Provisioning Profiles", "profiles.index");

			Assert.AreEqual (2, index.ProvisioningProfiles.Count);

			var idCompanyName = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("companyname"));
			var idXamarin = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("xamarin"));
			Assert.AreNotEqual (-1, idCompanyName, "Company Name Index");
			Assert.AreNotEqual (-1, idXamarin, "Xamarin Index");
			Assert.AreNotEqual (idXamarin, idCompanyName, "Indices");
			Assert.AreEqual ("YHT9CR87YA.com.companyname.*", index.ProvisioningProfiles[idCompanyName].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[idCompanyName].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[idCompanyName].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[idCompanyName].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[idCompanyName].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[idCompanyName].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc), index.ProvisioningProfiles[idCompanyName].ExpirationDate);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[idCompanyName].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("CompanyName Development Profile", index.ProvisioningProfiles[idCompanyName].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[idCompanyName].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[idCompanyName].Platforms[0]);
			Assert.AreEqual ("29cbf4b4-a170-4c74-a29a-64ecd55b102e", index.ProvisioningProfiles[idCompanyName].Uuid);

			Assert.AreEqual ("YHT9CR87YA.com.xamarin.*", index.ProvisioningProfiles[idXamarin].ApplicationIdentifier);
			Assert.AreEqual (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[idXamarin].CreationDate);
			Assert.AreEqual (1, index.ProvisioningProfiles[idXamarin].DeveloperCertificates.Count);
			Assert.AreEqual ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)", index.ProvisioningProfiles[idXamarin].DeveloperCertificates[0].Name);
			Assert.AreEqual ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718", index.ProvisioningProfiles[idXamarin].DeveloperCertificates[0].Thumbprint);
			Assert.AreEqual (MobileProvisionDistributionType.Development, index.ProvisioningProfiles[idXamarin].Distribution);
			Assert.AreEqual (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc), index.ProvisioningProfiles[idXamarin].ExpirationDate);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision", Path.GetFileName (index.ProvisioningProfiles[idXamarin].FileName));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.AreEqual ("Xamarin Development Profile", index.ProvisioningProfiles[idXamarin].Name);
			Assert.AreEqual (1, index.ProvisioningProfiles[idXamarin].Platforms.Count);
			Assert.AreEqual (MobileProvisionPlatform.iOS, index.ProvisioningProfiles[idXamarin].Platforms[0]);
			Assert.AreEqual ("7079f389-6ff4-4290-bf76-c8a222947616", index.ProvisioningProfiles[idXamarin].Uuid);
		}
	}
}
