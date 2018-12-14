package crocanbela.crocanbela;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;

import ServidorAutenticacao.AutenticacaoGrpc;
import ServidorAutenticacao.Auth;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorUsuarios.User;
import ServidorUsuarios.UsuariosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class UsuarioEdit extends AppCompatActivity implements View.OnClickListener{

    EditText txtLogin, txtSenha;
    Button btSalvar, btExcluir;
    private ProgressBar progress;
    private TextView txtProgress;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_usuario_edit);
        Globals gb = (Globals) getApplication();

        txtLogin = (EditText) findViewById(R.id.txtLogin);
        txtSenha = (EditText) findViewById(R.id.txtSenha);
        txtProgress = (TextView) findViewById(R.id.txtStatus);
        progress = (ProgressBar) findViewById(R.id.progressB);
        btSalvar = (Button) findViewById(R.id.btSalvar);
        btExcluir = (Button) findViewById(R.id.btExcluirUsuario);

        if(gb.curUserA) {
            txtLogin.setText(gb.curUser.login);
            txtSenha.setText(gb.curUser.senha);
        }

        btSalvar.setOnClickListener(this);
        btExcluir.setOnClickListener(this);
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals)getApplication();
        final String hostNome = gb.hostNome;
        final int portNome = gb.portNome;
        final TextView log = txtLogin;
        final TextView sen = txtSenha;
        final String act1, act2;

        if(v.getId() == R.id.btExcluirUsuario){
            act1 = "Excluir";
            act2 = "Excluindo";
        }else{
            act1 = "Salvar";
            act2 = "Salvando";
        }

        System.out.println("clickado! " + act1);
        progress.setVisibility(View.VISIBLE);
        txtProgress.setVisibility(View.VISIBLE);

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {
                        try{
                            ManagedChannel mChannel = ManagedChannelBuilder
                                    .forAddress(hostNome, portNome)
                                    .usePlaintext(true).build();

                            NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

                            System.out.println("Buscando servidor de usuarios");
                            a.AtualizarProgress("Buscando servidor de usuarios",
                                    false, false, progress, txtProgress);
                            System.out.println("Buscando servidor de usuarios(terminado)");

                            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                                    .setServico("Usuario").build();

                            Names.ServicoResponse reg = blockStub.obterServico(req);

                            if (reg.getError() != 0) {
                                throw new Exception(reg.getMessage());
                            }

                            System.out.println(reg.getMessage());
                            System.out.println(reg.getServico().getHost());
                            System.out.println(reg.getServico().getPorta());

                            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                                    reg.getServico().getPorta()).usePlaintext(true).build();

                            a.AtualizarProgress(act2, false, false, progress,
                                    txtProgress);

                            UsuariosGrpc.UsuariosBlockingStub blockStubUser =
                                    UsuariosGrpc.newBlockingStub(mChannel);

                            User.RegistroUsuario user = User.RegistroUsuario.newBuilder()
                                    .setLogin(txtLogin.getText().toString())
                                    .setSenha(txtSenha.getText().toString())
                                    .setId(gb.curUser.id)
                                    .build();

                            if(act1 == "Salvar") {
                                User.UsuarioResponse resp = blockStubUser.salvar(user);

                                if (resp.getError() != 0) {
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Salvo com sucesso!", false, true, progress
                                        , txtProgress);

                                System.out.println(resp.getMessage());
                                System.out.println(resp.getRusuario().getId());
                                System.out.println(resp.getRusuario().getLogin());
                                System.out.println(resp.getRusuario().getSenha());

                                if (gb.curUser.id == 0) {
                                    gb.curUser.id = resp.getRusuario().getId();
                                }
                            }else{
                                User.UsuarioResponse resp = blockStubUser.excluir(user);

                                if(resp.getError() != 0){
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Excluido com sucesso!", false, true, progress
                                        , txtProgress);

                                gb.curUser = new Usuario(User.RegistroUsuario.newBuilder().build());

                                txtLogin.setText("");
                                txtSenha.setText("");
                            }
                        } catch (Exception e) {
                            a.AtualizarProgress("Falha ao " + act1 + "\n" +
                                    e.getMessage(), true, false, progress, txtProgress);
                            return;
                        }
                    }
                }
        ).start();
    }

}
