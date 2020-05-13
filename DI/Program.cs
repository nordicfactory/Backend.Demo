using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using DI.HttpClientHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DI
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            InterceptorDemo.Demo();
            
            var builder = new HostBuilder().ConfigureServices(ConfigureServices).UseConsoleLifetime();
            using var host = builder.Build();
            await host.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //generic registration
            services.AddSingleton(typeof(IQueue<>), typeof(AzureQueue<>));

            //bulk registration, with scrutor.
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IMessageHandler<>))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            //http client stuff
            services.AddTransient<TimeoutHandler>();
            services.AddTransient<ClientCredentialsRefreshHandler>();

            //Manually, or by some convention.
            var clients = new List<IHttpClientBuilder>
            {
                services.AddHttpClient<IMyClient, MyClient>(x => x.BaseAddress = new Uri("https://"))
            };
            
            clients.ForEach(c => c
                .AddHttpMessageHandler<ClientCredentialsRefreshHandler>()
                .AddHttpMessageHandler<TimeoutHandler>())
                //e.g. add polly here, with some conventions.
                ;
            
            services.AddTransient<IToggledService>(ctx =>
            {
                var user = ctx.GetRequiredService<IHttpContextAccessor>().HttpContext.User;
                var id = (ClaimsIdentity) user.Identity;
                if(id.FindFirst("email")?.Value == "specialUser")
                    return new ToggledService(
                    //some new implementation of dependency
                    );
                return new ToggledService(
                    //some old implementation of dependency
                );
            });
            
        }
    }
    

    public interface IMyClient
    {
        Task<string> GetMessage(string input);
    }

    public class MyClient : IMyClient
    {
        private readonly HttpClient _client;

        public MyClient(HttpClient client) => _client = client;

        public async Task<string> GetMessage(string input)
        {
            var path = $"somePath/{input}";
            var response = await _client.GetAsync(path);
            if (!response.IsSuccessStatusCode)
                //do error handling
                return null;

            return await response.Content.ReadAsStringAsync();
        }
    }

    public class ToggledService : IToggledService
    {
        
    }

    public interface IToggledService
    {
        
    } 

}