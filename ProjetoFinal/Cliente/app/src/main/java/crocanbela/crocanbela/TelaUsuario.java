package crocanbela.crocanbela;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;

import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorUsuarios.User;
import ServidorUsuarios.UsuariosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class TelaUsuario extends AppCompatActivity implements AdapterView.OnItemClickListener{
    ListView listaUsuarios;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tela_usuario);

        try {
            List<Usuario> usuarios = obterUsuarios();

            listaUsuarios = (ListView) findViewById(R.id.listUsuarios);
            ArrayAdapter<Usuario> adapter = new ArrayAdapter<Usuario>(
                    this, android.R.layout.simple_list_item_1, usuarios
            );

            listaUsuarios.setAdapter(adapter);
            listaUsuarios.setOnItemClickListener(this);
        }catch (Exception e){
            System.out.println("Falha ao iniciar!\n" + e.getMessage());
        }
    }

    public void onItemClick(AdapterView<?> parent, View view, final int position,
                            long id) {
        final Globals gb = (Globals) getApplication();
        final TelaUsuario ctxUsuario = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {
                        Usuario user = (Usuario)listaUsuarios.getAdapter().getItem(position);

                        try{
                            gb.curUser = user;
                            gb.curUserA = true;
                        }catch (Exception e){
                            return;
                        }

                        Intent telaUser = new Intent(ctxUsuario, UsuarioEdit.class);
                        startActivity(telaUser);
                    }
                }
        ).start();



    }

    public ArrayList<Usuario> obterUsuarios() throws Exception {
        ArrayList<Usuario> list = new ArrayList<Usuario>();
        String hostNome = "192.168.25.10";
        int portNome = 1808;

        try{
            ManagedChannel mChannel = ManagedChannelBuilder.forAddress(hostNome, portNome)
                    .usePlaintext(true).build();

            NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                    .setServico("Usuario").build();

            Names.ServicoResponse reg = blockStub.obterServico(req);

            if (reg.getError() != 0) {
                throw new Exception(reg.getMessage());
            }

            System.out.println(reg.getServico().getHost());
            System.out.println(reg.getServico().getPorta());

            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                    reg.getServico().getPorta()).usePlaintext(true).build();

            UsuariosGrpc.UsuariosBlockingStub blockStubUsuarios = UsuariosGrpc.newBlockingStub(mChannel);
            User.ModoBusca mod = User.ModoBusca.newBuilder()
                    .setTipo(User.ModoBusca.Modo.TODOS).build();

            User.Resultado resultado = blockStubUsuarios.buscar(mod);

            for(int i = 0 ; i < resultado.getUsuariosCount() ; i++){
                list.add(new Usuario(resultado.getUsuarios(i)));
            }

        }catch (Exception e){
            throw new Exception("Falha ao obter Usuarios!\n" + e.getMessage());
        }

        return list;
    }

}
