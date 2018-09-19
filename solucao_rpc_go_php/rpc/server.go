package main

import (
	"fmt"
	"rafael.castro.sd.ufg/solucao_rpc_go_php/goridge"
	_ "github.com/go-sql-driver/mysql"
	"log"
	"net"	
	"net/rpc"
	"database/sql"
)

type Pessoa struct{
	Nome string
	Idade int
}

func (a *Pessoa) Salvar(pessoa Pessoa, r *string) error {
	db, err := sql.Open("mysql", "root:q8p8ugf3@tcp(127.0.0.1:3306)/universo")

    if err != nil {
		*r = fmt.Sprintf("falha, %s!\n", err.Error())
		return nil
    }

	defer db.Close()

	_, err = db.Query("insert into pessoa (nome, idade) values (?, ?)", pessoa.Nome, pessoa.Idade)
	

	if err != nil {
		*r = fmt.Sprintf("falha, %s!\n", err.Error())
		return nil
	}

	*r = "salvo com sucesso!\n"

	return nil
}

func (a *Pessoa) Procurar(nome string, p *Pessoa) error{
	db, err := sql.Open("mysql", "root:q8p8ugf3@tcp(127.0.0.1:3306)/universo")

    if err != nil {
		return nil
    }

	defer db.Close()

	err = db.QueryRow("select nome, idade from pessoa where nome = ?", nome).Scan(&((*p).Nome), &((*p).Idade));
	fmt.Printf((*p).Nome)
	
	if err != nil {
		return nil
	}

	return nil
}

func (a *Pessoa) Deletar(pessoa Pessoa, r *string) error {
	db, err := sql.Open("mysql", "root:q8p8ugf3@tcp(127.0.0.1:3306)/universo")

    if err != nil {
		*r = fmt.Sprintf("falha, %s!\n", err.Error())
		return nil
    }

	defer db.Close()

	_, err = db.Query("delete from pessoa where nome = ? and idade = ?", pessoa.Nome, pessoa.Idade)
	

	if err != nil {
		*r = fmt.Sprintf("falha, %s!\n", err.Error())
		return nil
	}


	*r = "deletado com sucesso!\n"

	return nil
}

func main() {
	ln, err := net.Listen("tcp", ":6001")
	if err != nil {
		panic(err)
	}

	rpc.Register(new(Pessoa))
	log.Printf("Servidor iniciado.")

	for {
		conn, err := ln.Accept()
		if err != nil {
			continue
		}

		log.Printf("nova conexão %+v", conn)
		go rpc.ServeCodec(goridge.NewCodec(conn))
	}
    
}
