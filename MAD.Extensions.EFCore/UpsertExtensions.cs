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
                var entryEntity = g.Entry.Entity;
                var entityType = g.Entry.OriginalValues.EntityType;

                if (entityType is null)
                    return;

                transformations?.Invoke(entryEntity);

                if (g.InboundNavigation is null == false && g.InboundNavigation.IsCollection())
                {
                    var principalKeyProperties = g.InboundNavigation.ForeignKey.PrincipalKey.Properties;
                    var dependantKeyProperties = g.InboundNavigation.ForeignKey.Properties;

                    var pkValues = new List<object>();

                    foreach (var pk in principalKeyProperties)
                    {
                        var value = dbContext.Entry(entryEntity).Property(pk.Name).CurrentValue;
                        pkValues.Add(value);
                    }
                    
                    for (int i = 0; i < pkValues.Count; i++)
                    {
                        var dk = dependantKeyProperties[i];
                        var pkVal = pkValues[i];

                        dbContext.Entry(entryEntity).Property(dk.Name).CurrentValue = pkVal;
                    }
                }               

                var primaryKey = entityType.FindPrimaryKey();
                var keys = primaryKey.Properties.ToDictionary(
                    keySelector: y => y.Name,
                    elementSelector: x =>
                    {
                        object result;

                        if (x.PropertyInfo is null)
                        {
                            result = g.Entry.Property(x.Name).CurrentValue;
                        }
                        else
                        {
                            result = x.PropertyInfo.GetValue(entryEntity);
                        }

                        if (result is DateTime dte)
                        {
                            // Dates are serialized by default as yyyy-MM-dd HH:mm:ss. Add more precision.
                            result = dte.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }

                        return result;
                    });

                if (entityType.IsOwned())
                {
                    var ownership = entityType.FindOwnership();
                    var navigation = ownership.GetNavigation(false);

                    if (navigation.IsCollection())
                    {
                        dbContext.AddOrUpdateEntity(g.Entry, entityType, keys);
                    }
                    else
                    {
                        g.Entry.State = g.SourceEntry.State;
                    }
                }
                else
                {
                    dbContext.AddOrUpdateEntity(g.Entry, entityType, keys);
                }
            });
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
    }
}
