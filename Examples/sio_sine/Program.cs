using System;
using libsoundio;

namespace sio_sine
{
	class Program
	{
		static void PrintUsage(string exe)
		{
			Console.Write("Usage: " + exe + " [options]\n" +
					"Options:\n" +
					"  [--backend dummy|alsa|pulseaudio|jack|coreaudio|wasapi]\n" +
					"  [--device id]\n" +
					"  [--raw]\n" +
					"  [--name stream_name]\n" +
					"  [--latency seconds]\n" +
					"  [--sample-rate hz]\n");
		}

		delegate void WriteSampleDelegate(IntPtr ptr, double sample);

		static void Main(string[] args)
		{
			WriteSampleDelegate WriteSample = (ptr, sample) => { };
			bool wantPause = false;
			bool raw = false;
			string deviceId = null;
			string streamName = null;
			double latency = 0.0;
			int sampleRate = 0;
			double secondsOffset = 0.0;
			SoundIoBackend backend = SoundIoBackend.SoundIoBackendNone;

			for (int i = 1; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg == "--raw")
					raw = true;
				else if (arg.StartsWith("--") && i+1 < args.Length)
				{
					string param = args[++i];

					if (arg == "--backend")
					{
						switch (param)
						{
							case "dummy":
								backend = SoundIoBackend.SoundIoBackendDummy;
								break;
							case "alsa":
								backend = SoundIoBackend.SoundIoBackendAlsa;
								break;
							case "pulseaudio":
								backend = SoundIoBackend.SoundIoBackendPulseAudio;
								break;
							case "jack":
								backend = SoundIoBackend.SoundIoBackendJack;
								break;
							case "coreaudio":
								backend = SoundIoBackend.SoundIoBackendCoreAudio;
								break;
							case "wasapi":
								backend = SoundIoBackend.SoundIoBackendWasapi;
								break;
							default:
								Console.WriteLine("Invalid backend: " + param);
								return;
						}
					}
					else if (arg == "--device")
						deviceId = param;
					else if (arg == "--name")
						streamName = param;
					else if (arg == "--latency")
						double.TryParse(param, out latency);
					else if (arg == "--sample-rate")
						int.TryParse(param, out sampleRate);
					else
					{
						PrintUsage(args[0]);
						return;
					}
				}
				else
				{
					PrintUsage(args[0]);
					return;
				}
			}
			
			SoundIo soundio = new SoundIo();

			if (backend == SoundIoBackend.SoundIoBackendNone)
				soundio.Connect();
			else
				soundio.ConnectBackend(backend);

			Console.WriteLine("Backend: " + soundio.backend.GetName());

			soundio.FlushEvents();

			SoundIoDevice device = null;
			if (!string.IsNullOrEmpty(deviceId))
				device = soundio.outputDevices.GetById(deviceId, raw);
			else
				device = soundio.DefaultOutputDevice();

			if (device == null)
			{
				Console.WriteLine("Output device not found");
				return;
			}

			Console.WriteLine("Output device: " + device.Name);
			if (!device.Probed)
			{
				Console.WriteLine("Cannot probe device: " + device.ProbeError.GetMessage());
				return;
			}

			SoundIoOutStream outstream = device.CreateOutstream();
			outstream.writeCallback = (os, frameCountMin, frameCountMax) =>
			{
				Console.WriteLine("callback");

				double secondsPerFrame = 1.0 / os.SampleRate;
				int framesLeft = frameCountMax;

				while (true)
				{
					int frameCount = framesLeft;
					SoundIoChannelArea[] areas;

					os.BeginWrite(out areas, ref frameCount);

					if (frameCount == 0)
						break;

					double pitch = 440.0;
					double radiansPerSecond = pitch * 2.0 * Math.PI;
					for (int frame = 0; frame < frameCount; frame++)
					{
						double sample = Math.Sin((secondsOffset + frame * secondsPerFrame) * radiansPerSecond);
						for (int channel = 0; channel < areas.Length; channel++)
						{
							WriteSample(areas[channel].ptr, sample);
							areas[channel].ptr = IntPtr.Add(areas[channel].ptr, areas[channel].step);
						}
					}
					secondsOffset += secondsPerFrame * frameCount;

					os.EndWrite();
					
					framesLeft -= frameCount;
					if (framesLeft <= 0)
						break;
				}

				outstream.Pause(wantPause);
			};

