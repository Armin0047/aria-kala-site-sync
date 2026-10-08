using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti
{
    internal class DatabaseSQL
    {
        internal static void InitConnectionString(string dbservername,string dbdatabasename,string dbusername,string dbpassword)
        {
            if (dbservername == null)
                Database.CONNECTION_STRING = @"Provider=SQLOLEDB.1;Data Source=.;Initial Catalog=emamreza;User ID=sa;Password=*";
            else
                Database.CONNECTION_STRING = string.Format("Provider=SQLOLEDB.1;Data Source = {0}; Initial Catalog = {1}; User ID = {2}; Password = {3};",
                    dbservername,
                    dbdatabasename,
                    dbusername,
                    dbpassword);
        }

        internal static void InitConnectionStringSQLConnection(string dbservername, string dbdatabasename, string dbusername, string dbpassword)
        {
            if (dbservername == null)
                Database.SQLCONNECTION_STRING = @"Data Source=.;Initial Catalog=emamreza;User ID=sa;Password=*";
            else
                Database.SQLCONNECTION_STRING = string.Format("Data Source = {0}; Initial Catalog = {1}; User ID = {2}; Password = {3};",
                    dbservername,
                    dbdatabasename,
                    dbusername,
                    dbpassword);
        }

    }
}
