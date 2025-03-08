using System.Security.Cryptography;
using System.Text;

namespace Prestamo.Web.Servives;

public class SecurityHeadersService
{
    private readonly Dictionary<string, string> _scriptHashes = new();
    private readonly Dictionary<string, string> _styleHashes = new();

    public string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hash);
    }

    public string GetScriptHash(string script)
    {
        if (_scriptHashes.TryGetValue(script, out var hash))
        {
            return hash;
        }

        hash = ComputeHash(script);
        _scriptHashes[script] = hash;
        return hash;
    }

    public string GetStyleHash(string style)
    {
        if (_styleHashes.TryGetValue(style, out var hash))
        {
            return hash;
        }

        hash = ComputeHash(style);
        _styleHashes[style] = hash;
        return hash;
    }

    public string GetCSPHeader(string nonce)
    {
        var trustedScriptHashes = string.Join(" ", _scriptHashes.Values.Select(h => $"'sha256-{h}'"));
        var trustedStyleHashes = string.Join(" ", _styleHashes.Values.Select(h => $"'sha256-{h}'"));

        // Lista de dominios confiables específicos
        var trustedDomains = new[]
        {
            "http://localhost:5267",
            "https://localhost:7267",
            "http://localhost:44303",
            "https://localhost:44303",
            "https://cdn.datatables.net"
        };

        var trustedWebsocketDomains = new[]
        {
            "ws://localhost:5267",
            "wss://localhost:7267",
            "ws://localhost:44303",
            "wss://localhost:44303"
        };

        var scriptSrc = new[]
        {
            "'self'",
            $"'nonce-{nonce}'",
            trustedScriptHashes,
            "'strict-dynamic'"
        }.Where(s => !string.IsNullOrEmpty(s));

        var connectSrc = new[]
        {
            "'self'",
            string.Join(" ", trustedDomains),
            string.Join(" ", trustedWebsocketDomains)
        };

        return new StringBuilder()
            .Append("default-src 'self'; ")
            .Append($"script-src {string.Join(" ", scriptSrc)}; ")
            .Append($"style-src 'self' 'nonce-{nonce}' {trustedStyleHashes}; ")
            .Append("img-src 'self' data:; ")
            .Append("font-src 'self' data:; ")
            .Append($"connect-src {string.Join(" ", connectSrc)}; ")
            .Append("frame-ancestors 'none'; ")
            .Append("form-action 'self'; ")
            .Append("base-uri 'self'; ")
            .Append("object-src 'none'; ")
            .Append("upgrade-insecure-requests;")
            .ToString();
    }
} 