namespace Nexx.Core.Mapping.Interfaces;

public interface IJsonMapper<T> where T : class
{
    T Map(string json);
}
