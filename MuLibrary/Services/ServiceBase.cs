using Microsoft.Extensions.DependencyInjection;
using System;

namespace MuLibrary.Services
{
    public class ServiceBase
    {
        protected readonly LoggingService _log;

        public ServiceBase(IServiceProvider provider)
        {
            _log = provider.GetService<LoggingService>();

            _log.Log($"{this.GetType().ToString()} created");
        }
    }
}
