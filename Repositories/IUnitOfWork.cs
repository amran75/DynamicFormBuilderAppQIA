namespace DynamicFormBuilderAppQIA.Repositories
{
    public interface IUnitOfWork
    {
        public interface IUnitOfWork : IDisposable
        {
            IFormRepository Forms { get; }
            Task<bool> SaveChangesAsync();
            Task BeginTransactionAsync();
            Task CommitTransactionAsync();
            Task RollbackTransactionAsync();
        }
    }
}
