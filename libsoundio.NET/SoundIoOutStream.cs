using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	public unsafe class SoundIoOutStream : IDisposable
	{
		unsafe private Internal* handle; // opaque handle

		public delegate void WriteDelegate(SoundIoOutStream outstream, int frame_count_min, int frame_count_max);
		private WriteDelegate writeDelegate;

		public delegate void UnderflowDelegate(SoundIoOutStream outstream);
		private UnderflowDelegate underflowDelegate;

		public delegate void ErrorDelegate(SoundIoOutStream outstream, int err);
		private ErrorDelegate errorDelegate;

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
		internal struct Internal
		{
			public IntPtr device;
			public SoundIoFormat format;
			public int sample_rate;
			public SoundIoChannelLayout layout;
			public double software_latency;
			public IntPtr userdata;
			public IntPtr write_callback;
			public IntPtr underflow_callback;
			public IntPtr error_callback;
			public IntPtr name;
			public byte non_terminal_hint;
			public int bytes_per_frame;
			public int bytes_per_sample;
			public int layout_error;
		}

		public WriteDelegate writeCallback
		{
			get
			{
				return writeDelegate;
			}
			set
			{
				writeDelegate = value;
				if (writeDelegate != null)
					handle->write_callback = Marshal.GetFunctionPointerForDelegate(writeCallbackWrapped);
				else
					handle->write_callback = IntPtr.Zero;
			}
		}

		public UnderflowDelegate underflowCallback
		{
			get
			{
				return underflowDelegate;
			}
			set
			{
				underflowDelegate = value;
				if (underflowDelegate != null)
					handle->underflow_callback = Marshal.GetFunctionPointerForDelegate(underflowCallbackWrapped);
				else
					handle->underflow_callback = IntPtr.Zero;
			}
		}

		public ErrorDelegate errorCallback
		{
			get
			{
				return errorDelegate;
			}
			set
			{
				errorDelegate = value;
				if (errorDelegate != null)
					handle->error_callback = Marshal.GetFunctionPointerForDelegate(errorCallbackWrapped);
				else
					handle->error_callback = IntPtr.Zero;
			}
		}

		public string Name
		{
			get { return (string)UTF8StringMarshaler.GetInstance("").MarshalNativeToManaged(handle->name); }
			set { handle->name = UTF8StringMarshaler.GetInstance("").MarshalManagedToNative(value); }
		}

		public double SoftwareLatency
		{
			get { return handle->software_latency; }
			set { handle->software_latency = value; }
		}

		public int SampleRate
		{
			get { return handle->sample_rate; }
			set { handle->sample_rate = value; }
		}

		public SoundIoFormat Format
		{
			get { return handle->format; }
			set { handle->format = value; }
		}

		public SoundIoChannelLayout Layout
		{
			get { return handle->layout; }
			set { handle->layout = value; }
		}

		internal SoundIoOutStream(Internal* handle)
		{
			this.handle = handle;

			if (handle != null)
				outstreamInstances.Add(this);
		}

		public void Dispose()
		{
			if (handle == null)
				return;

			outstreamInstances.Remove(this);
			soundio_outstream_destroy(handle);
			handle = null;
		}

		public void Open()
		{
			SoundIoError error = soundio_outstream_open(handle);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);

			if ((SoundIoError)handle->layout_error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException((SoundIoError)handle->layout_error);
		}

		public void Start()
		{
			SoundIoError error = soundio_outstream_start(handle);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);
		}

		public void Pause(bool pause)
		{
			SoundIoError error = soundio_outstream_pause(handle, pause);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);
		}

		public void ClearBuffer()
		{
			SoundIoError error = soundio_outstream_clear_buffer(handle);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);
		}

		public void BeginWrite(out SoundIoChannelArea[] areas, ref int frame_count)
		{
			SoundIoChannelArea* ptr;
			SoundIoError error = soundio_outstream_begin_write(handle, out ptr, ref frame_count);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);

			int length = handle->layout.ChannelCount;
			areas = new SoundIoChannelArea[length];

			for (int i = 0; i < length; i++)
			{
				SoundIoChannelArea p = ptr[i];
				areas[i] = p;
			}
		}

		public void EndWrite()
		{
			SoundIoError error = soundio_outstream_end_write(handle);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);
		}

		// hold the delegates here to prevent GC releasing them prematurely
		static internal HashSet<SoundIoOutStream> outstreamInstances = new HashSet<SoundIoOutStream>();
		static unsafe internal write_callback_t writeCallbackWrapped = WriteCallbackWrapped;
		static unsafe internal underflow_callback_t underflowCallbackWrapped = UnderflowCallbackWrapped;
		static unsafe internal error_callback_t errorCallbackWrapped = ErrorCallbackWrapped;

		static internal void WriteCallbackWrapped(Internal* outstream, int frame_count_min, int frame_count_max)
		{
			SoundIoOutStream outstreamManaged = null;

			foreach (SoundIoOutStream instance in outstreamInstances)
			{
				if (instance.handle != outstream)
					continue;

				outstreamManaged = instance;
			}

			if (outstreamManaged != null)
				outstreamManaged.writeDelegate(outstreamManaged, frame_count_min, frame_count_max);
		}

		static internal void UnderflowCallbackWrapped(Internal* outstream)
		{
			SoundIoOutStream outstreamManaged = null;

			foreach (SoundIoOutStream instance in outstreamInstances)
			{
				if (instance.handle != outstream)
					continue;

				outstreamManaged = instance;
			}

			if (outstreamManaged != null)
				outstreamManaged.underflowDelegate(outstreamManaged);
		}

		static internal void ErrorCallbackWrapped(Internal* outstream, int err)
		{
			SoundIoOutStream outstreamManaged = null;

			foreach (SoundIoOutStream instance in outstreamInstances)
			{
				if (instance.handle != outstream)
					continue;

				outstreamManaged = instance;
			}

			if (outstreamManaged != null)
				outstreamManaged.errorDelegate(outstreamManaged, err);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void write_callback_t(Internal* outstream, int frame_count_min, int frame_count_max);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void underflow_callback_t(Internal* outstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void error_callback_t(Internal* outstream, int err);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_outstream_destroy(Internal* outstream);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_open(Internal* outstream);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_start(Internal* outstream);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_pause(Internal* outstream, [MarshalAs(UnmanagedType.I1)] bool pause);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_clear_buffer(Internal* outstream);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_begin_write(Internal* outstream, out SoundIoChannelArea* areas, ref int frame_count);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_outstream_end_write(Internal* outstream);
	}
}
