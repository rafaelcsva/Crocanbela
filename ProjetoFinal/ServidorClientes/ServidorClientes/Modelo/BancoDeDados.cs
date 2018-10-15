using System;
using MySql.Data.MySqlClient;
using System.Data;

namespace ServidorClientes.Modelo
{
	public class BancoDeDados
    {
        private static MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            UserID = "root",
            Password = "q8p8ugf3",
            Database = "projeto.clientes",
            Port = 3306,
			SslMode = MySqlSslMode.None
        };
        
        private static MySqlConnection connect = new MySqlConnection(connectionStringBuilder.ConnectionString);

        private static void Conectar()
        {
            try
            {
                connect.Open();
            }
            catch (MySqlException e)
            {
                throw new Exception("Falha ao conectar ao banco.\n" + e.Message);
            }
        }

        private static MySqlCommand ConstruirComando(string comand, object[] cm)
        {
            var cmd = new MySqlCommand(comand, connect);

            for (int i = 1; i <= cm.Length; i++)
            {
                MySqlParameter pm = new MySqlParameter();
                pm.ParameterName = "?param" + i;
                pm.Value = cm[i - 1];
                cmd.Parameters.Add(pm);
            }

            return cmd;
        }

        public static DataRowCollection select(string comand, object[] cm)
        {
            Conectar();

            var tabela = new DataTable();
            var cmd = ConstruirComando(comand, cm);
            var mAdapter = new MySqlDataAdapter(cmd);

            try
            {
                mAdapter.Fill(tabela);
            }
            catch (MySqlException e)
            {
                throw new Exception("Falha ao buscar dados.\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            return tabela.Rows;
        }

        public static DataRowCollection select(string comand)
        {
            return select(comand, new object[] { });
        }

        public static long executar(string comand, object[] cm)
        {
            Conectar();

            var cmd = ConstruirComando(comand, cm);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                throw new Exception("Falha ao executar o comando.\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
            return cmd.LastInsertedId;
        }
    }
}
