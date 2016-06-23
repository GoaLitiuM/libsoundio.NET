using System;
using libsoundio;

namespace sio_list_devices
{
	class Program
	{
		public static bool shortOutput = false;

		static void PrintUsage(string exe)
		{
			Console.Write("Usage: " + exe + " [options]\n" +
					"Options:\n" + 
					"  [--watch]\n" +
					"  [--backend dummy|alsa|pulseaudio|jack|coreaudio|wasapi]\n" + 
					"  [--short]\n");
		}

		static void Main(string[] args)
		{
			bool watch = false;
			SoundIoBackend backend = SoundIoBackend.SoundIoBackendNone;

			for (int i = 1; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg == "--watch")
					watch = true;
				else if (arg == "--short")
					shortOutput = true;
				else if (arg == "--backend" && i+1 < args.Length)
				{
					string param = args[++i];
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

			if (watch)
			{
				soundio.onDevicesChange = OnDevicesChange;
				while (true)
					soundio.WaitEvents();
			}
			else
			{
				soundio.FlushEvents();
				ListDevices(soundio);
			}

			soundio.Dispose();

			Console.ReadKey();
		}

		static void OnDevicesChange(SoundIo soundio)
		{
			Console.WriteLine("devices changed");
			ListDevices(soundio);
		}

		static void ListDevices(SoundIo soundio)
		{
			SoundIoDevice default_output = soundio.DefaultOutputDevice();
			SoundIoDevice default_input = soundio.DefaultInputDevice();

			Console.Write("--------Input Devices--------\n\n");

			foreach (SoundIoDevice device in soundio.inputDevices)
				PrintDevice(device, default_input == device);

			Console.Write("\n--------Output Devices--------\n\n");
			foreach (SoundIoDevice device in soundio.outputDevices)
				PrintDevice(device, default_input == device);

			Console.Write("\n" + (soundio.inputDevices.Count + soundio.outputDevices.Count).ToString() + " devices found\n");
		}

		static void PrintDevice(SoundIoDevice device, bool is_default)
		{
			Console.Write(device.Name);
			if (is_default)
				Console.Write(" (default)");

			if (device.IsRaw)
				Console.Write(" (raw)");
			Console.Write("\n");

			if (shortOutput)
				return;

			Console.WriteLine("  id: " + device.Id);

			if (!device.Probed)
				Console.WriteLine("  probe error: " + device.ProbeError.GetMessage());
			else
			{
				Console.Write("  channel layouts:\n");
				for (int i = 0; i < device.LayoutCount; i += 1)
				{
					Console.Write("    ");
					PrintChannelLayout(device.Layouts[i]);
					Console.Write("\n");
				}
				if (device.CurrentLayout.ChannelCount > 0)
				{
					Console.Write("  current layout: ");
					PrintChannelLayout(device.CurrentLayout);
					Console.Write("\n");
				}

				Console.Write("  sample rates:\n");
				for (int i = 0; i < device.SampleRateCount; i += 1)
					Console.WriteLine("    " + device.SampleRates[i].Min + " - " + device.SampleRates[i].Max);

				if (device.CurrentSampleRate != 0)
					Console.WriteLine("  current sample rate: " + device.CurrentSampleRate);

				Console.Write("  formats: ");
				for (int i = 0; i < device.FormatCount; i += 1)
					Console.Write(device.Formats[i].GetName() + ((i == device.FormatCount - 1) ? "" : ", "));

				Console.Write("\n");
				if (device.CurrentFormat != SoundIoFormat.SoundIoFormatInvalid)
					Console.WriteLine("  current format: " + device.CurrentFormat.GetName());

				Console.Write("  min software latency: " + device.SoftwareLatencyMin.ToString("F8") + " sec\n");
				Console.Write("  max software latency: " + device.SoftwareLatencyMax.ToString("F8") + " sec\n");
				if (device.SoftwareLatencyCurrent != 0.0)
					Console.Write("  current software latency: " + device.SoftwareLatencyCurrent.ToString("F8") + " sec\n");
				
			}
			Console.Write("\n");
		}

		static void PrintChannelLayout(SoundIoChannelLayout layout)
		{
			/*if (!string.IsNullOrEmpty(layout.Name)) 
				Console.Write(layout.Name);
			else*/
			{
				SoundIoChannelId[] channels = layout.Channels;
				layout.Channels[0] = SoundIoChannelId.SoundIoChannelIdAux;
				Console.Write(channels[0].GetName());
				for (int i = 1; i < layout.ChannelCount; i += 1)
					Console.Write(", " + channels[i].GetName());
			}
		}
	}
}
