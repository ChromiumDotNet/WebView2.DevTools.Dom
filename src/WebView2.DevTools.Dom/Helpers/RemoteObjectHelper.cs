using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using WebView2.DevTools.Dom.Helpers.Json;
using WebView2.DevTools.Dom.Messaging;

namespace WebView2.DevTools.Dom.Helpers
{
    internal class RemoteObjectHelper
    {
        internal static object ValueFromRemoteObject<T>(Runtime.RemoteObject remoteObject, bool stringify = false)
        {
            var unserializableValue = remoteObject.UnserializableValue;

            if (unserializableValue != null)
            {
                return ValueFromUnserializableValue(remoteObject, unserializableValue);
            }

            if (remoteObject.Type == "undefined")
            {
                return stringify ? "undefined" : default(T);
            }

            var val = (JsonElement?)remoteObject.Value;

            if (val == null || val.Value.ValueKind == JsonValueKind.Null)
            {
                return stringify ? "null" : default(T);
            }

            var returnType = typeof(T);

            if (returnType == typeof(JsonElement))
            {
                return val.Value;
            }

            var objValue = ValueFromType<T>(val.Value, remoteObject.Type, stringify);

            if (objValue == null)
            {
                return default;
            }

            var objType = objValue.GetType();

            // If the object is already the return type then simply return
            if (returnType.IsAssignableFrom(objType))
            {
                return objValue;
            }

            var typeConverter = TypeDescriptor.GetConverter(returnType);

            // If the object can be converted to the modelType (eg: double to int)
            if (typeConverter.CanConvertFrom(objType))
            {
                return typeConverter.ConvertFrom(objValue);
            }

            if (returnType.IsEnum && objType == typeof(double))
            {
                return Enum.ToObject(returnType, int.Parse(objValue.ToString(), CultureInfo.InvariantCulture));
            }

            // TODO: Add support for collections
            return objValue;
        }

        // TODO: This method will need work
        private static object ValueFromType<T>(JsonElement value, string objectType, bool stringify = false)
        {
            if (!Enum.TryParse<RemoteObjectType>(objectType, ignoreCase: true, out var remoteObjectType))
            {
                throw new Exception("Unable to parse type");
            }

            if (stringify)
            {
                switch (remoteObjectType)
                {
                    case RemoteObjectType.String:
                        return value.GetString();
                    case RemoteObjectType.Object:
                    {
                        if (value.ValueKind == JsonValueKind.Null)
                        {
                            return string.Empty;
                        }

                        var type = typeof(T);
                        if (type == typeof(object) || type == typeof(object[]))
                        {
                            return value.ToDynamicObject();
                        }
                        return value.ToObject<T>(true);
                    }
                    case RemoteObjectType.Undefined:
                        return "undefined";
                    case RemoteObjectType.Number:
                        return ConvertNumber<T>(value);
                    case RemoteObjectType.Boolean:
                        return value.GetBoolean();
                    case RemoteObjectType.Bigint:
                        return value.GetDouble();
                    default: // string, symbol, function
                        return value.ToObject<T>(false);
                }
            }

            switch (remoteObjectType)
            {
                case RemoteObjectType.String:
                    return value.GetString();
                case RemoteObjectType.Object:
                {
                    if (value.ValueKind == JsonValueKind.Null)
                    {
                        return null;
                    }

                    var type = typeof(T);
                    if (type == typeof(object) || type == typeof(object[]))
                    {
                        return value.ToDynamicObject();
                    }
                    return value.ToObject<T>(true);
                }
                case RemoteObjectType.Undefined:
                    return null;
                case RemoteObjectType.Bigint:
                case RemoteObjectType.Number:
                    return ConvertNumber<T>(value);
                case RemoteObjectType.Boolean:
                    return value.GetBoolean();
                default: // string, symbol, function
                    return value.ToObject<T>(false);
            }
        }

        private static object ConvertNumber<T>(JsonElement val)
        {
            // TODO: Support more types (needs a rewrite)
            if (typeof(T) == typeof(int))
            {
                return val.GetInt32();
            }

            if (typeof(T) == typeof(decimal))
            {
                return val.GetDecimal();
            }

            if (typeof(T) == typeof(double))
            {
                return val.GetDouble();
            }

            if (typeof(T) == typeof(uint))
            {
                return val.GetUInt32();
            }

            return val.GetDouble();
        }

        private static object ValueFromUnserializableValue(Runtime.RemoteObject remoteObject, string unserializableValue)
        {
            if (remoteObject.Type == "bigint" &&
                                decimal.TryParse(remoteObject.UnserializableValue.Replace("n", string.Empty), out var decimalValue))
            {
                return new BigInteger(decimalValue);
            }
            switch (unserializableValue)
            {
                case "-0":
                    return -0;
                case "NaN":
                    return double.NaN;
                case "Infinity":
                    return double.PositiveInfinity;
                case "-Infinity":
                    return double.NegativeInfinity;
                default:
                    throw new Exception("Unsupported unserializable value: " + unserializableValue);
            }
        }
    }
}
