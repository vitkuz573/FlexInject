namespace ConsoleApp1;

public class MyService : IService
{
    private readonly IOtherService _otherService;

    public MyService(IOtherService otherService)
    {
        _otherService = otherService;
    }

    public void DoSomething()
    {
        Console.WriteLine("Doing something!");
        _otherService.DoOtherThing();
    }
}