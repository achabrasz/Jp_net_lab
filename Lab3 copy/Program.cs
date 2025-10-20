using Autofac;
using Lab3;

var builder = new ContainerBuilder();
builder.RegisterType<CatCalc>().As<ICalculator>().SingleInstance();
builder.RegisterType<PlusCalc>().SingleInstance();
builder.RegisterType<StateCalc>().WithParameter("start", 5).SingleInstance();
builder.RegisterType<Worker>().WithParameter(
    (p, c) => p.ParameterType == typeof(string),
    (p, c) => c.Resolve<CatCalc>());
builder.RegisterType<Worker2>()
    .OnActivated(e => e.Instance.SetCalculator(e.Context.Resolve<PlusCalc>()));
builder.RegisterType<Worker3>()
    .OnActivated(e => e.Instance.calculator = e.Context.Resolve<PlusCalc>());

builder.RegisterType<Worker>()
    .Named<Worker>("state")
    .WithParameter(
    (p, c) => p.ParameterType == typeof(string),
    (p, c) => c.Resolve<StateCalc>());
builder.RegisterType<Worker2>()
    .Named<Worker2>("state")
    .OnActivated(e => e.Instance.SetCalculator(e.Context.Resolve<StateCalc>()));
builder.RegisterType<Worker3>()
    .Named<Worker3>("state")
    .OnActivated(e => e.Instance.calculator = e.Context.Resolve<StateCalc>());

builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
builder.RegisterType<TransactionContext>().As<ITransactionContext>().InstancePerMatchingLifetimeScope("transaction");
builder.RegisterType<StepOneService>();
builder.RegisterType<StepTwoService>();
builder.RegisterType<TransactionProcessor>();

var container = builder.Build();

var worker = container.Resolve<Worker>();
var worker2 = container.Resolve<Worker2>();
var worker3 = container.Resolve<Worker3>();

var workerState = container.ResolveNamed<Worker>("state");
var worker2State = container.ResolveNamed<Worker2>("state");
var worker3State = container.ResolveNamed<Worker3>("state");

Console.WriteLine(worker.Work("3", "4"));
Console.WriteLine(workerState.Work("3", "4"));
Console.WriteLine(worker2.Work("3", "4"));
Console.WriteLine(worker2State.Work("3", "4"));
Console.WriteLine(worker3.Work("3", "4"));
Console.WriteLine(worker3State.Work("3", "4"));

IUnitOfWork uowFromScope1;
IUnitOfWork uowFromScope2;

using (var scope1 = container.BeginLifetimeScope())
{
    uowFromScope1 = scope1.Resolve<IUnitOfWork>();
    var uowAnotherFromScope1 = scope1.Resolve<IUnitOfWork>();
    Console.WriteLine($"Czy instancje wewnątrz scope1 są takie same? {uowFromScope1.Id == uowAnotherFromScope1.Id} (ID: {uowFromScope1.Id})");
}

using (var scope2 = container.BeginLifetimeScope())
{
    uowFromScope2 = scope2.Resolve<IUnitOfWork>();
    Console.WriteLine($" -> ID instancji wewnątrz scope2: {uowFromScope2.Id}");
}
Console.WriteLine($" -> Czy instancja ze scope1 jest taka sama jak ze scope2? {uowFromScope1.Id == uowFromScope2.Id}");