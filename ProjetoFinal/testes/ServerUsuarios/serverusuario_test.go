package ServerUsuarios

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorUsuarios"
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"google.golang.org/grpc"
	"log"
	"context"
	"testing"
	"fmt"
)

var mserv *ServidorNomes.RegistroServico

//Obter um host rodando um microservico de usuario
func TestObterServico(t *testing.T){
	conn, err := grpc.Dial("localhost:1808", grpc.WithInsecure())

	if err != nil {
		log.Fatalf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorNomes.NewNomesClient(conn)

	req := &ServidorNomes.ServicoRequest{}
	req.Servico = "Usuario"

	resp, err := client.ObterServico(context.Background(), req)

	if err != nil {
		t.Errorf("Falha ao chamar servico de nomes para obter microservico usuario %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao chamar servico de nomes para microservico usuario\n" + resp.Message)
	}

	mserv = resp.Servico

	t.Log("Servico obtido com sucesso!")

	t.Log("Host: " + fmt.Sprintf("%s", mserv.Host))
	t.Log("Porta: " + fmt.Sprintf("%d", mserv.Porta))
}

//Inclusao de um registro
func TestIncluirUsuario(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	//--------INCLUINDO USUARIO---------//
	client := ServidorUsuarios.NewUsuariosClient(conn)

	reg := &ServidorUsuarios.RegistroUsuario{}
	reg.Login = "rafaelcs"
	reg.Senha = "1234"

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
	ret := resp.Rusuario;

	resp, err = client.Excluir(context.Background(), ret);

	if err != nil {
		t.Errorf("Falha ao chamar servico de exclusao %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao excluir registro!\n" + resp.Message)
	}

	t.Log(resp.Message);
}

//Editar um registro
func TestEditarUsuario(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	//--------EDITANDO USUARIO---------//
	client := ServidorUsuarios.NewUsuariosClient(conn)

	reg := &ServidorUsuarios.RegistroUsuario{}
	reg.Id = 3
	reg.Login = "pedrohtu"
	reg.Senha = "1234"

	resp, err := client.Salvar(context.Background(), reg)

	if err != nil {
		t.Errorf("Falha ao chamar servico de salvar %v", err)
	}

	if resp.Error != 0 {
		t.Errorf("Falha ao salvar registro! " + resp.Message)
	}
}

//Testar obter todos os registros de usuarios e exibir
func TestBuscarTodosUsuarios(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorUsuarios.NewUsuariosClient(conn)

	mode := &ServidorUsuarios.ModoBusca{}
	mode.Tipo = ServidorUsuarios.ModoBusca_TODOS

	resp, err := client.Buscar(context.Background(), mode)

	if err != nil {
		t.Errorf("Falha ao chamar servico de busca %v", err)
	}
	
	if resp.Message.Error != 0 {
		t.Errorf("Falha ao buscar todos os usuarios!\n" + resp.Message.Message)
	}

	fmt.Printf("\nTotal de %d usuarios\n-------------------------\n", len(resp.Usuarios))

	for i := 0; i < len(resp.Usuarios) ; i++ {
		fmt.Printf("Id: %d\nLogin: %s\nSenha: %s\n-------------------------\n",
			 resp.Usuarios[i].Id, resp.Usuarios[i].Login, resp.Usuarios[i].Senha)
	}
}

//Testar buscar um unico usuario pelo login
func TestBuscarUsuarioLogin(t *testing.T){
	conn, err := grpc.Dial(mserv.Host + ":" + fmt.Sprintf("%d", mserv.Porta), grpc.WithInsecure())

	if err != nil {
		t.Errorf("Falha ao conectar %v", err)
	}

	defer conn.Close()

	client := ServidorUsuarios.NewUsuariosClient(conn)

	mode := &ServidorUsuarios.ModoBusca{}
	mode.Tipo = ServidorUsuarios.ModoBusca_NOME
	mode.Login = "pedrohtu"

	resp, err := client.Buscar(context.Background(), mode)

	if err != nil {
		t.Errorf("Falha ao chamar servico de busca %v", err)
	}
	
	if resp.Message.Error != 0 {
		t.Errorf("Falha ao buscar o usuario!\n" + resp.Message.Message)
	}

	if len(resp.Usuarios) == 1 {
		fmt.Printf("Usuario encontrado!\n-------------------------\n")

		fmt.Printf("Id: %d\nLogin: %s\nSenha: %s\n-------------------------\n",
			 resp.Usuarios[0].Id, resp.Usuarios[0].Login, resp.Usuarios[0].Senha)
	}else{
		t.Errorf("Falha ao buscar usuario por nome, nao foi encontrado o registro")
	}
}
