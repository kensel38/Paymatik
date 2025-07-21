using EL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class UnitOfWork : IDisposable
    {
        private PaymatikDBEntities _db = null;

        public UnitOfWork()
        {
            _db = new PaymatikDBEntities();
        }

        private Dictionary<Type, object> _dict = new Dictionary<Type, object>();

        public IRepository<T> GetRepo<T>() where T : class
        {
            if (_dict.Keys.Contains(typeof(T)))  // eğer daha önce oluşturuldu ise (dizide var ise) onu dönder yoksa yeniden oluştur....
            {
                return _dict[typeof(T)] as IRepository<T>;
            }

            IRepository<T> _repo = new Repository<T>(_db);
            _dict.Add(typeof(T), _repo);
            return _repo;
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
