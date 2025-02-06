using TopModel.Core;
using TopModel.Core.Loaders;

namespace TopModel.UI;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Startup>();
           })
           .ConfigureServices(services =>
           {
               var fileChecker = new FileChecker();
               var configFile = new FileInfo(args[0]);
               var config = fileChecker.Deserialize<ModelConfig>(configFile.OpenText().ReadToEnd()).Init(configFile.DirectoryName!);

               services
                   .AddModelStore(fileChecker, config)
                   .AddHostedService<ModelWatcherService>()
                   .AddSingleton<ModelFileProvider>()
                   .AddSingleton<IModelWatcher>(p => p.GetRequiredService<ModelFileProvider>());
           })
           .Build()
           .Run();
    }
}