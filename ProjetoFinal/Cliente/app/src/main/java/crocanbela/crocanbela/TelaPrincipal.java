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
        btUsuario.setOnClickListener(this);
    }

    public void onClick(View v){
        Intent telaUsuario = new Intent(this, TelaUsuario.class);
        System.out.println("tentando abrir tela!");

        startActivity(telaUsuario);
    }
}
