package crocanbela.crocanbela;

import android.os.Handler;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;

import ServidorClientes.Client;
import ServidorClientes.ClientesGrpc;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorProdutos.Product;
import ServidorProdutos.ProdutosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

/**
 * Created by perninha on 12/11/18.
 */

public class Common {

    private Handler handler = new Handler();

    public void AtualizarProgress(final String status, final boolean erro, final boolean finalize
            , final ProgressBar progress, final TextView txtProgress){

        new Thread(new Runnable() {
            @Override
            public void run() {
                handler.post(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            if (erro) {
                                progress.setVisibility(View.INVISIBLE);
                                txtProgress.setText(status);
                            } else {
                                txtProgress.setText(status);

                                if(finalize){
                                    progress.setVisibility(View.INVISIBLE);
                                }
                            }
                        }catch (Exception e){
                            txtProgress.setText(e.getMessage());
                        }
                    }
                });
            }
        }).start();
    }

    public List<Produto> obterProdutos(Globals gb) throws Exception{
        ArrayList<Produto> list = new ArrayList<Produto>();

        String hostNome = gb.hostNome;
        int portNome = gb.portNome;

        try{
            ManagedChannel mChannel = ManagedChannelBuilder.forAddress(hostNome, portNome)
                    .usePlaintext(true).build();

            NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                    .setServico("Produto").build();

            Names.ServicoResponse reg = blockStub.obterServico(req);

            if (reg.getError() != 0) {
                throw new Exception(reg.getMessage());
            }

            System.out.println(reg.getServico().getHost());
            System.out.println(reg.getServico().getPorta());

            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                    reg.getServico().getPorta()).usePlaintext(true).build();

            ProdutosGrpc.ProdutosBlockingStub blockStubProduto = ProdutosGrpc.newBlockingStub(mChannel);
            Product.ModoBusca mod = Product.ModoBusca.newBuilder()
                    .setTipo(Product.ModoBusca.Modo.TODOS).build();

            System.out.println("buscando...");

            Product.Resultado resultado = blockStubProduto.buscar(mod);

            if(resultado.getResponse().getError() != 0){
                throw new Exception(resultado.getResponse().getMessage());
            }

            System.out.println("Achei " + resultado.getProdutosCount() + " produtos");

            for(int i = 0 ; i < resultado.getProdutosCount() ; i++){
                list.add(new Produto(resultado.getProdutos(i)));
            }

        }catch (Exception e){
            throw new Exception("Falha ao obter Produtos!\n" + e.getMessage());
        }

        return list;
    }

    public List<Cliente> obterClientes(Globals gb) throws Exception{
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
