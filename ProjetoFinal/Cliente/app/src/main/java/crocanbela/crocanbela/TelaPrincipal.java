package crocanbela.crocanbela;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

public class TelaPrincipal extends AppCompatActivity implements View.OnClickListener{

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tela_principal);

        Button btUsuario = (Button) findViewById(R.id.btUsuario);
        Button btCliente = (Button) findViewById(R.id.btCliente);
        Button btProduto = (Button) findViewById(R.id.btProduto);
        Button btPedido = (Button) findViewById(R.id.btPedido);

        btUsuario.setOnClickListener(this);
        btCliente.setOnClickListener(this);
        btProduto.setOnClickListener(this);
        btPedido.setOnClickListener(this);
    }

    public void onClick(View v){

        System.out.println("Clickado");

        if(v.getId() == R.id.btUsuario){
            Intent telaUsuario = new Intent(this, TelaUsuario.class);
            System.out.println("tentando abrir tela usuario!");

            startActivity(telaUsuario);
        }else if(v.getId() == R.id.btCliente){
            Intent telaCliente = new Intent(this, TelaCliente.class);
            System.out.println("tentando abrir tela cliente!");

            startActivity(telaCliente);
        }else if(v.getId() == R.id.btProduto){
            Intent telaProduto = new Intent(this, TelaProduto.class);
            System.out.println("tentando abrir tela produto!");

            startActivity(telaProduto);
        }else if(v.getId() == R.id.btPedido){
            Intent telaPedido = new Intent(this, TelaPedido.class);
            System.out.println("tentando abrir tela pedido!");

            startActivity(telaPedido);
        }

    }
}
