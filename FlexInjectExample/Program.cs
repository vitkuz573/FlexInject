using FlexInject;
using FlexInject.Attributes;

namespace FlexInjectExample
{
    class Program
    {
        static void Main()
        {
            using var container = new FlexInjectContainer();
            container.RegisterSingleton<ILoggerService, ConsoleLoggerService>();
            container.Register<Application, Application>();

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
