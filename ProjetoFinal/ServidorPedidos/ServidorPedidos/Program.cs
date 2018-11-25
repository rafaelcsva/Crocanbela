using System;
using System.IO;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using ServidorNomes;

namespace ServidorPedidos
{
    class Program
    {
		static void Main(string[] args)
		{
			var file = File.ReadAllText("./Config/Info.json");

			try
			{
				var conf = JObject.Parse(file);

				var hostProduto = conf["pedidos"]["host"].ToString();
				var portaProduto = Int32.Parse(conf["pedidos"]["porta"].ToString());

				Server server = new Server
				{
					Services = { Pedidos.BindService(new Servidor()) },
					Ports = { new ServerPort(hostProduto, portaProduto, ServerCredentials.Insecure) }
				};

				server.Start();
				Console.WriteLine("Servidor de pedidos Ativo!");
                
				Console.WriteLine("Conectando com o servidor de nomes para registrar servico!");

				var hostNome = conf["nomes"]["host"].ToString();
				var portNome = conf["nomes"]["porta"].ToString();

				Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

				var client = new Nomes.NomesClient(channel);

				var resp = client.Cadastrar(new RegistroServico
				{
					Host = hostProduto,
					Porta = portaProduto,
					Servico = "Pedido"
				});

				if (resp.Error != 0)
				{
					throw new Exception("Erro ao cadastrar servico!\n" + resp.Message);
				}

				Console.WriteLine(resp.Message);

				var exitEvent = new System.Threading.ManualResetEvent(false);

				Console.CancelKeyPress += (sender, e) => exitEvent.Set();

				exitEvent.WaitOne();

				server.ShutdownAsync().Wait();

			}
			catch (Exception e)
			{
				Console.WriteLine("Erro ocorrido ao iniciar servidor de pedidos!\n" + e.Message);
			}
		}
    }
}
