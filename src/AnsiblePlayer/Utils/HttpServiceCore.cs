using Microsoft.Extensions.Logging;
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
    internal static partial class HttpServiceBaseLogging
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
    internal class HttpServiceCore
    {
        private readonly ILogger _logger;
        private readonly Func<MaybeDisposer<HttpClient>> _getHttpClientAsMabyeDisposableResource;

        private ILogger Log => _logger;

        #region Constructors

        private HttpServiceCore(Func<ILogger> loggerFactory, Func<HttpClient> httpClientFactory, bool transientClients = false)
        {
            _logger = loggerFactory();
            _getHttpClientAsMabyeDisposableResource = () => new(resource: httpClientFactory(), disposing: transientClients);
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

        #endregion

        protected T DoClientAction<T>(Func<HttpClient, T> action)
        {
            using var maybeDisposer = _getHttpClientAsMabyeDisposableResource();

            HttpServiceBaseLogging.LogActionStarting(Log, (typeof(T).Name));

            var result = action(maybeDisposer.Resource);

            HttpServiceBaseLogging.LogActionComplete(Log, (typeof(T).Name));

            return result;
        }

        protected async Task<T> DoClientActionAsync<T>(Func<HttpClient, Task<T>> action)
        {
            using var disposer = _getHttpClientAsMabyeDisposableResource();

            HttpServiceBaseLogging.LogActionStarting(Log, (typeof(T).Name));

            var result = await action(disposer.Resource);

            HttpServiceBaseLogging.LogActionComplete(Log, (typeof(T).Name));

            return result;
        }
    }
}
