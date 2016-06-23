using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace libsoundio
{
	public unsafe class SoundIo : IDisposable
	{
		unsafe internal Internal* handle;

		/// <summary> Enumerable list of input devices. Disposes devices after enumeration is completed. </summary>
		public SoundIoDeviceCollection inputDevices;

		/// <summary> Enumerable list of output devices. Disposes devices after enumeration is completed. </summary>
		public SoundIoDeviceCollection outputDevices;

		public delegate void OnDevicesChangeDelegate(SoundIo soundio);
		private OnDevicesChangeDelegate onDevicesChangeDelegate;

		public delegate void OnBackendDisconnectDelegate(SoundIo soundio, int error);
		private OnBackendDisconnectDelegate onBackendDisconnectDelegate;

		public delegate void OnEventsSignalDelegate();
		private OnEventsSignalDelegate onEventsSignalDelegate;

		/*public delegate void JackInfoCallback(string message);
		private JackInfoCallback jackInfoCallbackDelegate;

		public delegate void JackErrorCallback(string message);
		private JackErrorCallback jackErrorCallbackDelegate;*/

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
		internal struct Internal
		{
			public IntPtr userdata;
			public IntPtr on_devices_change;
			public IntPtr on_backend_disconnect;
			public IntPtr on_events_signal;
			public SoundIoBackend current_backend;
			public IntPtr app_name;
			public IntPtr emit_rtprio_warning;
			public IntPtr jack_info_callback;
			public IntPtr jack_error_callback;
		}

		public SoundIoBackend backend { get { return handle->current_backend; } }

		public OnDevicesChangeDelegate onDevicesChange
		{
			get
			{
				return onDevicesChangeDelegate;
			}
			set
			{
				onDevicesChangeDelegate = value;
				if (onDevicesChangeDelegate != null)
					handle->on_devices_change = Marshal.GetFunctionPointerForDelegate(onDevicesChangeCallbackWrapped);
				else
					handle->on_devices_change = IntPtr.Zero;
			}
		}

		public OnBackendDisconnectDelegate onBackendDisconnect
		{
			get
			{
				return onBackendDisconnectDelegate;
			}
			set
			{
				onBackendDisconnectDelegate = value;
				if (onBackendDisconnectDelegate != null)
					handle->on_backend_disconnect = Marshal.GetFunctionPointerForDelegate(onBackendDisconnectCallbackWrapped);
				else
					handle->on_backend_disconnect = IntPtr.Zero;
			}
		}

		public OnEventsSignalDelegate onEventsSignal
		{
			get
			{
				return onEventsSignalDelegate;
			}
			set
			{
				onEventsSignalDelegate = value;
				if (onEventsSignalDelegate != null)
					handle->on_events_signal = Marshal.GetFunctionPointerForDelegate(onEventsSignalDelegate);
				else
					handle->on_events_signal = IntPtr.Zero;
			}
		}

		public SoundIo()
		{
			handle = soundio_create();
			if (handle == null)
				throw new Exception("Failed to create SoundIO context, soundio_create returned invalid handle");

			inputDevices = new SoundIoInputDeviceCollection(this);
			outputDevices = new SoundIoOutputDeviceCollection(this);

			if (handle != null)
				soundioInstances.Add(this);
		}

		public void Dispose()
		{
			if (handle == null)
				return;

			soundioInstances.Remove(this);
			soundio_destroy(handle);
			handle = null;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (GetType() != obj.GetType())                
				return false;

			return this == (SoundIo)obj;
		}

		public override int GetHashCode()        
		{            
			return ((IntPtr)handle).GetHashCode();
		}

		static public bool operator ==(SoundIo left, SoundIo right)
		{
			return (ReferenceEquals(left, null) ? null : left.handle) == (ReferenceEquals(right, null) ? null : right.handle);
		}

		static public bool operator !=(SoundIo left, SoundIo right)
		{
			return !(left == right);
		}

		public void ConnectBackend(SoundIoBackend backend)
		{
			SoundIoError error = soundio_connect_backend(handle, backend);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);		
		}

		public void Connect()
		{
			SoundIoError error = soundio_connect(handle);
			if (error != SoundIoError.SoundIoErrorNone)
				throw new SoundIoException(error);
		}

		public void FlushEvents()
		{
			soundio_flush_events(handle);
		}

		public void WaitEvents()
		{
			soundio_wait_events(handle);
		}

		public void Wakeup()
		{
			soundio_wakeup(handle);
		}

		public SoundIoDevice DefaultInputDevice()
		{
			int index = soundio_default_input_device_index(handle);
			if (index < 0)
				return null;

			SoundIoDevice device = inputDevices[index];
			return device;
		}
		
		public SoundIoDevice DefaultOutputDevice()
		{
			int index = soundio_default_output_device_index(handle);
			if (index < 0)
				return null;

			SoundIoDevice device = outputDevices[index];
			return device;
		}
		
		

		

		

		

		

		// hold the delegates here to prevent GC releasing them prematurely
		static internal HashSet<SoundIo> soundioInstances = new HashSet<SoundIo>();
		static unsafe internal on_devices_change_t onDevicesChangeCallbackWrapped = OnDevicesChangeCallbackWrapped;
		static unsafe internal on_backend_disconnect_t onBackendDisconnectCallbackWrapped = OnBackendDisconnectCallbackWrapped;
		//static unsafe internal on_events_signal_t onEventsSignalCallbackWrapped = OnEventsSignalCallbackWrapped;

		static internal void OnDevicesChangeCallbackWrapped(Internal* soundio)
		{
			SoundIo soundioManaged = null;

			foreach (SoundIo instance in soundioInstances)
			{
				if (instance.handle != soundio)
					continue;

				soundioManaged = instance;
			}

			if (soundioManaged != null)
				soundioManaged.onDevicesChange(soundioManaged);
		}

		static internal void OnBackendDisconnectCallbackWrapped(Internal* soundio, int err)
		{
			SoundIo soundioManaged = null;

			foreach (SoundIo instance in soundioInstances)
			{
				if (instance.handle != soundio)
					continue;

				soundioManaged = instance;
			}

			if (soundioManaged != null)
				soundioManaged.onBackendDisconnect(soundioManaged, err);
		}

		/*static internal void OnEventsSignalCallbackWrapped()
		{
			SoundIo soundioManaged = null;

			foreach (SoundIo instance in soundioInstances)
			{
				if (instance.handle != soundio)
					continue;

				soundioManaged = instance;
			}

			if (soundioManaged != null)
				soundioManaged.onEventsSignal();
		}*/


		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void on_devices_change_t(Internal* soundio);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void on_backend_disconnect_t(Internal* soundio, int err);

		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//internal delegate void on_events_signal_t();
		

		

		/*
		/// Returns the number of available backends.
		SOUNDIO_EXPORT int soundio_backend_count(struct SoundIo *soundio);
		/// get the available backend at the specified index
		/// (0 <= index < ::soundio_backend_count)
		SOUNDIO_EXPORT enum SoundIoBackend soundio_get_backend(struct SoundIo *soundio, int index);

		/// Returns whether libsoundio was compiled with backend.
		SOUNDIO_EXPORT bool soundio_have_backend(enum SoundIoBackend backend);
		*/

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern Internal* soundio_create();

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_destroy(Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_connect(Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoError soundio_connect_backend(Internal* soundio, SoundIoBackend backend);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_flush_events(Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_wait_events(Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern void soundio_wakeup(Internal* soundio);


		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern int soundio_default_input_device_index(Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern int soundio_default_output_device_index(Internal* soundio);
	}

	internal unsafe class SoundIoInputDeviceCollection : SoundIoDeviceCollection
	{
		internal SoundIo soundio;

		public SoundIoInputDeviceCollection(SoundIo soundio)
		{
			this.soundio = soundio;
		}

		public override int Count { get { return soundio_input_device_count(soundio.handle); } }

		public override SoundIoDevice ElementAt(int index)
		{
			SoundIoDevice.Internal* device = soundio_get_input_device(soundio.handle, index);
			if (device == null)
				return null;

			return new SoundIoDevice(device);
		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern int soundio_input_device_count(SoundIo.Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoDevice.Internal* soundio_get_input_device(SoundIo.Internal* soundio, int index);
	}

	internal unsafe class SoundIoOutputDeviceCollection : SoundIoDeviceCollection
	{
		internal SoundIo soundio;

		public SoundIoOutputDeviceCollection(SoundIo soundio)
		{
			this.soundio = soundio;
		}

		public override int Count { get { return soundio_output_device_count(soundio.handle); } }

		public override SoundIoDevice ElementAt(int index)
		{
			SoundIoDevice.Internal* device = soundio_get_output_device(soundio.handle, index);
			if (device == null)
				return null;

			return new SoundIoDevice(device);
		}

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern int soundio_output_device_count(SoundIo.Internal* soundio);

		[DllImport(libsoundio.importLibrary, CallingConvention = libsoundio.importCall)]
		internal static extern SoundIoDevice.Internal* soundio_get_output_device(SoundIo.Internal* soundio, int index);
	}

	public unsafe class SoundIoDeviceCollection : IEnumerable<SoundIoDevice>
	{
		public virtual int Count { get; }
		public virtual SoundIoDevice ElementAt(int index) { return null; }

		/// <summary> Returns device at given index. Caller acquires the ownership of this device and is required to dispose it afterwards. </summary>
		public SoundIoDevice this[int index]
		{
			get
			{
				if (index < 0 || index > Count)
					throw new IndexOutOfRangeException();

				return ElementAt(index);
			}
		}

		/// <summary> Returns first device with matching id. Caller acquires the ownership of this device and is required to dispose it afterwards. </summary>
		public SoundIoDevice GetById(string id, bool raw)
		{
			foreach (SoundIoDevice device in this)
			{
				if (device.Id == id && device.IsRaw == raw)
					return device;

				device.Dispose();
			}
			return null;
		}

		public IEnumerator<SoundIoDevice> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				SoundIoDevice item = ElementAt(i);
				yield return item;
				//item.Dispose();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public enum SoundIoBackend
	{
		SoundIoBackendNone,
		SoundIoBackendJack,
		SoundIoBackendPulseAudio,
		SoundIoBackendAlsa,
		SoundIoBackendCoreAudio,
		SoundIoBackendWasapi,
		SoundIoBackendDummy,
	}

	public enum SoundIoFormat
	{
		SoundIoFormatInvalid,
		SoundIoFormatS8,        ///< Signed 8 bit
		SoundIoFormatU8,        ///< Unsigned 8 bit
		SoundIoFormatS16LE,     ///< Signed 16 bit Little Endian
		SoundIoFormatS16BE,     ///< Signed 16 bit Big Endian
		SoundIoFormatU16LE,     ///< Unsigned 16 bit Little Endian
		SoundIoFormatU16BE,     ///< Unsigned 16 bit Little Endian
		SoundIoFormatS24LE,     ///< Signed 24 bit Little Endian using low three bytes in 32-bit word
		SoundIoFormatS24BE,     ///< Signed 24 bit Big Endian using low three bytes in 32-bit word
		SoundIoFormatU24LE,     ///< Unsigned 24 bit Little Endian using low three bytes in 32-bit word
		SoundIoFormatU24BE,     ///< Unsigned 24 bit Big Endian using low three bytes in 32-bit word
		SoundIoFormatS32LE,     ///< Signed 32 bit Little Endian
		SoundIoFormatS32BE,     ///< Signed 32 bit Big Endian
		SoundIoFormatU32LE,     ///< Unsigned 32 bit Little Endian
		SoundIoFormatU32BE,     ///< Unsigned 32 bit Big Endian
		SoundIoFormatFloat32LE, ///< Float 32 bit Little Endian, Range -1.0 to 1.0
		SoundIoFormatFloat32BE, ///< Float 32 bit Big Endian, Range -1.0 to 1.0
		SoundIoFormatFloat64LE, ///< Float 64 bit Little Endian, Range -1.0 to 1.0
		SoundIoFormatFloat64BE, ///< Float 64 bit Big Endian, Range -1.0 to 1.0
	}

	public enum SoundIoError
	{
		SoundIoErrorNone,
		/// Out of memory.
		SoundIoErrorNoMem,
		/// The backend does not appear to be active or running.
		SoundIoErrorInitAudioBackend,
		/// A system resource other than memory was not available.
		SoundIoErrorSystemResources,
		/// Attempted to open a device and failed.
		SoundIoErrorOpeningDevice,
		SoundIoErrorNoSuchDevice,
		/// The programmer did not comply with the API.
		SoundIoErrorInvalid,
		/// libsoundio was compiled without support for that backend.
		SoundIoErrorBackendUnavailable,
		/// An open stream had an error that can only be recovered from by
		/// destroying the stream and creating it again.
		SoundIoErrorStreaming,
		/// Attempted to use a device with parameters it cannot support.
		SoundIoErrorIncompatibleDevice,
		/// When JACK returns `JackNoSuchClient`
		SoundIoErrorNoSuchClient,
		/// Attempted to use parameters that the backend cannot support.
		SoundIoErrorIncompatibleBackend,
		/// Backend server shutdown or became inactive.
		SoundIoErrorBackendDisconnected,
		SoundIoErrorInterrupted,
		/// Buffer underrun occurred.
		SoundIoErrorUnderflow,
		/// Unable to convert to or from UTF-8 to the native string format.
		SoundIoErrorEncodingString,
	}
}
