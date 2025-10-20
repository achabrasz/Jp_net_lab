namespace Lab3;

public class UnitOfWork : IUnitOfWork
{
    public Guid Id { get; }

    public UnitOfWork()
    {
        Id = Guid.NewGuid();
    }
}