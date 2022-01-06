using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
                var entity = g.Entry.Entity;
                var entityType = g.Entry.OriginalValues.EntityType;

                if (entityType is null)
                    return;

                transformations?.Invoke(entity);

                var primaryKey = entityType.FindPrimaryKey();

                var keys = new Dictionary<string, object>();
                foreach (var prop in primaryKey.Properties)
                {
                    object result;

                    var primaryKeyPropName = prop.GetType().GetProperty("Name").GetValue(prop).ToString();
                    var propInfo = entity.GetType().GetProperty(primaryKeyPropName);

                    if (propInfo is null)
                    {
                        result = g.Entry.Property(primaryKeyPropName).CurrentValue;
                    }
                    else
                    {
                        result = propInfo.GetValue(entity);
                    }

                    if (result is DateTime dte)
                    {
                        // Dates are serialized by default as yyyy-MM-dd HH:mm:ss. Add more precision.
                        result = dte.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }

                    keys.Add(propInfo?.Name ?? primaryKeyPropName, result);
                }

                if (IsOwned(entityType))
                {
                    var ownership = FindOwnership(entityType);
                    var navigation = GetNavigation(ownership, false);

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

        private static INavigation GetNavigation(IForeignKey foreignKey, bool pointsToPrincipal)
        {
            if (!pointsToPrincipal)
            {
                return foreignKey.PrincipalToDependent;
            }

            return foreignKey.DependentToPrincipal;
        }

        private static void AddOrUpdateEntity(this DbContext dbContext, EntityEntry entry, IEntityType entityType, IDictionary<string, object> keys)
        {
            var db = dbContext.GetQueryFactory();
            var tableName = GetTableName(entityType);

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

        public static string GetTableName(IEntityType entityType)
        {
            string tableName = null;

            if (entityType.BaseType == null)
            {
                var result = ExecuteMethod<Annotation>(entityType, entityType.GetType(), "FindAnnotation", new[] { "Relational:TableName" });

                if (result != null)                
                    tableName = result.GetType().GetProperty("Value").GetValue(result).ToString();                

                if (tableName == null)                
                    return GetDefaultTableName(entityType);                
            }
            else
            {
                tableName = GetTableName(GetRootType(entityType));
            }

            return tableName;
        }

        public static string GetDefaultTableName(IEntityType entityType)
        {
            var p2dName = string.Empty;
            var foreignKey = FindOwnership(entityType);

            if (foreignKey != null)
            {
                var fkType = foreignKey.GetType();
                var derivedFkTypes = fkType.Assembly.GetTypes().Where(x => x.IsAssignableFrom(fkType));
                var isUniquePropType = derivedFkTypes.FirstOrDefault(x => x.GetProperties().Any(x => x.Name == "IsUnique"));
                var isUnique = (bool)isUniquePropType.GetProperty("IsUnique").GetValue(foreignKey);

                var p2dPropType = derivedFkTypes.FirstOrDefault(x => x.GetProperties().Any(x => x.Name == "PrincipalToDependent"));
                var p2d = p2dPropType.GetProperty("PrincipalToDependent").GetValue(foreignKey);

                if (p2d != null)
                    p2dName = p2d.GetType().GetProperty("Name")?.GetValue(p2d).ToString();

                if (isUnique)
                    return GetTableName(foreignKey.PrincipalEntityType);
            }

            var maxIdentifierLength = ExecuteMethod<int?>(entityType, entityType.Model.GetType(), "FindAnnotation", new[] { "Relational:MaxIdentifierLength" }) ?? 32767;

            var type = GetRequiredEntityTypeForMethod(entityType, "ShortName");

            var shortName = string.Empty;

            if (type != null)
                shortName = ExecuteMethod<string>(entityType, type, "ShortName", null);

            return Uniquifier.Truncate(foreignKey.DeclaringEntityType != null ? (GetTableName(foreignKey.DeclaringEntityType) + "_" + p2dName) : shortName, maxIdentifierLength);
        }

        private static IForeignKey FindOwnership(IEntityType entityType)
        {
            var type = GetRequiredEntityTypeForMethod(entityType, "FindOwnership");

            if (type == null)
                return null;

            return ExecuteMethod<IForeignKey>(entityType, type, "FindOwnership", null);
        }

        private static IEntityType GetRootType(IEntityType entityType)
        {
            if (entityType == null)
                return null;

            return GetRootType(entityType.BaseType) ?? entityType;
        }

        private static bool IsOwned(IEntityType entityType)
        {
            var type = GetRequiredEntityTypeForMethod(entityType, "IsOwned");

            if (type == null)
                return false;

            return ExecuteMethod<bool>(entityType, type, "IsOwned", null);
        }

        private static T ExecuteMethod<T>(object obj, Type type, string methodName, object[] parameters)
        {
            var methodInfo = type.GetMethod(methodName);

            if (methodInfo is null)
                return default;

            return (T)methodInfo.Invoke(obj, parameters);
        }

        private static Type GetRequiredEntityTypeForMethod(object obj, string methodName)
        {
            var type = obj.GetType();
            var derivedTypes = type.Assembly.GetTypes().Where(x => x.IsAssignableFrom(type));
            return derivedTypes.FirstOrDefault(x => x.GetMethods().Any(x => x.Name == methodName));
        }
    }
}
