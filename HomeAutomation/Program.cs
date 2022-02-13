using System.Net.NetworkInformation;

var ping = new Ping();
var httpClient = new HttpClient();
var content = new StringContent(@"{""on"":false}");
while (true) {
    // やること増えてきたらやり方考える。
    Thread.Sleep(new TimeSpan(0, 0, 10));
    var reply = await ping.SendPingAsync("9700k.localnet");
    if (reply.Status != IPStatus.Success) {
        Thread.Sleep(new TimeSpan(0, 0, 20));
        reply = await ping.SendPingAsync("9700k.localnet");
        if (reply.Status != IPStatus.Success) {
            await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/1/state", content);
            await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/2/state", content);
            await httpClient.PutAsync("http://hue-bridge.localnet/api/lfRks8huc0jr-AIaa0l9iFdbatH38yPCsvEqUoh1/lights/3/state", content);
        }
    }
}