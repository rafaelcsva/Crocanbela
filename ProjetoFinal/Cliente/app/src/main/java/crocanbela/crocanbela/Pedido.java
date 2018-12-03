package crocanbela.crocanbela;

import java.util.ArrayList;
import java.util.List;

import ServidorPedidos.Order;

/**
 * Created by perninha on 27/11/18.
 */

public class Pedido {
    public int id;
    public List<ProdutoItem> itens = new ArrayList<ProdutoItem>();
    public String dataEntrada, dataEntrega;
    public String endereco;
    public String telefone;
    public String email;
    public String observacao;
    public String cliente;

    public Pedido(){

    }

    public Pedido(Order.RegistroPedido reg){
        this.id = reg.getId();

        for(int i = 0 ; i < reg.getItensCount() ; i++){
            itens.add(new ProdutoItem(reg.getItens(i)));
        }

        this.dataEntrada = reg.getDataEntrada();
        this.dataEntrega = reg.getDataEntrega();
        this.endereco = reg.getEndereco();
        this.telefone = reg.getTelefone();
        this.email = reg.getEmail();
        this.observacao = reg.getObservacao();
        this.cliente = reg.getCliente();
    }

    @Override
    public String toString(){
        return "Num: " + this.id + "Cliente: " + this.cliente;
    }
}
