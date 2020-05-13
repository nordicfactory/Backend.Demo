using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DI.HttpClientHandlers
{
    public class TimeoutHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(90));

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

            try
            {
                return await base.SendAsync(request, linkedToken.Token);
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
            catch (HttpRequestException e) when (e.InnerException is OperationCanceledException &&
                                                 cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException("The token was canceled", e);
            }
        }
    }
}