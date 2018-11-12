using System;
using System.Collections.Generic;
using System.Data;

namespace ServidorUsuarios.Modelo
{
    public class Usuario
    {
		public int Id { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }

        public static void preencherValores(Usuario a, DataRow row)
        {
            a.Id = (int)row["id"];
            a.Senha = row["senha"].ToString();
            a.Login = row["login"].ToString();
        }
        
		public RegistroUsuario toRegistroUsuario(){
			var usuario = this;

			return new RegistroUsuario { Id = usuario.Id, Login = usuario.Login, Senha = usuario.Senha };
		}

		public Usuario(){
			
		}

		public Usuario(RegistroUsuario usuario){
			this.Id = usuario.Id;
			this.Login = usuario.Login;
			this.Senha = usuario.Senha;
		}

        public static Usuario[] Buscar()
        {
            DataRowCollection reader;
            var n = new List<Usuario>();

            try
            {
                reader = BancoDeDados.select("select *from usuarios order by login");
            }
            catch (Exception e)
            {
				throw new Exception("Falha ao buscar usuarios.\n" + e.Message);
            }

            foreach (DataRow row in reader)
            {
                var a = new Usuario();

                preencherValores(a, row);
                n.Add(a);
            }

            return n.ToArray();
        }

        public static Usuario BuscarPorId(int id)
        {
            DataRowCollection read;
            var a = new Usuario();

            try
            {
                read = BancoDeDados.select("select *from usuarios where id=?param1", new object[] { id });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar usuario.\n" + e.Message.ToString());
            }

            if (read.Count > 0)
                preencherValores(a, read[0]);
            else
                return null;

            return a;
        }

        public static Usuario BuscarPorLogin(string nome)
        {
            var a = new Usuario();
            DataRowCollection read;

            try
            {
                read = BancoDeDados.select("select *from usuarios where login=?param1", new object[] { nome });
            }
            catch (Exception e)
            {
                throw new Exception("Falha ao buscar usuario.\n" + e.Message);
            }

            if (read.Count > 0)
                preencherValores(a, read[0]);
            else
                return null;

            return a;
        }

        public static void Salvar(Usuario usuario)
        {
            var s = BuscarPorLogin(usuario.Login);

            if (usuario.Id == 0)
            {
                if (s == null)
                {
                    try
                    {
                        usuario.Id = (int)BancoDeDados.executar("insert into usuarios(login,senha) values(?param1,?param2);", new object[] { usuario.Login, usuario.Senha });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Usuario nao salvo.\n" + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Nome de usuário já existe.");
                }
            }
            else
            {
                if (s == null || s.Id == usuario.Id)//se o nome nao existe nos registros ou se existe ele tem que ter o mesmo id do que quero salvar...
                {
                    try
                    {
                        BancoDeDados.executar("update usuarios set login=?param1, senha = ?param2 where id = ?param3;", new object[] { usuario.Login, usuario.Senha, usuario.Id });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Falha ao atualizar usuário.\n" + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Nome de usuário já existe.");
                }
            }
        }

        public static void Excluir(Usuario usuario)
        {

            try
            {
                BancoDeDados.executar("delete from usuarios where id=?param1", new object[] { usuario.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Usuario nao excluido.\n" + e.Message);
            }
        }
    }
}
