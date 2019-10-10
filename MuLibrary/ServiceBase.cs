using Microsoft.Extensions.DependencyInjection;
using MuLibrary.Logging;
using System;

namespace MuLibrary
{
    public class ServiceBase
    {
        protected readonly LoggingService _log;

        public ServiceBase(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();
            _log.Log($"{ this.GetType().Name } created");
        }
    }
}
