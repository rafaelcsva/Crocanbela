using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using ServidorAutenticacao.Modelo;
using ServidorNomes;
using ServidorUsuarios;

namespace ServidorAutenticacao
{
	public class ServidorUsuario : Usuarios.UsuariosBase
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
                var hostUser = conf["usuarios"]["host"].ToString();
                var portaUser = Int32.Parse(conf["usuarios"]["porta"].ToString());

                Channel channel = new Channel(hostNome + ":" + portNome, ChannelCredentials.Insecure);

                var client = new Nomes.NomesClient(channel);

                RegistroServico registro = new RegistroServico();
                registro.Host = hostUser;
                registro.Porta = portaUser;
                registro.Servico = "Usuario";

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

        public override Task<UsuarioResponse> Salvar(RegistroUsuario rusuario, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var usuario = new Usuario(rusuario);
                var response = new UsuarioResponse();
                response.Error = 0;

                try
                {
           //         AtualizarServidor();
                    Usuario.Salvar(usuario);
                }
                catch (Exception e)
                {
                    response.Message = e.Message;
                    response.Error = 1;

                    return response;
                }

                response.Message = "Salvo com Sucesso!";

                response.Rusuario = usuario.toRegistroUsuario();

                return response;
            });
        }

        public override Task<UsuarioResponse> Excluir(RegistroUsuario registroUsuario, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var usuario = new Usuario(registroUsuario);
                var response = new UsuarioResponse();
                response.Error = 0;

                try
                {
             //       AtualizarServidor();
                    Usuario.Excluir(usuario);
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

        public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new Resultado();

                try
                {
                 //   AtualizarServidor();

                    if (modo.Tipo == ModoBusca.Types.Modo.Id)
                    {
                        var usuario = Usuario.BuscarPorId(modo.Id);

                        if (usuario != null)
                        {
                            response.Usuarios.Add(usuario.toRegistroUsuario());
                        }
                    }
                    else if (modo.Tipo == ModoBusca.Types.Modo.Nome)
                    {
                        var usuario = Usuario.BuscarPorLogin(modo.Login);

                        if (usuario != null)
                        {
                            response.Usuarios.Add(usuario.toRegistroUsuario());
                        }
                    }
                    else if (modo.Tipo == ModoBusca.Types.Modo.Todos)
                    {
                        var usuarios = Usuario.Buscar();

                        foreach (Usuario usuario in usuarios)
                        {
                            response.Usuarios.Add(usuario.toRegistroUsuario());
                        }

                    }
                    else
                    {
                        throw new Exception("Modo de busca nao reconhecido!");
                    }

                }
                catch (Exception e)
                {
                    response.Message = new UsuarioResponse { Message = e.Message, Error = 1 };

                    return response;
                }

                response.Message = new UsuarioResponse { Message = "Busca ocorrida com sucesso!", Error = 0 };

                return response;
            });
        }
    }
}
