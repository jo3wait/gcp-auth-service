namespace AuthService.Application.Interfaces;

public interface IEncryptionService
{
    /* 註冊用：回傳 Salt、Hash、MAC、使用的 KMS 版本 (nullable) */
    (string salt, string hash, string mac, string? keyVer) Hash(string plainPassword);

    /* 登入驗證：成功 true / 失敗 false */
    bool Verify(string plainPassword, string salt, string hash, string mac, string? keyVer);
}
