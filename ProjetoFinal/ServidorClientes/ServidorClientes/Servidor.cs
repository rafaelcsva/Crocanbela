using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorClientes.Modelo;

namespace ServidorClientes
{
	public class Servidor : Clientes.ClientesBase
    {
		public override Task<ClienteResponse> Salvar(RegistroCliente rcliente, ServerCallContext context){
			var cliente = new Cliente(rcliente);
			var response = new ClienteResponse();

			try{
				Cliente.Salvar(cliente);
			}catch(Exception e){
				response.Message = e.Message;

				return Task.FromResult(response);
			}

			response.Message = "Salvo com Sucesso!";

			return Task.FromResult(response);
		}
  
		public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context){
			var response = new Resultado();

			try
			{
				if (modo.Tipo == ModoBusca.Types.Modo.Id)
				{
					var cliente = Cliente.buscarPorId(modo.Id);

					response.Clientes.Add(cliente.toRegistroCliente());
				}
				else if (modo.Tipo == ModoBusca.Types.Modo.Nome)
				{
					var cliente = Cliente.buscarPorNome(modo.Nome);

					response.Clientes.Add(cliente.toRegistroCliente());

				}else if (modo.Tipo == ModoBusca.Types.Modo.Todos){
					var clientes = Cliente.Buscar();
                    
					foreach(Cliente cliente in clientes){
						response.Clientes.Add(cliente.toRegistroCliente());
					}

				}else{
					throw new Exception("Modo de busca nao reconhecido!");
				}

			}catch(Exception e){
				response.Message = new ClienteResponse { Message = e.Message };
				response.Error = 1;

				return Task.FromResult(response);
			}

			response.Message = new ClienteResponse { Message = "Busca ocorrida com sucesso!" };

			return Task.FromResult(response);
		}
    }
}
