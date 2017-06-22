using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colorful;
using System.Drawing;
using Console = Colorful.Console;

namespace CCVShell
{
    public static class Alert
    {
        public enum AlertType
        {
            Error,
            Warning,
            Success,
            Information,
            Normal
        }


        public static void AlertUser(AlertType type, string message, bool crlf = true)
        {
            Color c = Color.White;

            switch (type)
            {
                case AlertType.Error:
                    c = Color.OrangeRed;
                    break;
                case AlertType.Warning:
                    c = Color.Yellow;
                    break;
                case AlertType.Success:
                    c = Color.GreenYellow;
                    break;
                case AlertType.Information:
                    c = Color.DodgerBlue;
                    break;
                case AlertType.Normal:
                    c = Color.Gray;
                    break;
            }

            if (crlf)
                Console.WriteLine(message, c);
            else
                Console.Write(message, c);
            
        }
    }
}
