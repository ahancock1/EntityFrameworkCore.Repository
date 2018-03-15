﻿// -----------------------------------------------------------------------
//  <copyright file="Repository.cs" company="Solentim">
//      Copyright (c) Solentim 2018. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Han.EntityFrameworkCore.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    ///     Creates an instance of a generic repository for a <see cref="DbContext" /> and
    ///     exposes basic CRUD functionality
    /// </summary>
    /// <typeparam name="TContext">The data context used for this repository</typeparam>
    /// <typeparam name="TEntity">The entity type used for this repository</typeparam>
    public abstract class Repository<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        /// <summary>
        ///     Retrieves entities from the <see cref="DbSet{TEntity}" /> and optionally performs a filter, order by,
        ///     number of entities to skip and take. Allows include to be performed on the <see cref="DbSet{TEntity}" />
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="orderby">The ascending order to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="skip">The number of entites to skip. </param>
        /// <param name="take">The number of entities to take. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>The queried entities</returns>
        public virtual IEnumerable<TEntity> All(
            Func<TEntity, bool> predicate = null,
            Func<TEntity, object> orderby = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            using (var context = GetDataContext())
            {
                var items = includes.Aggregate((IQueryable<TEntity>)context.Set<TEntity>(),
                    (current, item) => current.Include(item)).AsEnumerable();

                if (predicate != null)
                {
                    items = items.Where(predicate);
                }

                if (orderby != null)
                {
                    items = items.OrderBy(orderby);
                }

                if (skip.HasValue)
                {
                    items = items.Skip(skip.Value);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                return items.ToList();
            }
        }

        /// <summary>
        ///     Retrieves entities from the <see cref="DbSet{TEntity}" /> and optionally performs a filter, order by,
        ///     number of entities to skip and take. Allows include to be performed on the <see cref="DbSet{TEntity}" />
        ///     async.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="orderby">The ascending order to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="skip">The number of entites to skip. </param>
        /// <param name="take">The number of entities to take. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>The queried entities</returns>
        public Task<IEnumerable<TEntity>> AllAsync(
            Func<TEntity, bool> predicate = null,
            Func<TEntity, object> orderby = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return Task.Run(() => All(predicate, orderby, skip, take, includes));
        }

        /// <summary>
        ///     Determines whether any entities in the <see cref="DbSet{TEntity}"/> satisfies the
        /// filter.
        /// </summary>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>True if any entities satisfy the filter. </returns>
        public bool Any(
            Func<TEntity, bool> predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            using (var context = GetDataContext())
            {
                var entities = includes.Aggregate((IQueryable<TEntity>)context.Set<TEntity>(),
                    (current, item) => current.Include(item));

                return predicate != null ? entities.Any(predicate) : entities.Any();
            }
        }

        /// <summary>
        ///     Determines whether any entities in the <see cref="DbSet{TEntity}"/> satisfies the
        /// filter async.
        /// </summary>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>True if any entities satisfy the filter. </returns>
        public Task<bool> AnyAsync(
            Func<TEntity, bool> predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return Task.Run(() => Any(predicate, includes));
        }

        /// <summary>
        ///     Creates the entities in their <see cref="DbSet{TEntity}" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to create. </param>
        /// <returns>True if all entities have been created. </returns>
        public virtual bool Create(params TEntity[] entities)
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

        /// <summary>
        ///     Creates the entities in their <see cref="DbSet{TEntity}" /> async.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to create. </param>
        /// <returns>True if all entities have been created. </returns>
        public Task<bool> CreateAsync(params TEntity[] entities)
        {
            return Task.Run(() => Create(entities));
        }

        /// <summary>
        ///     Deletes the given entities in their <see cref="DbSet{TEntity}" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to delete. </param>
        /// <returns>True if all the entites have been deleted. </returns>
        public virtual bool Delete(params TEntity[] entities)
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

        /// <summary>
        ///     Deletes the given entities in their <see cref="DbSet{TEntity}" /> async.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to delete. </param>
        /// <returns>True if all the entites have been deleted. </returns>
        public Task<bool> DeleteAsync(params TEntity[] entities)
        {
            return Task.Run(() => Delete(entities));
        }

        /// <summary>
        ///     Retrieves the first entity from their <see cref="DbSet{TEntity}"/> otherwise returns
        /// null.
        /// </summary>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>The first entity matching the filter</returns>
        public TEntity Get(
            Func<TEntity, bool> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            using (var context = GetDataContext())
            {
                return includes.Aggregate((IQueryable<TEntity>)context.Set<TEntity>(),
                    (current, item) => current.Include(item)).FirstOrDefault(predicate);
            }
        }

        /// <summary>
        ///     Retrieves the first entity from their <see cref="DbSet{TEntity}"/> otherwise returns
        /// null async.
        /// </summary>
        /// <param name="predicate">The filter to apply to the <see cref="DbSet{TEntity}" />. </param>
        /// <param name="includes">The related entities to include. </param>
        /// <returns>The first entity matching the filter</returns>
        public Task<TEntity> GetAsync(
            Func<TEntity, bool> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            return Task.Run(() => Get(predicate, includes));
        }

        /// <summary>
        ///     Updates the given entities in their <see cref="DbSet{TEntity}" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to update. </param>
        /// <returns>True if all the entites have been updated. </returns>
        public virtual bool Update(params TEntity[] entities)
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

        /// <summary>
        ///     Updates the given entities in their <see cref="DbSet{TEntity}" /> async.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity used in <see cref="DbSet{TEntity}" />. </typeparam>
        /// <param name="entities">The entities to update. </param>
        /// <returns>True if all the entites have been updated. </returns>
        public Task<bool> UpdateAsync(params TEntity[] entities)
        {
            return Task.Run(() => Update(entities));
        }

        /// <summary>
        ///     Creates an instance of the <see cref="DbContext" />
        /// </summary>
        /// <returns>An instance of the <see cref="DbContext" /></returns>
        /// <remarks>This should always be disposed of afterwards</remarks>
        protected virtual TContext GetDataContext()
        {
            return Activator.CreateInstance<TContext>();
        }
    }
}