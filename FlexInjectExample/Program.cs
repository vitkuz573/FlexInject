using FlexInject;
using FlexInject.Attributes;

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

            var app = container.GetService<Application>();
            app.Run();
        }
    }

    public class Application
    {
        [Inject]
        private readonly ILoggerService _loggerService;

        public void Run()
        {
            _loggerService.Log("Hello from Application!");
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
