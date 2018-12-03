package crocanbela.crocanbela;

import android.app.Application;

import ServidorUsuarios.User;

/**
 * Created by perninha on 31/10/18.
 */

public class Globals extends Application{
    public User.RegistroUsuario usuarioLogado;
    public Usuario curUser;
    public Boolean curUserA;
    public Cliente curClient;
    public Boolean curClientA;
    public Produto curProduto;
    public Boolean curProdutoA;
    public Pedido curPedido;
    public Boolean curPedidoA;
    public String hostNome = "192.168.0.24";
    public int portNome = 1808;
}
