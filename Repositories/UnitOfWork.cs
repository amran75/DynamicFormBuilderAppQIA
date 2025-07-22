// Repositories/UnitOfWork.cs
using DynamicFormBuilderAppQIA.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DynamicFormBuilderAppQIA.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private bool _disposed = false;
        private SqlTransaction _transaction;

        public IFormRepository Forms { get; }

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            Forms = new FormRepository(dbContext);
        }

        public async Task BeginTransactionAsync()
        {
            if (_dbContext.Connection.State != System.Data.ConnectionState.Open)
            {
                await _dbContext.Connection.OpenAsync();
            }
            _transaction = _dbContext.Connection.BeginTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await Task.Run(() => _transaction.Commit());
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await Task.Run(() => _transaction.Rollback());
                _transaction = null;
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            // For ADO.NET, we don't have a change tracker
            // This method exists for interface consistency
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _dbContext.Connection?.Close();
                    _dbContext.Connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}