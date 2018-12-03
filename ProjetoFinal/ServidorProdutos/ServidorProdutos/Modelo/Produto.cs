using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ServidorProdutos.Modelo
{
	class Produto
	{
		private int id { get; set; }
		private string nome { get; set; }
		private double precoUnitario { get; set; }
		private DateTime dataCadastro { get; set; }

		public static void preencherValores(Produto a, DataRow row)
		{
			a.id = (int)row["id"];
			a.nome = row["nome"].ToString();
			a.precoUnitario = Convert.ToDouble(row["precoUnidade"]);
			a.dataCadastro = (DateTime)row["dataCadastro"];
		}

		public Produto(RegistroProduto rproduto)
		{
			this.id = rproduto.Id;
			this.nome = rproduto.Nome;
			this.precoUnitario = rproduto.PrecoUnidade;

			if (this.id == 0)
			{
				this.dataCadastro = DateTime.Now;
			}
		}

		public Produto()
		{

		}

		public RegistroProduto toRegistroProduto()
		{
			RegistroProduto registro = new RegistroProduto();
			registro.Id = this.id;
			registro.Nome = this.nome;
			registro.PrecoUnidade = precoUnitario;
			registro.DataCadastro = this.dataCadastro.ToString();

			return registro;
		}

		public static Produto BuscarPorId(int id)
		{
			DataRowCollection reader = null;
			var s = new Produto();

			try
			{
				reader = BancoDeDados.select("select *from produtos where id=?param1", new object[] { id });
			}
			catch (Exception e)
			{
				throw new Exception("Falha ao buscar produto por id.\n" + e.Message.ToString());
			}

			foreach (DataRow row in reader)
			{
				try
				{
					preencherValores(s, row);
				}
				catch (Exception e)
				{
					throw new Exception("Falha ao buscar produto por id.\n" + e.Message.ToString());
				}
			}

			return s;
		}

		public static Produto[] Buscar()
        {
            var listaProduto = new List<Produto>();
            DataRowCollection reader;

            try
            {
                reader = BancoDeDados.select("select *from produtos", new object[] {  });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar produtos.\n" + e.Message);
            }

            foreach (DataRow row in reader)
            {
                var s = new Produto();

                try
                {
                    preencherValores(s, row);
                }
                catch (Exception e)
                {
                    throw new Exception("Falha ao buscar produtos.\n" + e.Message.ToString());
                }

                listaProduto.Add(s);
            }

            return listaProduto.ToArray();
        }

		public static void Excluir(Produto b)
		{
			try
			{
				BancoDeDados.executar("delete from produtos where id=?param1", new object[] { b.id });
			}
			catch (Exception e)
			{
				throw new Exception("Falha ao excluir Produto.\n" + e.Message.ToString());
			}
		}

		public static void Salvar(Produto b)
		{
			if (b.id == 0)
			{
				try
				{
					b.id = (int)BancoDeDados.executar("insert into produtos (nome,precoUnidade,dataCadastro) values(?param1,?param2,?param3);",
													  new object[] { b.nome, b.precoUnitario, b.dataCadastro });
				}
				catch (Exception e)
				{
					throw new Exception("Falha ao salvar produto.\n" + e.Message.ToString());
				}
			}
			else
			{
				try
				{
					BancoDeDados.executar("update produtos set nome=?param1,precoUnidade=?param2 where id=?param3",
										  new object[] { b.nome, b.precoUnitario, b.id });
				}
				catch (Exception ex)
				{
					throw new Exception("Falha ao atualizar produto.\n" + ex.Message);
				}
			}
		}
	}
}
