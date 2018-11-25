using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorProdutos.Modelo;

namespace ServidorProdutos{
	public class Servidor : Produtos.ProdutosBase{      

		public override Task<ProdutoResponse> Salvar(RegistroProduto rproduto, ServerCallContext context)
		{
			return Task.Run(() =>
			{
				var produto = new Produto(rproduto);
				var response = new ProdutoResponse();
                
				try{
					Produto.Salvar(produto);
				}catch(Exception e){
					response.Message = e.Message;
					response.Error = 1;

					return response;
				}

				response.Message = "Salvo com sucesso!";
				response.Rprodutor = produto.toRegistroProduto();

				return response;
			});
		}

		public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context){
			return Task.Run(() =>
			{
				var resultado = new Resultado();

				try{
					if(modo.Tipo == ModoBusca.Types.Modo.Id){
						var produto = Produto.BuscarPorId(modo.Id);
                            
						resultado.Produtos.Add(produto.toRegistroProduto());
					}else if(modo.Tipo == ModoBusca.Types.Modo.Todos){
						var produtos = Produto.Buscar();

						for(int i = 0 ; i < produtos.Length ; i++){
							resultado.Produtos.Add(produtos[i].toRegistroProduto());
						}
					}
				}catch(Exception e){
					resultado.Response = new ProdutoResponse { Message = e.Message, Error = 1 };

					return resultado;
				}

				resultado.Response = new ProdutoResponse { Message = "Busca ocorrida com sucesso!", Error = 0 };

				return resultado;
			});
		}

		public override Task<ProdutoResponse> Excluir(RegistroProduto rproduto, ServerCallContext context){
			return Task.Run(() => 
			{
				var response = new ProdutoResponse();
				var produto = new Produto(rproduto);

				try{
					Produto.Excluir(produto);
				}catch(Exception e){
					response.Message = e.Message;
					response.Error = 1;

					return response;
				}

				response.Message = "Excluido com sucesso!";

				return response;
			});
		}
	}
}