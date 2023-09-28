using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDiscordRichPresence.EmbyModels;
public class EmbySessions
{

    public class Rootobject
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public Playstate PlayState { get; set; }
        public object[] AdditionalUsers { get; set; }
        public string RemoteEndPoint { get; set; }
        public string Protocol { get; set; }
        public string[] PlayableMediaTypes { get; set; }
        public int PlaylistIndex { get; set; }
        public int PlaylistLength { get; set; }
        public string Id { get; set; }
        public string ServerId { get; set; }
        public string Client { get; set; }
        public DateTime LastActivityDate { get; set; }
        public string DeviceName { get; set; }
        public int InternalDeviceId { get; set; }
        public string DeviceId { get; set; }
        public string ApplicationVersion { get; set; }
        public string[] SupportedCommands { get; set; }
        public bool SupportsRemoteControl { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Nowplayingitem NowPlayingItem { get; set; }
        public string AppIconUrl { get; set; }
        public Transcodinginfo TranscodingInfo { get; set; }
    }

    public class Playstate
    {
        public bool CanSeek { get; set; }
        public bool IsPaused { get; set; }
        public bool IsMuted { get; set; }
        public string RepeatMode { get; set; }
        public int SubtitleOffset { get; set; }
        public int PlaybackRate { get; set; }
        public long PositionTicks { get; set; }
        public int VolumeLevel { get; set; }
        public int AudioStreamIndex { get; set; }
        public int SubtitleStreamIndex { get; set; }
        public string MediaSourceId { get; set; }
        public string PlayMethod { get; set; }
    }

    public class Nowplayingitem
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string PresentationUniqueKey { get; set; }
        public object[] ExternalUrls { get; set; }
        public object[] Taglines { get; set; }
        public int Bitrate { get; set; }
        public string Number { get; set; }
        public string ChannelNumber { get; set; }
        public Providerids ProviderIds { get; set; }
        public bool IsFolder { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public float PrimaryImageAspectRatio { get; set; }
        public Mediastream[] MediaStreams { get; set; }
        public Imagetags ImageTags { get; set; }
        public object[] BackdropImageTags { get; set; }
        public string MediaType { get; set; }
        public Currentprogram CurrentProgram { get; set; }
    }

    public class Providerids
    {
    }

    public class Imagetags
    {
        public string Primary { get; set; }
    }

    public class Currentprogram
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string PresentationUniqueKey { get; set; }
        public object[] ExternalUrls { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string Overview { get; set; }
        public object[] Taglines { get; set; }
        public object[] Genres { get; set; }
        public long RunTimeTicks { get; set; }
        public string ChannelNumber { get; set; }
        public Providerids1 ProviderIds { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public object[] GenreItems { get; set; }
        public Imagetags1 ImageTags { get; set; }
        public object[] BackdropImageTags { get; set; }
        public string MediaType { get; set; }
        public DateTime EndDate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ChannelPrimaryImageTag { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsSeries { get; set; }
    }

    public class Providerids1
    {
    }

    public class Imagetags1
    {
    }

    public class Mediastream
    {
        public string Codec { get; set; }
        public string ColorTransfer { get; set; }
        public string ColorPrimaries { get; set; }
        public string ColorSpace { get; set; }
        public long StreamStartTimeTicks { get; set; }
        public string TimeBase { get; set; }
        public string VideoRange { get; set; }
        public string DisplayTitle { get; set; }
        public string NalLengthSize { get; set; }
        public bool IsInterlaced { get; set; }
        public int BitRate { get; set; }
        public int BitDepth { get; set; }
        public int RefFrames { get; set; }
        public bool IsDefault { get; set; }
        public bool IsForced { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int AverageFrameRate { get; set; }
        public int RealFrameRate { get; set; }
        public string Profile { get; set; }
        public string Type { get; set; }
        public string AspectRatio { get; set; }
        public int Index { get; set; }
        public bool IsExternal { get; set; }
        public bool IsTextSubtitleStream { get; set; }
        public bool SupportsExternalStream { get; set; }
        public string Protocol { get; set; }
        public string PixelFormat { get; set; }
        public int Level { get; set; }
        public bool IsAnamorphic { get; set; }
        public string Language { get; set; }
        public string DisplayLanguage { get; set; }
        public string ChannelLayout { get; set; }
        public int Channels { get; set; }
        public int SampleRate { get; set; }
        public string Extradata { get; set; }
        public string SubtitleLocationType { get; set; }
    }

    public class Transcodinginfo
    {
        public string AudioCodec { get; set; }
        public string VideoCodec { get; set; }
        public string SubProtocol { get; set; }
        public string Container { get; set; }
        public bool IsVideoDirect { get; set; }
        public bool IsAudioDirect { get; set; }
        public int Bitrate { get; set; }
        public int AudioBitrate { get; set; }
        public int VideoBitrate { get; set; }
        public int Framerate { get; set; }
        public long TranscodingPositionTicks { get; set; }
        public int TranscodingStartPositionTicks { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int AudioChannels { get; set; }
        public string[] TranscodeReasons { get; set; }
        public float CurrentCpuUsage { get; set; }
        public float AverageCpuUsage { get; set; }
        public Cpuhistory[] CpuHistory { get; set; }
        public string VideoDecoder { get; set; }
        public bool VideoDecoderIsHardware { get; set; }
        public string VideoDecoderMediaType { get; set; }
        public string VideoDecoderHwAccel { get; set; }
        public string VideoEncoder { get; set; }
        public bool VideoEncoderIsHardware { get; set; }
        public string VideoEncoderMediaType { get; set; }
        public string VideoEncoderHwAccel { get; set; }
        public Videopipelineinfo[] VideoPipelineInfo { get; set; }
        public object[] SubtitlePipelineInfos { get; set; }
    }

    public class Cpuhistory
    {
        public float Item1 { get; set; }
        public float Item2 { get; set; }
    }

    public class Videopipelineinfo
    {
        public string HardwareContextName { get; set; }
        public bool IsHardwareContext { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
        public string StepType { get; set; }
        public string StepTypeName { get; set; }
        public string FfmpegName { get; set; }
        public string FfmpegDescription { get; set; }
        public string FfmpegOptions { get; set; }
        public string Param { get; set; }
        public string ParamShort { get; set; }
    }


}
