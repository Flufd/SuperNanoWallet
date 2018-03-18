using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SuperNanoWallet
{
    public class Encryption
    {
        const int iterations = 1000;

        public static byte[] Encrypt(byte[] data, string password)
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[32];
            rng.GetBytes(salt);

            using (var db = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                using (var alg = new AesCryptoServiceProvider { KeySize = 128 })
                {
                    alg.Key = db.GetBytes(128 / 8);
                    var iv = db.GetBytes(alg.BlockSize / 8);
                    alg.IV = iv;
                    alg.Padding = PaddingMode.PKCS7;
                    using (var transform = alg.CreateEncryptor())
                    {
                        using(var ms = new MemoryStream())
                        {
                            using (var crytoStream = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                            {
                                ms.Write(salt, 0, salt.Length);
                                ms.Write(iv, 0, iv.Length);
                                crytoStream.Write(data, 0, data.Length);
                                crytoStream.FlushFinalBlock();
                                return ms.ToArray();
                            }
                        }
                    }                    
                }
            }
        }

        public static byte[] Decrypt(byte[] data, string password)
        {

            using (var ms = new MemoryStream(data))
            using (var cypherTextStream = new MemoryStream())
            {
                using (var sr = new StreamReader(ms))
                {
                    var salt = new byte[32];
                    ms.Read(salt, 0, 32);
                    
                    var iv = new byte[16];
                    ms.Read(iv, 0, 16);

                    ms.CopyTo(cypherTextStream);
                    cypherTextStream.Position = 0;
                    data = cypherTextStream.ToArray();

                    using (var db = new Rfc2898DeriveBytes(password, salt, iterations))
                    {
                        using (var alg = new AesCryptoServiceProvider { KeySize = 128 })
                        {
                            alg.Key = db.GetBytes(128 / 8);
                            alg.Padding = PaddingMode.PKCS7;
                            alg.IV = iv;
                            using (var transform = alg.CreateDecryptor())
                            {
                                using (var cs = new MemoryStream())
                                using (var crytoStream = new CryptoStream(cs, transform, CryptoStreamMode.Write))
                                {
                                    crytoStream.Write(data, 0, data.Length);                                    
                                    crytoStream.FlushFinalBlock();
                                    return cs.ToArray();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
