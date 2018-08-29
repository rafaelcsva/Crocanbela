package main

import (
    "fmt"
    "net"
    "os"
)

func main() {
    if len(os.Args) != 2 {
        fmt.Println("Por favor informe passe o nome do host e da porta no seguinte formato [host]:[porta] atraves da linha de comando!")
        os.Exit(1)
    }
    
    response := make([]byte, 128)
    request := make([]byte, 128)
    service := os.Args[1]
    tcpAddr, err := net.ResolveTCPAddr("tcp4", service)
    checkError(err)
    conn, err := net.DialTCP("tcp", nil, tcpAddr)
    checkError(err)

    fmt.Scanf("%d %d", &request[0], &request[1])
    
    _, err = conn.Write([]byte(request))
    conn.Read(response)
    checkError(err)
    fmt.Println(string(response))

    os.Exit(0)
}

func checkError(err error) {
    if err != nil {
        fmt.Fprintf(os.Stderr, "Erro fatal: %s", err.Error())
        os.Exit(1)
    }
}
