package crocanbela.crocanbela;

import android.content.Intent;
import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import java.util.ArrayList;
import java.util.List;

import ServidorClientes.Client;
import ServidorClientes.ClientesGrpc;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class TelaCliente extends AppCompatActivity implements AdapterView.OnItemClickListener, View.OnClickListener {
    ListView listaClientes;
    FloatingActionButton ftBt;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tela_cliente2);

        try{
            List<Cliente> clientes = obterClientes();

            listaClientes = findViewById(R.id.listClientes);
            ftBt = (FloatingActionButton) findViewById(R.id.btNovoCliente);

            ArrayAdapter<Cliente> adapter = new ArrayAdapter<Cliente>(
                    this, android.R.layout.simple_list_item_1, clientes
            );

            listaClientes.setAdapter(adapter);
            listaClientes.setOnItemClickListener(this);

            ftBt.setOnClickListener(this);
        }catch (Exception e){
            System.out.println("Falha ao iniciar!\n" + e.getMessage());
        }
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals) getApplication();
        final TelaCliente ctxCliente = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {

                        Intent telaCliente = new Intent(ctxCliente, ClienteEdit.class);
                        gb.curClient = new Cliente(Client.RegistroCliente.newBuilder().build());
                        gb.curClientA = false;

                        startActivity(telaCliente);
                    }
                }
        ).start();
    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, final int position,
                            long id) {
        final Globals gb = (Globals) getApplication();
        final TelaCliente ctxClient = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {
                        Cliente client = (Cliente)listaClientes.getAdapter().getItem(position);

                        gb.curClient = client;
                        gb.curClientA = true;

                        Intent telaClient = new Intent(ctxClient, ClienteEdit.class);
                        startActivity(telaClient);
                    }
                }
        ).start();
    }

    public List<Cliente> obterClientes() throws Exception{
        Globals gb = (Globals) getApplication();
        ArrayList<Cliente> list = new ArrayList<Cliente>();

        String hostNome = gb.hostNome;
        int portNome = gb.portNome;

        try{
            ManagedChannel mChannel = ManagedChannelBuilder.forAddress(hostNome, portNome)
                    .usePlaintext(true).build();

            NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                    .setServico("Cliente").build();

            Names.ServicoResponse reg = blockStub.obterServico(req);

            if (reg.getError() != 0) {
                throw new Exception(reg.getMessage());
            }

            System.out.println(reg.getServico().getHost());
            System.out.println(reg.getServico().getPorta());

            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                    reg.getServico().getPorta()).usePlaintext(true).build();

            ClientesGrpc.ClientesBlockingStub blockStubCliente = ClientesGrpc.newBlockingStub(mChannel);
            Client.ModoBusca mod = Client.ModoBusca.newBuilder()
                    .setTipo(Client.ModoBusca.Modo.TODOS).build();

            System.out.println("buscando...");

            Client.Resultado resultado = blockStubCliente.buscar(mod);

            if(resultado.getMessage().getError() != 0){
                throw new Exception(resultado.getMessage().getMessage());
            }

            System.out.println("Achei " + resultado.getClientesCount() + " clientes");

            for(int i = 0 ; i < resultado.getClientesCount() ; i++){
                list.add(new Cliente(resultado.getClientes(i)));
            }

        }catch (Exception e){
            throw new Exception("Falha ao obter Clientes!\n" + e.getMessage());
        }

        return list;
    }
}
