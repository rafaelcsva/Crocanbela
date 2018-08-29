package main

import (
    "fmt"
    "net"
	"os"
)

func main() {
    service := ":1200"
    tcpAddr, err := net.ResolveTCPAddr("tcp4", service)
    checkError(err)
    listener, err := net.ListenTCP("tcp", tcpAddr)
    checkError(err)
    for {
        conn, err := listener.Accept()
        if err != nil {
            continue
		}
		
        go handleClient(conn)
    }
}

func handleClient(conn net.Conn) {
	request := make([]byte, 128)

	defer conn.Close()
	_, err := conn.Read(request)

	checkError(err)
	
	if(request[0] >= 65 || request[1] >= 30 || (request[0] >= 60 && request[1] >= 25)){
		_, err = conn.Write([]byte("Pode aposentar!"))
	}else{
		_, err = conn.Write([]byte("Nao pode aposentar"))
	}
}

func checkError(err error) {
    if err != nil {
        fmt.Fprintf(os.Stderr, "Erro fatal: %s", err.Error())
        os.Exit(1)
    }
}
