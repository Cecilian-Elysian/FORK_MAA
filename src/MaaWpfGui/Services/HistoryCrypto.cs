// feat/recruit-result-display (D5) — AES-256-GCM 加密本地历史数据
//
// 密钥：config/.recruit_key (256-bit, gitignored)
// 缺失时首次运行时随机生成并保存
//
// 加密格式: [12 byte nonce][16 byte tag][ciphertext]

using System;
using System.IO;
using System.Security.Cryptography;

namespace MaaWpfGui.Services
{
    public class HistoryCrypto
    {
        public static HistoryCrypto Instance { get; } = new();

        private const int NonceSize = 12;
        private const int TagSize = 16;
        private readonly byte[] _key;
        private readonly string _keyPath;

        private HistoryCrypto()
        {
            _keyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaaAssistantArknights", "config", ".recruit_key");

            if (File.Exists(_keyPath))
            {
                _key = File.ReadAllBytes(_keyPath);
                if (_key.Length != 32)
                {
                    _key = GenerateAndPersist();
                }
            }
            else
            {
                _key = GenerateAndPersist();
            }
        }

        private byte[] GenerateAndPersist()
        {
            var k = RandomNumberGenerator.GetBytes(32);
            Directory.CreateDirectory(Path.GetDirectoryName(_keyPath)!);
            File.WriteAllBytes(_keyPath, k);
            return k;
        }

        public byte[] Encrypt(string plaintext)
        {
            var ptBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var ciphertext = new byte[ptBytes.Length];
            var tag = new byte[TagSize];

            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, ptBytes, ciphertext, tag);

            var output = new byte[NonceSize + TagSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, output, NonceSize + TagSize, ciphertext.Length);
            return output;
        }

        public string Decrypt(byte[] data)
        {
            if (data.Length < NonceSize + TagSize) throw new CryptographicException("Invalid data length");

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var ciphertext = new byte[data.Length - NonceSize - TagSize];

            Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(data, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(data, NonceSize + TagSize, ciphertext, 0, ciphertext.Length);

            var ptBytes = new byte[ciphertext.Length];
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, ptBytes);
            return System.Text.Encoding.UTF8.GetString(ptBytes);
        }
    }
}