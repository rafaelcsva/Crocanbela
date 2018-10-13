using System;
using System.Data;
using System.Collections.Generic;

namespace ServidorClientes.Modelo
{
    public class Cliente
    {
		public int Id { get; set;}
		public string Nome { get; set; }
		public string Email { get; set; }
		public string Telefone { get; set; }
		public DateTime dataCadastro { get; set; }

		public Cliente(){
			
		}

		public RegistroCliente toRegistroCliente(){
			var cliente = this;

			return new RegistroCliente { Id = cliente.Id, Email = cliente.Email, DataCadastro = cliente.dataCadastro.ToLongDateString(),
				Nome = cliente.Nome, Telefone = cliente.Telefone };
		}

		public Cliente(RegistroCliente cliente)
        {
			this.Id = cliente.Id;
			this.Nome = cliente.Nome;
			this.Email = cliente.Email;
			this.Telefone = cliente.Telefone;

			if (this.Id == 0)
				this.dataCadastro = DateTime.Now;
        }

		public static void preencherValores(Cliente a, DataRow row)
        {
            a.Id = (int)row["id"];
            a.Nome = row["nome"].ToString();
            a.Telefone = row["telefone"].ToString();
            a.dataCadastro = (DateTime)row["dataCadastro"];
            a.Email = row["email"].ToString();
        }

		public static Cliente[] Buscar()
		{
			var listaClientes = new List<Cliente>();

            DataRowCollection reader;

            try
            {
                reader = BancoDeDados.select("select *from clientes");
            }
            catch (Exception e)
            {
                throw e;
            }

            foreach (DataRow row in reader)
            {
                Cliente a = new Cliente();
                preencherValores(a, row);

                listaClientes.Add(a);
            }

            reader.Clear();

            return listaClientes.ToArray();
		}

		public static Cliente buscarPorId(int id)
        {
            var a = new Cliente();

            DataRowCollection reader;

            try
            {
                reader = BancoDeDados.select("select *from clientes where id =?param1", new object[] { id });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar cliente por id.\n" + e.Message);
            }

            if (reader.Count > 0)
                preencherValores(a, reader[0]);
            else
                return null;

            return a;
        }

		public static Cliente buscarPorNome(string nome)
        {
            var a = new Cliente();

            DataRowCollection reader;

            try
            {
                reader = BancoDeDados.select("select *from clientes where nome =?param1", new object[] { nome });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar cliente por nome.\n" + e.Message);
            }

            if (reader.Count > 0)
                preencherValores(a, reader[0]);
            else
                return null;

            return a;
        }

		public static void Salvar(Cliente a)
        {
            var s = buscarPorNome(a.Nome);

            if (a.Id == 0)
            {
                if (s == null)
                {
                    try
                    {
                        a.Id = (int)BancoDeDados.executar("insert into clientes (nome,telefone,email,dataCadastro) values(?param1,?param2,?param3,?param4);", new object[] { a.Nome, a.Telefone, a.Email, a.dataCadastro });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Falha ao salvar cliente.\n" + e.Message.ToString());
                    }
                }
                else
                {
                    throw new Exception("Nome de cliente já existe.");
                }
            }
            else
            {
                if (s == null || s.Id == a.Id) //se o nome nao existe nos registros ou se existe ele tem que ter o mesmo id do que quero salvar...
                {
                    try
                    {
                        BancoDeDados.executar("update clientes set nome=?param1,telefone=?param2,email=?param3,dataCadastro=?param4 where id = ?param5", new object[] { a.Nome, a.Telefone, a.Email, a.dataCadastro, a.Id });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Falha ao atualizar cliente.\n" + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Nome de cliente já existe.");
                }
            }
        }

		public static void Excluir(Cliente b)
        {
            try
            {
                BancoDeDados.executar("delete from clientes where id=?param1", new object[] { b.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao excluir Cliente.\n" + e.Message.ToString());
            }
        }
    }
}
