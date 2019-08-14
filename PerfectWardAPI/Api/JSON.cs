using PerfectWardAPI.Model;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PerfectWardAPI.Api
{
    public static class JSON
    {
        public static T GetObject<T>(string json) where T : class, IDeserializedCallback
        {
            var obj = GetObjectRaw<T>(json);
            obj?.OnDeserialized();
            return obj;
        }

        public static T GetObjectRaw<T>(string json) where T : class
        {
            if (json == null) return null;
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    var obj = serializer.ReadObject(ms);
                    return (T)obj;
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error in JSON deserialisation:\n{ex.ToString()}");
                return null;
            }
        }

        public static string GetString<T>(T obj) where T : class
        {
            if (obj == null) return null;
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, obj);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error in JSON deserialisation:\n{ex.ToString()}");
                return null;
            }
        }
    }
}
