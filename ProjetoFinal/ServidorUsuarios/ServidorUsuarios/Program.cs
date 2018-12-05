using System;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System.IO;
using ServidorNomes;
using System.Net.Sockets;
using ServidorUsuarios.Modelo;
using System.Net;
using System.Text;

namespace ServidorUsuarios
{
    class Program
    {
		

        static void Main(string[] args)
        {
			var file = File.ReadAllText("./Config/Info.json");

            try
            {
                var conf = JObject.Parse(file);

                var hostUser = conf["usuarios"]["host"].ToString();
                var portaUser = Int32.Parse(conf["usuarios"]["porta"].ToString());

                Server server = new Server
                {
                    Services = { Usuarios.BindService(new Servidor()) },
                    Ports = { new ServerPort(hostUser, portaUser, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine("Servidor de Usuarios Ativo!");

                Console.WriteLine("Conectando com o servidor de nomes para registrar servico!");

                var hostNome = conf["nomes"]["host"].ToString();
                var portNome = conf["nomes"]["porta"].ToString();
    
                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);
                
				RegistroServico registro = new RegistroServico();
				registro.Host = hostUser;
				registro.Porta = portaUser;
				registro.Servico = "Usuario";
                
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
                UdpClient listener = new UdpClient(portaUser);
				IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, portaUser);
                
                while (true)
                {
					Console.WriteLine("Tentando receber algo");
                    byte[] x = listener.Receive(ref groupEP);
					Console.WriteLine("Recebi conexao!");
					string mess = Encoding.ASCII.GetString(x);

					IPAddress broadcast = IPAddress.Parse(hostUser);

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