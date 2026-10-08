using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net;
using System.Data.Common;
using System.Web.Script.Serialization;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection.Emit;
using SiteSyncerAria_DrDoosti.classes;

namespace SiteSyncerAria_DrDoosti
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer(); // name space(using System.Timers;)
        private static readonly HttpClient client = new HttpClient();
        string url;
        string username;
        string password;
        string _token;
        bool gettoken;


        public Service1()
        {
            var startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            SiteSyncerAria.IniFile ini = new SiteSyncerAria.IniFile(@"" + startupPath + @"\Settings.ini");
            UserConnection.server = ini.IniReadValue("Form", "Server");
            UserConnection.database = ini.IniReadValue("Form", "dataBase");
            UserConnection.usernamesql = ini.IniReadValue("Form", "user");
            UserConnection.passwordsql = ini.IniReadValue("Form", "pass");
            UserConnection.LockNumber1 = ini.IniReadValue("Form", "LockNumber1");
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            WriteToFile("شروع بکار سرویس ارسال اطلاعات کالاها " + DateTime.Now);
            timer.Interval = 3000; //number in milisecinds
            DatabaseSQL.InitConnectionStringSQLConnection(UserConnection.server, UserConnection.database, UserConnection.usernamesql, UserConnection.passwordsql);
            DatabaseSQL.InitConnectionString(UserConnection.server, UserConnection.database, UserConnection.usernamesql, UserConnection.passwordsql);
            try
            {
                string connection = @"Data Source=" + UserConnection.server + ";Initial Catalog=" + UserConnection.database + " ;User ID= " + UserConnection.usernamesql + ";Password=" + UserConnection.passwordsql + ";Integrated Security=false";
                using (SqlConnection con = new SqlConnection(connection))
                {
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandText = " select val from setting where tag='sitesyncer' and [key]='Url' ";
                        SqlDataAdapter adapter = new SqlDataAdapter(com);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        url = dt.Rows.Count > 0 && !dt.Rows[0].IsNull(0)
                                ? dt.Rows[0][0].ToString()
                                : null;
                    }
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandText = " select val from setting where tag='sitesyncer' and [key]='User' ";
                        SqlDataAdapter adapter = new SqlDataAdapter(com);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        username = dt.Rows.Count > 0 && !dt.Rows[0].IsNull(0)
                                ? dt.Rows[0][0].ToString()
                                : null;
                    }
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandText = " select val from setting where tag='sitesyncer' and [key]='Password' ";
                        SqlDataAdapter adapter = new SqlDataAdapter(com);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        password = dt.Rows.Count > 0 && !dt.Rows[0].IsNull(0)
                                ? dt.Rows[0][0].ToString()
                                : null;
                    }
                    if (string.IsNullOrWhiteSpace(username) ||
                        string.IsNullOrWhiteSpace(password) ||
                        string.IsNullOrWhiteSpace(url))
                    {
                        throw new InvalidOperationException("یکی از تنظیمات API یعنی URL، نام کاربری یا رمز خالی است.");

                    }
                    get_Token();
                }
            }
            catch (Exception ex)
            {
                WriteToFile("خطا در شروع بکار سرویس ارسال کالاها: " + ex);
                throw;
            }

        }

        private async void get_Token()
        {
            try
            {
                requestoken tk = new requestoken() { mobile = username, api_key = password, };
                var service = new ProductsReqeust(url, client);
                try
                {
                    var response = await service.GetToken(tk);
                    if (response.success && response.data !=null && !string.IsNullOrEmpty(response.data.token))
                    {
                        _token = response.data.token.ToString();
                        gettoken = true;
                    }
                    else
                    {
                        _token = null;
                        gettoken = false;
                    }
                }
                catch (Exception ex)
                {
                    WriteToFile("خطا در اطلاعات توکن ارسال کالاها: " + ex);
                    _token = null;
                    gettoken = false;
                }
            }
            catch (Exception ex)
            {
                WriteToFile(" خطا دردریافت توکن برای ارسال کالاها : " + ex.Message);
                gettoken = false;
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        protected override void OnStop()
        {
            WriteToFile("سرویس ارسال اطلاعات کالاها متوقف شد " + DateTime.Now);
            timer.Stop();
        }

        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            //TimeSpan ts = DateTime.Now.Subtract(timetoken);
            //WriteToFile(" ts : " + ts);
            if (string.IsNullOrEmpty(_token))
            {
                timer.Enabled = false;
                gettoken = false;
                get_Token();

            }
            else
            {
                if (gettoken)
                {
                    await senttosite();
                }
                else
                {
                    timer.Enabled = false;
                    get_Token();
                }
            }

        }

        public async Task senttosite()
        {
            try
            {
                string connectionString = $@"Data Source={UserConnection.server};Initial Catalog={UserConnection.database};User ID={UserConnection.usernamesql};Password={UserConnection.passwordsql};Integrated Security=false";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    DataTable dt = new DataTable();
                    using (SqlCommand command = new SqlCommand("sp_kalachanged", con))
                    {
                        command.Parameters.AddWithValue("@state", 0);
                        command.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        timer.Enabled = false;
                        requestKala link = new requestKala();
                        int IDKalaChanged = Convert.ToInt32(dt.Rows[0]["ID"].ToString());
                        link.code = Convert.ToInt32(dt.Rows[0]["code"]);
                        link.price = dt.Rows[0]["price"] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[0]["price"]);
                        link.ava = Convert.ToInt32(dt.Rows[0]["ava"]);
                        link.avacheck = Convert.ToInt32(dt.Rows[0]["avacheck"]);
                        link.expire = dt.Rows[0]["expire"].ToString() ?? "";

                        await SendWebrequest_Post_Method(link, IDKalaChanged);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("خطا در دریافت اطلاعات کالا از آریا : " + ex.Message);
            }
            finally
            {
                if (!timer.Enabled) timer.Enabled = true;
            }
        }        

        private async Task SendWebrequest_Post_Method(requestKala kala, int IDKalaChanged)
        {
            var services = new ProductsReqeust(url, client);
            try
            {
                var response = await services.ProductS(kala, _token);
                resultinsert(response, IDKalaChanged);
            }
            catch (Exception ex)
            {
                WriteToFile($"خطا در بروزرسانی کالاها در سمت سایت {IDKalaChanged} : " + ex.Message);
            }
        }

        public void resultinsert(responseBody response, int IDKalaChanged)
        {
            try
            {
                string QMrec = JsonConvert.SerializeObject(response);
                int success = response.success ? 1 : 0;
                string error = response.success ? QMrec : response.message;
                string connectionString = (@"Data Source=" + UserConnection.server + ";Initial Catalog=" + UserConnection.database + " ;User ID= " + UserConnection.usernamesql + ";Password=" + UserConnection.passwordsql + ";Integrated Security=false");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand comm = new SqlCommand($@" UPDATE KalaChanged SET SentToSite = @SentToSite , SentToSiteTime = GETDATE() , ReasonPhrase = @ReasonPhrase , ChangeDate=GETDATE()  WHERE ID = @ID ", con);
                    comm.Parameters.Add(new SqlParameter("@SentToSite", SqlDbType.Bit)).Value = success;
                    comm.Parameters.Add(new SqlParameter("@ReasonPhrase", SqlDbType.VarChar, 500)).Value = error;
                    comm.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = IDKalaChanged;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    comm.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                WriteToFile(" خطا در بروزرسانی اطلاعات کالا در آریا : " + ex.Message);
            }

        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}