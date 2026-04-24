using System;
using System.Globalization;
using IoTLogic.Core.Reflection;
using Newtonsoft.Json.Linq;

namespace IoTLogic.Flow.Serialization
{
    internal static class LogicGraphValueCodec
    {
        public static JToken Encode(object value, string context)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }

            var type = value.GetType();

            if (type == typeof(string) ||
                type == typeof(bool) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal))
            {
                return JToken.FromObject(value);
            }

            if (type.IsEnum)
            {
                return new JObject
                {
                    ["$kind"] = "enum",
                    ["type"] = RuntimeCodebase.SerializeType(type),
                    ["value"] = value.ToString(),
                };
            }

            if (value is Type serializedType)
            {
                return new JObject
                {
                    ["$kind"] = "type",
                    ["value"] = RuntimeCodebase.SerializeType(serializedType),
                };
            }

            if (value is Guid guid)
            {
                return new JObject
                {
                    ["$kind"] = "guid",
                    ["value"] = guid.ToString("D"),
                };
            }

            if (value is DateTime dateTime)
            {
                return new JObject
                {
                    ["$kind"] = "datetime",
                    ["value"] = dateTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                };
            }

            throw new InvalidOperationException($"Unsupported value type '{type.FullName}' in {context}.");
        }

        public static object Decode(JToken token, Type expectedType, string context)
        {
            expectedType = expectedType ?? typeof(object);

            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token is JValue value)
            {
                var nonNullableExpectedType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

                if (nonNullableExpectedType == typeof(Type) ||
                    nonNullableExpectedType == typeof(Guid) ||
                    nonNullableExpectedType == typeof(DateTime) ||
                    nonNullableExpectedType.IsEnum)
                {
                    throw new InvalidOperationException($"Expected a tagged value for '{nonNullableExpectedType.FullName}' in {context}.");
                }

                if (expectedType == typeof(object))
                {
                    return value.ToObject<object>();
                }

                try
                {
                    return value.ToObject(expectedType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Unable to decode primitive JSON value for '{expectedType.FullName}' in {context}.",
                        ex);
                }
            }

            if (!(token is JObject taggedValue))
            {
                throw new InvalidOperationException($"Unsupported JSON value shape in {context}. Only primitives and tagged objects are supported.");
            }

            var kindToken = taggedValue["$kind"];

            if (kindToken == null || kindToken.Type != JTokenType.String)
            {
                throw new InvalidOperationException($"Tagged value is missing '$kind' in {context}.");
            }

            var kind = kindToken.Value<string>();

            switch (kind)
            {
                case "enum":
                    return DecodeEnum(taggedValue, expectedType, context);
                case "type":
                    return DecodeType(taggedValue, expectedType, context);
                case "guid":
                    return DecodeGuid(taggedValue, expectedType, context);
                case "datetime":
                    return DecodeDateTime(taggedValue, expectedType, context);
                default:
                    throw new InvalidOperationException($"Unsupported tagged value kind '{kind}' in {context}.");
            }
        }

        private static object DecodeEnum(JObject taggedValue, Type expectedType, string context)
        {
            var typeName = ReadRequiredString(taggedValue, "type", context);
            var memberName = ReadRequiredString(taggedValue, "value", context);
            var enumType = RuntimeCodebase.DeserializeType(typeName);

            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException($"Tagged enum type '{typeName}' is not an enum in {context}.");
            }

            var targetType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

            if (targetType != typeof(object) && targetType != enumType)
            {
                throw new InvalidOperationException(
                    $"Tagged enum type '{typeName}' does not match expected type '{targetType.FullName}' in {context}.");
            }

            try
            {
                return Enum.Parse(enumType, memberName, ignoreCase: false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to parse enum value '{memberName}' for '{typeName}' in {context}.", ex);
            }
        }

        private static object DecodeType(JObject taggedValue, Type expectedType, string context)
        {
            var targetType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

            if (targetType != typeof(object) && targetType != typeof(Type))
            {
                throw new InvalidOperationException(
                    $"Tagged type values are not valid for expected type '{targetType.FullName}' in {context}.");
            }

            var typeName = ReadRequiredString(taggedValue, "value", context);
            return RuntimeCodebase.DeserializeType(typeName);
        }

        private static object DecodeGuid(JObject taggedValue, Type expectedType, string context)
        {
            var targetType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

            if (targetType != typeof(object) && targetType != typeof(Guid))
            {
                throw new InvalidOperationException(
                    $"Tagged guid values are not valid for expected type '{targetType.FullName}' in {context}.");
            }

            var value = ReadRequiredString(taggedValue, "value", context);

            if (!Guid.TryParse(value, out var guid))
            {
                throw new InvalidOperationException($"Unable to parse guid value '{value}' in {context}.");
            }

            return guid;
        }

        private static object DecodeDateTime(JObject taggedValue, Type expectedType, string context)
        {
            var targetType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

            if (targetType != typeof(object) && targetType != typeof(DateTime))
            {
                throw new InvalidOperationException(
                    $"Tagged datetime values are not valid for expected type '{targetType.FullName}' in {context}.");
            }

            var value = ReadRequiredString(taggedValue, "value", context);

            if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var dateTime))
            {
                throw new InvalidOperationException($"Unable to parse datetime value '{value}' in {context}.");
            }

            return dateTime;
        }

        private static string ReadRequiredString(JObject taggedValue, string propertyName, string context)
        {
            var token = taggedValue[propertyName];

            if (token == null || token.Type != JTokenType.String || string.IsNullOrWhiteSpace(token.Value<string>()))
            {
                throw new InvalidOperationException($"Tagged value is missing string property '{propertyName}' in {context}.");
            }

            return token.Value<string>();
        }
    }
}
