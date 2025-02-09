using FlexInject;

namespace FlexInjectExample;

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

public class Application(ILoggerService loggerService)
{
    public void Run()
    {
        loggerService.Log("Hello from Application!");
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
