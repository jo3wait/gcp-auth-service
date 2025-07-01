namespace AuthService.Application.Interfaces;

public interface IEncryptionService
{
    /* ���U�ΡG�^�� Salt�BHash�BMAC�B�ϥΪ� KMS ���� (nullable) */
    (string salt, string hash, string mac, string? keyVer) Hash(string plainPassword);

    /* �n�J���ҡG���\ true / ���� false */
    bool Verify(string plainPassword, string salt, string hash, string mac, string? keyVer);
}
