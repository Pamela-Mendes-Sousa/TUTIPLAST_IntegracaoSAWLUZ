using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace Nexx.Core.ServiceLayer.Response
{
    public class ServiceLayerResponse<T>
    {
        [JsonPropertyName("value")]
        public T Value { get; set; }
    }
}
