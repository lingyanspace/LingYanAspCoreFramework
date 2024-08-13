using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LingYan.DynamicShardingDBT.DBTModel
{
    internal class DiagnosticObserver : IObserver<DiagnosticListener>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly int _minCommandElapsedMilliseconds;
        public DiagnosticObserver(ILoggerFactory loggerFactory, int minCommandElapsedMilliseconds)
        {
            _loggerFactory = loggerFactory;
            _minCommandElapsedMilliseconds = minCommandElapsedMilliseconds;
        }
        public void OnCompleted()
            => throw new NotImplementedException();

        public void OnError(Exception error)
            => throw new NotImplementedException();

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DbLoggerCategory.Name) // "Microsoft.EntityFrameworkCore"
            {
                value.Subscribe(new KeyValueObserver(_loggerFactory, _minCommandElapsedMilliseconds));
            }
        }
    }
}
