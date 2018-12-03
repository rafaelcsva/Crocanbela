package crocanbela.crocanbela;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.util.Date;

import ServidorClientes.Client;
import ServidorClientes.ClientesGrpc;
import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorProdutos.Product;
import ServidorProdutos.ProdutosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class ProdutoEdit extends AppCompatActivity implements View.OnClickListener{
    ProgressBar progress;
    TextView txtProgress, txtData;
    EditText txtNome, txtValor;
    Common a = new Common();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_produto_edit);
        Globals gb = (Globals) getApplication();

        progress = (ProgressBar) findViewById(R.id.progressProduto);
        txtProgress = (TextView) findViewById(R.id.txtProgressProduto);
        txtNome = (EditText) findViewById(R.id.txtNomeProduto);
        txtValor = (EditText) findViewById(R.id.txtValorProduto);
        txtData = (TextView) findViewById(R.id.txtDataProduto);

        if(gb.curProdutoA){
            txtNome.setText(gb.curProduto.nome);
            txtValor.setText(Double.toString(gb.curProduto.precoUnitario));
            txtData.setText("Cadastrado em: " + gb.curProduto.dataCadastro);
        }

        Button btSalvar = (Button) findViewById(R.id.btSalvarProduto);
        Button btExcluir = (Button) findViewById(R.id.btExcluirProduto);

        btSalvar.setOnClickListener(this);
        btExcluir.setOnClickListener(this);
    }

    @Override
    public void onClick(View v){
        final Globals gb = (Globals)getApplication();
        final String hostNome = gb.hostNome;
        final int portNome = gb.portNome;
        final EditText nome = txtNome;
        final EditText valor = txtValor;
        final TextView data = txtData;
        final String act1, act2;

        if(v.getId() == R.id.btSalvarProduto){
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

                            System.out.println("Buscando servidor de produtos");
                            a.AtualizarProgress("Buscando servidor de produtos",
                                    false, false, progress, txtProgress);

                            Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                                    .setServico("Produto").build();

                            Names.ServicoResponse reg = blockStub.obterServico(req);

                            if (reg.getError() != 0) {
                                throw new Exception(reg.getMessage());
                            }

                            System.out.println("Buscando servidor de produtos(terminado)");

                            System.out.println(reg.getMessage());
                            System.out.println(reg.getServico().getHost());
                            System.out.println(reg.getServico().getPorta());

                            mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                                    reg.getServico().getPorta()).usePlaintext(true).build();

                            a.AtualizarProgress(act2, false, false, progress,
                                    txtProgress);

                            ProdutosGrpc.ProdutosBlockingStub blockStubProd =
                                    ProdutosGrpc.newBlockingStub(mChannel);

                            Product.RegistroProduto prod = Product.RegistroProduto.newBuilder()
                                    .setNome(nome.getText().toString())
                                    .setId(gb.curProduto.id)
                                    .setPrecoUnidade(Double.parseDouble(valor.getText().toString()))
                                    .build();

                            if(act1 == "Salvar") {
                                Product.ProdutoResponse resp = blockStubProd.salvar(prod);

                                if (resp.getError() != 0) {
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Salvo com sucesso!", false, true, progress
                                        , txtProgress);

                                System.out.println(resp.getMessage());
                                System.out.println(resp.getRprodutor().getId());
                                System.out.println(resp.getRprodutor().getNome());
                                System.out.println(resp.getRprodutor().getPrecoUnidade());

                                if (gb.curProduto.id == 0) {
                                    gb.curProduto.id = resp.getRprodutor().getId();
                                    data.setText("Cadastrado em: " + resp.getRprodutor().getDataCadastro());
                                }
                            }else{
                                Product.ProdutoResponse resp = blockStubProd.excluir(prod);

                                if(resp.getError() != 0){
                                    throw new Exception(resp.getMessage());
                                }

                                a.AtualizarProgress("Excluido com sucesso!", false, true, progress
                                        , txtProgress);

                                gb.curProduto = new Produto(Product.RegistroProduto.newBuilder().build());

                                data.setText("");
                                nome.setText("");
                                valor.setText("");
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
