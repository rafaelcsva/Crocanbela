using System;
using Grpc.Core;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ServidorAutenticacao
{
	public class Servidor : Autenticacao.AutenticacaoBase
    {
		public override Task<AutResponse> Autenticar(ServidorUsuarios.RegistroUsuario registroUsuario, ServerCallContext context){
			return Task.Run(() =>
			{
				var file = File.ReadAllText("./Config/Info.json");
				var response = new AutResponse();

				try
				{
					var conf = JObject.Parse(file);
					var hostNome = conf["nomes"]["host"].ToString();
					var portNome = conf["nomes"]["porta"].ToString();

					var channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);
					var clientNomes = new ServidorNomes.Nomes.NomesClient(channel);
					var req = new ServidorNomes.ServicoRequest { Servico = "Usuario" };
					var resp = clientNomes.ObterServico(req);

					if(resp.Error != 0){
						throw new Exception("Falha ao tentar autenticar.\n" + resp.Message);
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
						throw new Exception("Falha ao autenticar.\n" + reg.Message.Message);
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
					response.Message = e.Message;

					return response;
				}

				return response;
			});
		}

    }
}
