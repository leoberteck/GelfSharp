using GelfSharpCore.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelfSharpCore.src
{
    public class GelfMessage
    {
        public string VERSION = "1.1";
        public string host { get; set; }
        public string short_message { get; set; }
        public string full_message { get; set; }
        public long timestamp { get; set; }
        public int level { get; set; }
        private Dictionary<string, object> aditionalFields { get; set; }

        public GelfMessage()
        {
            aditionalFields = new Dictionary<string, object>();
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Adds a value do the aditional fields Dictionary. 
        /// Every key name must be preceeded by '_'
        /// </summary>
        /// <param name="Key">the name of the field</param>
        /// <param name="Value">the new field value</param>
        public void Add(string Key, object Value)
        {
            Key = NormalizeKey(Key);
            aditionalFields.Add(Key, Value);
        }

        /// <summary>
        /// Adds or Updates a field based on the Key parameter. 
        /// Every key name must be preceeded by '_'
        /// </summary>
        /// <param name="Key">the name of the field</param>
        /// <param name="Value">the new field value</param>
        public void Put(string Key, object Value)
        {
            Key = NormalizeKey(Key);
        }

        /// <summary>
        /// Removes a field
        /// </summary>
        /// <param name="Key">the name of the field</param>
        public void Remove(string Key)
        {
            Key = NormalizeKey(Key);
        }

        public object this[string Key] {
            get {
                Key = NormalizeKey(Key);
                if (aditionalFields.ContainsKey(Key)) {
                    return aditionalFields[Key];
                }
                else {
                    return null;
                }

            } set {
                Put(Key, value);
            }
        }

        /// <summary>
        /// Every gelf aditional field MUST be preceeded by '_'. 
        /// If the Key string is does not start with a '_', adds one to the beginning of the Key 
        /// </summary>
        /// <param name="Key">user informed key string</param>
        /// <returns>Key with a '_' on the first position</returns>
        private string NormalizeKey(string Key)
        {
            if (!Key.StartsWith("_"))
            {
                Key = "_" + Key;
            }
            return Key;
        }

        /// <summary>
        /// Validate message based on gelf 1.1 requirements.
        /// See <see cref="http://docs.graylog.org/en/2.1/pages/gelf.html"/> for details
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Validate()
        {
            return true;
        }

        public async Task<string> SerializeToJsonAsync()
        {
            return await Task.Run(() =>
            {
                var jsonGelfObj = JObject.FromObject(this);
                var jsonAditionalFields = JObject.FromObject(aditionalFields);

                jsonGelfObj.Merge(jsonAditionalFields, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Concat
                });
                return JsonConvert.SerializeObject(jsonGelfObj, Formatting.Indented);
            });
        }
    }
}
