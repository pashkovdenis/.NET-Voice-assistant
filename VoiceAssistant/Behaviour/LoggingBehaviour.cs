using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace VoiceAssistant.Behaviour
{
    public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    { 
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }
       
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {typeof(TRequest).Name}");

            Type myType = request.GetType();

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(request, null);

                _logger.LogInformation("{Property} : {@Value}", prop.Name, propValue);
            }

            var response = await next();
            
            _logger.LogInformation($"Handled {typeof(TResponse).Name}"); 

            _logger.LogInformation($"Handled {JsonConvert.SerializeObject(response)}");

            return response;
        }
    }
}
