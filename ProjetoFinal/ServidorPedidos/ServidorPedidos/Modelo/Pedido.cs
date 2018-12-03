using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace ServidorPedidos.Modelo
{
	public class Pedido
	{
		public int Id { get; set; }
		private List<ProdutoItem> produtos = new List<ProdutoItem>();
		public string DataEntrada { get; set; }
		public string DataEntrega { get; set; }
		public string Endereco { get; set; }
		public string Telefone { get; set; }
		public string Email { get; set; }
		public string Observacao { get; set; }
		public string Cliente { get; set; }
        
		public Pedido(RegistroPedido rpedido){
			this.Id = rpedido.Id;
			this.DataEntrada = rpedido.DataEntrada;
			this.DataEntrega = rpedido.DataEntrega;
			this.Endereco = rpedido.Endereco;
			this.Telefone = rpedido.Telefone;
			this.Email = rpedido.Email;
			this.Observacao = rpedido.Observacao;
			this.Cliente = rpedido.Cliente;

			for (int i = 0; i < rpedido.Itens.Count; i++){
				this.produtos.Add(new ProdutoItem(rpedido.Itens[i]));
			}
		}      

		public RegistroPedido toRegistroPedido(){
			RegistroPedido registro = new RegistroPedido();

			registro.Id = this.Id;
			registro.DataEntrega = this.DataEntrega;
			registro.DataEntrada = this.DataEntrada;
			registro.Endereco = this.Endereco;
			registro.Telefone = this.Telefone;
			registro.Email = this.Email;
			registro.Observacao = this.Observacao;
			registro.Cliente = this.Cliente;

			try
			{
				for (int i = 0; i < this.Produtos.Count; i++)
				{
					registro.Itens.Add(this.Produtos[i].toServerProdutoItem());
				}
			}catch(Exception e){
				throw new Exception("Falha ao transpor pedido para registro " + e.Message);
			}

			return registro;
		}

        internal List<ProdutoItem> Produtos
        {
            get
            {
                if (this.produtos.Count == 0 && this.Id != 0)
                    produtos = ProdutoItem.Buscar(this);
                return produtos;
            }

            set { produtos = value; }
        }
              
        public static void preencherValores(Pedido pedido, DataRow row)
        {
            pedido.Id = (int)row["id"];
            pedido.Cliente = row["cliente"].ToString();
			pedido.DataEntrada = row["dataEntrada"].ToString();
			pedido.DataEntrega = row["dataEntrega"].ToString();

            pedido.Telefone = (string)row["telefone"];
            pedido.Email = (string)row["email"];
            pedido.Endereco = (string)row["endereco"];

            pedido.Observacao = row["observacao"].ToString();
            // pedido.Pago = (sbyte) row["pago"] == 1;
        }

        public static Pedido[] Buscar()
        {
            DataRowCollection reader;

            var pedido = new List<Pedido>();

            try
            {
                reader = BancoDeDados.select("select *from pedidos");

                foreach (DataRow row in reader)
                {
					var n = new Pedido(new RegistroPedido());

                    preencherValores(n, row);

                    pedido.Add(n);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar pedidos.\n" + e.Message.ToString());
            }

            return pedido.ToArray();
        }

        public static Pedido BuscarPorId(int id)
        {
            DataRowCollection reader;

			var n = new Pedido(new RegistroPedido());

            try
            {
                reader = BancoDeDados.select("select *from pedidos where id=?param1", new object[] { id });

                if (reader.Count > 0)
                {
                    preencherValores(n, reader[0]);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar pedido por id.\n" + e.Message);
            }

            return n;
        }

		public decimal ValorTotal()
        {
			decimal v = decimal.Zero;

            foreach (ProdutoItem p in produtos)
            {
                v += p.Total;
            }

            return v;
        }

        public static void Salvar(Pedido pedido)
        {
            try
            {
                if (pedido.Id == 0)
                {
					pedido.DataEntrada = DateTime.Now.ToString();

                    pedido.Id = (int)BancoDeDados.executar("insert into pedidos(dataEntrada,dataEntrega,endereco,telefone,email,observacao, cliente)" +
					                                       " values(?param1, ?param2, ?param3, ?param4, ?param5, ?param6, ?param7)",
														   new object[] {pedido.DataEntrada, pedido.DataEntrega, pedido.Endereco, pedido.Telefone, pedido.Email,
						                                    pedido.Observacao, pedido.Cliente});

                    
                }
                else
                {
                    BancoDeDados.executar("update pedidos SET dataEntrega=?param1, endereco=?param2, telefone=?param3, " +
					                      "email=?param4, observacao=?param5 where id = ?param6", 
					                      new object[] { pedido.DataEntrega, pedido.Endereco, pedido.Telefone, pedido.Email, pedido.Observacao, pedido.Id });
                    BancoDeDados.executar("delete from produtositens where idPedido = ?param1", new object[] { pedido.Id });
                }
					                                       
                if (pedido.produtos.Count > 0)
                {
                    foreach (ProdutoItem a in pedido.Produtos)
                    {
                        BancoDeDados.executar("insert into produtositens (idProduto,idPedido,quantidade,valor) " +
						                      "values(?param1,?param2,?param3,?param4)",
						                      new object[] { a.idProduto, pedido.Id, a.Qtd, a.Valor});
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao salvar pedido.\n" + ex.Message);
            }
        }

        public static void Excluir(Pedido pedido)
        {
            try
            {
                BancoDeDados.executar("delete from produtositens where idPedido=?param1", new object[] { pedido.Id });
                BancoDeDados.executar("delete from pedidos where id=?param1", new object[] { pedido.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao deletar pedido.\n" + e.Message);
            }
        }
    }
}
