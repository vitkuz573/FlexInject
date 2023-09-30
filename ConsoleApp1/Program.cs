using ConsoleApp1;
using FlexInject;

var container = new FlexInjectContainer();

container.Register<IService, MyService>();
container.Register<IOtherService, OtherService>();

var service = container.Resolve<IService>();
service.DoSomething();