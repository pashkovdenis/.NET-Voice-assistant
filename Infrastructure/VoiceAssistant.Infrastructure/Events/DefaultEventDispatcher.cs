using AuraSearch.Abstractions;
using AuraSearch.EventHandlers;
using Microsoft.Extensions.DependencyInjection; 

namespace VoiceAssistant.Infrastructure.Events
{
    public sealed class DefaultEventDispatcher : EventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultEventDispatcher(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        public DefaultEventDispatcher() => SetInstance(this);

        public override void Dispatch(IDomainEvent @event)
        {
            if (@event == null) throw new ArgumentNullException("event");

            Type handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());

            var handlers = _serviceProvider.GetServices(handlerType).Where(t => t != null);

            foreach (dynamic handler in handlers)
            {
                handler.HandleAsync((dynamic)@event);
            }
        }
    }
}
