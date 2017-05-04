// 
// IPhoneDeviceType.cs
//  
// Author: Michael Hutchinson <mhutch@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections.Generic;

namespace Xamarin.MacDev
{
	[Flags]
	public enum IPhoneDeviceType
	{
		NotSet        = 0,
		// Use different values than AppleDeviceFamily to catch incorrect usage earlier.
		IPhone        = 1 << 10,
		IPad          = 1 << 11,
		IPhoneAndIPad = IPhone | IPad,
		Watch         = 1 << 12,
		TV            = 1 << 13,
	}

	public enum AppleDeviceFamily
	{
		IPhone = 1,
		IPad   = 2,
		TV     = 3,
		Watch  = 4,
	}

	public static class AppleDeviceFamilyExtensions
	{
		public static string GetDeviceClass (this AppleDeviceFamily family)
		{
			switch (family) {
				case AppleDeviceFamily.TV: return "tvOS";
				case AppleDeviceFamily.Watch: return "watch";
				case AppleDeviceFamily.IPhone: return "iphone";
				case AppleDeviceFamily.IPad: return "ipad";
				case AppleDeviceFamily.IPod: return "ipod";
				default: return string.Empty;
			}
		}
	}

	public static class IPhoneDeviceTypeExtensions
	{
		public static IList<AppleDeviceFamily> ToDeviceFamily (this IPhoneDeviceType type)
		{
			var rv = new List<AppleDeviceFamily> ();

			if ((type & IPhoneDeviceType.IPhone) == IPhoneDeviceType.IPhone)
				rv.Add (AppleDeviceFamily.IPhone);

			if ((type & IPhoneDeviceType.IPad) == IPhoneDeviceType.IPad)
				rv.Add (AppleDeviceFamily.IPad);

			if ((type & IPhoneDeviceType.TV) == IPhoneDeviceType.TV)
				rv.Add (AppleDeviceFamily.TV);

			if ((type & IPhoneDeviceType.Watch) == IPhoneDeviceType.Watch)
				rv.Add (AppleDeviceFamily.Watch);

			return rv;
		}

		public static IPhoneDeviceType ToDeviceType (this IEnumerable<AppleDeviceFamily> families)
		{
			var rv = IPhoneDeviceType.NotSet;

			foreach (var family in families)
				rv |= ToDeviceType (family);

			return rv;
		}

		public static IPhoneDeviceType ToDeviceType (this AppleDeviceFamily family)
		{
			switch (family) {
			case AppleDeviceFamily.IPhone:
				return IPhoneDeviceType.IPhone;
			case AppleDeviceFamily.IPad:
				return IPhoneDeviceType.IPad;
			case AppleDeviceFamily.TV:
				return IPhoneDeviceType.TV;
			case AppleDeviceFamily.Watch:
				return IPhoneDeviceType.Watch;
			default:
				throw new ArgumentOutOfRangeException (string.Format ("Unknown device family: {0}", family));
			}
		}
	}
}
