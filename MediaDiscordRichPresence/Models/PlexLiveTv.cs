using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaDiscordRichPresence.Models;
public class PlexLiveTv
{
    public class DvrContainer
    {

        public class Rootobject
        {
            public Mediacontainer MediaContainer { get; set; }
        }

        public class Mediacontainer
        {
            public int size { get; set; }
            public Dvr[] Dvr { get; set; }
        }

        public class Dvr
        {
            public string key { get; set; }
            public string uuid { get; set; }
            public string language { get; set; }
            public string lineupTitle { get; set; }
            public string lineup { get; set; }
            public string country { get; set; }
            public int refreshedAt { get; set; }
            public string epgIdentifier { get; set; }
            public Device[] Device { get; set; }
            public Lineup[] Lineup { get; set; }
            public Setting1[] Setting { get; set; }
        }

        public class Device
        {
            public int parentID { get; set; }
            public string key { get; set; }
            public string uuid { get; set; }
            public string uri { get; set; }
            public string protocol { get; set; }
            public string status { get; set; }
            public string state { get; set; }
            public int lastSeenAt { get; set; }
            public string canTranscode { get; set; }
            public string deviceAuth { get; set; }
            public string deviceId { get; set; }
            public string make { get; set; }
            public string model { get; set; }
            public string modelNumber { get; set; }
            public string source { get; set; }
            public string sources { get; set; }
            public string thumb { get; set; }
            public string title { get; set; }
            public string tuners { get; set; }
            public Channelmapping[] ChannelMapping { get; set; }
            public Setting[] Setting { get; set; }
        }

        public class Channelmapping
        {
            public string channelKey { get; set; }
            public string deviceIdentifier { get; set; }
            public string enabled { get; set; }
            public string lineupIdentifier { get; set; }
        }

        public class Setting
        {
            public string id { get; set; }
            public string label { get; set; }
            public string summary { get; set; }
            public string type { get; set; }
            public string _default { get; set; }
            public string value { get; set; }
            public bool hidden { get; set; }
            public bool advanced { get; set; }
            public string group { get; set; }
            public string enumValues { get; set; }
        }

        public class Lineup
        {
            public string id { get; set; }
            public string title { get; set; }
        }

        public class Setting1
        {
            public string id { get; set; }
            public string label { get; set; }
            public string summary { get; set; }
            public string type { get; set; }
            public string _default { get; set; }
            public string value { get; set; }
            public bool hidden { get; set; }
            public bool advanced { get; set; }
            public string group { get; set; }
            public string enumValues { get; set; }
        }

    }

    public class XmlTvGridContainer
    {

        public class Rootobject
        {
            public Mediacontainer MediaContainer { get; set; }
        }

        public class Mediacontainer
        {
            public int size { get; set; }
            public Metadata[] Metadata { get; set; }
        }

        public class Metadata
        {
            public string ratingKey { get; set; }
            public string key { get; set; }
            public string guid { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public string summary { get; set; }
            public int duration { get; set; }
            public int addedAt { get; set; }
            public Medium[] Media { get; set; }
            public Channel[] Channel { get; set; }
            public bool onAir { get; set; }
            public string titleSort { get; set; }
        }

        public class Medium
        {
            public int id { get; set; }
            public int duration { get; set; }
            public int audioChannels { get; set; }
            public string videoResolution { get; set; }
            public string channelCallSign { get; set; }
            public string channelIdentifier { get; set; }
            public string channelThumb { get; set; }
            public string channelTitle { get; set; }
            public string channelVcn { get; set; }
            public string protocol { get; set; }
            public int beginsAt { get; set; }
            public int endsAt { get; set; }
            public int channelID { get; set; }
            public bool onAir { get; set; }
        }

        public class Channel
        {
            public int id { get; set; }
            public string filter { get; set; }
            public string tag { get; set; }
        }

    }
}
