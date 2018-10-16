package main

import(
	"rafael.castro.sd.ufg/ProjetoFinal/ServidorNomes/grpc/ServidorClientes"
	"google.golang.org/grpc"
	"log"
	"context"
	"flag"
)

var (
	tls                = flag.Bool("tls", false, "Connection uses TLS if true, else plain TCP")
	caFile             = flag.String("ca_file", "", "The file containning the CA root cert file")
	serverAddr         = flag.String("server_addr", "localhost:5001", "The server address in the format of host:port")
	serverHostOverride = flag.String("server_host_override", "x.test.youtube.com", "The server name use to verify the hostname returned by TLS handshake")
)

func main(){
	conn, err := grpc.Dial("localhost:5001", grpc.WithInsecure())

	if err != nil {
		log.Fatalf("Falha ao conectar %v", err)
	}

	defer conn.Close()
	client := ServidorClientes.NewClientesClient(conn)

	reg := &ServidorClientes.RegistroCliente{}
	reg.Nome = "Josssse Da Silvaa Alencar"
	reg.Email = "jose@jose.com"
	reg.Telefone = "6294949494"

	resp, err := client.Salvar(context.Background(), reg)

	if err != nil {
		log.Fatalf("Falha ao chamar servico de salvar %v", err)
	}

	if resp.Error != 0 {
		log.Printf("Falha ao salvar registro!\n")
	}

	log.Printf(resp.Message);

	ret := resp.Rcliente;

	resp, err = client.Excluir(context.Background(), ret);

	if err != nil {
		log.Fatalf("Falha ao chamar servico de excluir %v", err)
	}

	if resp.Error != 0 {
		log.Printf("Falha ao salvar registro!\n")
	}

	log.Printf(resp.Message);
}	