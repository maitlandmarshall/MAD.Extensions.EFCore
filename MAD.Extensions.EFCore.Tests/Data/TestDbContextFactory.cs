using Microsoft.EntityFrameworkCore;

namespace MAD.Extensions.EFCore.Tests.Data
{
    internal class TestDbContextFactory
    {
        public static TestDbContext Create()
        {
            var connectionString = TestConfigFactory.Create().ConnectionString;
            return new TestDbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options);
        }
    }
}
