package server

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"context"
	"log"
	"sync"
)

type server struct{
	list map[string][]ServidorNomes.RegistroServico
	mutex map[string]*sync.Mutex
	servicosOferecidos []string
}

func Criar()(server, error){
	s := server{}
	s.list = make(map[string][]ServidorNomes.RegistroServico)
	s.mutex = make(map[string]*sync.Mutex)
	s.servicosOferecidos = []string{"Autenticacao", "Cliente", "Usuario", "Produto", "Pedido"}

	for i := 0 ; i < len(s.servicosOferecidos) ; i++ {
		s.mutex[s.servicosOferecidos[i]] = &sync.Mutex{}
	}

	return s, nil
}

func Valido(servico string, servicosOferecidos []string) bool{
	for i := 0 ; i < len(servicosOferecidos) ; i++ {
		if(servico == servicosOferecidos[i]){
			return true
		}
	}

	return false
}

func (s *server) Cadastrar(ctx context.Context, in *ServidorNomes.RegistroServico) (*ServidorNomes.ServicoResponse, error){
	log.Printf("Chamada a cadastros recebida! ")

	response := ServidorNomes.ServicoResponse{}
	response.Error = 0;

	if(!Valido(in.Servico, s.servicosOferecidos)){
		response.Message = in.Servico + " é um servico que nao está sendo oferecido!"
		response.Error = 1;
		return &response, nil
	}

	s.mutex[in.Servico].Lock()
	s.list[in.Servico] = append(s.list[in.Servico], *in)
	s.mutex[in.Servico].Unlock()

	response.Message = "Servidor de " + in.Servico + " cadastrado com sucesso!"
	
	return &response, nil
}

func (s *server) ObterServico(ctx context.Context, in *ServidorNomes.ServicoRequest) (*ServidorNomes.ServicoResponse, error){
	log.Printf("Chamada a obter serviços recebida!")

	response := ServidorNomes.ServicoResponse{}
	response.Error = 0;

	if(!Valido(in.Servico, s.servicosOferecidos)){
		response.Message = in.Servico + " é um servico que não está sendo oferecido!"
		response.Error = 1;

		return &response, nil
	}

	if(len(s.list[in.Servico]) > 0){
		response.Message = "Servico ativo."
		response.Servico = &s.list[in.Servico][0];

		return &response, nil
	}else{
		response.Message = "Nenhum servico de " + in.Servico + " está ativo no momento!"
		response.Error = 1;

		return &response, nil
	}
}