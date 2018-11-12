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
    public String hostNome = "192.168.25.10";
    public int portNome = 1808;
}
