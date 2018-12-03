package crocanbela.crocanbela;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.AutoCompleteTextView;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import ServidorPedidos.Order;
import ServidorPedidos.PedidosGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

public class PedidoEdit extends AppCompatActivity implements View.OnClickListener, View.OnTouchListener{
    AutoCompleteTextView txtCliente, txtProduto;
    EditText txtDataEntrega, txtEndereco, txtTelefone, txtEmail, txtQuantidade, txtObservacao;
    ListView listItens;
    Button btSalvar, btNovoItem, btExcluir;
    ProgressBar progress;
    TextView txtProgress, txtValor;
    Common a = new Common();
    List<ProdutoItem> list = new ArrayList<ProdutoItem>();
    ArrayAdapter<ProdutoItem> adapteritens;
    Produto curProd = null;
    double valor = 0.0;

    void atualizaProgress(String text, int inc){
        txtProgress.setText(text);
        progress.incrementProgressBy(inc);
    }

    void atualizaProgress(final String text,final int inc,final ProgressBar a,final TextView b){
        b.setText(text);
        a.incrementProgressBy(inc);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_pedido_edit);

        Globals gb = (Globals) getApplication();
        txtCliente = (AutoCompleteTextView) findViewById(R.id.txtClientePedido);
        txtProduto = (AutoCompleteTextView) findViewById(R.id.txtProdutoPedido);
        txtDataEntrega = (EditText) findViewById(R.id.txtDataEntrega);
        txtEndereco = (EditText) findViewById(R.id.txtEnderecoPedido);
        txtTelefone = (EditText) findViewById(R.id.txtTelefonePedido);
        txtEmail = (EditText) findViewById(R.id.txtEmailPedido);
        txtQuantidade = (EditText) findViewById(R.id.txtQuantidade);
        listItens = (ListView) findViewById(R.id.listItens);
        btSalvar = (Button) findViewById(R.id.btSalvarPedido);
        progress = (ProgressBar) findViewById(R.id.progressPedido);
        txtProgress = (TextView) findViewById(R.id.txtProgressPedido);
        btSalvar = (Button) findViewById(R.id.btSalvarPedido);
        btNovoItem = (Button) findViewById(R.id.btNovoItem);
        txtValor = (TextView) findViewById(R.id.txtValor);
        btExcluir = (Button) findViewById(R.id.btExcluirPedido);

        try {

            atualizaProgress("Buscando Clientes", 0);

            ArrayAdapter<Cliente> adapterclient = new ArrayAdapter<Cliente>(this,
                    android.R.layout.simple_dropdown_item_1line, a.obterClientes((Globals) getApplication()));

            atualizaProgress("Preenchendo clientes na view", 25);

            txtCliente.setAdapter(adapterclient);

            progress.setProgress(50);

            atualizaProgress("Buscando Produtos", 25);

            ArrayAdapter<Produto> adapterproduto = new ArrayAdapter<Produto>(this,
                    android.R.layout.simple_dropdown_item_1line, a.obterProdutos((Globals) getApplication()));

            atualizaProgress("Preenchendo produtos na view", 25);

            txtProduto.setAdapter(adapterproduto);
            txtProduto.setOnItemClickListener(onClickProduto);
            atualizaProgress("Carregado com sucesso", 25);

            txtDataEntrega.setText(new Date().toString());
            if(gb.curPedidoA){
                System.out.println("Cliente: " + gb.curPedido.cliente);
                txtCliente.setText(gb.curPedido.cliente);
                txtDataEntrega.setText(gb.curPedido.dataEntrega);
                txtEndereco.setText(gb.curPedido.endereco);
                txtTelefone.setText(gb.curPedido.telefone);
                txtEmail.setText(gb.curPedido.email);
                list = gb.curPedido.itens;

                for(int i = 0 ; i < list.size() ; i++){
                    valor += list.get(i).valor * list.get(i).qtd;
                }

                txtValor.setText("R$ " + valor);
            }

            adapteritens = new ArrayAdapter<ProdutoItem>(this,
                    android.R.layout.simple_dropdown_item_1line, list);

            listItens.setAdapter(adapteritens);
            listItens.setOnTouchListener(this);
        }catch (Exception e){
            atualizaProgress("Erro ocorrido!\n" + e.getMessage(), 0);
        }

