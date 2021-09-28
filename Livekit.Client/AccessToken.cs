using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Linq;

namespace Livekit.Client
{
    public class AccessToken
    {
        private string key;
        private string secret;
        private Grant grants;
        private string identity;

        /// <summary>
        /// AccessToken
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="secret">API secret</param>
        /// <param name="grants">Grants to include as claims</param>
        public AccessToken(string key, string secret, Grant grants, string identity = null)
        {
            this.key = key;
            this.secret = secret;
            this.grants = grants;
            this.identity = identity;
        }

        /// <summary>
        /// Generates a new JWT token with 10 minute validity
        /// </summary>
        /// <returns>A JWT token string</returns>
        public string GetToken(string room = null)
        {
            if (room != null)
            {
                grants.video.room = room;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = grants.ToDictionary(),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = this.key,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            if (this.identity != null)
            {
                token.Payload.Add("sub", this.identity);
            }

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a new JWT token and sets it as header
        /// </summary>
        /// <seealso cref="GetToken"/>
        /// <returns>GRPC metadata with Authorization header set</returns>
        public Grpc.Core.Metadata GetAsHeader(string room = null)
        {
            var metadata = new Grpc.Core.Metadata();
            metadata.Add("Authorization", "Bearer " + this.GetToken(room));

            return metadata;
        }

        /// <summary>
        /// Verify a given signed token with issuer and lifetime validation
        /// </summary>
        /// <param name="token">A signed token</param>
        /// <returns>SHA256 Claim included in the token</returns>
        public string VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = this.key,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.secret))
            };
            SecurityToken validated;
            IPrincipal principal = tokenHandler.ValidateToken(token, parameters, out validated);

            return ((JwtSecurityToken)validated).Claims.First(x => x.Type == "sha256").Value;
        }
    }
}