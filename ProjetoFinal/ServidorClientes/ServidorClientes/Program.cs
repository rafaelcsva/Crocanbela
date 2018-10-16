using System;
using System.IO;
using ServidorNomes;
using Grpc.Core;
using Newtonsoft.Json.Linq;

namespace ServidorClientes
{
	class Program
	{
		static void Main(string[] args)
		{
			var file = File.ReadAllText("./Config/Info.json");

			try
			{
				var conf = JObject.Parse(file);

				var hostClient = conf["clientes"]["host"].ToString();
				var portaClient = Int32.Parse(conf["clientes"]["porta"].ToString());

				Server server = new Server
				{
					Services = { Clientes.BindService(new Servidor()) },
					Ports = { new ServerPort(hostClient, portaClient, ServerCredentials.Insecure) }
				};

				server.Start();
				Console.WriteLine("Servidor de Clientes Ativo!");

				Console.WriteLine("Conectando com o servidor de nomes para registrar servico!");

				var hostNome = conf["nomes"]["host"].ToString();
				var portNome = conf["nomes"]["porta"].ToString();

				Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

				var client = new Nomes.NomesClient(channel);

				var resp = client.Cadastrar(new RegistroServico { Host = "localhost", Porta = 1808, Servico = "Cliente" });

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
				Console.WriteLine("Erro ocorrido ao iniciar servidor de clientes!\n" + e.Message);
			}


		}
	}
}