using System;
using System.Security.Cryptography;
using System.Text;

namespace Gribble
{
    public static class EntityExtensions
    {
        public enum HashAlgorithim { Md5, Sha1 }

        public static byte[] Hash(this string value, HashAlgorithim algorithm)
        {
            HashAlgorithm hash;
            switch (algorithm)
            {
                case HashAlgorithim.Md5: hash = new MD5CryptoServiceProvider(); break;
                case HashAlgorithim.Sha1: hash = new SHA1CryptoServiceProvider(); break;
                default : throw new ArgumentException("algorithm");
            }
            return hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        public static string ToHex(this byte[] value)
        {
            var hex = new StringBuilder(value.Length * 2);
            foreach (var b in value) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
