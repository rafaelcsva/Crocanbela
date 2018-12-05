using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace ServidorUsuarios.Modelo
{
    public class Diagnostico
    {
		public static int ObterUsoCpu()
        {
			return 30;
        }

		public static int ObterUsoMemoria(){
			return Convert.ToInt32(GC.GetTotalMemory(false));
		}
    }
}
