using System;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data;
using Newtonsoft.Json.Linq;

namespace ServidorClientes.Modelo
{
	public class BancoDeDados
    {
		private static bool inicializada = false;
		private static MySqlConnectionStringBuilder connectionStringBuilder;
        
        private static MySqlConnection connect;

        private static void Conectar()
        {
            try
            {
				if(!inicializada){
					var file = File.ReadAllText("./Config/Info.json");
                    var conf = JObject.Parse(file);

					connectionStringBuilder = new MySqlConnectionStringBuilder
                    {
						Server = conf["banco"]["host"].ToString(),
						UserID = conf["banco"]["userID"].ToString(),
						Password = conf["banco"]["password"].ToString(),
						Database = conf["banco"]["database"].ToString(),
						Port = uint.Parse(conf["banco"]["porta"].ToString()),
                        SslMode = MySqlSslMode.None
                    };

					connect = new MySqlConnection(connectionStringBuilder.ConnectionString);

					inicializada = true;
				}

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
