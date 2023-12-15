/*
 * NodeEditor A node editor control for Avalonia.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodeEditor.Model;

namespace NodeEditorDemo.Services;

internal class NodeSerializer : INodeSerializer
{
    private readonly JsonSerializerSettings _settings;

    private class ListContractResolver : DefaultContractResolver
    {
        private readonly Type _listType;

        public ListContractResolver(Type listType)
        {
            _listType = listType;
        }

        public override JsonContract ResolveContract(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return base.ResolveContract(_listType.MakeGenericType(type.GenericTypeArguments[0]));
            }
            return base.ResolveContract(type);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
        }
    }

    public NodeSerializer(Type listType)
    {
        _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ListContractResolver(listType),
            NullValueHandling = NullValueHandling.Ignore,
        };
    }

    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, _settings);
    }

    public T? Deserialize<T>(string text)
    {
        return JsonConvert.DeserializeObject<T>(text, _settings);
    }

    public T? Load<T>(string path)
    {
        using var stream = System.IO.File.OpenRead(path);
        using var streamReader = new System.IO.StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    public void Save<T>(string path, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text)) return;
        using var stream = System.IO.File.Create(path);
        using var streamWriter = new System.IO.StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}
