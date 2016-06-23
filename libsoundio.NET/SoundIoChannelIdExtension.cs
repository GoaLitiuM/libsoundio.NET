using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public static class SoundIoChannelIdExtension
	{
		public static string GetName(this SoundIoChannelId layout)
		{
			return soundio_get_channel_name(layout);
		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
		internal static extern string soundio_get_channel_name(SoundIoChannelId id);
	}
}
