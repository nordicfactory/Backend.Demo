using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using DI;

public class InterceptorDemo
{
    public void Configure()
    {
        var builder = new ContainerBuilder();
        
        builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(InterceptorDemo)))
            .Where(t => t.Name.EndsWith("Interceptor"));
        
        builder
            .RegisterGeneric(typeof(AzureQueue<>))
            .As(typeof(IQueue<>))
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(ExceptionLoggingInterceptor))
            .SingleInstance();
        
    }

    public static void Demo()
    {
        var azq = new AzureQueue<ExampleMessage>();
        var generator = new ProxyGenerator();
        var proxy = (IQueue<ExampleMessage>)generator.CreateInterfaceProxyWithTarget(typeof(IQueue<ExampleMessage>), azq, new ExceptionLoggingInterceptor(null));
        proxy.Send(new ExampleMessage());
    }
}