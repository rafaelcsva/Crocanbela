using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorProdutos.Modelo;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using ServidorNomes;

namespace ServidorProdutos{
	public class Servidor : Produtos.ProdutosBase{      
		private static Mutex mt = new Mutex();
        DateTime ultimaAtualizacao = DateTime.Now;
        const int maximoTempo = 30;

        public void AtualizarServidor()
        {
            try
            {
                mt.WaitOne();
                int diff = Convert.ToInt32(DateTime.Now.Subtract(ultimaAtualizacao).TotalSeconds);

                if (diff < maximoTempo)
                {
                    mt.ReleaseMutex();
                    return;
                }

                var file = File.ReadAllText("./Config/Info.json");
                var conf = JObject.Parse(file);

                var hostNome = conf["nomes"]["host"].ToString();
                var portNome = conf["nomes"]["porta"].ToString();
                var hostUser = conf["produtos"]["host"].ToString();
                var portaUser = Int32.Parse(conf["produtos"]["porta"].ToString());

                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);

                RegistroServico registro = new RegistroServico();
                registro.Host = hostUser;
                registro.Porta = portaUser;
                registro.Servico = "Produto";

                registro.Estado = new Estado();
                registro.Estado.Cpu = Diagnostico.ObterUsoCpu();
                registro.Estado.Memoria = Diagnostico.ObterUsoMemoria();

                var resp = client.AtualizarEstado(registro);

                if (resp.Error != 0)
                {
                    throw new Exception(resp.Message);
                }

            }
            catch (Exception e)
            {
                mt.ReleaseMutex();
                throw new Exception("Falha ao atualizar dados do servidor!\n" + e.Message);
            }

            ultimaAtualizacao = DateTime.Now;
            mt.ReleaseMutex();
        }

		public override Task<ProdutoResponse> Salvar(RegistroProduto rproduto, ServerCallContext context)
		{
			return Task.Run(() =>
			{
				var produto = new Produto(rproduto);
				var response = new ProdutoResponse();
                
				try{
					AtualizarServidor();
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
					AtualizarServidor();

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
					AtualizarServidor();

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