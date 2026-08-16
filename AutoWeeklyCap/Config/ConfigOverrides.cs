using System.Collections;
using System.Reflection;

namespace AutoWeeklyCap.Config;

/// <summary>
/// Allows temporarily overriding existing values on <see cref="AutoWeeklyCap.Config"/> via
/// dot-notation paths (e.g. "LevelJobs.MinimumGilThreshold"). Overrides are in-memory only and
/// lock the configuration against being saved until <see cref="Clear"/> is called.
/// </summary>
public static class ConfigOverrides
{
    private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.Instance;

    private static readonly Dictionary<string, object?> OriginalValues = new();

    private static readonly List<string> Locked =
    [
        "Version",
        "LastSeenPluginVersion",
        "DevMode",
        "Characters",
        "CollectedTomes",
        "Window"
    ];

    public static bool IsLocked => OriginalValues.Count > 0;

    public static Dictionary<string, string> GetKeyValuePairs()
    {
        var values = new Dictionary<string, string>();

        AddKeyValuePairs(values, AWC.Config, string.Empty);

        return values;
    }

    public static bool Set(Dictionary<string, object> values)
    {
        foreach (var (key, value) in values) {
            if (!Set(key, value)) {
                return false;
            }
        }

        return true;
    }

    public static bool Set(string key, object value)
    {
        if (IsOverrideLocked(key)) {
            AWC.Log.Warning($"ConfigOverrides: Cannot override '{key}', the property is locked");
            return false;
        }

        if (!TryResolveMember(key, out var container, out var member)) {
            AWC.Log.Warning($"ConfigOverrides: Cannot override '{key}', it does not exist on the configuration");
            return false;
        }

        if (!TryConvertValue(value, GetMemberType(member), out var convertedValue)) {
            AWC.Log.Warning($"ConfigOverrides: Cannot override '{key}', value '{value}' is not compatible with type {GetMemberType(member)}");
            return false;
        }

        if (!OriginalValues.ContainsKey(key)) {
            OriginalValues[key] = GetMemberValue(member, container);
        }

        SetMemberValue(member, container, convertedValue);
        AWC.Log.Debug($"ConfigOverrides: Set '{key}' = '{convertedValue}'");

        return true;
    }

    public static void Clear()
    {
        foreach (var (key, originalValue) in OriginalValues) {
            if (!TryResolveMember(key, out var container, out var member)) {
                AWC.Log.Warning($"ConfigOverrides: Could not restore '{key}', it no longer exists on the configuration");
                continue;
            }

            SetMemberValue(member, container, originalValue);
        }

        OriginalValues.Clear();
        Configuration.Save();
    }

    private static bool IsOverrideLocked(string key)
    {
        return Locked.Any(lockedKey =>
            key.Equals(lockedKey, StringComparison.Ordinal) ||
            key.StartsWith($"{lockedKey}.", StringComparison.Ordinal)
        );
    }

    private static void AddKeyValuePairs(Dictionary<string, string> values, object container, string prefix)
    {
        foreach (var member in GetMembers(container.GetType())) {
            string key = string.IsNullOrEmpty(prefix) ? member.Name : $"{prefix}.{member.Name}";
            if (IsOverrideLocked(key)) {
                continue;
            }

            object? value = GetMemberValue(member, container);
            if (value != null && IsConfigObject(value.GetType())) {
                AddKeyValuePairs(values, value, key);
                continue;
            }

            values[key] = $"{value} ({value?.GetType() ?? GetMemberType(member)})";
        }
    }

