using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public unsafe class SoundIoDevice : IDisposable
	{
		unsafe internal Internal* handle;

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
		internal struct Internal
		{
			public IntPtr soundio;
			public IntPtr id;
			public IntPtr name;
			public SoundIoDeviceAim aim;
			public IntPtr layouts;
			public int layout_count;
			public SoundIoChannelLayout current_layout;
			public IntPtr formats;
			public int format_count;
			public SoundIoFormat current_format;
			public IntPtr sample_rates;
			public int sample_rate_count;
			public int sample_rate_current;
			public double software_latency_min;
			public double software_latency_max;
			public double software_latency_current;
			public byte /*bool*/ is_raw;
			public int ref_count;
			public SoundIoError probe_error;
		}

		public string Name
		{
			get { return (string)UTF8StringMarshaler.GetInstance("").MarshalNativeToManaged(handle->name); }
		}

		public string Id
		{
			get { return (string)UTF8StringMarshaler.GetInstance("").MarshalNativeToManaged(handle->id); }
		}

		public bool IsRaw { get { return handle->is_raw != 0; } }
		public bool Probed { get { return ProbeError == SoundIoError.SoundIoErrorNone; } }

		/// Probe status
		/// <remarks>
		/// In case of an error, information about formats, sample rates, and channel layouts
		/// might be missing. (ALSA)
		/// </remarks>
		public SoundIoError ProbeError
		{
			get { return handle->probe_error; }
		}

		public int LayoutCount { get { return handle->layout_count; } }

		public SoundIoChannelLayout CurrentLayout
		{
			get { return handle->current_layout; }
			set { handle->current_layout = value; }
		}

		public SoundIoChannelLayout[] Layouts
		{
			get
			{
				int count = LayoutCount;
				SoundIoChannelLayout[] layouts = new SoundIoChannelLayout[count];
				SoundIoChannelLayout* ptr = (SoundIoChannelLayout*)handle->layouts;
				for (int i = 0; i < count; i++)
					layouts[i] = ptr[i];
					
				return layouts;
			}
		}


		public int FormatCount { get { return handle->format_count; } }
		public SoundIoFormat CurrentFormat { get { return handle->current_format; } }

		public SoundIoFormat[] Formats
		{
			get
			{
				int count = FormatCount;
				SoundIoFormat[] formats = new SoundIoFormat[count];
				SoundIoFormat* ptr = (SoundIoFormat*)handle->formats;
				for (int i = 0; i < count; i++)
					formats[i] = ptr[i];
					
				return formats;
			}
		}


		public int SampleRateCount { get { return handle->sample_rate_count; } }
		public int CurrentSampleRate { get { return handle->sample_rate_current; } }

		public SoundIoSampleRateRange[] SampleRates
		{
			get
			{
				int count = SampleRateCount;
				SoundIoSampleRateRange[] sampleRates = new SoundIoSampleRateRange[count];
				SoundIoSampleRateRange* ptr = (SoundIoSampleRateRange*)handle->sample_rates;
				for (int i = 0; i < count; i++)
					sampleRates[i] = ptr[i];
					
				return sampleRates;
			}
		}

		public double SoftwareLatencyCurrent { get { return handle->software_latency_current; } }
		public double SoftwareLatencyMin { get { return handle->software_latency_min; } }
		public double SoftwareLatencyMax { get { return handle->software_latency_max; } }

		internal SoundIoDevice(Internal* handle)
		{
			this.handle = handle;
		}

		/// <summary> Copies the reference to the device, incrementing the internal reference counter. </summary>
		public SoundIoDevice(SoundIoDevice device)
		{
			handle = device.handle;
			if (handle != null)
				soundio_device_ref(handle);
		}

		~SoundIoDevice()
		{
			Dispose();
		}

		public SoundIoDevice Clone()
		{
			if (handle != null)
				soundio_device_ref(handle);
			return new SoundIoDevice(this);
		}

		public void Dispose()
		{
			if (handle == null)
				return;

			soundio_device_unref(handle);
			GC.SuppressFinalize(this);
			handle = null;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (GetType() != obj.GetType())                
				return false;

			return this == (SoundIoDevice)obj;
		}

		public override int GetHashCode()        
		{            
			return ((IntPtr)handle).GetHashCode();
		}

		static public bool operator ==(SoundIoDevice left, SoundIoDevice right)
		{
			return (ReferenceEquals(left, null) ? null : left.handle) == (ReferenceEquals(right, null) ? null : right.handle);
		}

		static public bool operator !=(SoundIoDevice left, SoundIoDevice right)
		{
			return !(left == right);
		}

		public SoundIoOutStream CreateOutstream()
		{
			SoundIoOutStream.Internal* outstream = soundio_outstream_create(handle);
			if (outstream == null)
				throw new Exception("Failed to create outstream, soundio_outstream_create returned invalid handle");
			return new SoundIoOutStream(outstream);
		}

		public bool SupportsFormat(SoundIoFormat format)
		{
			return soundio_device_supports_format(handle, format);
		}


		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_device_ref(Internal* device);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_device_unref(Internal* device);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoOutStream.Internal* soundio_outstream_create(Internal* device);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool soundio_device_supports_format(Internal* device, SoundIoFormat format);
	}
}
