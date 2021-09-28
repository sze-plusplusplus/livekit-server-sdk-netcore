using Grpc.Net.Client;
using Livekit.Client.Proto;

namespace Livekit.Client
{
    public class RecordingClient
    {
        private GrpcChannel Channel;
        private RecordingService.RecordingServiceClient ServiceClient;
        private AccessToken Token;

        /// <summary>
        /// Create a new GRPC channel, create a RecordingServiceClient to be reused by the class functions
        /// </summary>
        /// <param name="address">Livekit server address (https://my.host)</param>
        /// <param name="apiKey">Server API key</param>
        /// <param name="apiSecret">Server API secret</param>
        public RecordingClient(string address, string apiKey, string apiSecret, GrpcChannel channel = null)
        {
            Channel = channel ?? GrpcChannel.ForAddress(address);
            ServiceClient = new RecordingService.RecordingServiceClient(Channel);
            Token = new AccessToken(apiKey, apiSecret, new Grant()
            {
                video = new VideoGrant()
                {
                    roomRecord = true,
                }
            });
        }

        /// <summary>
        /// Start a recording session (recording service is required)
        /// </summary>
        /// <param name="inputUrl">Record a web stream</param>
        /// <param name="inputTemplate">Record a room</param>
        /// <param name="outputFile">Output to local file</param>
        /// <param name="outputS3">Output to S3 bucket</param>
        /// <param name="outputRTMP">Output to RTMP server</param>
        /// <param name="options">Record options (Preset/Resolution/Bitrate)</param>
        /// <returns>The ID of the started recording (to be able to stop it)</returns>
        public string StartRecording(RecordingTemplate inputTemplate = null, string inputUrl = null, string outputFile = null, string outputS3 = null, string outputRTMP = null, RecordingOptions options = null)
        {
            var req = new StartRecordingRequest()
            {
                Input = new RecordingInput()
                {
                    Template = inputTemplate,
                    Url = inputUrl
                },
                Output = new RecordingOutput()
                {
                    Rtmp = outputRTMP,
                    S3Path = outputS3
                },
                Options = options
            };
            return ServiceClient.StartRecording(req, Token.GetAsHeader()).RecordingId;
        }

        /// <summary>
        /// End the recording in a given room
        /// </summary>
        /// <param name="id">Recording ID</param>
        public void EndRecording(string id)
        {
            var req = new EndRecordingRequest()
            {
                RecordingId = id
            };
            ServiceClient.EndRecording(req, Token.GetAsHeader());
        }

        ~RecordingClient()
        {
            Channel.Dispose();
        }
    }
}