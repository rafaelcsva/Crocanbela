using System;
using Grpc.Core;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using ServidorNomes;
using ServidorAutenticacao.Modelo;

namespace ServidorAutenticacao
{
	public class Servidor : Autenticacao.AutenticacaoBase
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
                var hostUser = conf["autenticacao"]["host"].ToString();
                var portaUser = Int32.Parse(conf["autenticacao"]["porta"].ToString());

                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);

                RegistroServico registro = new RegistroServico();
                registro.Host = hostUser;
                registro.Porta = portaUser;
                registro.Servico = "Autenticacao";

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

		public override Task<AutResponse> Autenticar(ServidorUsuarios.RegistroUsuario registroUsuario, ServerCallContext context){
			return Task.Run(() =>
			{
				var file = File.ReadAllText("./Config/Info.json");
				var response = new AutResponse();

				try
				{
					AtualizarServidor();
					var conf = JObject.Parse(file);
					var hostNome = conf["nomes"]["host"].ToString();
					var portNome = conf["nomes"]["porta"].ToString();

					var channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);
					var clientNomes = new ServidorNomes.Nomes.NomesClient(channel);
					var req = new ServidorNomes.ServicoRequest { Servico = "Usuario" };
					var resp = clientNomes.ObterServico(req);

					if(resp.Error != 0){
						throw new Exception("Falha ao tentar autenticar2.\n" + resp.Message);
					}

					channel = new Channel(resp.Servico.Host + ":" + resp.Servico.Porta, ChannelCredentials.Insecure);
					var clientUsuarios = new ServidorUsuarios.Usuarios.UsuariosClient(channel);
     

					var mode = new ServidorUsuarios.ModoBusca
					{
						Tipo = ServidorUsuarios.ModoBusca.Types.Modo.Nome,
						Login = registroUsuario.Login
					};

					var reg = clientUsuarios.Buscar(mode);

					if(reg.Message.Error != 0){
						throw new Exception("Falha ao autenticar1.\n" + reg.Message.Message);
					}
                    
					if(reg.Usuarios.Count == 0){
						throw new Exception("Usuario invalido!");
					}

					if(registroUsuario.Senha != reg.Usuarios[0].Senha){
						throw new Exception("Falha ao autenticar, a senha eh invalida!");	
					}

					response.Error = 0;
					response.Message = "Autenticado com sucesso!";

                    
					response.Rusuario = reg.Usuarios[0];
                    
				}catch(Exception e){
					response.Error = 1;
					response.Message = e.Message + " eu nao toco raul";

					return response;
				}

				return response;
			});
		}

    }
}
