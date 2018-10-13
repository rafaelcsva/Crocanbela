using System;
using ServidorNomes;
using Grpc.Core;

namespace ServidorClientes
{
    class Program
    {
		const int Port = 50051;

        static void Main(string[] args)
        {
			/*const string host = "localhost";
			const int port = 1808;

			Channel channel = new Channel(host + ":" + port.ToString(), ChannelCredentials.Insecure);

			var client = new Nomes.NomesClient(channel);

			var resp = client.Cadastrar(new RegistroServico { Host = "localhost", Porta = 1808, Servico = "Cliente" });
			Console.WriteLine(resp.Message);*/
			Server server = new Server
			{
				Services = { Clientes.BindService(new Servidor()) },
				Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
			};

			server.Start();

			Console.WriteLine("Servidor de Clientes Ativo!");
        }
    }
}