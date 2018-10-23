package crocanbela.crocanbela;

import android.support.v4.os.IResultReceiver;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;


import ServidorNomes.Names;
import ServidorNomes.NomesGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;


public class Login extends AppCompatActivity implements View.OnClickListener{

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        Button btEnt = (Button)findViewById(R.id.btEntrar);
        btEnt.setOnClickListener(this);
    }

    public void onClick(View v){
        String hostNome = "192.168.25.10";
        int portNome = 1808;

        ManagedChannel mChannel = ManagedChannelBuilder.forAddress(hostNome, portNome)
                .usePlaintext(true).build();

        NomesGrpc.NomesBlockingStub blockStub = NomesGrpc.newBlockingStub(mChannel);

        Names.ServicoRequest req = Names.ServicoRequest.newBuilder()
                .setServico("Autenticacao").build();

        Names.ServicoResponse reg = blockStub.obterServico(req);

        if(reg.getError() != 0){
            System.out.println(reg.getMessage());
            return;
        }

        System.out.println(reg.getMessage());
        System.out.println(reg.getServico().getHost());
        System.out.println(reg.getServico().getPorta());
    }

}