			outstream.underflowCallback = UnderflowCallback;
			outstream.Name = streamName;
			outstream.SoftwareLatency = latency;
			outstream.SampleRate = sampleRate;

			if (device.SupportsFormat(SoundIoFormat.SoundIoFormatFloat32LE.InOSEndian()))
			{
				outstream.Format = SoundIoFormat.SoundIoFormatFloat32LE.InOSEndian();
				WriteSample = WriteSampleFloat32;
			}
			else if (device.SupportsFormat(SoundIoFormat.SoundIoFormatFloat64LE.InOSEndian()))
			{
				outstream.Format = SoundIoFormat.SoundIoFormatFloat64LE.InOSEndian();
				WriteSample = WriteSampleFloat64;
			}
			else if (device.SupportsFormat(SoundIoFormat.SoundIoFormatS32LE.InOSEndian()))
			{
				outstream.Format = SoundIoFormat.SoundIoFormatS32LE.InOSEndian();
				WriteSample = WriteSampleS32;
			}
			else if (device.SupportsFormat(SoundIoFormat.SoundIoFormatS16LE.InOSEndian()))
			{
				outstream.Format = SoundIoFormat.SoundIoFormatS16LE.InOSEndian();
				WriteSample = WriteSampleS16;
			}
			else
			{
				Console.WriteLine("No suitable device format available.");
				return;
			}

			outstream.Open();

			Console.WriteLine("Software latency: ", outstream.SoftwareLatency.ToString());
			Console.Write(
					"'p\\n' - pause\n" + 
					"'u\\n' - unpause\n" +
					"'P\\n' - pause from within callback\n" +
					"'c\\n' - clear buffer\n" +
					"'q\\n' - quit\n");

			outstream.Start();

			while (true)
			{
				soundio.FlushEvents();

				var keyInfo = Console.ReadKey(true);
				int c = keyInfo.KeyChar;

				if (c == 'p')
				{
					Console.WriteLine("pausing...");
					outstream.Pause(true);
				}
				else if (c == 'P')
					wantPause = true;
				else if (c == 'u')
				{
					wantPause = false;
					Console.WriteLine("unpausing...");
					outstream.Pause(false);
				}
				else if (c == 'c')
				{
					Console.WriteLine("clear buffer...");
					outstream.ClearBuffer();
				}
				else if (c == 'q')
					break;
				else if (c != '\r' && c != '\n')
					Console.WriteLine("Unrecognized command: " + (char)c);
			}
			
			outstream.Dispose();
			device.Dispose();
			soundio.Dispose();
		}

		static int underflowCount = 0;
		static void UnderflowCallback(SoundIoOutStream outstream)
		{
			Console.WriteLine("underflow " + (underflowCount++).ToString());
		}

		static void WriteSampleS16(IntPtr ptr, double sample)
		{
			unsafe
			{
				short* buf = (short*)ptr;
				*buf = (short)(sample * ((double)short.MaxValue - short.MinValue) / 2.0);
			}
		}

		static void WriteSampleS32(IntPtr ptr, double sample)
		{
			unsafe
			{
				int* buf = (int*)ptr;
				*buf = (int)(sample * ((double)int.MaxValue - int.MinValue) / 2.0);
			}
		}

		static void WriteSampleFloat32(IntPtr ptr, double sample)
		{
			unsafe
			{
				float* buf = (float*)ptr;
				*buf = (float)sample;
			}
		}

		static void WriteSampleFloat64(IntPtr ptr, double sample)
		{
			unsafe
			{
				double* buf = (double*)ptr;
				*buf = sample;
			}
		}
	}
}
