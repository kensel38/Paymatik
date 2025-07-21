using EL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace DAL
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private PaymatikDBEntities _db = null;

        public Repository(PaymatikDBEntities _context)
        {
            _db = _context;
        }

        public virtual T Add(T entity)
        {
            _db.Set<T>().Add(entity);
            _db.SaveChanges();
            return entity;
        }

        public int AddRange(List<T> entities)
        {
            _db.Set<T>().AddRange(entities);
            int addCount = _db.SaveChanges();
            return addCount;
        }

        public virtual int Delete(int id)
        {
            _db.Set<T>().Remove(GetByID(id));
            return _db.SaveChanges();
        }

        public int DeleteByParam(Expression<Func<T, bool>> _where)
        {
            _db.Set<T>().RemoveRange(GetAll_ByParam(_where));
            return _db.SaveChanges();
        }

        public virtual List<T> GetAll()
        {
            return _db.Set<T>().ToList();
        }

        public List<TType> GetAll_Where_Select<TType>(Expression<Func<T, bool>> _where, Expression<Func<T, TType>> _select) where TType : class
        {
            if (_where == null)
            {
                return _db.Set<T>().Select(_select).ToList();
            }
            else
            {
                return _db.Set<T>().Where(_where).Select(_select).ToList();
            }

        }

        public virtual T GetByID(int id)
        {
            // var aaa = _db.Set<T>().Find(id);
            return _db.Set<T>().Find(id);
        }

        public virtual List<T> GetAll_ByParam(Expression<Func<T, bool>> _where)
        {
            return _db.Set<T>().Where(_where).ToList();
        }

        public T Get_ByParam(Expression<Func<T, bool>> _where)
        {
            return _db.Set<T>().FirstOrDefault(_where);
        }

        public virtual T Update(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
            _db.SaveChanges();
            return entity;
        }


    }
}
