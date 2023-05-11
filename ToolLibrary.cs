using DatabaseAdapters;
using FatumCore;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace PhlozLib
{
    public class ToolLibrary
    {
        public static IntDatabase getManagementDatabaseConnection(fatumconfig fatumConfig, Config config)
        {
            IntDatabase managementDB = null;

            try
            {
                string connectionstring = "";

                switch (config.GetProperty("InstanceType").ToLower())
                {
                    case "federation server":
                    case "federation master":
                        connectionstring = FatumLib.Unscramble(config.GetProperty("ConnectionString"), config.GetProperty("UniqueID"));
                        managementDB = new ADOConnector(connectionstring, "[Phloz]");
                        break;
                    case "standalone":
                        managementDB = new SQLiteDatabase(config.GetProperty("ManagementDatabase") + "\\management.s3db");
                        break;
                    case "cloud client":
                        managementDB = new SQLiteDatabase(config.GetProperty("ManagementDatabase") + "\\management.s3db");
                        break;
                }
            }
            catch (Exception xyz)
            {
                return null;
            }
            return managementDB;
        }

        public static IntDatabase getDatabaseConnection(HttpContent context)
        {
            IntDatabase managementDB = null;

            if (context.Headers["connectionstring"] == null)
            {
                Config config = new Config();

                fatumconfig fatumConfig = new fatumconfig();

                try
                {
                    //fatumConfig.Configuration = fatumConfig.loadPreferences();
                    string PreferenceFile = fatumConfig.ConfigDirectory + "\\Settings.xml";
                    if (!File.Exists(PreferenceFile))
                    {
                        // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
                        PreferenceFile = @"C:\Program Files\Phloz\settings.xml";
                    }
                    config.LoadConfig(PreferenceFile);
                    string connectionstring = "";

                    switch (config.GetProperty("InstanceType").ToLower())
                    {
                        case "federation server":
                        case "federation master":
                            connectionstring = FatumLib.Unscramble(config.GetProperty("ConnectionString"), config.GetProperty("UniqueID"));
                            managementDB = new ADOConnector(connectionstring, "[Phloz]");
                            context.Headers.Add("connectionstring", "A" + connectionstring);
                            break;
                        case "standalone":
                            managementDB = new SQLiteDatabase(config.GetProperty("ManagementDatabase") + "\\management.s3db");
                            context.Headers.Add("connectionstring", "F" + config.GetProperty("ManagementDatabase") + "\\management.s3db");
                            break;
                        case "cloud client":
                            managementDB = new SQLiteDatabase(config.GetProperty("ManagementDatabase") + "\\management.s3db");
                            context.Headers.Add("connectionstring", "F" + config.GetProperty("ManagementDatabase") + "\\management.s3db");
                            break;
                    }
                }
                catch (Exception xyz)
                {
                    return null;
                }
            }
            else
            {
                // We have a connection string stored in the session already, so let's go ahead and use it.

                string cstring = context.Headers["connectionstring"].ToString();
                string contype = cstring.Substring(0, 1);
                string cdata = cstring.Substring(1);
                switch (contype)
                {
                    case "A":  // ADO Connector
                        managementDB = new ADOConnector(cdata, "[Phloz]");
                        break;
                    case "F":  // SQLite Connector
                        managementDB = new SQLiteDatabase(cdata);
                        break;
                }
            }
            return managementDB;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string getSystemEmailForwarder(HttpContext context)
        {
            Config config = new Config();

            fatumconfig fatumConfig = new fatumconfig();

            try
            {
                //fatumConfig.Configuration = fatumConfig.loadPreferences();
                string PreferenceFile = fatumConfig.ConfigDirectory + "\\Settings.xml";
                if (!File.Exists(PreferenceFile))
                {
                    // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
                    PreferenceFile = @"C:\Program Files\Phloz\settings.xml";
                }
                config.LoadConfig(PreferenceFile);
                return config.GetProperty("SystemEmailForwarder");
            }
            catch (Exception xyz)
            {
                return null;
            }
        }

        public static string getSystemEmailParameter(HttpContext context)
        {
            Config config = new Config();
            fatumconfig fatumConfig = new fatumconfig();

            try
            {
                string PreferenceFile = fatumConfig.ConfigDirectory + "\\Settings.xml";
                if (!File.Exists(PreferenceFile))
                {
                    // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
                    PreferenceFile = @"C:\Program Files\Phloz\settings.xml";
                }
                config.LoadConfig(PreferenceFile);
                return config.GetProperty("SystemEmailParameter");
            }
            catch (Exception xyz)
            {
                return null;
            }
        }
    }
}
