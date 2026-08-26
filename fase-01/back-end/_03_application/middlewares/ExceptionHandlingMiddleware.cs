
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace fase_01.application.middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // continua a fluxo normal
                await _next(context);
            }
            catch (System.Exception ex)
            {
                // intercepta qualquer exceção não tratada e registra nos logs
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // 1. Log detalha da falha com nível de severidade
            _logger.LogError(
                ex,
                "Ocorreu uma falha não tratada na requisição {Method} em {Path}. Message: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message
            );

            // 2. Define o cabeçalho HTTP e o StatusCode para 500
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 3. Monta resposta padronizada
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Ocorreu um erro interno no servidor",
                Detail = _env.IsDevelopment() ? ex.Message : "Contate o suporte caso o problema persista", // em produção oculta dados sensíveis
                Instance = context.Request.Path
            };

            if (_env.IsDevelopment())
                problemDetails.Extensions.Add("StackTrace", ex.StackTrace);

            // 4. escreve na resposta
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}