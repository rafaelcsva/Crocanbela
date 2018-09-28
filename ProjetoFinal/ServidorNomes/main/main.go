package main

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/ServidorNomes"
	"context"
	"google.golang.org/grpc"
	"log"
	"net"
	"fmt"
)

type server struct{}

var listLogin []ServidorNomes.RegistroServico

func (s *server) Cadastrar(ctx context.Context, in *ServidorNomes.RegistroServico) (*ServidorNomes.RegistroServico, error){
	listLogin = append(listLogin, *in)

	return in, nil
}

const (
	port = 1808
)

func main(){
	ln, err := net.Listen("tcp", fmt.Sprintf(":%d", port))

	if(err != nil){
		log.Fatal(err)
	}

	grpcServer := grpc.NewServer()
	ServidorNomes.RegisterNomesServer(grpcServer, &server{})

	log.Printf("Servidor de nomes iniciado!")

	grpcServer.Serve(ln)

}