using System.Configuration;

namespace FaxOut
{
    public class PhaxioHelper
    {
        public static bool IsTest
        {
            get { return ConfigurationManager.AppSettings["phaxio.mode"] != "live"; }
        }

        public static string Key
        {
            get { return ConfigurationManager.AppSettings[string.Format("phaxio.{0}.key", IsTest ? "test" : "live")]; }
        }

        public static string Secret
        {
            get { return ConfigurationManager.AppSettings[string.Format("phaxio.{0}.secret", IsTest ? "test" : "live")]; }
        }
    }
}