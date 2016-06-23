using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public class SoundIoException : Exception
	{
		public SoundIoException(SoundIoError error)
			: base(soundio_strerror((int)error))
		{

		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall, CharSet = libsoundio.importCharSet)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
		internal static extern string soundio_strerror(int error);
	}
}
