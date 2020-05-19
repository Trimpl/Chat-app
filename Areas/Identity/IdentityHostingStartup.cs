using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Chat.Areas.Identity.IdentityHostingStartup))]
namespace Chat.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}