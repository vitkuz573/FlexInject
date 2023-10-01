using FlexInject;
using FlexInject.Attributes;
using System;

namespace FlexInjectExample
{
    class Program
    {
        static void Main()
        {
            var services = new FlexServiceCollection();
            services.AddSingleton<ILoggerService, ConsoleLoggerService>();
            services.AddTransient<Application, Application>();

            using var container = services.BuildServiceProvider();

            var app = container.Resolve<Application>();
            app.Run();
        }
    }

    public class Application
    {
        [Inject]
        public ILoggerService LoggerService { get; set; }

        public void Run()
        {
            LoggerService.Log("Hello from Application!");
        }
    }

    public interface ILoggerService
    {
        void Log(string message);
    }

    public class ConsoleLoggerService : ILoggerService
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
