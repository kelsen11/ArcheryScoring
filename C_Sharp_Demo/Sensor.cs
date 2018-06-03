using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_Sharp_Demo
{
    public static class WindSensor
    {
        public static bool Connect = false;
        private static string Port;
        private static string StatMessage = "未连接";
        public static char Direction;
        public static double SpeedDouble;   /*风速数值*/
        public static string Speed;         /*风速文字*/

        public static void SetSerialPort(string port)
        {
            Port = port;  
        }

        public static string GetSerialPort()
        {
            return Port;
        }

        public static void SetStatMessage(string msg)
        {
            StatMessage = msg;
        }

        public static string GetErrMsg()
        {
            return StatMessage;
        }


    }
}
