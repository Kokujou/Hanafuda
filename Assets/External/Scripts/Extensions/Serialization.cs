using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtensionMethods
{
    public static class Serialization
    {
        public static byte[] Serialize<T>(this T target)
        {
            if (target == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, target);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] source)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            memStream.Write(source, 0, source.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            T obj = (T)binForm.Deserialize(memStream);

            return obj;
        }
    }
}
