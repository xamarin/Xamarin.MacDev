// 
// IPhoneSdkVersion.cs
//  
// Authors: Michael Hutchinson <mhutchinson@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2011-2013 Xamarin Inc. (http://www.xamarin.com)
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

namespace Xamarin.MacDev
{
	public struct IPhoneSdkVersion : IComparable<IPhoneSdkVersion>, IEquatable<IPhoneSdkVersion>
	{
		int[] version;
		
		public IPhoneSdkVersion (params int[] version)
		{
			if (version == null)
				throw new ArgumentNullException ();

			this.version = version;
		}
		
		public static IPhoneSdkVersion Parse (string s)
		{
			var vstr = s.Split ('.');
			var vint = new int[vstr.Length];

			for (int j = 0; j < vstr.Length; j++)
				vint[j] = int.Parse (vstr[j]);

			return new IPhoneSdkVersion (vint);
		}
		
		public static bool TryParse (string s, out IPhoneSdkVersion result)
		{
			result = new IPhoneSdkVersion ();

			if (s == null)
				return false;

			var vstr = s.Split ('.');
			var vint = new int[vstr.Length];

			for (int j = 0; j < vstr.Length; j++) {
				int component;
				if (!int.TryParse (vstr[j], out component))
					return false;

				vint[j] = component;
			}

			result.version = vint;
			return true;
		}
		
		public int[] Version { get { return version; } }
		
		public override string ToString ()
		{
			if (IsUseDefault)
				return "";

			var v = new string [version.Length];
			for (int i = 0; i < v.Length; i++)
				v[i] = version[i].ToString ();

			return string.Join (".", v);
		}
		
		public int CompareTo (IPhoneSdkVersion other)
		{
			var x = Version;
			var y = other.Version;
			if (ReferenceEquals (x, y))
				return 0;
			
			if (x == null)
				return -1;
			if (y == null)
				return 1;
			
			for (int i = 0; i < Math.Min (x.Length,y.Length); i++) {
				int res = x[i] - y[i];
				if (res != 0)
					return res;
			}
			return x.Length - y.Length;
		}
		
		public bool Equals (IPhoneSdkVersion other)
		{
			var x = Version;
			var y = other.Version;
			if (ReferenceEquals (x, y))
				return true;
			if (x == null || y == null || x.Length != y.Length)
				return false;
			for (int i = 0; i < x.Length; i++)
				if (x[i] != y[i])
					return false;
			return true;
		}
		
		public override bool Equals (object obj)
		{
			if (obj is IPhoneSdkVersion)
				return Equals ((IPhoneSdkVersion)obj);
			return false;
		}
		
		public override int GetHashCode ()
		{
			unchecked {
				var x = Version;
				int acc = 0;
				for (int i = 0; i < x.Length; i++)
					acc ^= x[i] << i;
				return acc;
			}
		}
		
		public static bool operator == (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.Equals (b);
		}
		
		public static bool operator != (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return !a.Equals (b);
		}
		
		public static bool operator < (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) < 0;
		}
		
		public static bool operator > (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) > 0;
		}
		
		public static bool operator <= (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) <= 0;
		}
		
		public static bool operator >= (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) >= 0;
		}
		
		public bool IsUseDefault {
			get { return version == null || version.Length == 0; }
		}

#if !WINDOWS
		public IPhoneSdkVersion ResolveIfDefault (AppleSdk sdk, bool sim)
		{
			return IsUseDefault ? GetDefault (sdk, sim) : this;
		}

		public static IPhoneSdkVersion GetDefault (AppleSdk sdk, bool sim)
		{
			var v = sdk.GetInstalledSdkVersions (sim);
			return v.Count > 0 ? v[v.Count - 1] : UseDefault;
		}
#endif

		public static readonly IPhoneSdkVersion UseDefault = new IPhoneSdkVersion (new int[0]);

		public static readonly IPhoneSdkVersion V1_0 = new IPhoneSdkVersion (1, 0);
		public static readonly IPhoneSdkVersion V2_0 = new IPhoneSdkVersion (2, 0);
		public static readonly IPhoneSdkVersion V2_1 = new IPhoneSdkVersion (2, 1);
		public static readonly IPhoneSdkVersion V2_2 = new IPhoneSdkVersion (2, 2);
		public static readonly IPhoneSdkVersion V3_0 = new IPhoneSdkVersion (3, 0);
		public static readonly IPhoneSdkVersion V3_1 = new IPhoneSdkVersion (3, 1);
		public static readonly IPhoneSdkVersion V3_2 = new IPhoneSdkVersion (3, 2);
		public static readonly IPhoneSdkVersion V3_99 = new IPhoneSdkVersion (3, 99);
		public static readonly IPhoneSdkVersion V4_0 = new IPhoneSdkVersion (4, 0);
		public static readonly IPhoneSdkVersion V4_1 = new IPhoneSdkVersion (4, 1);
		public static readonly IPhoneSdkVersion V4_2 = new IPhoneSdkVersion (4, 2);
		public static readonly IPhoneSdkVersion V4_3 = new IPhoneSdkVersion (4, 3);
		public static readonly IPhoneSdkVersion V5_0 = new IPhoneSdkVersion (5, 0);
		public static readonly IPhoneSdkVersion V5_1 = new IPhoneSdkVersion (5, 1);
		public static readonly IPhoneSdkVersion V5_1_1 = new IPhoneSdkVersion (5, 1, 1);
		public static readonly IPhoneSdkVersion V5_2 = new IPhoneSdkVersion (5, 2);
		public static readonly IPhoneSdkVersion V6_0 = new IPhoneSdkVersion (6, 0);
		public static readonly IPhoneSdkVersion V6_1 = new IPhoneSdkVersion (6, 1);
		public static readonly IPhoneSdkVersion V6_2 = new IPhoneSdkVersion (6, 2);
		public static readonly IPhoneSdkVersion V7_0 = new IPhoneSdkVersion (7, 0);
		public static readonly IPhoneSdkVersion V7_1 = new IPhoneSdkVersion (7, 1);
		public static readonly IPhoneSdkVersion V7_2_1 = new IPhoneSdkVersion (7, 2, 1);
		public static readonly IPhoneSdkVersion V8_0 = new IPhoneSdkVersion (8, 0);
		public static readonly IPhoneSdkVersion V8_1 = new IPhoneSdkVersion (8, 1);
		public static readonly IPhoneSdkVersion V8_2 = new IPhoneSdkVersion (8, 2);
		public static readonly IPhoneSdkVersion V8_3 = new IPhoneSdkVersion (8, 3);
		public static readonly IPhoneSdkVersion V8_4 = new IPhoneSdkVersion (8, 4);
		public static readonly IPhoneSdkVersion V9_0 = new IPhoneSdkVersion (9, 0);
		public static readonly IPhoneSdkVersion V9_1 = new IPhoneSdkVersion (9, 1);
		public static readonly IPhoneSdkVersion V9_2 = new IPhoneSdkVersion (9, 2);
		public static readonly IPhoneSdkVersion V9_3 = new IPhoneSdkVersion (9, 3);
		public static readonly IPhoneSdkVersion V10_0 = new IPhoneSdkVersion (10, 0);
	}
}
