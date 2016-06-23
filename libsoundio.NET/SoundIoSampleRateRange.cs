using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
	public unsafe struct SoundIoSampleRateRange
	{
		public int Min;
		public int Max;
	}
}
