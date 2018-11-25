package ServerCliente

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorProdutos"
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"google.golang.org/grpc"
	"log"
	"context"
	"testing"
	"fmt"
)

var mserv *ServidorNomes.RegistroServico

//Obter um host rodando um microservico de cliente
func TestObterServico(t *testing.T){
	conn, err := grpc.Dial("localhost:1808", grpc.WithInsecure())

	if err != nil {
		log.Fatalf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorNomes.NewNomesClient(conn)

	req := &ServidorNomes.ServicoRequest{}
	req.Servico = "Produto"

	resp, err := client.ObterServico(context.Background(), req)

	if err != nil {
		t.Errorf("Falha ao chamar servico de nomes para obter microservico cliente %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao chamar servico de nomes para microservico cliente\n" + resp.Message)
	}

	mserv = resp.Servico

	t.Log("Servico obtido com sucesso!")

	t.Log("Host: " + fmt.Sprintf("%s", mserv.Host))
	t.Log("Porta: " + fmt.Sprintf("%d", mserv.Porta))
}

//Inclusao de um registro
func TestIncluirProduto(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	//--------INCLUINDO CLIENTE---------//
	client := ServidorProdutos.NewProdutosClient(conn)

	reg := &ServidorProdutos.RegistroProduto{}
	reg.Nome = "Cone nasquik"
	reg.PrecoUnidade = 2.25

	resp, err := client.Salvar(context.Background(), reg)

	if err != nil {
		t.Errorf("Falha ao chamar servico de salvar %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao salvar registro! " + resp.Message)
	}

	t.Log(resp.Message);
	//----------FIM INCLUSAO----------//

	//----------LIMPAR REGISTRO INCLUIDO-----------//
	ret := resp.Rprodutor;

	resp, err = client.Excluir(context.Background(), ret);

	if err != nil {
		t.Errorf("Falha ao chamar servico de exclusao %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao excluir registro!\n" + resp.Message)
	}

	t.Log(resp.Message);
}

//Testar obter todos os registros de clientes e exibir
func TestBuscarTodosProdutos(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorProdutos.NewProdutosClient(conn)

	mode := &ServidorProdutos.ModoBusca{}
	mode.Tipo = ServidorProdutos.ModoBusca_TODOS

	resp, err := client.Buscar(context.Background(), mode)

	if err != nil {
		t.Errorf("Falha ao chamar servico de busca %v", err)
	}
	
	if resp.Response.Error != 0 {
		t.Errorf("Falha ao buscar todos os produtos!\n" + resp.Response.Message)
	}

	fmt.Printf("\nTotal de %d Produtos\n-------------------------\n", len(resp.Produtos))

	for i := 0; i < len(resp.Produtos) ; i++ {
		fmt.Printf("Id: %d\nNome: %s\nPreco: %.2f\nData Cadastro: %s\n-------------------------\n",
			 resp.Produtos[i].Id, resp.Produtos[i].Nome, resp.Produtos[i].PrecoUnidade, 
			 	resp.Produtos[i].DataCadastro)
	}
}
/*
//Testar buscar um unico cliente pelo nome
func TestBuscarClienteNome(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorClientes.NewClientesClient(conn)

	mode := &ServidorClientes.ModoBusca{}
	mode.Tipo = ServidorClientes.ModoBusca_NOME
	mode.Nome = "Jose Da Silvaa Alencar"

	resp, err := client.Buscar(context.Background(), mode)

	if err != nil {
		t.Errorf("Falha ao chamar servico de busca %v", err)
	}
	
	if resp.Message.Error != 0 {
		t.Errorf("Falha ao buscar o cliente!\n" + resp.Message.Message)
	}

	if len(resp.Clientes) == 1 {
		fmt.Printf("Cliente encontrado!\n-------------------------\n")

		fmt.Printf("Id: %d\nNome: %s\nEmail: %s\nTelefone: %s\nData Cadastro: %s\n-------------------------\n",
			 resp.Clientes[0].Id, resp.Clientes[0].Nome, resp.Clientes[0].Email, 
			 	resp.Clientes[0].Telefone, resp.Clientes[0].DataCadastro)
	}else{
		t.Errorf("Falha ao buscar cliente por nome, nao foi encontrado o registro")
	}
}*/
