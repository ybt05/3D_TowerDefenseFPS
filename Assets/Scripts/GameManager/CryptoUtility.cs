using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptoUtility
{
    private static readonly string key = "Super%Secret@Key"; // 16文字（AES128）または32文字（AES256）
    // 平文文字列をAESで暗号化し、Base64文字列として返す
    public static string Encrypt(string plainText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        // 毎回ランダムな初期化ベクトルを生成
        // 同じ平文でも毎回異なる暗号文になる
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        // 平文をバイト列に変換
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        // AESで暗号化
        byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // 復号時に初期化ベクトルが必要なので先頭に付けて保存する
        byte[] result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        // Base64文字列として返す
        return Convert.ToBase64String(result);
    }
    // Base64文字列をAESで復号化し、平文文字列として返す
    public static string Decrypt(string encryptedText)
    {
        byte[] fullBytes = Convert.FromBase64String(encryptedText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullBytes.Length - 16];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(fullBytes, 16, cipher, 0, cipher.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        // AESで復号
        byte[] decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        // UTF8文字列に戻して返す
        return Encoding.UTF8.GetString(decrypted);
    }
}