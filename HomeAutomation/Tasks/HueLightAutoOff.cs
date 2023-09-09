﻿using System.Net.NetworkInformation;

namespace HomeAutomation.Tasks;
public class HueLightAutoOff : IAutomationTask
{
    public HueLightAutoOff()
    {

    }

    public async Task ExecuteAsync()
    {
        var ping = new Ping();
        var httpClient = new HttpClient();
        var content = new StringContent(@"{""on"":false}");
        while (true)
        {
            await Task.Delay(new TimeSpan(0, 0, 10));
            var reply = await ping.SendPingAsync("13700k.localnet");
            if (reply.Status != IPStatus.Success)
            {
                await Task.Delay(new TimeSpan(0, 0, 20));
                reply = await ping.SendPingAsync("13700k.localnet");
                if (reply.Status != IPStatus.Success)
                {
                    await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/1/state", content);
                    await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/2/state", content);
                    await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/3/state", content);
                }
            }
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