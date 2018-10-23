using System;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ServidorAutenticacao
{
    class Program
    {
        static void Main(string[] args)
        {
			var file = File.ReadAllText("./Config/Info.json");

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

                server.Start();
                Console.WriteLine("Servidor de Autenticacao Ativo!");

                Console.WriteLine("Conectando com o servidor de nomes para registrar servico!");

                var hostNome = conf["nomes"]["host"].ToString();
                var portNome = conf["nomes"]["porta"].ToString();
                
                var channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new ServidorNomes.Nomes.NomesClient(channel);

                var resp = client.Cadastrar(new ServidorNomes.RegistroServico
                {
					Host = hostAut,
					Porta = portaAut,
                    Servico = "Autenticacao"
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
                Console.WriteLine("Erro ocorrido ao iniciar servidor de autenticacao!\n" + e.Message);
            }
        }
    }
}