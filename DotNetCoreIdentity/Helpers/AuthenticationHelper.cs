using DotNetCoreIdentity.Models.Identity;
using DotNetCoreIdentity.Models.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreIdentity.Helpers
{
    public static class AuthenticationHelper
    {
        public static TokenResponse GenerateJwtToken(ApplicationUser applicationUser, IConfiguration configuration)
        {
            TokenResponse tokenResponse = new TokenResponse();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, applicationUser.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["ApplicationSettings:JWT_Secret"]));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            tokenResponse.Expiration = DateTime.Now.AddDays(Convert.ToDouble(configuration["ApplicationSettings:JwtExpireDays"]));

            JwtSecurityToken securityToken = new JwtSecurityToken(
                issuer: configuration["ApplicationSettings:JwtIssuer"],
                audience: configuration["ApplicationSettings:JwtAudience"],
                notBefore: DateTime.Now,
                expires: tokenResponse.Expiration,
                signingCredentials: signingCredentials,
                claims: claims
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //create Token
            tokenResponse.AccessToken = tokenHandler.WriteToken(securityToken);

            return tokenResponse;
        }
    }
}
