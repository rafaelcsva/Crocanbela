package crocanbela.crocanbela;

import ServidorUsuarios.User;

class Usuario {
    public int id;
    public String login, senha;

    public Usuario(User.RegistroUsuario rusuario){
        this.id = rusuario.getId();
        this.login = rusuario.getLogin();
        this.senha = rusuario.getSenha();
    }

    @Override
    public String toString(){
        return "ID: " + id + " Login: " + this.login;
    }
}
