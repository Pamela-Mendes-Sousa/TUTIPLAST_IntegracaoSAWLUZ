namespace Nexx.Core.ODBC.Helpers
{
    public static class SqlQueryLoader
    {
        //pega a query do arquivo .sql
        public static async Task<string> LoadAsync(string relativePath)
        {
            // Busca no diretório da própria DLL da camada Infrastructure
            var infraAssemblyLocation = Path.GetDirectoryName(typeof(SqlQueryLoader).Assembly.Location)!;
            var basePath = Path.Combine(infraAssemblyLocation, "DataBase", "Queries"); // só "Queries" pq é local à DLL

            var fullPath = Path.Combine(basePath, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Query file not found: {fullPath}");

            return await File.ReadAllTextAsync(fullPath);
        }

    }
}
