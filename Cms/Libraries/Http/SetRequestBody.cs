﻿using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Cms.Libraries.Http
{


    /// <summary>
    /// 获取RequsetBody内容设置到静态变量中
    /// </summary>
    public class SetRequestBody
    {
        private readonly RequestDelegate _next;

        public SetRequestBody(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            context.Request.EnableBuffering();
            var requestReader = new StreamReader(context.Request.Body);

            var requestContent = requestReader.ReadToEnd();

            Cms.Libraries.Http.HttpContext.RequestBody = requestContent;

            context.Request.Body.Position = 0;

            await _next(context);
        }
    }
}