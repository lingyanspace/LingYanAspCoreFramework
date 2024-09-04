namespace LingYanAspCoreFramework.Events
{
    public class LYEventBus : ILYEventBus
    {
        private readonly IDictionary<Type, List<Type>> _eventHandlers;
        public LYEventBus()
        {
            _eventHandlers = new Dictionary<Type, List<Type>>();
        }
        public async Task<TBackObject> Publish<TEvent, TBackObject>(TEvent @event, LYEventType eventMakeType, Type targetEvent = null)
        {
            var eventType = typeof(TEvent);
            if (_eventHandlers.ContainsKey(eventType))
            {
                var handlers = _eventHandlers[eventType];
                foreach (var handlerType in handlers)
                {
                    if (targetEvent == null || handlerType == targetEvent)
                    {
                        var handler = Activator.CreateInstance(handlerType) as LYEventHanle<TEvent>;
                        switch (eventMakeType)
                        {
                            case LYEventType.CREATE:
                                return await handler.CreateAsync<TBackObject>(@event);
                            case LYEventType.UPDATE:
                                return await handler.UpdateAsync<TBackObject>(@event);
                            case LYEventType.DELETE:
                                return await handler.DeleteAsync<TBackObject>(@event);
                            case LYEventType.GET:
                                return await handler.GetAsync<TBackObject>(@event);
                            case LYEventType.OTHER:
                                return await handler.OtherAsync<TBackObject>(@event);
                        }
                    }
                }
            }
            return default;
        }

        public async Task Publish<TEvent>(TEvent @event, LYEventType eventMakeType, Type targetEvent = null)
        {
            var eventType = typeof(TEvent);
            if (_eventHandlers.ContainsKey(eventType))
            {
                var handlers = _eventHandlers[eventType];
                foreach (var handlerType in handlers)
                {
                    if (targetEvent == null || handlerType == targetEvent)
                    {
                        var handler = Activator.CreateInstance(handlerType) as LYEventHanle<TEvent>;
                        switch (eventMakeType)
                        {
                            case LYEventType.CREATE:
                                await handler.CreateAsync(@event);
                                break;
                            case LYEventType.UPDATE:
                                await handler.UpdateAsync(@event);
                                break;
                            case LYEventType.DELETE:
                                await handler.DeleteAsync(@event);
                                break;
                            case LYEventType.GET:
                                await handler.GetAsync(@event);
                                break;
                            case LYEventType.OTHER:
                                await handler.OtherAsync(@event);
                                break;
                        }
                    }
                }
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEventHandler : LYEventHanle<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(TEventHandler);

            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Add(handlerType);
            }
            else
            {
                _eventHandlers[eventType] = new List<Type> { handlerType };
            }
        }

        public void Subscribe<TEvent>(LYEventHanle<TEvent> eventHandler)
        {
            var eventType = typeof(TEvent);
            var handlerType = eventHandler.GetType();

            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Add(handlerType);
            }
            else
            {
                _eventHandlers[eventType] = new List<Type> { handlerType };
            }
        }
    }
}
