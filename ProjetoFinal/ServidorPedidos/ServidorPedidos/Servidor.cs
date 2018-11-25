using System;
using Grpc.Core;
using System.Threading.Tasks;
using ServidorPedidos.Modelo;

namespace ServidorPedidos
{
	public class Servidor : Pedidos.PedidosBase
    {
		public override Task<PedidoResponse> Salvar(RegistroPedido rproduto, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var produto = new Pedido(rproduto);
                var response = new PedidoResponse();

                try
                {
                    Pedido.Salvar(produto);

					response.Error = 0;
                    response.Message = "Salvo com sucesso!";
                    response.Pedido = produto.toRegistroPedido();
                }
                catch (Exception e)
                {
                    response.Message = e.Message;
                    response.Error = 1;

                    return response;
                }

                return response;
            });
        }

        public override Task<Resultado> Buscar(ModoBusca modo, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var resultado = new Resultado();

                try
                {
                    if (modo.Tipo == ModoBusca.Types.Modo.Id)
                    {
                        var produto = Pedido.BuscarPorId(modo.Id);

                        resultado.Pedidos.Add(produto.toRegistroPedido());
                    }
                    else if (modo.Tipo == ModoBusca.Types.Modo.Todos)
                    {
                        var produtos = Pedido.Buscar();

                        for (int i = 0; i < produtos.Length; i++)
                        {
                            resultado.Pedidos.Add(produtos[i].toRegistroPedido());
                        }
                    }
                }
                catch (Exception e)
                {
                    resultado.Response = new PedidoResponse { Message = e.Message, Error = 1 };

                    return resultado;
                }

                resultado.Response = new PedidoResponse { Message = "Busca ocorrida com sucesso!", Error = 0 };

                return resultado;
            });
        }

        public override Task<PedidoResponse> Excluir(RegistroPedido rproduto, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var response = new PedidoResponse();
                var produto = new Pedido(rproduto);

                try
                {
                    Pedido.Excluir(produto);
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
    }
}
