using DataBase;

using HomeAutomation.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HomeAutomation;

public class Executor
{
    private readonly IEnumerable<IAutomationTask> _targets;
    private bool _disposedValue;
    private readonly CompositeDisposable _disposable = new();
    private readonly ILogger<Executor> _logger;

    public Executor(ILogger<Executor> logger, IServiceProvider serviceProvider)
    {
        this._targets = serviceProvider.GetServices<IAutomationTask>();
        this._logger = logger;
    }
    public async Task StartAsync()
    {
        await Task.WhenAll(this._targets.Select(x => x.ExecuteAsync()).ToArray());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposedValue)
        {
            return;
        }

        if (disposing)
        {
            this._disposable.Dispose();
        }
        this._disposedValue = true;
    }

    public void Dispose()
    {
        this.Dispose(true);
        System.GC.SuppressFinalize(this);
    }
}
