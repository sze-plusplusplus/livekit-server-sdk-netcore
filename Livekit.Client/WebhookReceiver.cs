using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Livekit.Client.Proto;

namespace Livekit.Client
{
    public struct WebhookEvents
    {
        public const string ROOM_STARTED = "room_started";
        public const string ROOM_FINISHED = "room_finished";
        public const string PARTICIPANT_JOINED = "participant_joined";
        public const string PARTICIPANT_LEFT = "participant_left";
        public const string RECORDING_FINISHED = "recording_finished";
    }

    public class WebhookReceiver
    {
        /// <summary>
        /// The header which contains the auth token for the webhook requests
        /// </summary>
        public const string AuthHeader = "Authorization";

        private AccessToken Token;

        /// <summary>
        /// WebhookReceiver
        /// </summary>
        /// <param name="apiKey">Server API key</param>
        /// <param name="apiSecret">Server API secret</param>
        public WebhookReceiver(string apiKey, string apiSecret)
        {
            Token = new AccessToken(apiKey, apiSecret, new Grant());
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="authHeader">Auth header value</param>
        /// <param name="skipAuth">Skip the token and checksum validation</param>
        /// <returns>Deserialized webhook event</returns>
        public WebhookEvent Receive(string body, string authHeader, bool skipAuth = false)
        {
            if (!skipAuth)
            {
                if (authHeader == null || authHeader.Length < 1)
                {
                    throw new Exception("Auth Header is empty!");
                }

                var decodedHash = Token.VerifyToken(authHeader);
                var currentHash = Sha256(body);

                if (decodedHash != currentHash)
                {
                    throw new Exception("Checksum of body does not match!");
                }
            }

            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            };
            options.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Deserialize<WebhookEvent>(body, options);
        }

        string Sha256(string data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                return Convert.ToBase64String(bytes);
            }
        }
    }
}