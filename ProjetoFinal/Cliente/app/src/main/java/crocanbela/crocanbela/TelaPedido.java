package crocanbela.crocanbela;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;

import java.util.ArrayList;
import java.util.List;

import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorPedidos.Order;
import ServidorPedidos.PedidosGrpc;
import ServidorProdutos.Product;
import ServidorProdutos.ProdutosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class TelaPedido extends AppCompatActivity implements AdapterView.OnItemClickListener, View.OnClickListener{
    ListView listaPedidos;
    Button btNovo;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tela_pedido);

        try{
            List<Pedido> pedidos = obterPedidos();

            listaPedidos = findViewById(R.id.listPedido);
            btNovo = (Button) findViewById(R.id.btNovoPedido);

            ArrayAdapter<Pedido> adapter = new ArrayAdapter<Pedido>(
                    this, android.R.layout.simple_list_item_1, pedidos
            );

            listaPedidos.setAdapter(adapter);
            listaPedidos.setOnItemClickListener(this);

            btNovo.setOnClickListener(this);
        }catch (Exception e){
            System.out.println("Falha ao iniciar!\n" + e.getMessage());
        }
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals) getApplication();
        final TelaPedido ctxPedido = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {

                        Intent telaPedido = new Intent(ctxPedido, PedidoEdit.class);
                        gb.curPedido = new Pedido();
                        gb.curPedidoA = false;

                        startActivity(telaPedido);
                    }
                }
        ).start();
    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, final int position,
                            long id) {
        final Globals gb = (Globals) getApplication();
        final TelaPedido ctxPedido = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {
                        Pedido ped = (Pedido) listaPedidos.getAdapter().getItem(position);

                        gb.curPedido = ped;
                        gb.curPedidoA = true;

                        Intent telaPedido = new Intent(ctxPedido, PedidoEdit.class);
                        startActivity(telaPedido);
                    }
                }
        ).start();
    }

    public List<Pedido> obterPedidos() throws Exception{
        Globals gb = (Globals) getApplication();
        ArrayList<Pedido> list = new ArrayList<Pedido>();

        String hostNome = gb.hostNome;
        int portNome = gb.portNome;

        try{
            ManagedChannel mChannel = ManagedChannelBuilder.forAddress(hostNome, portNome)
                    .usePlaintext(true).build();

            NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                    .setServico("Pedido").build();

            Names.ServicoResponse reg = blockStub.obterServico(req);

            if (reg.getError() != 0) {
                throw new Exception(reg.getMessage());
            }

            System.out.println(reg.getServico().getHost());
            System.out.println(reg.getServico().getPorta());

            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                    reg.getServico().getPorta()).usePlaintext(true).build();

            PedidosGrpc.PedidosBlockingStub blockStubPedido = PedidosGrpc.newBlockingStub(mChannel);
            Order.ModoBusca mod = Order.ModoBusca.newBuilder()
                    .setTipo(Order.ModoBusca.Modo.TODOS).build();

            System.out.println("buscando...");

            Order.Resultado resultado = blockStubPedido.buscar(mod);

            if(resultado.getResponse().getError() != 0){
                throw new Exception(resultado.getResponse().getMessage());
            }

            System.out.println("Achei " + resultado.getPedidosCount() + " pedidos");

            for(int i = 0 ; i < resultado.getPedidosCount() ; i++){
                list.add(new Pedido(resultado.getPedidos(i)));
            }

        }catch (Exception e){
            throw new Exception("Falha ao obter Produtos!\n" + e.getMessage());
        }

        return list;
    }
}
