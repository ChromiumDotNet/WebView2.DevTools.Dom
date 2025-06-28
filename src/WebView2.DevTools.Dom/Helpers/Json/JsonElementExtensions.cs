using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebView2.DevTools.Dom.Helpers.Json
{
    /// <summary>
    /// A set of extension methods for JsonElement
    /// </summary>
    internal static class JsonElementExtensions
    {
        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="T:JsonElement" />.
        /// </summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <param name="token">Json token</param>
        /// <param name="camelCase">If set to <c>true</c> the CamelCasePropertyNamesContractResolver will be used.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static T ToObject<T>(this JsonElement token, bool camelCase)
        {
            var json = token.GetRawText();

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = camelCase
            });
        }

        /// <summary>
        /// Convert <see cref="JsonElement"/> to a <see cref="ExpandoObject"/>
        /// Using <see cref="JsonSerializer.Deserialize{TValue}(string, JsonSerializerOptions)"/> doesn't
        /// yield a usable <see cref="ExpandoObject"/>
        /// </summary>
        /// <param name="jsonElement">json element</param>
        /// <returns>expando object</returns>
        public static object ToDynamicObject(this JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                IDictionary<string, object> obj = new ExpandoObject();

                foreach (var property in jsonElement.EnumerateObject())
                {
                    obj[property.Name] = property.Value.ToDynamicObject();
                }

                return obj;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Null || jsonElement.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }
            else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
            {
                return jsonElement.GetBoolean();
            }
            else if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetDouble();
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object>();

                foreach (var element in jsonElement.EnumerateArray())
                {
                    list.Add(element.ToDynamicObject());
                }

                return list.ToArray();
            }

            return jsonElement.GetString();
        }

        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="T:JsonElement" />.
        /// </summary>
        /// <param name="token">Json token</param>
        /// <param name="returnType">Return Type</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static object ToObject(this JsonElement token, Type returnType)
        {
            if (returnType == typeof(object))
            {
                return token.ToDynamicObject();
            }

            var json = token.GetRawText();

            return JsonSerializer.Deserialize(json, returnType, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
