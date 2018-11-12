package crocanbela.crocanbela;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;


import ServidorAutenticacao.AutenticacaoGrpc;
import ServidorAutenticacao.Auth;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorUsuarios.User;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class Login extends AppCompatActivity implements View.OnClickListener{

    private ProgressBar progress;
    private TextView txtProgress;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        progress = (ProgressBar)findViewById(R.id.progressLogin);
        txtProgress = (TextView)findViewById(R.id.txtProgressLogin);

        Button btEnt = (Button)findViewById(R.id.btEntrar);
        btEnt.setOnClickListener(this);
    }

    public void onClick(View v) {
        final Globals gb = (Globals) getApplication();
        final String hostNome = gb.hostNome;
        final int portNome = gb.portNome;

        final TextView txtLogin = (TextView) findViewById(R.id.txtLogin1);
        final TextView txtSenha = (TextView) findViewById(R.id.txtSenha1);
        final Login ctxLogin = this;
        System.out.println("clickado!");
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

                            System.out.println("Buscando servidor de autenticacao");
                            a.AtualizarProgress("Buscando servidor de autenticacao",
                                    false, false, progress, txtProgress);
                            System.out.println("Buscando servidor de autenticacao(terminado)");

                            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                                    .setServico("Autenticacao").build();

                            Names.ServicoResponse reg = blockStub.obterServico(req);

                            if (reg.getError() != 0) {
                                throw new Exception(reg.getMessage());
                            }

                            System.out.println(reg.getMessage());
                            System.out.println(reg.getServico().getHost());
                            System.out.println(reg.getServico().getPorta());

                            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                                    reg.getServico().getPorta()).usePlaintext(true).build();

                            a.AtualizarProgress("Autenticando", false, false, progress,
                                    txtProgress);

                            AutenticacaoGrpc.AutenticacaoBlockingStub blockStubAuth =
                                    AutenticacaoGrpc.newBlockingStub(mChannel);

                            User.RegistroUsuario user = User.RegistroUsuario.newBuilder()
                                    .setLogin(txtLogin.getText().toString())
                                    .setSenha(txtSenha.getText().toString()).build();

                            Auth.AutResponse resp = blockStubAuth.autenticar(user);

                            if (resp.getError() != 0) {
                                throw new Exception(resp.getMessage());
                            }

                            a.AtualizarProgress("Autenticado com sucesso!", false, true, progress
                            , txtProgress);

                            System.out.println(resp.getMessage());
                            System.out.println(resp.getRusuario().getId());
                            System.out.println(resp.getRusuario().getLogin());
                            System.out.println(resp.getRusuario().getSenha());

                            gb.usuarioLogado = resp.getRusuario();

                        } catch (Exception e) {
                            a.AtualizarProgress("Falha ao autenticar.\n" +
                                    e.getMessage(), true, false, progress, txtProgress);
                            return;
                        }

                        Intent telaP = new Intent(ctxLogin, TelaPrincipal.class);
                        startActivity(telaP);
                    }
                }
        ).start();

    }

}
