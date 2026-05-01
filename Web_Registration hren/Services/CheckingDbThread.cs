using Microsoft.Extensions.Hosting;
using Write;
namespace Web_Registration.Services;

public class CheckingDbThread : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Write.WriteDB.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
