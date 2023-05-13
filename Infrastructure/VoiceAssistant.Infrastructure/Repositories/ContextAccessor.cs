using AuraSearch.Abstractions;
using AuraSearch.Models;
using Microsoft.Extensions.Logging;

namespace VoiceAssistant.Infrastructure.Repositories
{
    public sealed class ContextAccessor : IContextAccessor
    {
        private static List<Context> _context = new();

        private readonly ILogger _logger;

        public ContextAccessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ContextAccessor>();
            _logger.LogInformation("Creating instance of context accessor"); 
        }

        public ValueTask<IReadOnlyCollection<Context>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Context> rdOnly = _context.ToList().AsReadOnly();

            return ValueTask.FromResult(rdOnly);
        }

        public Task GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask<Context> GetContextAsync(Guid clientIdentifier, CancellationToken cancellationToken)
        {
            if (!_context.Any(x => x.ClientIdentifier == clientIdentifier))
            {
                _context.Add(new Context
                { 
                    ClientIdentifier = clientIdentifier  
                }); 
            }
            
            return ValueTask.FromResult(_context.FirstOrDefault(x => x.ClientIdentifier == clientIdentifier));

        }

        public void SetContext(Context context) => _context.Add(context);
    }
}
