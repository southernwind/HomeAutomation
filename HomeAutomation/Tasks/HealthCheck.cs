using Database.Tables;

using DataBase;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Slack.Webhooks;

using System.Net.NetworkInformation;

namespace HomeAutomation.Tasks;
public class HealthCheck : IAutomationTask
{
    public enum CheckType
    {
        Ping = 0
    };

    private readonly ILogger<HealthCheck> _logger;
    private readonly HomeServerDbContext _dbContext;
    private readonly string _healthCheckWebHookUrl;
    public HealthCheck(ILogger<HealthCheck> logger, HomeServerDbContext dbContext, IConfiguration configuration)
    {
        this._logger = logger;
        this._dbContext = dbContext;
        this._healthCheckWebHookUrl = configuration.GetSection("WebHookUrl").GetSection("HealthCheckSlack").Get<string>();
    }

    public async Task ExecuteAsync()
    {
        this._logger.LogInformation($"Start");
        var slackClient = new SlackClient(this._healthCheckWebHookUrl);
        var ping = new Ping();
        while (true)
        {
            this._logger.LogInformation($"Check");
            var idList = from r in this._dbContext.HealthCheckResults
                         group r by r.HealthCheckTargetId into g
                         select g.Max(x => x.HealthCheckResultId);

            var query = from t in this._dbContext.HealthCheckTargets
                        join r in this._dbContext.HealthCheckResults
                        on t.HealthCheckTargetId equals r.HealthCheckTargetId
                        where t.IsEnable
                        where idList.Any(x => x == r.HealthCheckResultId) || r.HealthCheckResultId == 0
                        select new 
                        {
                            t.HealthCheckTargetId,
                            t.Name,
                            t.Host,
                            t.CheckType,
                            r.HealthCheckResultId,
                            r.DateTime,
                            r.State,
                            r.Reason
                        };
            var targets  = await query.ToListAsync();
            foreach (var target in targets){
                if ((CheckType)target.CheckType == CheckType.Ping)
                {
                    var reply = await ping.SendPingAsync(target.Host);
                    var state = reply.Status != IPStatus.Success;
                    if (state != target.State)
                    {
                        if (state)
                        {
                            await Task.Delay(new TimeSpan(0, 1, 0));
                            reply = await ping.SendPingAsync(target.Host);
                            if(reply.Status == IPStatus.Success)
                            {
                                continue;
                            }
                        }
                        await slackClient.PostAsync(new SlackMessage { Text = $"Name:{target.Name} \nHost:{target.Host} \nReplyStatus:{reply.Status}({(int)reply.Status})", Username = "ヘルスチェッカー" });

                        this._dbContext.HealthCheckResults.Add(new HealthCheckResult()
                        {
                            HealthCheckTargetId = target.HealthCheckTargetId,
                            DateTime = DateTime.Now,
                            State = reply.Status != IPStatus.Success,
                            Reason = $"ReplyStatus = {reply.Status}"
                        });
                        await this._dbContext.SaveChangesAsync();
                    }
                }
            }
            await Task.Delay(new TimeSpan(0, 1, 0));

        }
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    void IDisposable.Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}