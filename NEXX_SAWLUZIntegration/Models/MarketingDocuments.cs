using Newtonsoft.Json;
using NEXX_SAWLUZIntegration.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEXX_SAWLUZIntegration.Models
{
    public class MarketingDocuments
    {
        public int? DocEntry { get; set; }
        public int? BPL_IDAssignedToInvoice { get; set; }
        public int? DocNum { get; set; }
        public string DocType { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string TaxDate { get; set; }
        public string CardCode { get; set; }
        public string U_IdPrograma { get; set; }

        public List<Documentline> DocumentLines { get; set; }

        public class Documentline
        {
            public string ItemCode { get; set; }
            public double? Quantity { get; set; }
            public string ShipDate { get; set; }
            public string VendorNum { get; set; }
            public string SupplierCatNum { get; set; }
            public string U_Fabrica { get; set; }
            public string U_PV_PedC { get; set; }
            public string U_Local_PE { get; set; }
            public string U_TipoFornec { get; set; }
            public string U_CallDelivery { get; set; }
            public string U_UE_Serie { get; set; }
        }

    }
}
