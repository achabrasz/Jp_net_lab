using Autofac;
using Lab3;
using Xunit;

public class Lab3Tests
{
    private IContainer BuildContainer()
    {
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

        return builder.Build();
    }

    [Fact]
    public void Worker_ReturnsConcatenation()
    {
        var container = BuildContainer();
        var worker = container.Resolve<Worker>();
        var result = worker.Work("a", "b");
        Assert.Equal("ab", result);
    }

    [Fact]
    public void Worker2_ReturnsSum()
    {
        var container = BuildContainer();
        var worker2 = container.Resolve<Worker2>();
        var result = worker2.Work("2", "3");
        Assert.Equal("5", result);
    }

    [Fact]
    public void Worker3_ReturnsSum()
    {
        var container = BuildContainer();
        var worker3 = container.Resolve<Worker3>();
        var result = worker3.Work("2", "3");
        Assert.Equal("5", result);
    }

    [Fact]
    public void WorkerState_UsesStateCalc()
    {
        var container = BuildContainer();
        var workerState = container.ResolveNamed<Worker>("state");
        var result = workerState.Work("2", "3");
        Assert.Equal("23", result);
    }

    [Fact]
    public void StateCalc_IsSingleton()
    {
        var container = BuildContainer();
        var state1 = container.Resolve<StateCalc>();
        var state2 = container.Resolve<StateCalc>();
        Assert.Same(state1, state2);
    }

    [Fact]
    public void UnitOfWork_IsScoped()
    {
        var container = BuildContainer();
        IUnitOfWork uow1, uow1b, uow2;
        using (var scope1 = container.BeginLifetimeScope())
        {
            uow1 = scope1.Resolve<IUnitOfWork>();
            uow1b = scope1.Resolve<IUnitOfWork>();
            Assert.Same(uow1, uow1b);
        }
        using (var scope2 = container.BeginLifetimeScope())
        {
            uow2 = scope2.Resolve<IUnitOfWork>();
        }
        Assert.NotEqual(uow1.Id, uow2.Id);
    }

    [Fact]
    public void TransactionContext_IsPerMatchingScope()
    {
        var container = BuildContainer();
        using (var scope = container.BeginLifetimeScope("transaction"))
        {
            var stepOne = scope.Resolve<StepOneService>();
            var stepTwo = scope.Resolve<StepTwoService>();

            var context1 = GetPrivateField<ITransactionContext>(stepOne, "_context");
            var context2 = GetPrivateField<ITransactionContext>(stepTwo, "_context");

            Assert.Same(context1, context2);
        }
    }

    private static T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)field.GetValue(obj);
    }
}