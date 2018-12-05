using System;
using System.Diagnostics;

namespace ServidorProdutos.Modelo
{
    public class Diagnostico
    {
		public static int ObterUsoCpu()
        {
			return 50;
        }

        public static int ObterUsoMemoria()
        {
            return Convert.ToInt32(GC.GetTotalMemory(false));
        }
    }
}
