using System.Collections.Generic;

namespace Livekit.Client
{
    public class Grant
    {
        public string identity { get; set; }
        public VideoGrant video { get; set; }
        public string metadata { get; set; }

        public IDictionary<string, object> ToDictionary()
        {
            var dir = new Dictionary<string, object>();
            dir.Add("identity", identity);
            dir.Add("video", video);
            dir.Add("metadata", metadata);
            return dir;
        }
    }
}