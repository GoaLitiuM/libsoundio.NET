using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CppSharp;

namespace libsoundio.Gen
{
	class Program
	{
		static void Main(string[] args)
		{
			ConsoleDriver.Run(new libsoundioLibrary());
		}
	}
}
