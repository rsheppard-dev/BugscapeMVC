using BugscapeMVC.Models;
using BugscapeMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BugscapeMVC.Data
{
    public static class DataUtility
    {
        //Company Ids
        private static int company1Id;
        private static int company2Id;

        public static string? GetConnectionString(IConfiguration configuration)
        {
            //The default connection string will come from appSettings like usual
            var connectionString = configuration.GetSection("DBSettings")["ConnectionString"];
            //It will be automatically overwritten if we are running on Heroku
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            using var svcScope = host.Services.CreateScope();
            var svcProvider = svcScope.ServiceProvider;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            //Service: An instance of RoleManager
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            //Service: An instance of RoleManager
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //Service: An instance of the UserManager
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<AppUser>>();
            //Migration: This is the programmatic equivalent to Update-Database
            await dbContextSvc.Database.MigrateAsync();

            
            //Custom  Bug Tracker Seed Methods
            await SeedRolesAsync(roleManagerSvc);
            await SeedDefaultCompaniesAsync(dbContextSvc);
            await SeedDefaultUsersAsync(userManagerSvc);
            await SeedDemoUsersAsync(userManagerSvc);
            await SeedDefaultTicketTypeAsync(dbContextSvc);
            await SeedDefaultTicketStatusAsync(dbContextSvc);
            await SeedDefaultTicketPriorityAsync(dbContextSvc);
            await SeedDefaultProjectPriorityAsync(dbContextSvc);
            await SeedDefaultProjectsAsync(dbContextSvc);
            await SeedDefautTicketsAsync(dbContextSvc);
        }


        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Project_Manager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Developer.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Submitter.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Demo_User.ToString()));
        }

        public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Company> defaultcompanies = new List<Company>() {
                    new Company() { Name = "Company1", Description="This is default Company 1" },
                    new Company() { Name = "Company2", Description="This is default Company 2" },
                    new Company() { Name = "Company3", Description="This is default Company 3" },
                    new Company() { Name = "Company4", Description="This is default Company 4" },
                    new Company() { Name = "Company5", Description="This is default Company 5" }
                };

                var dbCompanies = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultcompanies.Where(c => !dbCompanies.Contains(c.Name)));
                await context.SaveChangesAsync();

                //Get company Ids
                company1Id = context.Companies.FirstOrDefault(p => p.Name == "Company1")?.Id ?? throw new Exception("Failed to get companyId");
                company2Id = context.Companies.FirstOrDefault(p => p.Name == "Company2")?.Id ?? throw new Exception("Failed to get companyId");
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Companies.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultProjectPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Models.ProjectPriority> projectPriorities = new List<ProjectPriority>() {
                    new ProjectPriority() { Name = Priorities.Low.ToString() },
                    new ProjectPriority() { Name = Priorities.Medium.ToString() },
                    new ProjectPriority() { Name = Priorities.High.ToString() },
                    new ProjectPriority() { Name = Priorities.Urgent.ToString() },
                };

                var dbProjectPriorities = context.ProjectPriorities.Select(c => c.Name).ToList();
                await context.ProjectPriorities.AddRangeAsync(projectPriorities.Where(c => !dbProjectPriorities.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Project Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultProjectsAsync(ApplicationDbContext context)
        {
            try
            {
                //Get project priority Ids
                int priorityLow = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Low.ToString())?.Id ?? throw new Exception("Failed to get projectPriortyId");
                int priorityMedium = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Medium.ToString())?.Id ?? throw new Exception("Failed to get projectPriortyId");;
                int priorityHigh = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.High.ToString())?.Id ?? throw new Exception("Failed to get projectPriortyId");;
                int priorityUrgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Urgent.ToString())?.Id ?? throw new Exception("Failed to get projectPriortyId");;
           
                IList<Project> projects = new List<Project>() {
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a Personal Porfolio",
                         Description="Single page html, css & javascript page.  Serves as a landing page for candidates and contains a bio and links to all applications and challenges." ,
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(1),
                         ProjectPriorityId = priorityLow
                     },
                     new Project()
                     {
                         CompanyId = company2Id,
                         Name = "Build a supplemental Blog Web Application",
                         Description="Candidate's custom built web application using .Net Core with MVC, a postgres database and hosted in a heroku container.  The app is designed for the candidate to create, update and maintain a live blog site.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(4),
                         ProjectPriorityId = priorityMedium
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build an Issue Tracking Web Application",
                         Description="A custom designed .Net Core application with postgres database.  The application is a multi tennent application designed to track issue tickets' progress.  Implemented with identity and user roles, Tickets are maintained in projects which are maintained by users in the role of Project_Manager.  Each project has a team and team members.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(6),
                         ProjectPriorityId = priorityHigh
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a Movie Information Web Application",
                         Description="An internet movie database using web api.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(6),
                         ProjectPriorityId = priorityHigh
                     },
                };

                var dbProjects = context.Projects.Select(c => c.Name).ToList();
                await context.Projects.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Projects.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultUsersAsync(UserManager<AppUser> userManager)
        {
            //Seed Default Admin User
            var defaultUser = new AppUser
            {
                UserName = "btadmin1@bugtracker.com",
                Email = "btadmin1@bugtracker.com",
                FirstName = "Bill",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Admin User
            defaultUser = new AppUser
            {
                UserName = "btadmin2@bugtracker.com",
                Email = "btadmin2@bugtracker.com",
                FirstName = "Steve",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default ProjectManager1 User
            defaultUser = new AppUser
            {
                UserName = "ProjectManager1@bugtracker.com",
                Email = "ProjectManager1@bugtracker.com",
                FirstName = "John",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Project_Manager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Project_Manager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Project_Manager2 User
            defaultUser = new AppUser
            {
                UserName = "ProjectManager2@bugtracker.com",
                Email = "ProjectManager2@bugtracker.com",
                FirstName = "Jane",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Project_Manager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Project_Manager2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer1 User
            defaultUser = new AppUser
            {
                UserName = "Developer1@bugtracker.com",
                Email = "Developer1@bugtracker.com",
                FirstName = "Elon",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer2 User
            defaultUser = new AppUser
            {
                UserName = "Developer2@bugtracker.com",
                Email = "Developer2@bugtracker.com",
                FirstName = "James",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer3 User
            defaultUser = new AppUser
            {
                UserName = "Developer3@bugtracker.com",
                Email = "Developer3@bugtracker.com",
                FirstName = "Natasha",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer3 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer4 User
            defaultUser = new AppUser
            {
                UserName = "Developer4@bugtracker.com",
                Email = "Developer4@bugtracker.com",
                FirstName = "Carol",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer4 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer5 User
            defaultUser = new AppUser
            {
                UserName = "Developer5@bugtracker.com",
                Email = "Developer5@bugtracker.com",
                FirstName = "Tony",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer5 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Developer6 User
            defaultUser = new AppUser
            {
                UserName = "Developer6@bugtracker.com",
                Email = "Developer6@bugtracker.com",
                FirstName = "Bruce",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer5 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Submitter1 User
            defaultUser = new AppUser
            {
                UserName = "Submitter1@bugtracker.com",
                Email = "Submitter1@bugtracker.com",
                FirstName = "Scott",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Submitter1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Submitter2 User
            defaultUser = new AppUser
            {
                UserName = "Submitter2@bugtracker.com",
                Email = "Submitter2@bugtracker.com",
                FirstName = "Sue",
                LastName = "Appuser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Submitter2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

        }

        public static async Task SeedDemoUsersAsync(UserManager<AppUser> userManager)
        {
            // seed demo admin user
            var defaultUser = new AppUser
            {
                UserName = "demoadmin@bugscape.com",
                Email = "demoadmin@bugscape.com",
                FirstName = "Demo",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Demo_User.ToString());

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            // seed demo project manager user
            defaultUser = new AppUser
            {
                UserName = "demopm@bugscape.com",
                Email = "demopm@bugscape.com",
                FirstName = "Demo",
                LastName = "Project-Manager",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Project_Manager.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Demo_User.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Project_Manager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            // seed demo developer user
            defaultUser = new AppUser
            {
                UserName = "demodev@bugscape.com",
                Email = "demodev@bugscape.com",
                FirstName = "Demo",
                LastName = "Developer",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Demo_User.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            // seed demo submitter user
            defaultUser = new AppUser
            {
                UserName = "demosub@bugscape.com",
                Email = "demosub@bugscape.com",
                FirstName = "Demo",
                LastName = "Submitter",
                EmailConfirmed = true,
                CompanyId = company1Id
            };

            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Demo_User.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Submitter User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            // seed demo new user
            defaultUser = new AppUser
            {
                UserName = "demonew@bugscape.com",
                Email = "demonew@bugscape.com",
                FirstName = "Demo",
                LastName = "NewUser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Roles.Demo_User.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo New User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>() {
                     new TicketType() { Name = TicketTypes.New_Development.ToString() },      // Ticket involves development of a new, uncoded solution 
                     new TicketType() { Name = TicketTypes.Work_Task.ToString() },            // Ticket involves development of the specific ticket description 
                     new TicketType() { Name = TicketTypes.Defect.ToString()},               // Ticket involves unexpected development/maintenance on a previously designed feature/functionality
                     new TicketType() { Name = TicketTypes.Change_Request.ToString() },       // Ticket involves modification development of a previously designed feature/functionality
                     new TicketType() { Name = TicketTypes.Enhancement.ToString() },         // Ticket involves additional development on a previously designed feature or new functionality
                     new TicketType() { Name = TicketTypes.General_Task.ToString() }          // Ticket involves no software development but may involve tasks such as configuations, or hardware setup
                };

                var dbTicketTypes = context.TicketTypes.Select(c => c.Name).ToList();
                await context.TicketTypes.AddRangeAsync(ticketTypes.Where(c => !dbTicketTypes.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Types.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketStatusAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketStatus> ticketStatuses = new List<TicketStatus>() {
                    new TicketStatus() { Name = TicketStatuses.New.ToString() },                 // Newly Created ticket having never been assigned
                    new TicketStatus() { Name = TicketStatuses.Development.ToString() },         // Ticket is assigned and currently being worked 
                    new TicketStatus() { Name = TicketStatuses.Testing.ToString()  },            // Ticket is assigned and is currently being tested
                    new TicketStatus() { Name = TicketStatuses.Resolved.ToString()  },           // Ticket remains assigned to the developer but work in now complete
                };

                var dbTicketStatuses = context.TicketStatuses.Select(c => c.Name).ToList();
                await context.TicketStatuses.AddRangeAsync(ticketStatuses.Where(c => !dbTicketStatuses.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Statuses.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketPriority> ticketPriorities = new List<TicketPriority>() {
                                                    new TicketPriority() { Name = Priorities.Low.ToString()  },
                                                    new TicketPriority() { Name = Priorities.Medium.ToString() },
                                                    new TicketPriority() { Name = Priorities.High.ToString()},
                                                    new TicketPriority() { Name = Priorities.Urgent.ToString()},
                };

                var dbTicketPriorities = context.TicketPriorities.Select(c => c.Name).ToList();
                await context.TicketPriorities.AddRangeAsync(ticketPriorities.Where(c => !dbTicketPriorities.Contains(c.Name)));
                context.SaveChanges();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefautTicketsAsync(ApplicationDbContext context)
        {
            //Get project Ids
            int portfolioId = (context.Projects.FirstOrDefault(p => p.Name == "Build a Personal Porfolio")?.Id) ?? throw new Exception("Failed to get projectId.");
            int blogId = context.Projects.FirstOrDefault(p => p.Name == "Build a supplemental Blog Web Application")?.Id ?? throw new Exception("Failed to get projectId.");
            int bugtrackerId = context.Projects.FirstOrDefault(p => p.Name == "Build an Issue Tracking Web Application")?.Id ?? throw new Exception("Failed to get projectId.");
            int movieId = context.Projects.FirstOrDefault(p => p.Name == "Build a Movie Information Web Application")?.Id ?? throw new Exception("Failed to get projectId.");

            //Get ticket type Ids
            int typeNewDev = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypes.New_Development.ToString())?.Id ?? throw new Exception("Failed to get ticketTypeId.");
            int typeWorkTask = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypes.Work_Task.ToString())?.Id ?? throw new Exception("Failed to get ticketTypeId.");
            int typeDefect = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypes.Defect.ToString())?.Id ?? throw new Exception("Failed to get ticketTypeId.");
            int typeEnhancement = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypes.Enhancement.ToString())?.Id ?? throw new Exception("Failed to get ticketTypeId.");
            int typeChangeRequest = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypes.Change_Request.ToString())?.Id ?? throw new Exception("Failed to get ticketTypeId.");

            //Get ticket priority Ids
            int priorityLow = context.TicketPriorities.FirstOrDefault(p => p.Name == Priorities.Low.ToString())?.Id ?? throw new Exception("Failed to get ticketPriorityId.");;
            int priorityMedium = context.TicketPriorities.FirstOrDefault(p => p.Name == Priorities.Medium.ToString())?.Id ?? throw new Exception("Failed to get ticketPriorityId.");;
            int priorityHigh = context.TicketPriorities.FirstOrDefault(p => p.Name == Priorities.High.ToString())?.Id ?? throw new Exception("Failed to get ticketPriorityId.");;
            int priorityUrgent = context.TicketPriorities.FirstOrDefault(p => p.Name == Priorities.Urgent.ToString())?.Id ?? throw new Exception("Failed to get ticketPriorityId.");;

            //Get ticket status Ids
            int statusNew = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatuses.New.ToString())?.Id ?? throw new Exception("Failed to get ticketStatusId.");;
            int statusDev = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatuses.Development.ToString())?.Id ?? throw new Exception("Failed to get ticketStatusId.");;
            int statusTest = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatuses.Testing.ToString())?.Id ?? throw new Exception("Failed to get ticketStatusId.");;
            int statusResolved = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatuses.Resolved.ToString())?.Id ?? throw new Exception("Failed to get ticketStatusId.");;


            try
            {
                int? priorityLow1 = priorityLow;
                IList<Ticket> tickets = new List<Ticket>() {
                                //PORTFOLIO
                                new Ticket() { Title = "Portfolio Ticket 1", Description = "Ticket details for portfolio ticket 1", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Portfolio Ticket 2", Description = "Ticket details for portfolio ticket 2", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Portfolio Ticket 4", Description = "Ticket details for portfolio ticket 4", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Portfolio Ticket 5", Description = "Ticket details for portfolio ticket 5", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Portfolio Ticket 6", Description = "Ticket details for portfolio ticket 6", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Portfolio Ticket 7", Description = "Ticket details for portfolio ticket 7", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Portfolio Ticket 8", Description = "Ticket details for portfolio ticket 8", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                //BLOG
                                new Ticket() { Title = "Blog Ticket 1", Description = "Ticket details for blog ticket 1", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Blog Ticket 2", Description = "Ticket details for blog ticket 2", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Blog Ticket 3", Description = "Ticket details for blog ticket 3", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Blog Ticket 4", Description = "Ticket details for blog ticket 4", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Blog Ticket 5", Description = "Ticket details for blog ticket 5", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Blog Ticket 6", Description = "Ticket details for blog ticket 6", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Blog Ticket 7", Description = "Ticket details for blog ticket 7", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Blog Ticket 8", Description = "Ticket details for blog ticket 8", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Blog Ticket 9", Description = "Ticket details for blog ticket 9", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Blog Ticket 10", Description = "Ticket details for blog ticket 10", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Blog Ticket 11", Description = "Ticket details for blog ticket 11", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Blog Ticket 12", Description = "Ticket details for blog ticket 12", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Blog Ticket 13", Description = "Ticket details for blog ticket 13", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Blog Ticket 14", Description = "Ticket details for blog ticket 14", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Blog Ticket 15", Description = "Ticket details for blog ticket 15", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Blog Ticket 16", Description = "Ticket details for blog ticket 16", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Blog Ticket 17", Description = "Ticket details for blog ticket 17", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                //BUGTRACKER                                                                                                                         
                                new Ticket() { Title = "Bug Tracker Ticket 1", Description = "Ticket details for bug tracker ticket 1", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 2", Description = "Ticket details for bug tracker ticket 2", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 3", Description = "Ticket details for bug tracker ticket 3", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 4", Description = "Ticket details for bug tracker ticket 4", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 5", Description = "Ticket details for bug tracker ticket 5", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 6", Description = "Ticket details for bug tracker ticket 6", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 7", Description = "Ticket details for bug tracker ticket 7", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 8", Description = "Ticket details for bug tracker ticket 8", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 9", Description = "Ticket details for bug tracker ticket 9", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 10", Description = "Ticket details for bug tracker 10", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 11", Description = "Ticket details for bug tracker 11", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 12", Description = "Ticket details for bug tracker 12", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 13", Description = "Ticket details for bug tracker 13", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 14", Description = "Ticket details for bug tracker 14", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 15", Description = "Ticket details for bug tracker 15", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 16", Description = "Ticket details for bug tracker 16", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 17", Description = "Ticket details for bug tracker 17", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 18", Description = "Ticket details for bug tracker 18", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 19", Description = "Ticket details for bug tracker 19", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 20", Description = "Ticket details for bug tracker 20", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 21", Description = "Ticket details for bug tracker 21", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 22", Description = "Ticket details for bug tracker 22", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 23", Description = "Ticket details for bug tracker 23", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 24", Description = "Ticket details for bug tracker 24", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 25", Description = "Ticket details for bug tracker 25", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 26", Description = "Ticket details for bug tracker 26", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 27", Description = "Ticket details for bug tracker 27", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 28", Description = "Ticket details for bug tracker 28", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 29", Description = "Ticket details for bug tracker 29", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 30", Description = "Ticket details for bug tracker 30", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                //MOVIE
                                new Ticket() { Title = "Movie Ticket 1", Description = "Ticket details for movie ticket 1", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 2", Description = "Ticket details for movie ticket 2", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 3", Description = "Ticket details for movie ticket 3", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 4", Description = "Ticket details for movie ticket 4", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 5", Description = "Ticket details for movie ticket 5", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 6", Description = "Ticket details for movie ticket 6", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 7", Description = "Ticket details for movie ticket 7", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 8", Description = "Ticket details for movie ticket 8", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 9", Description = "Ticket details for movie ticket 9", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 10", Description = "Ticket details for movie ticket 10", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 11", Description = "Ticket details for movie ticket 11", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 12", Description = "Ticket details for movie ticket 12", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 13", Description = "Ticket details for movie ticket 13", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 14", Description = "Ticket details for movie ticket 14", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 15", Description = "Ticket details for movie ticket 15", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 16", Description = "Ticket details for movie ticket 16", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 17", Description = "Ticket details for movie ticket 17", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 18", Description = "Ticket details for movie ticket 18", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 19", Description = "Ticket details for movie ticket 19", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 20", Description = "Ticket details for movie ticket 20", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},

                };


                var dbTickets = context.Tickets.Select(c => c.Title).ToList();
                await context.Tickets.AddRangeAsync(tickets.Where(c => !dbTickets.Contains(c.Title)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Tickets.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }
    }
}
