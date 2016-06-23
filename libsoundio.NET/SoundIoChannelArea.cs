using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
	public unsafe struct SoundIoChannelArea
	{
		public IntPtr ptr;
		public int step;
	}
}
