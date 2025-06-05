// Nexx.Core.Mapping.DataReader.DataReaderMapper.cs
using Nexx.Core.Mapping.Interfaces;
using System.Data.Common;
using System.Reflection;

namespace Nexx.Core.Mapping.DataReader;

public class DataReaderMapper<T> : IDataReaderMapper<T> where T : class, new()
{
    private static readonly Dictionary<string, PropertyInfo> _propertyCache;

    static DataReaderMapper()
    {
        _propertyCache = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name.ToLower(), p => p);
    }

    public T Map(DbDataReader reader)
    {
        var instance = new T();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i).ToLower();
            if (!_propertyCache.TryGetValue(columnName, out var property)) continue;
            if (reader.IsDBNull(i)) continue;

            try
            {
                var value = reader.GetValue(i);
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                var safeValue = Convert.ChangeType(value, targetType);
                property.SetValue(instance, safeValue);
            }
            catch
            {
                // Logar conversão ignorada (se quiser injetar logger no futuro)
                continue;
            }
        }

        return instance;
    }
}
