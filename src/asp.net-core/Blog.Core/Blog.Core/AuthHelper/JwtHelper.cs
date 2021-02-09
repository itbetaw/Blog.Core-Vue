using Blog.Core.Common;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Web.Host
{
    public class JwtHelper
    {
        public static string IssueJwt(TokenModelJwt tokenModel)
        {
            string iss = Appsettings.app(new string[] { "Audience", "Issuer" });
            string aud = Appsettings.app(new string[] { "Audience", "Audience" });
            string secret = AppSecretConfig.Audience_Secret_String;

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti,tokenModel.Uid.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),

                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now).AddSeconds(1000).ToUnixTimeSeconds()}"),
                new Claim (JwtRegisteredClaimNames.Iss,iss),
                new Claim(JwtRegisteredClaimNames.Aud, aud)
            };
            claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: iss,
                claims: claims,
                signingCredentials: creds
                );
            var jwtHandler = new JwtSecurityTokenHandler();
            var encodedJwt = jwtHandler.WriteToken(jwt);

            return encodedJwt;
        }

        public static TokenModelJwt SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            object role;
            try
            {

                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            var tm = new TokenModelJwt
            {
                Uid = Convert.ToInt64(jwtToken.Id),
                Role = role != null ? role.ToString() : ""
            };
            return tm;
        }
    }
    /// <summary>
    ///  令牌
    /// </summary>
    public class TokenModelJwt
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string work { get; set; }
    }
}
