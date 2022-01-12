using MAD.Extensions.EFCore.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MAD.Extensions.EFCore.Tests
{
    [TestClass()]
    public class UpsertExtensionsTests
    {
        [TestMethod()]
        public void Upsert_Update_OwnedTypePropertiesAreSet()
        {
            var project = new Project
            {
                Id = 1337,
                Name = "Cool project",
                Office = new ProjectOffice
                {
                    Id = 1899,
                    Name = "Main office",
                    OfficeAddress = new OfficeAddress
                    {
                        Address = "123 Fake Street",
                        City = "Fake City"
                    }
                },
                Region = new ProjectRegion
                {
                    Name = "Australia",
                    Id = 998
                },
                Departments = new List<ProjectDepartment>
                {
                   new ProjectDepartment
                   {
                       Id = 1,
                       Name = "Mech",
                       Employees = new List<DepartmentEmployee>
                       {
                           new DepartmentEmployee
                           {
                               Id = 1,
                               Name = "Kramer"
                           },
                           new DepartmentEmployee
                           {
                               Id = 2,
                               Name = "Slaymer"
                           }
                       }
                   },
                   new ProjectDepartment
                   {
                       Id = 2,
                       Name = "Dech",
                       Employees = new List<DepartmentEmployee>
                       {
                           new DepartmentEmployee
                           {
                               Id = 3,
                               Name = "Vramer"
                           },
                           new DepartmentEmployee
                           {
                               Id = 4,
                               Name = "Claymer"
                           }
                       }
                   }
                }
            };

            using (var db = TestDbContextFactory.Create())
            {
                db.Add(new Project
                {
                    Id = 1337,
                    Name = "Cool project"
                });

                db.SaveChanges();
            }

            using (var db = TestDbContextFactory.Create())
            {
                db.Upsert(project);
                //db.Upsert(project, entity =>
                //{
                //    switch (entity)
                //    {
                //        case ProjectDepartment pd:
                //            db.Entry(entity).Property("ProjectId").CurrentValue = project.Id;
                //            break;
                //    }
                //});

                db.SaveChanges();
            }
        }

        [TestMethod()]
        public void Upsert_Update_WithSingleTypeThatMultipleOwnersPropertiesAreSet()
        {
            var project = new Project
            {
                Id = 1337,
                Name = "Cool project",
                Office = new ProjectOffice
                {
                    Id = 1899,
                    Name = "Main office",
                    Region = new ProjectRegion
                    {
                        Name = "Australia",
                        Id = 998
                    },
                    OfficeAddress = new OfficeAddress
                    {
                        Address = "123 Fake Street",
                        City = "Fake City"
                    }
                },
                Region = new ProjectRegion
                {
                    Name = "Australia",
                    Id = 998
                },
                Departments = new List<ProjectDepartment>
                {
                   new ProjectDepartment
                   {
                       Id = 1,
                       Name = "Mech"
                   },
                   new ProjectDepartment
                   {
                       Id = 2,
                       Name = "Dech"
                   }
                }
            };

            using (var db = TestDbContextFactory.Create())
            {
                db.Add(new Project
                {
                    Id = 1337,
                    Name = "Cool project"
                });

                db.SaveChanges();
            }

            using (var db = TestDbContextFactory.Create())
            {
                db.Upsert(project);
                //db.Upsert(project, entity =>
                //{
                //    switch (entity)
                //    {
                //        case ProjectDepartment pd:
                //            db.Entry(entity).Property("ProjectId").CurrentValue = project.Id;
                //            break;
                //        case ProjectRegion region:

                //            break;
                //    }
                //});

                db.SaveChanges();
            }
        }

        [TestMethod()]
        public void Upsert_Update_NestedOwnedTypePropertiesAreSet()
        {
            var project = new Project
            {
                Id = 1337,
                Name = "Cool project",
                Departments = new List<ProjectDepartment>
                {
                    new ProjectDepartment
                    {
                        Id = 1,
                        Name = "Mech",
                        Employees = new List<DepartmentEmployee>
                        {
                            new DepartmentEmployee
                            {
                                Id = 1,
                                Name = "Kramer"
                            },
                            new DepartmentEmployee
                            {
                                Id = 2,
                                Name = "Slaymer"
                            }
                        }
                    },
                    new ProjectDepartment
                    {
                        Id = 2,
                        Name = "Dech",
                        Employees = new List<DepartmentEmployee>
                        {
                            new DepartmentEmployee
                            {
                                Id = 3,
                                Name = "Vramer"
                            },
                            new DepartmentEmployee
                            {
                                Id = 4,
                                Name = "Claymer"
                            }
                        }
                    }
                }
            };

            using (var db = TestDbContextFactory.Create())
            {
                db.Add(new Project
                {
                    Id = 1337,
                    Name = "Cool project"
                });

                db.SaveChanges();
            }

            using (var db = TestDbContextFactory.Create())
            {
                db.Upsert(project);                
                db.SaveChanges();
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            using var db = TestDbContextFactory.Create();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}