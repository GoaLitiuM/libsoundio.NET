using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public static class SoundIoFormatExtension
	{
		public static string GetName(this SoundIoFormat format)
		{
			return soundio_format_string(format);
		}
		
		// returns the correct endian variant of the given format for this environment
		public static SoundIoFormat InOSEndian(this SoundIoFormat format)
		{
			if (BitConverter.IsLittleEndian)
			{
				switch (format)
				{
					case SoundIoFormat.SoundIoFormatFloat32BE:
						return SoundIoFormat.SoundIoFormatFloat32LE;
					case SoundIoFormat.SoundIoFormatFloat64BE:
						return SoundIoFormat.SoundIoFormatFloat64LE;
					case SoundIoFormat.SoundIoFormatS16BE:
						return SoundIoFormat.SoundIoFormatS16LE;
					case SoundIoFormat.SoundIoFormatS24BE:
						return SoundIoFormat.SoundIoFormatS24LE;
					case SoundIoFormat.SoundIoFormatS32BE:
						return SoundIoFormat.SoundIoFormatS32LE;
					case SoundIoFormat.SoundIoFormatU16BE:
						return SoundIoFormat.SoundIoFormatU16LE;
					case SoundIoFormat.SoundIoFormatU24BE:
						return SoundIoFormat.SoundIoFormatU24LE;
					case SoundIoFormat.SoundIoFormatU32BE:
						return SoundIoFormat.SoundIoFormatU32LE;
					default:
						break;
				}
			}
			else
			{
				switch (format)
				{
					case SoundIoFormat.SoundIoFormatFloat32LE:
						return SoundIoFormat.SoundIoFormatFloat32BE;
					case SoundIoFormat.SoundIoFormatFloat64LE:
						return SoundIoFormat.SoundIoFormatFloat64BE;
					case SoundIoFormat.SoundIoFormatS16LE:
						return SoundIoFormat.SoundIoFormatS16BE;
					case SoundIoFormat.SoundIoFormatS24LE:
						return SoundIoFormat.SoundIoFormatS24BE;
					case SoundIoFormat.SoundIoFormatS32LE:
						return SoundIoFormat.SoundIoFormatS32BE;
					case SoundIoFormat.SoundIoFormatU16LE:
						return SoundIoFormat.SoundIoFormatU16BE;
					case SoundIoFormat.SoundIoFormatU24LE:
						return SoundIoFormat.SoundIoFormatU24BE;
					case SoundIoFormat.SoundIoFormatU32LE:
						return SoundIoFormat.SoundIoFormatU32BE;
					default:
						break;
				}
			}

			return format;
		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
		internal static extern string soundio_format_string(SoundIoFormat format);
	}
}
