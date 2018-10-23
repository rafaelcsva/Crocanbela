using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorUsuarios.Modelo;

namespace ServidorUsuarios
{
	public class Servidor : Usuarios.UsuariosBase
	{
		public override Task<UsuarioResponse> Salvar(RegistroUsuario rusuario, ServerCallContext context)
		{
			return Task.Run(() =>
			{
				var usuario = new Usuario(rusuario);
				var response = new UsuarioResponse();
				response.Error = 0;

				try
				{
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
