using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

public class ExceptionLoggingInterceptor : AsyncInterceptorBase, IInterceptor
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IInterceptor _self;


    public ExceptionLoggingInterceptor(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _self = this.ToInterceptor();
    }

    protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
    {
        try
        {
            await proceed(invocation).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var logger = _loggerFactory.CreateLogger(invocation.TargetType);
            logger.LogError(e, e.Message);
            throw;
        }
    }


    protected override async Task<T> InterceptAsync<T>(IInvocation invocation, Func<IInvocation, Task<T>> proceed)
    {
        try
        {
            T t = await proceed(invocation).ConfigureAwait(false);
            return t;
        }
        catch (Exception e)
        {
            var logger = _loggerFactory.CreateLogger(invocation.TargetType);
            logger.LogError(e, e.Message);
            throw;
        }
    }

    public void Intercept(IInvocation invocation) => _self.Intercept(invocation);
}