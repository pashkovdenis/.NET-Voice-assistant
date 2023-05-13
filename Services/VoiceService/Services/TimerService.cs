using AuraSearch.Domain.Events;
using AuraSearch.EventHandlers; 
using Microsoft.Extensions.Hosting;
using VoiceAssistant.Models;

namespace VoiceService.Services
{
    public sealed class TimerService : IHostedService, IDisposable
    { 
        private Timer _timer;
        private readonly Settings _settings;

        public TimerService(Settings settings)
        {
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {  
            _timer = new Timer(async (_) => await DoWorkAsync(_), null, TimeSpan.Zero, TimeSpan.FromSeconds(_settings.TimerInterval));

            return Task.CompletedTask;
        } 

        private async Task DoWorkAsync(object state)
        { 
            EventDispatcher.Instance?.Dispatch(new BoostOldThoughtEvent());
            EventDispatcher.Instance?.Dispatch(new RefreshEvent());
        }
         
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() => _timer?.Dispose();
    }
}
