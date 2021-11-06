using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NodeEditorDemo.ViewModels
{
    public static class NodeSerializer
    {
        private static readonly JsonSerializerSettings s_settings;

        private class ListContractResolver : DefaultContractResolver
        {
            private readonly Type _type;

            public ListContractResolver(Type type)
            {
                _type = type;
            }

            public override JsonContract ResolveContract(Type type)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return base.ResolveContract(_type.MakeGenericType(type.GenericTypeArguments[0]));
                }
                return base.ResolveContract(type);
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
            }
        }

        static NodeSerializer()
        {
            s_settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ListContractResolver(typeof(ObservableCollection<>)),
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        public static string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, s_settings);
        }

        public static T Deserialize<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, s_settings)!;
        }

        public static T Load<T>(string path)
        {
            using var stream = System.IO.File.OpenRead(path);
            using var streamReader = new System.IO.StreamReader(stream, Encoding.UTF8);
            var text = streamReader.ReadToEnd();
            return Deserialize<T>(text);
        }

        public static void Save<T>(string path, T value)
        {
            var text = Serialize<T>(value);
            if (string.IsNullOrWhiteSpace(text)) return;
            using var stream = System.IO.File.Create(path);
            using var streamWriter = new System.IO.StreamWriter(stream, Encoding.UTF8);
            streamWriter.Write(text);
        }
    }
}
