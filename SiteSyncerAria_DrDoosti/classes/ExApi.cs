using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti
{
    class ExApi
    {
        private bool _dateExpire = false;
        private bool _access = false;

        private static string GetMacAddress()
        {
            string macaddress = string.Empty;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macaddress += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(macaddress))
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                        continue;
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macaddress += nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(macaddress))
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet)
                        continue;
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macaddress += nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(macaddress))
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.FastEthernetT)
                        continue;
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macaddress += nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
            }
            return macaddress;
        }

        private void InsertExpire(string ExpireDateSite)
        {
            try
            {
                    Database.ExecuteSql(@"delete setting Where [tag]='AriaSite' and [key]='ExpireDate24' and [sys]='-1'");
                    Database.ExecuteSql(@"insert into setting (tag,[Key],val,def,[sys]) 
                                           values('AriaSite','ExpireDate13'," + ExpireDateSite + "," + ExpireDateSite + ",'-1')");                   
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> CheckAccess(string codeco, string tinycode)
        {
            bool access = false;
            tinycode = tinycode.Replace("-", "");
            try
            {                
                string macadresse = GetMacAddress();

                macadresse = string.IsNullOrEmpty(macadresse) ? "1234567890" : macadresse;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://ariaic.ir/api/softwarelock/checkaccess?codeco={codeco}&locknumber={tinycode}&softwareid=24&mac={macadresse}");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string content = new StreamReader(response.GetResponseStream()).ReadToEnd();
                content = content.Replace("\"", "");
                string[] accessArray = content.Split(',');

                if (accessArray[1] == "access:true")
                {
                    access = true;
                    _access = true;
                    string[] expire = accessArray[3].Split(':');
                    string ExpireDateSite = expire[1].Replace("-", "").Replace("}", "");

                    string ExpireDateSiteReverse = string.Empty; //variable to stored the result  
                    int Length = ExpireDateSite.Length - 1;
                    while (Length >= 0)
                    {
                        ExpireDateSiteReverse = ExpireDateSiteReverse + ExpireDateSite[Length];
                        Length--;
                    } // End of while 
                      //int ex1 = Convert.ToInt32(ExpireDateSite) + Convert.ToInt32(ExpireDateSiteReverse) + 1367;

                    string ex = (Convert.ToInt32(ExpireDateSite) + Convert.ToInt32(ExpireDateSiteReverse) + 1367).ToString();

                    ex = ex.Substring(0, 2) + ExpireDateSite.Substring(6, 2) + ex.Substring(2, 2) + ExpireDateSite.Substring(4, 2) + ex.Substring(4, 2) + ExpireDateSite.Substring(2, 2) + ex.Substring(6, 2) + ExpireDateSite.Substring(0, 2) + ex.Substring(8, ex.Length - 8);
                    DateTime date = DateTime.Now;
                    if (Convert.ToDateTime(expire[1].Replace("}", "")) > date)
                    {
                        InsertExpire(ex);
                        _dateExpire = true;
                    }
                    else
                    {
                        access = true;
                        // ********* 
                        _dateExpire = true;
                    }
                }
                else
                {
                    access = false;
                }
            }
            catch (Exception)
            {

                DataTable tExpireDate13 = Database.TableSqlQuery(@"select isnull(val,0) from setting Where [tag]='AriaSite' and [key]='ExpireDate24' and [sys]='-1'");                    
                string ex = tExpireDate13.Rows[0][0].ToString();
                ex = ex.Substring(14, 2) + ex.Substring(10, 2) + ex.Substring(6, 2) + ex.Substring(2, 2);
                DateTime date = DateTime.Now;
                string dateex = (date.Year.ToString("00") + date.Month.ToString("00") + date.Day.ToString("00")).ToString();
                if (Convert.ToUInt32(ex) > Convert.ToInt32(dateex))
                {
                    access = true;
                    _access = true;
                    _dateExpire = true;
                }
                else
                {
                    access = false;
                    _access = true;
                    _dateExpire = false;
                }                                    
            }

            return access;
        }
    }
}
