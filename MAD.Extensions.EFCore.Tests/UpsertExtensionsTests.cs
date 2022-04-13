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

        [TestMethod()]
        public void Upsert_Update_EntityPreviouslyTracked()
        {
            var project1 = new Project
            {
                Id = 1337,
                Name = "Cool Project 1",
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
                }
            };

            var project2 = new Project
            {
                Id = 1337,
                Name = "Cool Project 2",
                Office = new ProjectOffice
                {
                    Id = 1900,
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
                    Id = 999
                }
            };

            using (var db = TestDbContextFactory.Create())
            {
                db.AddRange(new List<Project>()
                {
                    new Project
                    {
                        Id = 1337,
                        Name = "Cool Project 1"
                    }
                });

                db.SaveChanges();
            }

            using (var db = TestDbContextFactory.Create())
            {
                var lstProjects = new List<Project>()
                {
                    project1,
                    project2
                };

                db.Upsert(lstProjects);

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