using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Grpc.Core;
using Newtonsoft.Json.Linq;

namespace ServidorPedidos.Modelo
{
	public class Produto
	{
		public int Id { get; set; }
		public string nome { get; set; }
		public double precoUnitario { get; set; }
		public string dataCadastro { get; set; }

		public Produto(ServidorProdutos.RegistroProduto produto){
			this.Id = produto.Id;
			this.nome = produto.Nome;
			this.precoUnitario = produto.PrecoUnidade;
			this.dataCadastro = produto.DataCadastro;
		}

		public Produto(){
			
		}

		public static Produto BuscarPorId(int id)
		{
			Produto produto;

			try
			{
				var file = File.ReadAllText("./Config/Info.json");
				var conf = JObject.Parse(file);

				var channel = new Channel(conf["nomes"]["host"] + ":" + conf["nomes"]["porta"], ChannelCredentials.Insecure);

				var client = new ServidorNomes.Nomes.NomesClient(channel);
				var req = new ServidorNomes.ServicoRequest();
				req.Servico = "Produto";

				var rep = client.ObterServico(req);

				if (rep.Error != 0)
				{
					throw new Exception("Falha ao obter produtox!\n" + rep.Message);
				}

				//Consegui o servico..

				channel = new Channel(rep.Servico.Host + ":" + rep.Servico.Porta, ChannelCredentials.Insecure);

				var clientP = new ServidorProdutos.Produtos.ProdutosClient(channel);
				var request = new ServidorProdutos.ModoBusca();
				request.Tipo = ServidorProdutos.ModoBusca.Types.Modo.Id;
				request.Id = id;

				var resp = clientP.Buscar(request);

				if(resp.Response.Error != 0){
					throw new Exception("Falha ao obter produtoy!\n" + resp.Response.Message + " " + id.ToString());
				}

				produto = new Produto(resp.Produtos[0]);
			} 
			catch (Exception e)
			{
				throw e;
			}

			return produto;
		}
	}
}
