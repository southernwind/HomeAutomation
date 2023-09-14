using DataBase;

using HomeAutomation.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Slack.Webhooks;

using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HomeAutomation;

public class Executor
{
    private readonly IEnumerable<IAutomationTask> _targets;
    private bool _disposedValue;
    private readonly CompositeDisposable _disposable = new();
    private readonly ILogger<Executor> _logger;
    private readonly string _errorNotifyWebHookUrl;

    public Executor(ILogger<Executor> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this._targets = serviceProvider.GetServices<IAutomationTask>();
        this._logger = logger;
        this._errorNotifyWebHookUrl = configuration.GetSection("WebHookUrl").GetSection("ErrorSlack").Get<string>();
    }
    public async Task StartAsync()
    {
        await Task.WhenAll(this._targets.Select(this.ExecuteAsync).ToArray());
    }
    
    private async Task ExecuteAsync(IAutomationTask task)
    {
        while (true)
        {
            try
            {
                await task.ExecuteAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(0, ex, "Automation Error");
                using var slackClient = new SlackClient(this._errorNotifyWebHookUrl);
                await slackClient.PostAsync(new SlackMessage { Text = ex.Message });
            }
        }
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
