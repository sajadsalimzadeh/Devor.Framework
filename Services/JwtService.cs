﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Models.Jwts;
using Microsoft.IdentityModel.Tokens;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JwtService
{
    public Task<JwtCreateTokenResponse> CreateTokenAsync(JwtCreateTokenRequest request)
    {
        var secret = App.Setting.Security.Secret.GetDecryptedValue();
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Claims = new Dictionary<string, object>(),
            Issuer = App.Setting.Security.Issuer,
            Audience = App.Setting.Security.Audience,
            Expires = request.Expires,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        if (request.Claims is not null)
        {
            foreach (var claim in request.Claims)
            {
                tokenDescriptor.Claims.Add(claim.Key, claim.Value);
            }
        }

        var csrf = Guid.NewGuid().ToString();
        tokenDescriptor.Claims.Add("csrf", csrf);

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var key = tokenHandler.WriteToken(token);

        return Task.FromResult(new JwtCreateTokenResponse()
        {
            Key = key,
            Csrf = csrf
        });
    }

    public Task<bool> TryValidateTokenAsync(string token)
    {
        return TryValidateTokenAsync(token, out _, out _);
    }

    public Task<bool> TryValidateTokenAsync(string token, out SecurityToken securityToken, out ClaimsPrincipal claimsPrincipal)
    {
        var secret = App.Setting.Security.Secret.GetDecryptedValue();
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
            };
            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return Task.FromResult(securityToken.ValidTo > DateTime.UtcNow);
        }
        catch
        {
            // ignored
        }

        claimsPrincipal = null;
        securityToken = null;
        return Task.FromResult(false);
    }
}