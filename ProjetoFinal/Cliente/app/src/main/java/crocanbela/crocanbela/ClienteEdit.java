package crocanbela.crocanbela;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.time.LocalDateTime;
import java.util.Date;

import ServidorClientes.Client;
import ServidorClientes.ClientesGrpc;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorUsuarios.User;
import ServidorUsuarios.UsuariosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class ClienteEdit extends AppCompatActivity implements View.OnClickListener{
    EditText txtNome, txtTel, txtEmail;
    ProgressBar progress;
    TextView txtProgress, txtData;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_cliente_edit);
        Globals gb = (Globals) getApplication();

        progress = (ProgressBar) findViewById(R.id.progressCliente);
        txtProgress = (TextView) findViewById(R.id.txtProgressCliente);
        txtNome = (EditText) findViewById(R.id.txtNomeCliente);
        txtTel = (EditText) findViewById(R.id.txtTelefoneCliente);
        txtEmail = (EditText) findViewById(R.id.txtEmailCliente);
        txtData = (TextView) findViewById(R.id.txtDataCliente);

        if(gb.curClientA){
            txtNome.setText(gb.curClient.nome);
            txtEmail.setText(gb.curClient.email);
            txtTel.setText(gb.curClient.telefone);
            txtData.setText("Cadastrado em: " + gb.curClient.dataCadastro);
        }

        Button btSalvar = (Button) findViewById(R.id.btSalvarCliente);
        Button btExcluir = (Button) findViewById(R.id.btExcluirClient);

        btSalvar.setOnClickListener(this);
        btExcluir.setOnClickListener(this);
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals)getApplication();
        final String hostNome = gb.hostNome;
        final int portNome = gb.portNome;
        final EditText nome = txtNome;
        final EditText telefone = txtTel;
        final EditText email = txtEmail;
        final TextView data = txtData;
        final String act1, act2;

        if(v.getId() == R.id.btSalvarCliente){
            act1 = "Salvar";
            act2 = "Salvando";
        }else{
            act1 = "Excluir";
            act2 = "Excluindo";
        }

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

                            System.out.println("Buscando servidor de clientes");
                            a.AtualizarProgress("Buscando servidor de clientes",
                                    false, false, progress, txtProgress);

                            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                                    .setServico("Cliente").build();

                            Names.ServicoResponse reg = blockStub.obterServico(req);

                            if (reg.getError() != 0) {
                                throw new Exception(reg.getMessage());
                            }

                            System.out.println("Buscando servidor de clientes(terminado)");

                            System.out.println(reg.getMessage());
                            System.out.println(reg.getServico().getHost());
                            System.out.println(reg.getServico().getPorta());

                            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                                    reg.getServico().getPorta()).usePlaintext(true).build();

                            a.AtualizarProgress(act2, false, false, progress,
                                    txtProgress);

                            ClientesGrpc.ClientesBlockingStub blockStubClient =
                                    ClientesGrpc.newBlockingStub(mChannel);

                            if(gb.curClient.id == 0){
                                gb.curClient.dataCadastro = (new Date()).toString();
                            }

                            Client.RegistroCliente client = Client.RegistroCliente.newBuilder()
                                    .setNome(nome.getText().toString())
                                    .setEmail(email.getText().toString())
                                    .setTelefone(telefone.getText().toString())
                                    .setId(gb.curClient.id)
                                    .build();

                            if(act1 == "Salvar") {
                                Client.ClienteResponse resp = blockStubClient.salvar(client);

                                if (resp.getError() != 0) {
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Salvo com sucesso!", false, true, progress
                                        , txtProgress);

                                System.out.println(resp.getMessage());
                                System.out.println(resp.getRcliente().getId());
                                System.out.println(resp.getRcliente().getNome());
                                System.out.println(resp.getRcliente().getTelefone());
                                System.out.println(resp.getRcliente().getEmail());

                                if (gb.curClient.id == 0) {
                                    gb.curClient.id = resp.getRcliente().getId();
                                    data.setText("Cadastrado em: " + gb.curClient.dataCadastro);
                                }
                            }else{
                                Client.ClienteResponse resp = blockStubClient.excluir(client);

                                if(resp.getError() != 0){
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Excluido com sucesso!", false, true, progress
                                        , txtProgress);

                                gb.curClient = new Cliente(Client.RegistroCliente.newBuilder().build());

                                data.setText("");
                                email.setText("");
                                nome.setText("");
                                telefone.setText("");
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