        btNovoItem.setOnClickListener(addItemListener);
        btSalvar.setOnClickListener(this);
        btExcluir.setOnClickListener(this);

    }

    public boolean onTouch(View v, MotionEvent e)
    {
        switch (e.getAction()) {
            case MotionEvent.ACTION_DOWN:
                v.getParent().requestDisallowInterceptTouchEvent (true);
                break;
            case MotionEvent.ACTION_UP:
                v.getParent().requestDisallowInterceptTouchEvent (false);
                break;
        }

        v.onTouchEvent (e);
        return true;
    }

    private AdapterView.OnItemClickListener onClickProduto = new AdapterView.OnItemClickListener() {
        @Override
        public void onItemClick(AdapterView<?> adapterView, View view, int i, long l) {
            Object item = adapterView.getItemAtPosition(i);

            System.out.println("Clickou no item " + i);

            if(item instanceof Produto){
                System.out.println("Ok");
                curProd = (Produto)item;
            }else{
                System.out.println("Falhou");
                curProd = null;
            }
        }
    };

    public void onClick(View v){
        final Globals gb = (Globals)getApplication();
        final String hostNome = gb.hostNome;
        final int portNome = gb.portNome;
        final ProgressBar mprogress = progress;
        final EditText cliente = txtCliente, endereco = txtEndereco;
        final TextView data = txtDataEntrega, texto = txtProgress, valor = txtValor, tprogress = txtProgress;
        final String act1, act2;
        final EditText email = txtEmail, telefone = txtTelefone;

        if(v.getId() == R.id.btSalvarPedido){
            act1 = "Salvar";
            act2 = "Salvando";
        }else{
            act1 = "Excluir";
            act2 = "Excluindo";
        }

        System.out.println("clickado!");

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try{
                    ManagedChannel mChannel = ManagedChannelBuilder
                            .forAddress(hostNome, portNome)
                            .usePlaintext(true).build();

                    NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

                    System.out.println("Buscando servidor de pedidos");

                    atualizaProgress("Buscando servidor de pedidos", 0, mprogress, tprogress);

                    Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                            .setServico("Pedido").build();
                    System.out.println("aqui1");
                    Names.ServicoResponse reg = blockStub.obterServico(req);
                    System.out.println("aqui2");
                    if (reg.getError() != 0) {
                        throw new Exception(reg.getMessage());
                    }
                    System.out.println("aqui3");

                    atualizaProgress("Servidor encrontrado", 25, mprogress, tprogress);

                    System.out.println("Buscando servidor de pedidos(terminado)");

                    System.out.println(reg.getMessage());
                    System.out.println(reg.getServico().getHost());
                    System.out.println(reg.getServico().getPorta());

                    mChannel = ManagedChannelBuilder.forAddress(reg.getServico().getHost(),
                            reg.getServico().getPorta()).usePlaintext(true).build();

                    atualizaProgress(act2, 25, mprogress, tprogress);

                    PedidosGrpc.PedidosBlockingStub blockStubPed =
                            PedidosGrpc.newBlockingStub(mChannel);

                    Order.RegistroPedido.Builder ped = Order.RegistroPedido.newBuilder();

                    System.out.println(data.getText().toString());
                    ped.setCliente(cliente.getText().toString());
                    ped.setDataEntrega(data.getText().toString());
                    ped.setEmail(email.getText().toString());
                    ped.setEndereco(txtEndereco.getText().toString());
                    ped.setId(gb.curPedido.id);
                    ped.setTelefone(telefone.getText().toString());

                    for(int i = 0 ; i < gb.curPedido.itens.size() ; i++){
                        Order.ProdutoItem.Builder item = Order.ProdutoItem.newBuilder();
                        item.setIdProduto(gb.curPedido.itens.get(i).idProduto);
                        item.setNome(gb.curPedido.itens.get(i).nome);
                        item.setQtd(gb.curPedido.itens.get(i).qtd);
                        item.setValor(gb.curPedido.itens.get(i).valor);
                        System.out.println("Valor:");
                        System.out.println(gb.curPedido.itens.get(i).valor);

                        ped.addItens(item);
                    }

                    Order.RegistroPedido pedido = ped.build();

                    if(act1 == "Salvar") {
                        Order.PedidoResponse resp = blockStubPed.salvar(pedido);

                        if (resp.getError() != 0) {
                            throw new Exception(resp.getMessage());
                        }

                        if (gb.curPedido.id == 0) {
                            gb.curPedido.dataEntrega = resp.getPedido().getDataEntrega();
                            gb.curPedido.id = resp.getPedido().getId();
                        }

                        atualizaProgress("Salvo com sucesso!", 50, mprogress, tprogress);
                    }else{
                        Order.PedidoResponse resp = blockStubPed.excluir(pedido);

                        if(resp.getError() != 0){
                            throw new Exception(resp.getMessage());
                        }

                        atualizaProgress("Excluido com sucesso!", 50, mprogress, tprogress);

                        gb.curPedido = new Pedido();

                        data.setText("");
                        valor.setText("R$ 0,00");
                        cliente.setText("");
                        endereco.setText("");
                        data.setText("");
                        email.setText("");
                    }
                } catch (Exception e) {
                    atualizaProgress("Falha ao " + act1 + " " + e.getMessage(), 0, mprogress, tprogress);

                    return;
                }
            }

        });
    }

    private View.OnClickListener addItemListener = new View.OnClickListener(){
        public void onClick(View v){
            txtProduto.setActivated(false);
            txtQuantidade.setActivated(false);
            btNovoItem.setActivated(false);
            Globals gb = (Globals) getApplication();

            int qtd = 0;

            if(txtQuantidade.getText().length() > 0){
                qtd = Integer.valueOf(txtQuantidade.getText().toString());
            }

            System.out.println("Tentando adicionar item " + " " + qtd );

            if(curProd != null && qtd != 0){
                System.out.println("Selecionado: " + curProd.nome);
                Produto a = curProd;

                ProdutoItem nw = new ProdutoItem();
                nw.idProduto = a.id;
                nw.nome = a.nome;
                nw.valor = a.precoUnitario;
                nw.qtd = qtd;

                gb.curPedido.itens.add(nw);
                list.add(nw);
                adapteritens.notifyDataSetChanged();
                System.out.println(list.size() + " itens = " + listItens.getAdapter().getCount());

                valor += nw.qtd * nw.valor;
                txtValor.setText("R$ " + valor);
            }

            btNovoItem.setActivated(false);
            txtQuantidade.setActivated(false);
            txtProduto.setActivated(true);
        }
    };
}
