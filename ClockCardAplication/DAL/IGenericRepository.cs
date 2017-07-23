using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using ClockCardAplication.Models;


namespace ClockCardAplication.DAL
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null);
        TEntity GetByID(object id);
        void Insert(TEntity entity);
        void Delete(object id);
        void Update(TEntity entityToUpdate);
    }
}

