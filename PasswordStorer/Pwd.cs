using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Security.Cryptography;

namespace PasswordStorer
{
    public class Pwd
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes("gj4i5o26h258960");

        public string nameSpace { get; set; }
        public string category { get; set; }
        public DateTime datetime { get; set; }

        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string notes { get; set; }

        public int KEY { get; set; }

        public Pwd(string nameSpace, string category, DateTime datetime, string username, string email, string password, string notes)
        {
            this.nameSpace = nameSpace;
            this.category = category;
            this.datetime = datetime;
            this.username = username;
            this.email = email;
            this.password = password;
            this.notes = notes;
        }

        public static string toXml(Pwd pwd)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("nameSpace", pwd.nameSpace);
            data.Add("category", pwd.category);
            data.Add("datetime", pwd.datetime);
            data.Add("username", pwd.username);
            data.Add("email", pwd.email);
            data.Add("password", pwd.password);
            data.Add("notes", pwd.notes);

            StringBuilder xml = new StringBuilder();
            xml.AppendLine(" <Pwd>");
            foreach (KeyValuePair<string, object> item in data)
            {
                xml.AppendLine(String.Format("  <item key=\"{0}\">{1}</item>", item.Key, item.Value.ToString()));
            }
            xml.AppendLine(" </Pwd>");
            return xml.ToString();
        }

        public static List<Pwd> fromXml(string content)
        {
            TextReader xmlfs = new StringReader(content);
            XmlReader xmlr = XmlReader.Create(xmlfs);

            List<Pwd> loaded = new List<Pwd>();

            Pwd current = null;
            List<float> cpoints = new List<float>();

            while (xmlr.Read())
            {
                if (xmlr.Name == "Pwd" && xmlr.NodeType == XmlNodeType.Element)
                {
                    current = new Pwd("", "",DateTime.Now,"","","","");
                    cpoints = new List<float>();
                }
                else if (xmlr.Name == "item" && xmlr.NodeType == XmlNodeType.Element)
                {
                    string nodekey = xmlr.GetAttribute("key");
                    var properties = current.GetType().GetProperties();
                    Type ptype = current.GetType().GetProperty(nodekey).PropertyType;
                    current.GetType().GetProperty(nodekey).SetValue(current, Convert.ChangeType(xmlr.ReadElementContentAsString(), ptype), null);
                }
                else if (xmlr.Name == "Pwd" && xmlr.NodeType == XmlNodeType.EndElement)
                {
                    loaded.Add(current);
                }
            }
            xmlfs.Close();
            return loaded;
        }

        

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}
