using LingYan.Model.EventModel;

namespace LingYan.Extension.Events
{
    public interface ILYEventBus 
    {
        Task<TBackObject> Publish<TEvent, TBackObject>(TEvent @event, LYEventType eventMakeType, Type targetEvent = null);
        Task Publish<TEvent>(TEvent @event, LYEventType eventMakeType, Type targetEvent = null);
        void Subscribe<TEvent, TEventHandler>()
            where TEventHandler : LYEventHanle<TEvent>;
        void Subscribe<TEvent>(LYEventHanle<TEvent> eventHandler);
    }
}
