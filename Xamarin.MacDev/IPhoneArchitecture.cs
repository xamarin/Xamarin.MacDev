//
// IPhoneArchitecture.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
//

using System;

namespace Xamarin.MacDev {
	[Flags]
	public enum IPhoneArchitecture {
		Default = 0,

		i386 = 1,
		x86_64 = 2,

		ARMv6 = 4,
		ARMv7 = 8,
		ARMv7s = 16,
		ARMv7k = 32,
		ARM64 = 64,
		ARM64_32 = 128,
	}
}