    private static bool IsConfigObject(Type type)
    {
        return type.IsClass && type != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool TryResolveMember(string path, out object container, out MemberInfo member)
    {
        container = AWC.Config;
        member = null!;

        var segments = path.Split('.');

        for (var i = 0; i < segments.Length; i++) {
            MemberInfo? found = FindMember(container.GetType(), segments[i]);
            if (found == null) {
                return false;
            }

            if (i == segments.Length - 1) {
                member = found;
                return true;
            }

            object? value = GetMemberValue(found, container);
            if (value == null) {
                return false;
            }

            container = value;
        }

        return false;
    }

    private static IEnumerable<MemberInfo> GetMembers(Type type)
    {
        return type.GetProperties(MemberBindingFlags)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Cast<MemberInfo>()
            .Concat(type.GetFields(MemberBindingFlags));
    }

    private static MemberInfo? FindMember(Type type, string name)
    {
        return (MemberInfo?)type.GetProperty(name, MemberBindingFlags) ?? type.GetField(name, MemberBindingFlags);
    }

    private static Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,

            _ => throw new ArgumentOutOfRangeException(nameof(member)),
        };
    }

    private static object? GetMemberValue(MemberInfo member, object container)
    {
        return member switch
        {
            PropertyInfo property => property.GetValue(container),
            FieldInfo field => field.GetValue(container),

            _ => throw new ArgumentOutOfRangeException(nameof(member)),
        };
    }

    private static bool TryConvertValue(object? value, Type targetType, out object? convertedValue)
    {
        if (value == null) {
            convertedValue = null;
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
        }

        if (targetType.IsInstanceOfType(value)) {
            convertedValue = value;
            return true;
        }

        if (targetType.IsEnum) {
            return TryConvertToEnum(value, targetType, out convertedValue);
        }

        if (value is not string) {
            if (IsListType(targetType, out var elementType) && value is IEnumerable listSource) {
                return TryConvertToList(listSource, targetType, elementType, out convertedValue);
            }

            if (IsDictionaryType(targetType, out var keyType, out var valueType) && value is IDictionary dictSource) {
                return TryConvertToDictionary(dictSource, targetType, keyType, valueType, out convertedValue);
            }
        }

        if (targetType.IsClass && targetType != typeof(string) && targetType.GetConstructor(Type.EmptyTypes) != null) {
            return TryConvertToObject(value, targetType, out convertedValue);
        }

        try {
            convertedValue = Convert.ChangeType(value, targetType);
            return true;
        } catch (Exception) {
            convertedValue = null;
            return false;
        }
    }

    private static bool IsListType(Type type, out Type elementType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        elementType = typeof(object);
        return false;
    }

    private static bool IsDictionaryType(Type type, out Type keyType, out Type valueType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            Type[] arguments = type.GetGenericArguments();

            keyType = arguments[0];
            valueType = arguments[1];

            return true;
        }

        keyType = typeof(object);
        valueType = typeof(object);

        return false;
    }

    private static bool TryConvertToList(IEnumerable source, Type targetType, Type elementType, out object? convertedValue)
    {
        var list = (IList)Activator.CreateInstance(targetType)!;

        foreach (var item in source) {
            if (!TryConvertValue(item, elementType, out var convertedItem)) {
                convertedValue = null;
                return false;
            }

            list.Add(convertedItem);
        }

        convertedValue = list;
        return true;
    }

    private static bool TryConvertToDictionary(IDictionary source, Type targetType, Type keyType, Type valueType, out object? convertedValue)
    {
        var dictionary = (IDictionary)Activator.CreateInstance(targetType)!;

        foreach (DictionaryEntry entry in source) {
            if (!TryConvertValue(entry.Key, keyType, out var key) || !TryConvertValue(entry.Value, valueType, out var value) || key == null) {
                convertedValue = null;
                return false;
            }

            dictionary.Add(key, value);
        }

        convertedValue = dictionary;
        return true;
    }

    private static bool TryConvertToEnum(object value, Type enumType, out object? convertedValue)
    {
        if (value is string stringValue) {
            if (Enum.TryParse(enumType, stringValue, true, out var parsed)) {
                convertedValue = parsed;
                return true;
            }

            convertedValue = null;
            return false;
        }

        try {
            convertedValue = Enum.ToObject(enumType, value);
            return true;
        } catch (Exception) {
            convertedValue = null;
            return false;
        }
    }

    private static bool TryConvertToObject(object value, Type targetType, out object? convertedValue)
    {
        object instance = Activator.CreateInstance(targetType)!;

        foreach (var (name, sourceValue) in GetPropertyBag(value)) {
            MemberInfo? targetMember = FindMember(targetType, name);
            if (targetMember == null) {
                continue;
            }

            if (!TryConvertValue(sourceValue, GetMemberType(targetMember), out var convertedMemberValue)) {
                convertedValue = null;
                return false;
            }

            SetMemberValue(targetMember, instance, convertedMemberValue);
        }

        convertedValue = instance;

        return true;
    }

    private static IEnumerable<(string Name, object? Value)> GetPropertyBag(object value)
    {
        if (value is IDictionary dictionary) {
            foreach (DictionaryEntry entry in dictionary) {
                if (entry.Key is string name) {
                    yield return (name, entry.Value);
                }
            }

            yield break;
        }

        foreach (var property in value.GetType().GetProperties(MemberBindingFlags)) {
            if (property.GetIndexParameters().Length == 0) {
                yield return (property.Name, property.GetValue(value));
            }
        }

        foreach (var field in value.GetType().GetFields(MemberBindingFlags)) {
            yield return (field.Name, field.GetValue(value));
        }
    }

    private static void SetMemberValue(MemberInfo member, object container, object? value)
    {
        switch (member) {
            case PropertyInfo property:
                property.SetValue(container, value);
                break;
            case FieldInfo field:
                field.SetValue(container, value);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(member));
        }
    }
}
