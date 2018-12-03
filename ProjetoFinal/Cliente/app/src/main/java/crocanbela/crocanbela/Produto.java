package crocanbela.crocanbela;

import ServidorProdutos.Product;

/**
 * Created by perninha on 26/11/18.
 */

public class Produto {
    public int id;
    String nome;
    double precoUnitario;
    String dataCadastro;

    public Produto(){

    }

    public Produto(Product.RegistroProduto reg){
        this.id = reg.getId();
        this.nome = reg.getNome();
        this.precoUnitario = reg.getPrecoUnidade();
        this.dataCadastro = reg.getDataCadastro();
    }

    @Override
    public String toString(){
        return this.nome;
    }
}
