using System;
using System.Collections.Generic;
using System.Data;

namespace ServidorPedidos.Modelo
{
    public class ProdutoItem
    {
		public Produto Produto { get; set; }
		public int idProduto;
		private string nome;
		public int Qtd { get; set; }
		public decimal Valor { get; set; }

		public decimal Total
        {
            get
            {
				return Valor * Qtd;
            }
        }

		public string Nome{
			get{
				if (!nome.Equals(null)){
					return nome;
				}

				if(Produto.Equals(null)){
					Produto = Produto.BuscarPorId(this.idProduto);
				}

				return Produto.nome;
			}set{
				nome = value;
			}
		}

        private static void PreencherValores(ProdutoItem n, DataRow row)
        {
			n.Qtd = Convert.ToInt32(row["quantidade"]);
			n.Valor = Convert.ToDecimal(row["valor"]);
        }

		public ProdutoItem(){
			
		}

		public ProdutoItem(ServidorPedidos.ProdutoItem prodItem){
			this.idProduto = prodItem.IdProduto;
			this.Qtd = prodItem.Qtd;
			this.Valor = Convert.ToDecimal(prodItem.Valor);
			this.Nome = prodItem.Nome;
		}

		public ServidorPedidos.ProdutoItem toServerProdutoItem(){
			ServidorPedidos.ProdutoItem item = new ServidorPedidos.ProdutoItem();
			item.IdProduto = this.idProduto;
			item.Nome = this.Nome;
			item.Qtd = this.Qtd;
			item.Valor = Convert.ToDouble(this.Valor);

			return item;
		}

        public static List<ProdutoItem> Buscar(Pedido pedido)
        {
            DataRowCollection reader;
            var listaProdutos = new List<ProdutoItem>();

            try
            {
                reader = BancoDeDados.select("select *from produtositens where idPedido=?param1", new object[] { pedido.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar itens de pedido.\n" + e.Message.ToString());
            }

            foreach (DataRow row in reader)
            {
                var n = new ProdutoItem();

                PreencherValores(n, row);

                try
                {
                    n.Produto = Produto.BuscarPorId((int)row["idProduto"]);
					n.Nome = n.Produto.nome;
                }
                catch (Exception e)
                {
                    throw new Exception("Falha ao buscar item de produto.\n" + e.Message.ToString());
                }

                listaProdutos.Add(n);
            }

            return listaProdutos;
        }
    }
}
