package main

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/server"
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorNomes"
	"google.golang.org/grpc"
	"log"
	"net"
	"fmt"
)

const (
	port = 1808
)

func main(){
	ln, err := net.Listen("tcp", fmt.Sprintf(":%d", port))

	if(err != nil){
		log.Fatal(err)
	}
	
	s, err := server.Criar()
	grpcServer := grpc.NewServer()
	ServidorNomes.RegisterNomesServer(grpcServer, &s)
	
	log.Printf("Servidor de nomes iniciado!")

	grpcServer.Serve(ln)

}