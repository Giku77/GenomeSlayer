using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public static class SecureSave
{
    private const string MAGIC = "SSV1"; // 포맷 식별자
    private const byte VERSION = 1;
    private const int SaltSize = 16;
    private const int KeySize = 32;   // 256-bit
    private const int IvSize = 16;    // CBC 128-bit
    private const int MacSize = 32;   // HMAC-SHA256

    // 앱 고정 시크릿: 실제 서비스에선 난독화/분산보관 권장 (여기선 예시용)
    private static readonly byte[] AppSecret = Encoding.UTF8.GetBytes("Change-This-Secret!");

    // ============== 외부에서 쓰는 API ==============
    public static string EncryptJsonToBase64(object data, bool compress = true)
    {
        var json = JsonConvert.SerializeObject(data);
        return EncryptToBase64(Encoding.UTF8.GetBytes(json), compress);
    }

    public static bool TryDecryptBase64ToJson<T>(string b64, out T obj, bool compressed = true)
    {
        obj = default;
        if (!TryDecryptToBytes(b64, out var plain)) return false;
        if (compressed) plain = Decompress(plain);
        var json = Encoding.UTF8.GetString(plain);
        obj = JsonConvert.DeserializeObject<T>(json);
        return obj != null;
    }

    public static bool LooksLikeEncrypted(string b64)
    {
        // 빠른 판별: base64 디코드 후 MAGIC 존재 여부 체크
        try
        {
            var raw = Convert.FromBase64String(b64);
            if (raw.Length < 4) return false;
            return raw[0] == (byte)'S' && raw[1] == (byte)'S' && raw[2] == (byte)'V' && raw[3] == (byte)'1';
        }
        catch { return false; }
    }

    // ============== 내부 구현 ==============
    private static string EncryptToBase64(byte[] plain, bool compress)
    {
        if (compress) plain = Compress(plain);

        var salt = RandomBytes(SaltSize);
        var iv = RandomBytes(IvSize);

        // encKey / macKey 파생
        var master = PBKDF2(AppSecret, salt, 100_000, KeySize);
        var encKey = KdfExpand(master, "enc", KeySize);
        var macKey = KdfExpand(master, "mac", KeySize);

        var cipher = AesCbcEncrypt(plain, encKey, iv);

        // mac = HMAC(iv || cipher)
        var macInput = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, macInput, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, macInput, iv.Length, cipher.Length);
        var mac = HmacSha256(macKey, macInput);

        // [MAGIC|VER|salt|iv|mac|cipher] → base64
        using var ms = new MemoryStream();
        WriteHeader(ms, salt, iv, mac);
        ms.Write(cipher, 0, cipher.Length);
        return Convert.ToBase64String(ms.ToArray());
    }

    private static bool TryDecryptToBytes(string b64, out byte[] plain)
    {
        plain = null;
        byte[] raw;
        try { raw = Convert.FromBase64String(b64); }
        catch { return false; }

        using var ms = new MemoryStream(raw);
        if (!TryReadHeader(ms, out var salt, out var iv, out var mac)) return false;

        var cipher = new byte[raw.Length - (4 + 1 + SaltSize + IvSize + MacSize)];
        ms.Read(cipher, 0, cipher.Length);

        var master = PBKDF2(AppSecret, salt, 100_000, KeySize);
        var encKey = KdfExpand(master, "enc", KeySize);
        var macKey = KdfExpand(master, "mac", KeySize);

        // MAC 검증
        var macInput = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, macInput, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, macInput, iv.Length, cipher.Length);
        var calc = HmacSha256(macKey, macInput);
        if (!FixedEq(mac, calc)) return false;

        try
        {
            plain = AesCbcDecrypt(cipher, encKey, iv);
            return true;
        }
        catch { return false; }
    }

    private static void WriteHeader(Stream s, byte[] salt, byte[] iv, byte[] mac)
    {
        var magic = Encoding.ASCII.GetBytes(MAGIC);
        s.Write(magic, 0, magic.Length);
        s.WriteByte(VERSION);
        s.Write(salt, 0, salt.Length);
        s.Write(iv, 0, iv.Length);
        s.Write(mac, 0, mac.Length);
    }

    private static bool TryReadHeader(Stream s, out byte[] salt, out byte[] iv, out byte[] mac)
    {
        salt = iv = mac = null;
        var magic = new byte[4];
        if (s.Read(magic, 0, 4) != 4) return false;
        if (Encoding.ASCII.GetString(magic) != MAGIC) return false;

        var ver = s.ReadByte();
        if (ver != 1) return false;

        salt = new byte[SaltSize]; if (s.Read(salt, 0, salt.Length) != salt.Length) return false;
        iv = new byte[IvSize]; if (s.Read(iv, 0, iv.Length) != iv.Length) return false;
        mac = new byte[MacSize]; if (s.Read(mac, 0, mac.Length) != mac.Length) return false;
        return true;
    }

    private static byte[] AesCbcEncrypt(byte[] plain, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        aes.Key = key; aes.IV = iv;
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(plain, 0, plain.Length);
        return ms.ToArray();
    }

    private static byte[] AesCbcDecrypt(byte[] cipher, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        aes.Key = key; aes.IV = iv;
        using var msOut = new MemoryStream();
        using (var cs = new CryptoStream(new MemoryStream(cipher), aes.CreateDecryptor(), CryptoStreamMode.Read))
            cs.CopyTo(msOut);
        return msOut.ToArray();
    }

    private static byte[] PBKDF2(byte[] secret, byte[] salt, int iter, int bytes)
    {
        using var kdf = new Rfc2898DeriveBytes(secret, salt, iter, HashAlgorithmName.SHA256);
        return kdf.GetBytes(bytes);
    }
    private static byte[] KdfExpand(byte[] master, string context, int outLen)
    {
        using var h = new HMACSHA256(master);
        return h.ComputeHash(Encoding.UTF8.GetBytes(context + "|v1")).AsSpan(0, outLen).ToArray();
    }

    private static byte[] HmacSha256(byte[] key, byte[] data) { using var h = new HMACSHA256(key); return h.ComputeHash(data); }
    private static byte[] RandomBytes(int n) { var b = new byte[n]; RandomNumberGenerator.Fill(b); return b; }

    private static bool FixedEq(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int diff = 0; for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }

    private static byte[] Compress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.Optimal, true))
            gz.Write(data, 0, data.Length);
        return ms.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        using var gz = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
        using var ms = new MemoryStream();
        gz.CopyTo(ms);
        return ms.ToArray();
    }
}
