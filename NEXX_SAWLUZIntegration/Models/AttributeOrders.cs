using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NEXX_SAWLUZIntegration.Models
{
    public class AttributeOrders
    {
        [Layout(0, 20)] //inicio, tamanho
        public string ClienteInterno { get; set; }
        [Layout(21, 20)]
        public string ProdutoLocal { get; set; }
        [Layout(41, 30)]
        public string ProdutoCliente { get; set; }
        [Layout(71, 12)]
        public string PedidoNro { get; set; }
        [Layout(83, 7)]
        public string Fabrica { get; set; }
        [Layout(90, 16)]
        public string Local_PE { get; set; }
        [Layout(106, 1)]
        public string TipoFornecimento { get; set; }
        [Layout(107, 10)]
        public string Quantidade { get; set; }
        [Layout(117, 15)]
        public string DtHrDE { get; set; }
        [Layout(132, 15)]
        public string CallDelivery { get; set; }
        [Layout(147, 15)]
        public string ProgramaAtualDtHr { get; set; }
        [Layout(162, 14)]
        public string HONDA_SlipNumber { get; set; }
        [Layout(176, 2)]
        public string HONDA_TipoPedido { get; set; }
        [Layout(178, 12)]
        public string HONDA_ProductionLotNumber { get; set; }
        [Layout(190, 12)]
        public string HONDA_Seppen { get; set; }
        [Layout(202, 3)]
        public string HONDA_InitialProductionControl { get; set; }
        [Layout(205, 16)]
        public string Doca_PE { get; set; }
        [Layout(221, 10)]
        public string PontoDeCorteDocumento { get; set; }
        [Layout(231, 4)]
        public string UE_Serie { get; set; }
        [Layout(235, 8)]
        public string UE_DtHrEmissao { get; set; }
        [Layout(243, 3)]
        public string Orig_PE_PD { get; set; }
        [Layout(246, 15)]
        public string MovimentoDtHr { get; set; }
        [Layout(261, 14)]
        public string AcumuladoProgramadoPE_Qtd { get; set; }
        [Layout(275, 14)]
        public string AcumuladoProgramadoPD_Qtd { get; set; }
        [Layout(289, 3)]
        public string Frequencia { get; set; }
        [Layout(292, 2)]
        public string IdPrograma { get; set; }


        public static object GetPropertyAttributes(string Campo, int attrposition)
        {
            string attributeName = "Layout";
            PropertyInfo prop = typeof(AttributeOrders).GetProperty(Campo);

            // look for an attribute that takes one constructor argument
            foreach (CustomAttributeData attribData in prop.GetCustomAttributesData())
            {
                string typeName = attribData.Constructor.DeclaringType.Name;
                if (
                    (typeName == attributeName))
                {
                    return attribData.ConstructorArguments[attrposition].Value;
                }
            }
            return null;
        }

        public static AttributeOrders MapLinhaParaObjeto(string linha)
        {
            var obj = new AttributeOrders();

            foreach (var prop in typeof(AttributeOrders).GetProperties())
            {
                var inicio = (int)GetPropertyAttributes(prop.Name, 0);
                var tamanho = (int)GetPropertyAttributes(prop.Name, 1);
                var valor = linha.Substring(inicio, tamanho).Trim();
                prop.SetValue(obj, valor);
            }

            return obj;
        }
    }

    public class Layout : Attribute
    {
        public Layout(int tamanho, int inicio)
        {
            this.tamanho = tamanho;
            this.inicio = inicio;

        }
        public int tamanho { get; set; }
        public int inicio { get; set; }

    }
}
