using System;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Sockets;
using ServidorNomes;
using ServidorAutenticacao.Modelo;
using System.Net;
using System.Text;
using ServidorUsuarios;

namespace ServidorAutenticacao
{
    class Program
    {
        static void Main(string[] args)
        {
			var file = File.ReadAllText("./Config/Info.json");
			const int off_set = 10;

            try
            {
                var conf = JObject.Parse(file);

                var hostAut = conf["autenticacao"]["host"].ToString();
                var portaAut = Int32.Parse(conf["autenticacao"]["porta"].ToString());

                Server server = new Server
                {
					Services = { Autenticacao.BindService(new Servidor()) },
					Ports = { new ServerPort(hostAut, portaAut, ServerCredentials.Insecure) }
                };

				Server serverUsuario = new Server
				{
					Services = { Usuarios.BindService(new ServidorUsuario()) },
					Ports = { new ServerPort ( hostAut, portaAut + off_set, ServerCredentials.Insecure ) }
				};

                server.Start();
				serverUsuario.Start();

				Console.WriteLine("Servidor de Autenticacao Ativo!");

                Console.WriteLine("Conectando com o servidor de nomes para registrar servico!");

                var hostNome = conf["nomes"]["host"].ToString();
                var portNome = conf["nomes"]["porta"].ToString();
                
                var channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new ServidorNomes.Nomes.NomesClient(channel);

				RegistroServico registro = new RegistroServico();
				registro.Host = hostAut;
				registro.Porta = portaAut;
                registro.Servico = "Autenticacao";

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
                UdpClient listener = new UdpClient(portaAut);
				IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Convert.ToInt32(portaAut));

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
                Console.WriteLine("Erro ocorrido ao iniciar servidor de autenticacao!\n" + e.Message);
            }
        }
    }
}