using System;
using System.IO;
using ServidorNomes;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using ServidorClientes.Modelo;
using System.Net;
using System.Text;

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

				RegistroServico registro = new RegistroServico();
				registro.Host = hostClient;
				registro.Porta = portaClient;
                registro.Servico = "Cliente";

                registro.Estado = new Estado();
                registro.Estado.Cpu = Diagnostico.ObterUsoCpu();
                registro.Estado.Memoria = Diagnostico.ObterUsoMemoria();

                var resp = client.Cadastrar(registro);

				if (resp.Error != 0)
				{
					throw new Exception("Erro ao cadastrar servico!\n" + resp.Message);
				}

				Console.WriteLine(resp.Message);

				Console.WriteLine("Levantando listener udp para aguardar conexoes de confirmacao de ativo");
                Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpClient listener = new UdpClient(portaClient);
				IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, portaClient);

                while (true)
                {
                    Console.WriteLine("Tentando receber algo");
                    byte[] x = listener.Receive(ref groupEP);
                    Console.WriteLine("Recebi conexao!");
                    string mess = Encoding.ASCII.GetString(x);

					IPAddress broadcast = IPAddress.Parse(hostNome);

                    byte[] sendbuf = Encoding.ASCII.GetBytes("Servidor Ativo");
                    IPEndPoint ep = new IPEndPoint(broadcast, Convert.ToInt32(portNome));

                    receiver.SendTo(sendbuf, ep);
                }            
			}
			catch (Exception e)
			{
				Console.WriteLine("Erro ocorrido ao iniciar servidor de clientes!\n" + e.Message);
			}


		}
	}
}