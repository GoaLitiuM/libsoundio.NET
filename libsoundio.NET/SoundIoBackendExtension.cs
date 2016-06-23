using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public static class SoundIoBackendExtension
	{
		public static string GetName(this SoundIoBackend backend)
		{
			return soundio_backend_name(backend);
		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall, CharSet = libsoundio.importCharSet)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
		internal static extern string soundio_backend_name(SoundIoBackend backend);
	}
}
