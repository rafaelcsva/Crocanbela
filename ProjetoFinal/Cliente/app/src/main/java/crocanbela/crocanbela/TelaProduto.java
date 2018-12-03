package crocanbela.crocanbela;

import android.content.Intent;
import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;

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

public class TelaProduto extends AppCompatActivity implements AdapterView.OnItemClickListener, View.OnClickListener{
    ListView listaProdutos;
    Button btNovo;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tela_produto);

        try{
            List<Produto> produtos = a.obterProdutos((Globals) getApplication());

            listaProdutos = findViewById(R.id.listProduto);
            btNovo = (Button) findViewById(R.id.btNovoProduto);

            ArrayAdapter<Produto> adapter = new ArrayAdapter<Produto>(
                    this, android.R.layout.simple_list_item_1, produtos
            );

            listaProdutos.setAdapter(adapter);
            listaProdutos.setOnItemClickListener(this);

            btNovo.setOnClickListener(this);
        }catch (Exception e){
            System.out.println("Falha ao iniciar!\n" + e.getMessage());
        }
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals) getApplication();
        final TelaProduto ctxProduto = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {

                        Intent telaProduto = new Intent(ctxProduto, ProdutoEdit.class);
                        gb.curProduto = new Produto();
                        gb.curProdutoA = false;

                        startActivity(telaProduto);
                    }
                }
        ).start();
    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, final int position,
                            long id) {
        final Globals gb = (Globals) getApplication();
        final TelaProduto ctxProduto = this;

        new Thread(
                new Runnable() {
                    @Override
                    public void run() {
                        Produto prod = (Produto) listaProdutos.getAdapter().getItem(position);

                        gb.curProduto = prod;
                        gb.curProdutoA = true;

                        Intent telaProduto = new Intent(ctxProduto, ProdutoEdit.class);
                        startActivity(telaProduto);
                    }
                }
        ).start();
    }


}
