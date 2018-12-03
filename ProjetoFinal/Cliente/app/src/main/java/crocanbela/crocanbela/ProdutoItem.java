package crocanbela.crocanbela;

import ServidorPedidos.Order;

/**
 * Created by perninha on 27/11/18.
 */

public class ProdutoItem {
    public int idProduto;
    public String nome;
    public int qtd;
    public double valor;

    public ProdutoItem(){

    }

    public ProdutoItem(Order.ProdutoItem item){
        this.idProduto = item.getIdProduto();
        this.nome = item.getNome();
        this.qtd = item.getQtd();
        this.valor = item.getValor();
    }

    @Override
    public String toString(){
        return "Nome: " + this.nome + "|Qtd:" + this.qtd + "|Valor:" + Double.toString(this.valor * this.qtd);
    }
}
