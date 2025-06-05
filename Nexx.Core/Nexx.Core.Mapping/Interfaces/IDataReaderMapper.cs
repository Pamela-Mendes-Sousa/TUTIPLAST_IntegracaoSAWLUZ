using System.Data.Common;


namespace Nexx.Core.Mapping.Interfaces;
public interface IDataReaderMapper<T> where T : class, new()
{
    T Map(DbDataReader reader);
}