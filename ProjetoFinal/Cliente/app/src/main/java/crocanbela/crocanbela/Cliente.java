package crocanbela.crocanbela;

import java.util.Date;

import ServidorClientes.Client;

/**
 * Created by perninha on 16/11/18.
 */

class Cliente {
    public int id;
    String nome, email, telefone;
    String dataCadastro;

    public Cliente(Client.RegistroCliente regcliente){
        this.id = regcliente.getId();
        this.nome = regcliente.getNome();
        this.email = regcliente.getEmail();
        this.telefone = regcliente.getTelefone();
        this.dataCadastro = regcliente.getDataCadastro();
    }

    @Override
    public String toString(){
        return "ID: " + this.id + " Nome: " + this.nome + " Telefone: " + this.telefone;
    }
}
