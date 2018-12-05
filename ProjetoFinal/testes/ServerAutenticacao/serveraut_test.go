package ServerAutenticacao

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorAutenticacao"
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorUsuarios"
	"google.golang.org/grpc"
	"log"
	"context"
	"testing"
	"fmt"
)

var mserv *ServidorNomes.RegistroServico

// Obter um host rodando um microservico de cliente
func TestObterServico(t *testing.T){
	conn, err := grpc.Dial("localhost:1808", grpc.WithInsecure())

	if err != nil {
		log.Fatalf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorNomes.NewNomesClient(conn)

	req := &ServidorNomes.ServicoRequest{}
	req.Servico = "Autenticacao"

	resp, err := client.ObterServico(context.Background(), req)

	if err != nil {
		t.Errorf("Falha ao chamar servico de nomes para obter microservico Autenticacao %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao chamar servico de nomes para microservico Autenticacao\n" + resp.Message)
	}

	mserv = resp.Servico

	t.Log("Servico obtido com sucesso!")

	t.Log("Host: " + fmt.Sprintf("%s", mserv.Host))
	t.Log("Porta: " + fmt.Sprintf("%d", mserv.Porta))
}

//Tentar autenticar com um usuario e senha validos
func TestAutenticacaoValido(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorAutenticacao.NewAutenticacaoClient(conn)

	reg := &ServidorUsuarios.RegistroUsuario{}
	reg.Login = "admin"
	reg.Senha = "admin"

	resp, err := client.Autenticar(context.Background(), reg)

	if err != nil {
		t.Errorf("Falha ao chamar servico de autenticacao %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao autenticar!\n" + resp.Message)
	}

	t.Log(resp.Message);
}

//Tentar autenticar com um login invalido
func TestAutenticacaoLoginInvalido(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorAutenticacao.NewAutenticacaoClient(conn)

	reg := &ServidorUsuarios.RegistroUsuario{}
	reg.Login = "josedasneves"
	reg.Senha = "7894"

	resp, err := client.Autenticar(context.Background(), reg)

	if err != nil {
		t.Errorf("Falha ao chamar servico de autenticacao %v", err)
	}

	if resp.Error == 0 {
		t.Errorf("Falha, o usuario foi autenticado!\n" + resp.Message)
	}

	t.Log(resp.Message);
}

func TestAutenticacaoSenhaInvalida(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorAutenticacao.NewAutenticacaoClient(conn)

	reg := &ServidorUsuarios.RegistroUsuario{}
	reg.Login = "joaoneves"
	reg.Senha = "9877"

	resp, err := client.Autenticar(context.Background(), reg)

	if err != nil {
		t.Errorf("Falha ao chamar servico de autenticacao %v", err)
	}

	if resp.Error == 0 {
		t.Errorf("Falha, o usuario foi autenticado!\n" + resp.Message)
	}

	t.Log(resp.Message);
}
