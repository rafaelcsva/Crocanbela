<?php
use Spiral\Goridge;
require "vendor/autoload.php";

$rpc = new Goridge\RPC(new Goridge\SocketRelay("127.0.0.1", 6001));

echo "1 - Salvar\n";
echo "2 - Deletar\n";
echo "3 - Procurar\n";

while(true){
    $op = readline("informe uma opcao ");

    if($op == "1"){
        $nome = readline("informe o nome ");
        $idade = readline("informe a idade ");

        echo $rpc->call(("Pessoa.Salvar"), [
            'Nome' => $nome,
            "Idade" => intval($idade)
        ]);

    }else if($op == "2"){
        $nome = readline("informe o nome ");
        $idade = readline("informe a idade ");

        echo $rpc->call("Pessoa.Deletar", [
            'Nome'  => $nome,
            'Idade' => intval($idade)
        ]);
    }else{
        $nome = readLine("informe o nome ");

        $pessoa = $rpc->call("Pessoa.Procurar", $nome);

        echo $pessoa["Nome"] . " " . $pessoa["Idade"] . "\n";
    }
}