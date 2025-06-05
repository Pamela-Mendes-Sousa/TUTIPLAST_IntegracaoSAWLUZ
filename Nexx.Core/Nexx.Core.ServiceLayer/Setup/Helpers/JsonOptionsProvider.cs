using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexx.Core.ServiceLayer.Setup.Helpers
{
    public static class JsonOptionsProvider
    {
        public static readonly JsonSerializerOptions PascalCaseOptions = new()
        {
            PropertyNamingPolicy = null, // Mantém PascalCase
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }
}
