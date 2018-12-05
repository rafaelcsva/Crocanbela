using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorClientes.Modelo;
using System.IO;
using Newtonsoft.Json.Linq;
using ServidorNomes;
using System.Threading;

namespace ServidorClientes
{
	public class Servidor : Clientes.ClientesBase
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
                var hostUser = conf["clientes"]["host"].ToString();
                var portaUser = Int32.Parse(conf["clientes"]["porta"].ToString());

                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);

                RegistroServico registro = new RegistroServico();
                registro.Host = hostUser;
                registro.Porta = portaUser;
                registro.Servico = "Cliente";

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

		public override Task<ClienteResponse> Salvar(RegistroCliente rcliente, ServerCallContext context){
			return Task.Run(() =>
			{
				var cliente = new Cliente(rcliente);
				var response = new ClienteResponse();
				response.Error = 0;

				try
				{
					AtualizarServidor();
					Cliente.Salvar(cliente);
				}
				catch (Exception e)
				{
					response.Message = e.Message;
					response.Error = 1;

					return response;
				}

				response.Message = "Salvo com Sucesso!";
				response.Rcliente = cliente.toRegistroCliente();

				return response;
			});

		}

		public override Task<ClienteResponse> Excluir(RegistroCliente registroCliente, ServerCallContext context){
			return Task.Run(() =>
			{
				var cliente = new Cliente(registroCliente);
				var response = new ClienteResponse();
				response.Error = 0;

				try
				{
					AtualizarServidor();
					Cliente.Excluir(cliente);
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
  
		public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context){
			return Task.Run(() =>
			{

				var response = new Resultado();

				try
				{
					AtualizarServidor();
					if (modo.Tipo == ModoBusca.Types.Modo.Id)
					{
						var cliente = Cliente.buscarPorId(modo.Id);

						response.Clientes.Add(cliente.toRegistroCliente());
					}
					else if (modo.Tipo == ModoBusca.Types.Modo.Nome)
					{
						var cliente = Cliente.buscarPorNome(modo.Nome);

						response.Clientes.Add(cliente.toRegistroCliente());

					}
					else if (modo.Tipo == ModoBusca.Types.Modo.Todos)
					{
						var clientes = Cliente.Buscar();

						foreach (Cliente cliente in clientes)
						{
							response.Clientes.Add(cliente.toRegistroCliente());
						}

					}
					else
					{
						throw new Exception("Modo de busca nao reconhecido!");
					}

				}
				catch (Exception e)
				{
					response.Message = new ClienteResponse { Message = e.Message, Error = 1 };

					return response;
				}

				response.Message = new ClienteResponse { Message = "Busca ocorrida com sucesso!", Error = 0 };

				return response;
			});
		}
    }
}
