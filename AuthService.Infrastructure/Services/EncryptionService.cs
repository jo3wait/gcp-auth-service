using AuthService.Application.Interfaces;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EncryptionService> _logger;
    private readonly KeyManagementServiceClient? _kms;
    private readonly string? _keyPath;           // projects/.../cryptoKeyVersions
    private readonly byte[] _localPepper;       // fallback key


    public EncryptionService(IConfiguration config, ILogger<EncryptionService> logger)
    {
        _config = config;
        _logger = logger;

        _keyPath = _config["Kms:PasswordKeyPath"];         // 若為 null => fallback
        
        var pepperB64 = _config["Crypto:LocalPepperKey"] ?? "";
        _localPepper = Convert.TryFromBase64String(pepperB64, Span<byte>.Empty, out _)
            ? Convert.FromBase64String(pepperB64)
            : Encoding.UTF8.GetBytes(pepperB64);   // 兼容舊字串

        if (string.IsNullOrWhiteSpace(_keyPath) || _keyPath.StartsWith("devtest"))
            _logger.LogWarning("KMS PasswordKeyPath 未設定，使用本機 HMAC fallback。");
        else
            _kms = KeyManagementServiceClient.Create();
    }

    ///<summary>
    ///註冊
    /// </summary>
    public (string salt, string hash, string mac, string? keyVer) Hash(string plain)
    {
        // salt
        var salt = RandomNumberGenerator.GetBytes(16);
        // argon2id雜湊
        var argon = new Argon2id(Encoding.UTF8.GetBytes(plain))
        {
            Salt = salt,
            MemorySize = 19 * 1024,
            Iterations = 2,
            DegreeOfParallelism = 1,
        };
        var hash = argon.GetBytes(32);

        // HMAC
        byte[] macBytes;
        string? ver = null;

        if (_kms is not null)
        {
            var verName = CryptoKeyVersionName.Parse(_keyPath!);
            var resp = _kms.MacSign(verName, ByteString.CopyFrom(hash));
            macBytes = resp.Mac.ToByteArray();
            ver = verName.CryptoKeyVersionId;
        }
        else
        {
            using var hmac = new HMACSHA256(_localPepper);
            macBytes = hmac.ComputeHash(hash);
        }

        return (Convert.ToBase64String(salt),
                Convert.ToBase64String(hash),
                Convert.ToBase64String(macBytes),
                ver);
    }

    ///<summary>
    ///登入驗證
    /// </summary>
    public bool Verify(string plain, string saltB64, string hashB64, string macB64, string? keyVer)
    {
        // 計算argon2id
        var salt = Convert.FromBase64String(saltB64);
        var argon = new Argon2id(Encoding.UTF8.GetBytes(plain))
        {
            Salt = salt,
            MemorySize = 19 * 1024,
            Iterations = 2,
            DegreeOfParallelism = 1,
        };
        var candidateHash = argon.GetBytes(32);

        // 比對 HMAC
        var expectMac = Convert.FromBase64String(macB64);

        if (_kms != null)
        {
            var keyVerPath = CryptoKeyVersionName.Parse(_keyPath!);
            var verName = CryptoKeyVersionName.FromProjectLocationKeyRingCryptoKeyCryptoKeyVersion(
            keyVerPath.ProjectId,
            keyVerPath.LocationId,
            keyVerPath.KeyRingId,
            keyVerPath.CryptoKeyId,
            keyVer ?? keyVerPath.CryptoKeyVersionId);

            var verify = _kms.MacVerify(verName,
                ByteString.CopyFrom(candidateHash),
                ByteString.CopyFrom(expectMac));
            return verify.Success;
        }
        else
        {
            using var hmac = new HMACSHA256(_localPepper);
            var mac = hmac.ComputeHash(candidateHash);
            return CryptographicOperations.FixedTimeEquals(mac, expectMac);
        }
    }
}
