sudo docker run --rm -v $(pwd):$(pwd) -w $(pwd) znly/protoc --go_out=plugins=grpc:. -I. ServidorProdutos.proto
