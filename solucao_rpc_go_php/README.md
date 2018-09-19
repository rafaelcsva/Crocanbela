É necessário instalar o mysql driver para o GO, e utilizar o composer para as pendências do php

"go get -u github.com/go-sql-driver/mysql"

"composer require spiral/goridge"

Os arquivos de teste para comunicação entre o PHP e GO estão na pasta rpc, (client.php, server.go)
