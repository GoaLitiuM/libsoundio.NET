using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace libsoundio
{
	public static partial class libsoundio
	{
		public const string importLibrary = "soundio.dll";
		public const CallingConvention importCall = CallingConvention.Cdecl;
		public const CharSet importCharSet = CharSet.Ansi;
	}
}
