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
        [Layout(20, 20)]
        public string ProdutoLocal { get; set; }
        [Layout(40, 30)]
        public string ProdutoCliente { get; set; }
        [Layout(70, 12)]
        public string PedidoNro { get; set; }
        [Layout(82, 7)]
        public string Fabrica { get; set; }
        [Layout(89, 16)]
        public string Local_PE { get; set; }
        [Layout(105, 1)]
        public string TipoFornecimento { get; set; }
        [Layout(106, 10)]
        public string Quantidade { get; set; }
        [Layout(116, 15)]
        public string DtHrDE { get; set; }
        [Layout(131, 15)]
        public string CallDelivery { get; set; }
        [Layout(146, 15)]
        public string ProgramaAtualDtHr { get; set; }
        [Layout(161, 14)]
        public string HONDA_SlipNumber { get; set; }
        [Layout(175, 2)]
        public string HONDA_TipoPedido { get; set; }
        [Layout(177, 12)]
        public string HONDA_ProductionLotNumber { get; set; }
        [Layout(189, 12)]
        public string HONDA_Seppen { get; set; }
        [Layout(201, 3)]
        public string HONDA_InitialProductionControl { get; set; }
        [Layout(204, 16)]
        public string Doca_PE { get; set; }
        [Layout(220, 10)]
        public string PontoDeCorteDocumento { get; set; }
        [Layout(230, 4)]
        public string UE_Serie { get; set; }
        [Layout(234, 8)]
        public string UE_DtHrEmissao { get; set; }
        [Layout(242, 3)]
        public string Orig_PE_PD { get; set; }
        [Layout(245, 15)]
        public string MovimentoDtHr { get; set; }
        [Layout(260, 14)]
        public string AcumuladoProgramadoPE_Qtd { get; set; }
        [Layout(274, 14)]
        public string AcumuladoProgramadoPD_Qtd { get; set; }
        [Layout(288, 3)]
        public string Frequencia { get; set; }
        [Layout(291, 2)]
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
            Console.WriteLine(linha.Length); 

             foreach (var prop in typeof(AttributeOrders).GetProperties())
            {
                var inicio = (int)GetPropertyAttributes(prop.Name, 0);
                var tamanho = (int)GetPropertyAttributes(prop.Name, 1);

                if (linha.Length < inicio + tamanho)
                    throw new ArgumentOutOfRangeException(nameof(linha), $"A linha não tem caracteres suficientes. Esperado: {inicio + tamanho}, Atual: {linha.Length}");

                var valor = linha.Length >= inicio + tamanho
                            ? linha.Substring(inicio, tamanho).Trim()
                            : string.Empty;

                prop.SetValue(obj, valor);
            }

            return obj;
        }

    }

    public class Layout : Attribute
    {
        public Layout(int inicio, int tamanho)
        {
            this.inicio = inicio;
            this.tamanho = tamanho;
        }

        public int inicio { get; set; }
        public int tamanho { get; set; }
    }

}
