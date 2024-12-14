using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using plenidev.AnsiblePlayer.Core;
using plenidev.AnsiblePlayer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace plenidev.AnsiblePlayer.Utils
{
    internal static partial class HttpServiceCoreLogging
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "{Caller} starting action for result type {ResultType}")]
        public static partial void LogActionStarting(ILogger logger, string resultType, [CallerMemberName] string? caller = null);

        [LoggerMessage(Level = LogLevel.Information, Message = "{Caller} action complete with result type {ResultType}")]
        public static partial void LogActionComplete(ILogger logger, string resultType, [CallerMemberName] string? caller = null);
    }

    /// <summary>
    /// Base class for HttpServices enforcing recommended lifetimes on
    /// transient/non-transient HttpClient instances used via Do* methods.
    /// </summary>
    internal class HttpServiceCore: IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;

        private readonly Func<MaybeDisposer<HttpClient>> _getHttpClientAsMabyeDisposableResource;

        private readonly bool _transientClients;
        private readonly bool _disposeNonTransient;

        private ILogger Log => _logger;

        #region Constructors

        private HttpServiceCore(Func<ILogger> loggerFactory, Func<HttpClient> httpClientFactory, bool transientClients = false, bool disposeNonTransient = true)
        {
            _logger = loggerFactory();
            _transientClients = transientClients;
            _disposeNonTransient = disposeNonTransient;
            _getHttpClientAsMabyeDisposableResource = () => new MaybeDisposer<HttpClient>(resource: httpClientFactory(), disposing: transientClients);
        }

        public HttpServiceCore(ILogger logger, IHttpClientFactory factory, string name) : this(
            () => logger, 
            () => factory.CreateClient(name),
            transientClients: true
            ) { }

        public HttpServiceCore(ILogger logger, IHttpClientFactory factory) : this(
            () => logger, 
            factory.CreateClient,
            transientClients: true
            ) { }

        public HttpServiceCore(ILogger logger, HttpClient httpClient) : this(
            () => logger,
            () => httpClient,
            transientClients: false
            ) { }

        public HttpServiceCore(HttpClient httpClient) : this(
            () => NullLogger.Instance,
            () => httpClient,
            transientClients: false
            )
        { }

        static public HttpServiceCore Create(Action<HttpClient> configure)
        {
            var httpClient = new HttpClient();

            configure(httpClient);

            return new HttpServiceCore(
                () => NullLogger.Instance,
                () => httpClient,
                transientClients: false, 
                disposeNonTransient: true
                );
        }

        #endregion

        protected T DoClientAction<T>(Func<HttpClient, T> action)
        {
            using var maybeDisposer = _getHttpClientAsMabyeDisposableResource();

            HttpServiceCoreLogging.LogActionStarting(Log, (typeof(T).Name));

            var result = action(maybeDisposer.Resource);

            HttpServiceCoreLogging.LogActionComplete(Log, (typeof(T).Name));

            return result;
        }

        protected async Task<T> DoClientActionAsync<T>(Func<HttpClient, Task<T>> action)
        {
            using var disposer = _getHttpClientAsMabyeDisposableResource();

            HttpServiceCoreLogging.LogActionStarting(Log, (typeof(T).Name));

            var result = await action(disposer.Resource);
            
            HttpServiceCoreLogging.LogActionComplete(Log, (typeof(T).Name));

            return result;
        }

        public void Dispose()
        {
            if(!_transientClients && _disposeNonTransient)
            {
                _getHttpClientAsMabyeDisposableResource().ForceDispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_transientClients && _disposeNonTransient)
            {
                await _getHttpClientAsMabyeDisposableResource().ForceDisposeAsync();
            }
        }
    }

}
