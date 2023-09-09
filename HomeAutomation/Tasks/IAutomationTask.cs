namespace HomeAutomation.Tasks;
public interface IAutomationTask : IDisposable
{
    /// <summary>
    /// スクレイピング実行
    /// </summary>
    public Task ExecuteAsync();
}