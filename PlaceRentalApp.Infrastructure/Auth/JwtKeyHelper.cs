using System;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PlaceRentalApp.Infrastructure.Auth;

internal static class JwtKeyHelper
{
    internal static byte[] GetSigningKeyBytes(IConfiguration configuration)
    {
        string? key = configuration["JWT:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT:Key is missing.");
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length < 16)
        {
            throw new InvalidOperationException(
                "JWT:Key must be at least 16 bytes (128 bits) long for HS256."
            );
        }

        return keyBytes;
    }
}