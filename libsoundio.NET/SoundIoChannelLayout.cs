using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libsoundio
{
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
	public unsafe struct SoundIoChannelLayout
	{
		public const int SOUNDIO_MAX_CHANNELS = 24;

		internal Internal data;

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = libsoundio.importCharSet)]
		internal struct Internal
		{
			internal IntPtr name;
			public int channel_count;
			internal fixed int /*SoundIoChannelId*/ channels[SOUNDIO_MAX_CHANNELS];
			//[MarshalAs(UnmanagedType.ByValArray, SizeConst = SOUNDIO_MAX_CHANNELS)]
			//internal SoundIoChannelId[] channels;
		}


		public string Name
		{
			get { return (string)UTF8StringMarshaler.GetInstance("").MarshalNativeToManaged(data.name); }
		}

		public int ChannelCount { get { return data.channel_count; } }

		public SoundIoChannelId[] Channels
		{
			get
			{
				SoundIoChannelId[] array = new SoundIoChannelId[SOUNDIO_MAX_CHANNELS];
				fixed (int* c = data.channels)
					for (int i=0; i<SOUNDIO_MAX_CHANNELS; i++)
						array[i] = (SoundIoChannelId)c[i];
				return array;
			}
			set
			{
				int count = Math.Min(value.Length, SOUNDIO_MAX_CHANNELS);
				fixed (int* c = data.channels)
					for (int i=0; i<count; i++)
						c[i] = (int)value[i];
			}
		}
	}

	public enum SoundIoDeviceAim
	{
		SoundIoDeviceAimInput,  ///< capture / recording
		SoundIoDeviceAimOutput, ///< playback
	}

	public enum SoundIoChannelId
	{
		SoundIoChannelIdInvalid,

		SoundIoChannelIdFrontLeft, ///< First of the more commonly supported ids.
		SoundIoChannelIdFrontRight,
		SoundIoChannelIdFrontCenter,
		SoundIoChannelIdLfe,
		SoundIoChannelIdBackLeft,
		SoundIoChannelIdBackRight,
		SoundIoChannelIdFrontLeftCenter,
		SoundIoChannelIdFrontRightCenter,
		SoundIoChannelIdBackCenter,
		SoundIoChannelIdSideLeft,
		SoundIoChannelIdSideRight,
		SoundIoChannelIdTopCenter,
		SoundIoChannelIdTopFrontLeft,
		SoundIoChannelIdTopFrontCenter,
		SoundIoChannelIdTopFrontRight,
		SoundIoChannelIdTopBackLeft,
		SoundIoChannelIdTopBackCenter,
		SoundIoChannelIdTopBackRight, ///< Last of the more commonly supported ids.

		SoundIoChannelIdBackLeftCenter, ///< First of the less commonly supported ids.
		SoundIoChannelIdBackRightCenter,
		SoundIoChannelIdFrontLeftWide,
		SoundIoChannelIdFrontRightWide,
		SoundIoChannelIdFrontLeftHigh,
		SoundIoChannelIdFrontCenterHigh,
		SoundIoChannelIdFrontRightHigh,
		SoundIoChannelIdTopFrontLeftCenter,
		SoundIoChannelIdTopFrontRightCenter,
		SoundIoChannelIdTopSideLeft,
		SoundIoChannelIdTopSideRight,
		SoundIoChannelIdLeftLfe,
		SoundIoChannelIdRightLfe,
		SoundIoChannelIdLfe2,
		SoundIoChannelIdBottomCenter,
		SoundIoChannelIdBottomLeftCenter,
		SoundIoChannelIdBottomRightCenter,

		/// Mid/side recording
		SoundIoChannelIdMsMid,
		SoundIoChannelIdMsSide,

		/// first order ambisonic channels
		SoundIoChannelIdAmbisonicW,
		SoundIoChannelIdAmbisonicX,
		SoundIoChannelIdAmbisonicY,
		SoundIoChannelIdAmbisonicZ,

		/// X-Y Recording
		SoundIoChannelIdXyX,
		SoundIoChannelIdXyY,

		SoundIoChannelIdHeadphonesLeft, ///< First of the "other" channel ids
		SoundIoChannelIdHeadphonesRight,
		SoundIoChannelIdClickTrack,
		SoundIoChannelIdForeignLanguage,
		SoundIoChannelIdHearingImpaired,
		SoundIoChannelIdNarration,
		SoundIoChannelIdHaptic,
		SoundIoChannelIdDialogCentricMix, ///< Last of the "other" channel ids

		SoundIoChannelIdAux,
		SoundIoChannelIdAux0,
		SoundIoChannelIdAux1,
		SoundIoChannelIdAux2,
		SoundIoChannelIdAux3,
		SoundIoChannelIdAux4,
		SoundIoChannelIdAux5,
		SoundIoChannelIdAux6,
		SoundIoChannelIdAux7,
		SoundIoChannelIdAux8,
		SoundIoChannelIdAux9,
		SoundIoChannelIdAux10,
		SoundIoChannelIdAux11,
		SoundIoChannelIdAux12,
		SoundIoChannelIdAux13,
		SoundIoChannelIdAux14,
		SoundIoChannelIdAux15,
	};
	
	public enum SoundIoChannelLayoutId
	{
		SoundIoChannelLayoutIdMono,
		SoundIoChannelLayoutIdStereo,
		SoundIoChannelLayoutId2Point1,
		SoundIoChannelLayoutId3Point0,
		SoundIoChannelLayoutId3Point0Back,
		SoundIoChannelLayoutId3Point1,
		SoundIoChannelLayoutId4Point0,
		SoundIoChannelLayoutIdQuad,
		SoundIoChannelLayoutIdQuadSide,
		SoundIoChannelLayoutId4Point1,
		SoundIoChannelLayoutId5Point0Back,
		SoundIoChannelLayoutId5Point0Side,
		SoundIoChannelLayoutId5Point1,
		SoundIoChannelLayoutId5Point1Back,
		SoundIoChannelLayoutId6Point0Side,
		SoundIoChannelLayoutId6Point0Front,
		SoundIoChannelLayoutIdHexagonal,
		SoundIoChannelLayoutId6Point1,
		SoundIoChannelLayoutId6Point1Back,
		SoundIoChannelLayoutId6Point1Front,
		SoundIoChannelLayoutId7Point0,
		SoundIoChannelLayoutId7Point0Front,
		SoundIoChannelLayoutId7Point1,
		SoundIoChannelLayoutId7Point1Wide,
		SoundIoChannelLayoutId7Point1WideBack,
		SoundIoChannelLayoutIdOctagonal,
	}
}
