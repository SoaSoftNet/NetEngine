using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Models.Dtos;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskAdmin.Libraries.Http
{
    public static class HttpContext
    {
        public static Microsoft.AspNetCore.Http.HttpContext Current()
        {
            var httpContextAccessor = Program.ServiceProvider.GetService<IHttpContextAccessor>();

            httpContextAccessor.HttpContext.Request.Body.Position = 0;

            return httpContextAccessor.HttpContext;
        }

        /// <summary>
        /// 获取Url信息
        /// </summary>
        /// <returns></returns>
        public static string GetUrl()
        {
            return GetBaseUrl() + $"{Current().Request.Path}{Current().Request.QueryString}";
        }


        /// <summary>
        /// 获取基础Url信息
        /// </summary>
        /// <returns></returns>
        public static string GetBaseUrl()
        {

            var url = $"{Current().Request.Scheme}://{Current().Request.Host.Host}";

            if (Current().Request.Host.Port != null)
            {
                url = url + $":{Current().Request.Host.Port}";
            }

            return url;
        }



        /// <summary>
        /// RequestBody中的内容
        /// </summary>
        public static string GetRequestBody()
        {
            using (Stream requestBody = new MemoryStream())
            {
                Current().Request.Body.CopyTo(requestBody);
                Current().Request.Body.Position = 0;
                requestBody.Position = 0;
                using (var requestReader = new StreamReader(requestBody))
                {
                    return requestReader.ReadToEnd();
                }
            }
        }



        /// <summary>
        /// 获取 http 请求中的全部参数
        /// </summary>
        public static List<dtoKeyValue> GetParameter()
        {
            var context = Current();

            var parameters = new List<dtoKeyValue>();

            if (context.Request.Method == "POST")
            {
                string body = HttpContext.GetRequestBody();

                if (!string.IsNullOrEmpty(body))
                {
                    parameters.Add(new dtoKeyValue { Key = "body", Value = body });
                }
                else if (context.Request.HasFormContentType)
                {
                    var fromlist = context.Request.Form.OrderBy(t => t.Key).ToList();

                    foreach (var fm in fromlist)
                    {
                        parameters.Add(new dtoKeyValue { Key = fm.Key, Value = fm.Value.ToString() });
                    }
                }
            }
            else if (context.Request.Method == "GET")
            {
                var queryList = context.Request.Query.ToList();

                foreach (var query in queryList)
                {
                    parameters.Add(new dtoKeyValue { Key = query.Key, Value = query.Value });
                }
            }

            return parameters;
        }


        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIpAddress()
        {
            return Current().Connection.RemoteIpAddress.ToString();
        }



        /// <summary>
        /// 获取Header中的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetHeader(string key)
        {
            var query = Current().Request.Headers.Where(t => t.Key.ToLower() == key.ToLower()).Select(t => t.Value);

            var ishave = query.Count();

            if (ishave != 0)
            {
                return query.FirstOrDefault().ToString();
            }
            else
            {
                return "";
            }
        }

    }
}
