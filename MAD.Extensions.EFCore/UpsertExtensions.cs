using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Extensions.EFCore
{
    public static class UpsertExtensions
    {
        public static void Upsert(this DbContext dbContext, object entity, Action<object> transformations = null)
        {
            dbContext.ChangeTracker.TrackGraph(entity, g =>
            {
                // The current entity
                var dependentEntity = g.Entry.Entity;
                var dependentEntityType = g.Entry.OriginalValues.EntityType;

                // The entities parent / principal
                var principalEntity = g.SourceEntry?.Entity;

                // If the dependentEntityType is null, it is not a part of the EFCore DbContext model
                if (dependentEntityType is null)
                    return;

                transformations?.Invoke(dependentEntity);

                if (g.InboundNavigation is null == false && IsCollection(g.InboundNavigation as INavigation))
                {
                    // Ensure the foreign keys are set so the sourceEntryEntity and entryEntity continue to be a part of the same object graph.
                    dbContext.SetForeignKeys(g.InboundNavigation as INavigation, dependentEntity, principalEntity);
                }

                var primaryKey = dependentEntityType.FindPrimaryKey();
                var keys = GetPrimaryKeyValues(primaryKey, g.Entry, dependentEntity);

                if (dbContext.EntityPreviouslyTracked(g.Entry, dependentEntityType, keys))
                    return;

                if (dependentEntityType.IsOwned())
                {
                    var ownership = dependentEntityType.FindOwnership();
                    var navigation = ownership.GetNavigation(false);

                    if (IsCollection(navigation))
                    {
                        dbContext.AddOrUpdateEntity(g.Entry, dependentEntityType, keys);
                    }
                    else
                    {
                        g.Entry.State = g.SourceEntry.State;
                    }
                }
                else
                {
                    dbContext.AddOrUpdateEntity(g.Entry, dependentEntityType, keys);
                }
            });
        }

        private static bool EntityPreviouslyTracked(this DbContext dbContext, EntityEntry entry, IEntityType entityType, IDictionary<string, object> keys)
        {
            if (entry.IsKeySet == false)
                return false;

            var existingEntries = dbContext.ChangeTracker.Entries().Where(y => y.OriginalValues.EntityType == entityType);

            foreach (var existingEntry in existingEntries)
            {
                var primaryKey = entityType.FindPrimaryKey();
                var existingKeys = GetPrimaryKeyValues(primaryKey, existingEntry, existingEntry.Entity);

                foreach (var keyValue in keys)
                {
                    if (existingKeys.TryGetValue(keyValue.Key, out object existingValue) && keyValue.Value.Equals(existingValue))
                        return true;
                }
            }           

            return false;
        }

        private static IDictionary<string, object> GetPrimaryKeyValues(IKey primaryKey, EntityEntry entry, object dependentEntity)
        {
            return primaryKey.Properties.ToDictionary(
                    keySelector: y => y.Name,
                    elementSelector: x =>
                    {
                        object result;

                        if (x.PropertyInfo is null)
                        {
                            result = entry.Property(x.Name).CurrentValue;
                        }
                        else
                        {
                            result = x.PropertyInfo.GetValue(dependentEntity);
                        }

                        if (result is DateTime dte)
                        {
                            // Dates are serialized by default as yyyy-MM-dd HH:mm:ss. Add more precision.
                            result = dte.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }

                        return result;
                    });
        }

        private static void SetForeignKeys(this DbContext dbContext, INavigation navigation, object dependent, object principal)
        {
            var principalKeyProperties = navigation.ForeignKey.PrincipalKey.Properties;
            var principalPkValues = new List<object>();
            var dependantKeyProperties = navigation.ForeignKey.Properties;

            // Get all the principal key values from the principal entity
            foreach (var pk in principalKeyProperties)
            {
                var value = dbContext.Entry(principal).Property(pk.Name).CurrentValue;
                principalPkValues.Add(value);
            }

            // Using the princpal key values, set the dependent's fk values to equal them
            for (int i = 0; i < principalPkValues.Count; i++)
            {
                var dk = dependantKeyProperties[i];
                var pkVal = principalPkValues[i];

                dbContext.Entry(dependent).Property(dk.Name).CurrentValue = pkVal;
            }
        }

        private static void AddOrUpdateEntity(this DbContext dbContext, EntityEntry entry, IEntityType entityType, IDictionary<string, object> keys)
        {
            var db = dbContext.GetQueryFactory();
            var tableName = entityType.GetTableName();

            var query = db
                .Query(tableName)
                .Where(keys);

            var existingEntityCount =
                query
                .Count<int>();

            if (existingEntityCount == 0)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                entry.State = EntityState.Modified;
            }
        }

        private static QueryFactory GetQueryFactory(this DbContext dbContext)
        {
            var compiler = new SqlServerCompiler();
            var db = new QueryFactory(dbContext.Database.GetDbConnection(), compiler);

            return db;
        }

        private static bool IsCollection(INavigation navigation)
        {
#if NET6_0_OR_GREATER
            return navigation.IsCollection;
#else
            return navigation.IsCollection();
#endif
        }

    }
}
