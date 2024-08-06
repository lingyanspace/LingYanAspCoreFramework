namespace LingYan.Extension.Events
{
    public class LYEventHanle<IEvent>
    {
        public virtual Task<T> CreateAsync<T>(IEvent @event) { return Task.FromResult(default(T)); }
        public virtual Task<T> UpdateAsync<T>(IEvent @event) { return Task.FromResult(default(T)); }
        public virtual Task<T> DeleteAsync<T>(IEvent @event) { return Task.FromResult(default(T)); }
        public virtual Task<T> GetAsync<T>(IEvent @event) { return Task.FromResult(default(T)); }
        public virtual Task<T> OtherAsync<T>(IEvent @event) { return Task.FromResult(default(T)); }
        public virtual Task CreateAsync(IEvent @event) { return Task.CompletedTask; }
        public virtual Task UpdateAsync(IEvent @event) { return Task.CompletedTask; }
        public virtual Task DeleteAsync(IEvent @event) { return Task.CompletedTask; }
        public virtual Task GetAsync(IEvent @event) { return Task.CompletedTask; }
        public virtual Task OtherAsync(IEvent @event) { return Task.CompletedTask; }
    }
}
