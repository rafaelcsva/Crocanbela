package server

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"context"
	"log"
	"sync"
	"net"
	"fmt"
	"time"
)

type server struct{
	list map[string][]ServidorNomes.RegistroServico
	mutex map[string]*sync.Mutex
	servicosOferecidos []string
	pesoCpu map[string]int32
	pesoMem map[string]int32
}

func Criar(pesosCpu []int32, pesosMem []int32)(server, error){
	s := server{}
	s.list = make(map[string][]ServidorNomes.RegistroServico)
	s.pesoCpu = make(map[string]int32)
	s.pesoMem = make(map[string]int32)

	s.mutex = make(map[string]*sync.Mutex)
	s.servicosOferecidos = []string{"Autenticacao", "Cliente", "Usuario", "Produto", "Pedido"}

	for i := 0 ; i < len(s.servicosOferecidos) ; i++ {
		s.mutex[s.servicosOferecidos[i]] = &sync.Mutex{}
		s.pesoCpu[s.servicosOferecidos[i]] = pesosCpu[i];
		s.pesoMem[s.servicosOferecidos[i]] = pesosMem[i];
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

func (s *server) AtualizarEstado(ctx context.Context, in *ServidorNomes.RegistroServico) (*ServidorNomes.ServicoResponse, error){
	log.Printf("Chamada a atualizar status recebida!")

	response := ServidorNomes.ServicoResponse{}
	response.Error = 0;

	s.mutex[in.Servico].Lock()
	for _, v := range s.list[in.Servico] {
		if(v.Host == in.Host){
			v.Estado = in.Estado
		}
	}
	s.mutex[in.Servico].Unlock()

	response.Message = "Estado atualizado!"

	return &response, nil
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

const(
	port = 1808
	host = "192.168.0.24"
	off_set = 10
)

func Ativo(in ServidorNomes.RegistroServico, s *server) bool {
	
	conn, err := net.Dial("udp", fmt.Sprintf("%s:%d", in.Host, in.Porta))
	conn1, err1 := net.ListenPacket("udp", fmt.Sprintf("%s:%d", host, port))
	tolerancia := time.Second * 5

	if(err != nil){
		log.Fatal(err)
		return false
	}

	if(err1 != nil){
		log.Fatal(err)
		return false
	}

	defer conn.Close()
	defer conn1.Close()

	log.Printf("Mandando teste de conexao para " + in.Host)
	conn.Write([]byte("Teste"))

	buffer := make([]byte, 1024)
	
	conn1.SetReadDeadline(time.Now().Add(tolerancia))
	_, _, err = conn1.ReadFrom(buffer)

	if(err != nil){
		log.Printf("Caiu")
		return false
	}

	return true
}

func ObterMelhor(s *server, servico string, in *ServidorNomes.ServicoResponse) bool{
	pa := s.pesoCpu[servico]
	pb := s.pesoMem[servico]
	tmp := make([]ServidorNomes.RegistroServico, 0)

	find := false
	var best int32 = 1e9

	s.mutex[servico].Lock()

	for _, v := range s.list[servico] {
		val := pa * v.Estado.Cpu + pb * v.Estado.Memoria
		
		if(!Ativo(v, s)){
			log.Printf("Servico de " + servico + " perdido")
			continue
		}
		
		tmp = append(tmp, v)

		if(val < best){
			find = true
			best = val
			in.Servico = &v
		}
	}

	s.list[servico] = tmp
	s.mutex[servico].Unlock()

	if(find){
		return true
	}else{
		if(servico == "Usuario" && ObterMelhor(s, "Autenticacao", in)){
			(*in).Servico.Porta += off_set
			return true
		}
	}

	return false
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

	if(ObterMelhor(s, in.Servico, &response)){
		response.Message = "Servico ativo."

		return &response, nil
	}else{
		response.Message = "Nenhum servico de " + in.Servico + " está ativo no momento!"
		response.Error = 1;

		return &response, nil
	}
}