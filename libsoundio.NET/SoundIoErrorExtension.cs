using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public static class SoundIoErrorExtension
	{
		public static string GetMessage(this SoundIoError error)
		{
			return new SoundIoException(error).Message;
		}
	}
}
