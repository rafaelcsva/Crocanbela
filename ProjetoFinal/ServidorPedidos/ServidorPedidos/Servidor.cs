using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorPedidos.Modelo;
using System.IO;
using Newtonsoft.Json.Linq;
using ServidorNomes;
using System.Threading;

namespace ServidorPedidos
{
	public class Servidor : Pedidos.PedidosBase
    {
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
                var hostUser = conf["pedidos"]["host"].ToString();
                var portaUser = Int32.Parse(conf["pedidos"]["porta"].ToString());

                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);

                RegistroServico registro = new RegistroServico();
                registro.Host = hostUser;
                registro.Porta = portaUser;
                registro.Servico = "Pedido";

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

		public override Task<PedidoResponse> Salvar(RegistroPedido rproduto, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var produto = new Pedido(rproduto);
                var response = new PedidoResponse();

                try
                {
					AtualizarServidor();
                    Pedido.Salvar(produto);

					response.Error = 0;
                    response.Message = "Salvo com sucesso!";
                    response.Pedido = produto.toRegistroPedido();
                }
                catch (Exception e)
                {
                    response.Message = e.Message;
                    response.Error = 1;

                    return response;
                }

                return response;
            });
        }

        public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var resultado = new Resultado();

                try
                {
					AtualizarServidor();

                    if (modo.Tipo == ModoBusca.Types.Modo.Id)
                    {
                        var produto = Pedido.BuscarPorId(modo.Id);

                        resultado.Pedidos.Add(produto.toRegistroPedido());
                    }
                    else if (modo.Tipo == ModoBusca.Types.Modo.Todos)
                    {
                        var produtos = Pedido.Buscar();

                        for (int i = 0; i < produtos.Length; i++)
                        {
                            resultado.Pedidos.Add(produtos[i].toRegistroPedido());
                        }
                    }
                }
                catch (Exception e)
                {
                    resultado.Response = new PedidoResponse { Message = e.Message, Error = 1 };

                    return resultado;
                }

                resultado.Response = new PedidoResponse { Message = "Busca ocorrida com sucesso!", Error = 0 };

                return resultado;
            });
        }

        public override Task<PedidoResponse> Excluir(RegistroPedido rproduto, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new PedidoResponse();
                var produto = new Pedido(rproduto);

                try
                {
					AtualizarServidor();

                    Pedido.Excluir(produto);
                }
                catch (Exception e)
                {
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
