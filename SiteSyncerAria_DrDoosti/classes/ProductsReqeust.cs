using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.classes
{
    public class ProductsReqeust
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ProductsReqeust(string baseUrl, HttpClient httpClient)
        {
            //var handler = new HttpClientHandler()
            //{
            //    Proxy = new WebProxy("http://127.0.0.1:8888", false),
            //    UseProxy = true
            //};
            //_httpClient = new HttpClient(handler);
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/') + "/";
        }

        private async Task<HttpResponseMessage> PostJsonAsync(string url, string jsonPayload, string token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }                     
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                request.Content = new StringContent(
                    jsonPayload, Encoding.UTF8, "application/json");

                return await _httpClient.SendAsync(request);
            }
        }       

        public async Task<responseBody> ProductS(requestKala products, string token)
        {

            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback +=
            //(sender, cert, chain, sslPolicyErrors) => true;

            //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            string apiUrl = $"{_baseUrl}api/v1/login";
           
            string jsonPayload = JsonConvert.SerializeObject(products);

            HttpResponseMessage response;

            try
            {
                response = await PostJsonAsync(apiUrl, jsonPayload, token);
            }
            catch (TaskCanceledException ex)
            {
                return new responseBody
                {
                    message = $"خطای تایم‌اوت ارتباط با سرور (Timeout): {ex.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (HttpRequestException ex)
            {
                return new responseBody
                {
                    message = $"خطای شبکه یا اتصال HTTP: {ex.Message} | جزئیات: {ex.InnerException?.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (Exception ex)
            {
                return new responseBody
                {
                    message = $"خطای ناشناخته در ارسال درخواست: {ex.Message}",
                    success = false,
                    dtos = null
                };
            }
            using (response)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 404)
                {
                    return new responseBody
                    {
                        message = $"404 : بدنه`فروشنده یافت نشد. یا محصول یافت نشد.",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 422)
                {
                    return new responseBody
                    {
                        message = $"422: اطلاعات ارسالی معتبر نیست.",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 403)
                {
                    return new responseBody
                    {
                        message = $"403: فروش این محصول متوقف شده است.",
                        success = false,
                        dtos = null,
                    };
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return new responseBody
                    {
                        message = $"HTTP Error: {response.StatusCode} - {responseBody}",
                        success = false,
                        dtos = null,
                    };
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<responceKala>(responseBody);
                        if (result != null)
                        {
                            return new responseBody
                            {
                                message = "",
                                success = true,
                                dtos = result,
                            };
                        }
                        else
                        {
                            return new responseBody
                            {
                                message = "Received an empty or invalid response from the server.",
                                success = false,
                                dtos = null,
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {

                        return new responseBody
                        {
                            message = $"Failed to parse server response. JSON Error: {jsonEx.Message}. Raw response: {responseBody}",
                            success = false,
                            dtos = null,
                        };
                    }
                }
            }

        }

        public async Task<responsetokem> GetToken(requestoken requestoken)
        {
            string apiUrl = $"{_baseUrl}api/v1/authenticate";

            string jsonPayload = JsonConvert.SerializeObject(requestoken);

            HttpResponseMessage response;
            try
            {
                response = await PostJsonAsync(apiUrl, jsonPayload, null);
            }
            catch (TaskCanceledException ex)
            {
                return new responsetokem
                {
                    message = $"خطای تایم‌اوت ارتباط با سرور (Timeout): {ex.Message}",
                    success = false,
                    data = null
                };
            }
            catch (HttpRequestException ex)
            {
                return new responsetokem
                {
                    message = $"خطای شبکه یا اتصال HTTP: {ex.Message} | جزئیات: {ex.InnerException?.Message}",
                    success = false,
                    data = null
                };
            }
            catch (Exception ex)
            {
                return new responsetokem
                {
                    message = $"خطای ناشناخته در ارسال درخواست: {ex.Message}",
                    success = false,
                    data = null
                };
            }
            using (response)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 404)
                {
                    return new responsetokem
                    {
                        message = $"404 : بدنه`فروشنده یافت نشد. یا محصول یافت نشد.",
                        success = false,
                        data = null,
                    };
                }
                else if ((int)response.StatusCode == 422)
                {
                    return new responsetokem
                    {
                        message = $"422: اطلاعات ارسالی معتبر نیست.",
                        success = false,
                        data = null,
                    };
                }
                else if ((int)response.StatusCode == 403)
                {
                    return new responsetokem
                    {
                        message = $"403: حساب فروشنده غیرفعال است/حساب فروشنده هنوز تایید نشده است/API Key نامعتبر است.",
                        success = false,
                        data = null,
                    };
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return new responsetokem
                    {
                        message = $"HTTP Error: {response.StatusCode} - {responseBody}",
                        success = false,
                        data = null,
                    };
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Data>(responseBody);
                        if (result != null)
                        {
                            return new responsetokem
                            {
                                message = "",
                                success = true,
                                data = result,
                            };
                        }
                        else
                        {
                            return new responsetokem
                            {
                                message = "Received an empty or invalid response from the server.",
                                success = false,
                                data = null,
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {

                        return new responsetokem
                        {
                            message = $"Failed to parse server response. JSON Error: {jsonEx.Message}. Raw response: {responseBody}",
                            success = false,
                            data = null,
                        };
                    }
                }
            }
        }
    }
}
