using System.Web;

namespace FaxOut
{
    public class ModelHelper
    {
        public static string PhoneFormat(string phone)
        {
            return string.Format("{0:###-###-####}", long.Parse(phone));
        }

        public static string CentsToDollars(long cents)
        {
            return ((decimal)cents / 100).ToString("C");
        }


        private string GetUserIP()
        {
            string ipList = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}