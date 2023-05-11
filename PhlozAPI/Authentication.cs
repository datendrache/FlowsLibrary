using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatumCore;
using System.IO;
using System.Security.Cryptography;

namespace PhlozLib
{
    public class Authentication
    {
        static public string getPasswordHash(string username, string password)
        {
            return BaseAccount.getPasswordHash(username, password);
        }

        static public Boolean sessionValid(CollectionState State, string Session)
        {
            Boolean result = false;

            return result;
        }

        static public string generateSession(string UserID)
        {
            string sessionGarbage = DateTime.Now.ToLongTimeString() + DateTime.Now.ToShortTimeString() + new Random(DateTime.Now.Millisecond).ToString() + UserID + new Random(DateTime.Now.Second).ToString();

            byte[] bytes = new byte[sessionGarbage.Length * sizeof(char)];
            System.Buffer.BlockCopy(sessionGarbage.ToCharArray(), 0, bytes, 0, bytes.Length);

            MD5CryptoServiceProvider md5hash = new MD5CryptoServiceProvider();
            SHA1CryptoServiceProvider sha1hash = new SHA1CryptoServiceProvider();

            md5hash.Initialize();
            sha1hash.Initialize();

            md5hash.ComputeHash(bytes, 0, bytes.Length);
            string md5text = FatumLib.convertBytesTostring(md5hash.Hash);
            md5text += sessionGarbage;

            bytes = new byte[md5text.Length * sizeof(char)];
            System.Buffer.BlockCopy(md5text.ToCharArray(), 0, bytes, 0, bytes.Length);
            sha1hash.ComputeHash(bytes, 0, bytes.Length);

            string sessionString = FatumLib.convertBytesTostring(sha1hash.Hash);

            return sessionString;
        }

        static public string endSession(CollectionState State, string Session)
        {
            return "";
        }
    }
}
