﻿// -----------------------------------------------------------------------
//   Copyright (C) 2017 Adam Hancock
//    
//   Repository.cs can not be copied and/or distributed without the express
//   permission of Adam Hancock
// -----------------------------------------------------------------------

namespace EntityFrameworkCore.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using Microsoft.EntityFrameworkCore;

    public class Repository<TContext> : IRepository<TContext>
        where TContext : DbContext
    {
        public IEnumerable<TEntity> All<TEntity>(
            Func<TEntity, bool> predicate = null,
            Func<TEntity, object> orderBy = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] include)
            where TEntity : class
        {
            using (var context = GetDataContext())
            {
                return Query<TEntity>(context, predicate, orderBy, skip, take, include).ToList();
            }
        }

        public bool Create<TEntity>(params TEntity[] entities)
            where TEntity : class
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();
                foreach (var entity in entities)
                {
                    set.Add(entity);
                }

                return context.SaveChanges() >= entities.Length;
            }
        }

        public bool Delete<TEntity>(params TEntity[] entities)
            where TEntity : class
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();

                foreach (var entity in entities)
                {
                    set.Remove(entity);
                }

                return context.SaveChanges() >= entities.Length;
            }
        }

        public bool Update<TEntity>(params TEntity[] entities)
            where TEntity : class
        {
            using (var context = GetDataContext())
            {
                var set = context.Set<TEntity>();
                foreach (var entity in entities)
                {
                    set.Update(entity);
                }

                return context.SaveChanges() >= entities.Length;
            }
        }

        protected TContext GetDataContext()
        {
            var context = Activator.CreateInstance<TContext>();

            context?.Database.Migrate();

            return context;
        }

        protected IEnumerable<TEntity> Query<TEntity>(
            DbContext context,
            Func<TEntity, bool> predicate = null,
            Func<TEntity, object> orderBy = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] includes)
            where TEntity : class
        {
            var items = includes.Aggregate((IQueryable<TEntity>)context.Set<TEntity>(),
                (current, item) => current.Include(item)).AsEnumerable();

            if (predicate != null)
            {
                items = items.Where(predicate);
            }

            if (orderBy != null)
            {
                items = items.OrderBy(orderBy);
            }

            if (skip.HasValue)
            {
                items = items.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                items = items.Take(take.Value);
            }

            return items;
        }
    }
}
