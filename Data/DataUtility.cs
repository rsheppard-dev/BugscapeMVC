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
            await SeedDefaultTicketsAsync(dbContextSvc);
            await SeedDefaultTicketHistoriesAsync(dbContextSvc);
            await SeedDefaultTicketCommentsAsync(dbContextSvc);
            await SeedDefaultNotificationsAsync(dbContextSvc);
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
                IList<Company> defaultCompanies = new List<Company>() {
                    new() 
                    { 
                        Name = "Democorp",
                        Description=@"
                        <h2>Your Software and Web Development Partner</h2>

                        <p>Welcome to Democorp, where innovation meets technology excellence. As a leading software and web development company, we specialise in crafting digital solutions that empower businesses and elevate online experiences. Our commitment to quality, creativity, and client satisfaction sets us apart in the competitive landscape of technology.</p>

                        <h2>Our Expertise:</h2>
                        <p>At Democorp, we excel in:</p>
                        <ul>
                            <li><strong>Custom Software Development:</strong> Tailored software solutions designed to meet your unique business needs.</li>
                            <li><strong>Web Development:</strong> Engaging and responsive websites that captivate audiences and drive user interaction.</li>
                            <li><strong>Mobile App Development:</strong> Innovative mobile applications that bring your ideas to life on iOS and Android platforms.</li>
                            <li><strong>E-commerce Solutions:</strong> Robust and scalable e-commerce platforms for seamless online transactions.</li>
                            <li><strong>UI/UX Design:</strong> Intuitive and visually appealing user interfaces that enhance user experiences.</li>
                        </ul>

                        <h2>Industries We Serve:</h2>
                        <ul>
                            <li>Technology and IT</li>
                            <li>Healthcare and Life Sciences</li>
                            <li>Finance and Banking</li>
                            <li>Manufacturing and Logistics</li>
                            <li>E-commerce</li>
                            <li>Entertainment and Media</li>
                        </ul>

                        <p>Partner with Democorp for forward-thinking solutions that drive your business growth. Let's innovate together!</p>"
                    },
                };

                var dbCompanies = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultCompanies.Where(c => !dbCompanies.Contains(c.Name)));
                await context.SaveChangesAsync();

                //Get company Ids
                company1Id = context.Companies.FirstOrDefault(p => p.Name == "Democorp")?.Id ?? throw new Exception("Failed to get company1Id");
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
                    new() { Name = Priorities.Low.ToString() },
                    new() { Name = Priorities.Medium.ToString() },
                    new() { Name = Priorities.High.ToString() },
                    new() { Name = Priorities.Urgent.ToString() },
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

        public static async Task SeedDefaultUsersAsync(UserManager<AppUser> userManager)
        {
            // get member images
            // admins
            byte[] scottMilesImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/scott-miles.jpg");
            byte[] michaelBrownImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/michael-brown.jpg");
            // project managers
            byte[] oliviaJonesImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/olivia-jones.jpg");
            byte[] natalieCarterImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/natalie-carter.jpg");
            // developers
            byte[] lucasJacksonImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/lucas-jackson.jpg");
            byte[] ameliaWilsonImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/amelia-wilson.jpg");
            byte[] leoMartinImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/leo-martin.jpg");
            byte[] sophieRobertsImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/sophie-roberts.jpg");
            byte[] liamHarrisonImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/liam-harrison.jpg");
            byte[] isabellaMurrayImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/isabella-murray.jpg");
            // submitters
            byte[] ethanMarshallImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/ethan-marshall.jpg");
            byte[] marcusDonovanImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/marcus-donovan.jpg");

            // Seed Default Admin User
            var defaultUser = new AppUser
            {
                UserName = "scott.miles@democorp.com",
                Email = "scott.miles@democorp.com",
                FirstName = "Scott",
                LastName = "Miles",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = scottMilesImage,
                AvatarFileName = "scott-miles.jpg",
                AvatarContentType = "image/jpeg"
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

            // Seed Default Admin User
            defaultUser = new AppUser
            {
                UserName = "michael.brown@democorp.com",
                Email = "michael.brown@democorp.com",
                FirstName = "Michael",
                LastName = "Brown",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = michaelBrownImage,
                AvatarFileName = "michael-brown.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Project_Manager User
            defaultUser = new AppUser
            {
                UserName = "olivia.jones@democorp.com",
                Email = "olivia.jones@democorp.com",
                FirstName = "Olivia",
                LastName = "Jones",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = oliviaJonesImage,
                AvatarFileName = "olivia-jones.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Project_Manager User
            defaultUser = new AppUser
            {
                UserName = "natalie.carter@democorp.com",
                Email = "natalie.carter@democorp.com",
                FirstName = "Natalie",
                LastName = "Carter",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = natalieCarterImage,
                AvatarFileName = "natalie-carter.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "lucas.jackson@democorp.com",
                Email = "lucas.jackson@democorp.com",
                FirstName = "Lucas",
                LastName = "Jackson",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = lucasJacksonImage,
                AvatarFileName = "lucas-jackson.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "amelia.wilson@democorp.com",
                Email = "amelia.wilson@democorp.com",
                FirstName = "Amelia",
                LastName = "Wilson",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = ameliaWilsonImage,
                AvatarFileName = "amelia-wilson.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "leo.martin@democorp.com",
                Email = "leo.martin@democorp.com",
                FirstName = "Leo",
                LastName = "Martin",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = leoMartinImage,
                AvatarFileName = "leo-martin.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "sophie.roberts@democorp.com",
                Email = "sophie.roberts@democorp.com",
                FirstName = "Sophie",
                LastName = "Roberts",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = sophieRobertsImage,
                AvatarFileName = "sophie-roberts.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "liam.harrison@democorp.com",
                Email = "liam.harrison@democorp.com",
                FirstName = "Liam",
                LastName = "Harrison",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = liamHarrisonImage,
                AvatarFileName = "liam-harrison.jpg",
                AvatarContentType = "image/jpeg"
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

            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "isabella.murray@democorp.com",
                Email = "isabella.murray@democorp.com",
                FirstName = "Isabella",
                LastName = "Murray",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = isabellaMurrayImage,
                AvatarFileName = "isabella-murray.jpg",
                AvatarContentType = "image/jpeg"
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

            // Seed Default Submitter User
            defaultUser = new AppUser
            {
                UserName = "ethan.marshall@democorp.com",
                Email = "ethan.marshall@democorp.com",
                FirstName = "Ethan",
                LastName = "Marshall",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = ethanMarshallImage,
                AvatarFileName = "ethan-marshall.jpg",
                AvatarContentType = "image/jpeg"
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


            // Seed Default Submitter User
            defaultUser = new AppUser
            {
                UserName = "marcus.donovan@democorp.com",
                Email = "marcus.donovan@democorp.com",
                FirstName = "Marcus",
                LastName = "Donovan",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = marcusDonovanImage,
                AvatarFileName = "marcus-donovan.jpg",
                AvatarContentType = "image/jpeg"
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
            // get demo user images
            byte[] demoAdminImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/demo-admin.jpg");
            byte[] demoPmImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/demo-pm.jpg");
            byte[] demoDevImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/demo-dev.jpg");
            byte[] demoSubmitterImage = await File.ReadAllBytesAsync("wwwroot/images/demo/members/demo-submitter.jpg");

            // seed demo admin user
            var defaultUser = new AppUser
            {
                UserName = "demo.admin@democorp.com",
                Email = "demo.admin@democorp.com",
                FirstName = "Demo",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = demoAdminImage,
                AvatarFileName = "demo-admin.jpg",
                AvatarContentType = "image/jpeg"
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
                UserName = "demo.projectmanager@democorp.com",
                Email = "demo.projectmanager@democorp.com",
                FirstName = "Demo",
                LastName = "Project-Manager",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = demoPmImage,
                AvatarFileName = "demo-pm.jpg",
                AvatarContentType = "image/jpeg"
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
                UserName = "demo.developer@democorp.com",
                Email = "demo.developer@democorp.com",
                FirstName = "Demo",
                LastName = "Developer",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = demoDevImage,
                AvatarFileName = "demo-dev.jpg",
                AvatarContentType = "image/jpeg"
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
                UserName = "demo.submitter@democorp.com",
                Email = "demo.submitter@democorp.com",
                FirstName = "Demo",
                LastName = "Submitter",
                EmailConfirmed = true,
                CompanyId = company1Id,
                AvatarFileData = demoSubmitterImage,
                AvatarFileName = "demo-submitter.jpg",
                AvatarContentType = "image/jpeg"
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
                     new TicketType() { Name = TicketTypes.General_Task.ToString() }          // Ticket involves no software development but may involve tasks such as configurations, or hardware setup
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

        public static async Task SeedDefaultProjectsAsync(ApplicationDbContext context)
        {
            try
            {
                // get project images
                byte[] blogImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/blog.png");
                byte[] onlineLearningImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/onlineLearning.png");
                byte[] ecommerceImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/ecommerce.png");
                byte[] crmImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/crm.png");

                // get project managers
                AppUser oliviaJones = context.Users.FirstOrDefault(p => p.FirstName == "Olivia" && p.LastName == "Jones") ?? throw new Exception("Failed to get oliviaJones");
                AppUser natalieCarter = context.Users.FirstOrDefault(p => p.FirstName == "Natalie" && p.LastName == "Carter") ?? throw new Exception("Failed to get natalieCarter");
                AppUser demoPm = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Project-Manager") ?? throw new Exception("Failed to get demoPm");

                // get developers
                AppUser lucasJackson = context.Users.FirstOrDefault(p => p.FirstName == "Lucas" && p.LastName == "Jackson") ?? throw new Exception("Failed to get lucasJackson");
                AppUser ameliaWilson = context.Users.FirstOrDefault(p => p.FirstName == "Amelia" && p.LastName == "Wilson") ?? throw new Exception("Failed to get ameliaWilson");
                AppUser leoMartin = context.Users.FirstOrDefault(p => p.FirstName == "Leo" && p.LastName == "Martin") ?? throw new Exception("Failed to get leoMartin");
                AppUser sophieRoberts = context.Users.FirstOrDefault(p => p.FirstName == "Sophie" && p.LastName == "Roberts") ?? throw new Exception("Failed to get sophieRoberts");
                AppUser liamHarrison = context.Users.FirstOrDefault(p => p.FirstName == "Liam" && p.LastName == "Harrison") ?? throw new Exception("Failed to get liamHarrison");
                AppUser isabellaMurray = context.Users.FirstOrDefault(p => p.FirstName == "Isabella" && p.LastName == "Murray") ?? throw new Exception("Failed to get isabellaMurray");
                AppUser demoDev = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Developer") ?? throw new Exception("Failed to get demoDev");

                // get submitters
                AppUser ethanMarshallId = context.Users.FirstOrDefault(p => p.FirstName == "Ethan" && p.LastName == "Marshall") ?? throw new Exception("Failed to get ethanMarshallId");
                AppUser marcusDonovanId = context.Users.FirstOrDefault(p => p.FirstName == "Marcus" && p.LastName == "Donovan") ?? throw new Exception("Failed to get marcusDonovanId");
                AppUser demoSubmitterId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Submitter") ?? throw new Exception("Failed to get demoSubmitterId");

                // get project priority Ids
                int priorityLow = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Low.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");
                int priorityMedium = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Medium.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
                int priorityHigh = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.High.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
                int priorityUrgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Urgent.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
           
                Project onlineLearningPortal = new()
                {
                    CompanyId = company1Id,
                    Name = "Online Learning Platform",
                    Description = @"
                    <p>Embark on the creation of a dynamic Online Learning Platform designed to revolutionise education delivery and provide a seamless learning experience. This project focuses on building an engaging platform that empowers educators, facilitates student learning, and fosters collaboration in the digital education landscape.</p>

                    <h2>Key Features:</h2>
                    <ul>
                        <li><strong>Course Management:</strong> Easily create, manage, and organise courses with diverse content types.</li>
                        <li><strong>User Authentication:</strong> Secure user authentication and access control for educators and students.</li>
                        <li><strong>Interactive Content:</strong> Engage learners with multimedia content, quizzes, and interactive assessments.</li>
                        <li><strong>Discussion Forums:</strong> Foster a sense of community with discussion forums for students and educators to interact.</li>
                        <li><strong>Progress Tracking:</strong> Monitor student progress, track completion, and provide performance analytics.</li>
                        <li><strong>Mobile Accessibility:</strong> Ensure a responsive design for seamless access on various devices.</li>
                    </ul>

                    <h2>Technologies Used:</h2>
                    <ul>
                        <li>HTML5</li>
                        <li>CSS3</li>
                        <li>JavaScript</li>
                        <li>React.js (or Angular/Vue.js)</li>
                        <li>Node.js (or Django/Flask for Python)</li>
                        <li>MongoDB (or MySQL/PostgreSQL)</li>
                    </ul>

                    <p>Join us in creating an Online Learning Platform that transforms education into an interactive and accessible experience. Let's build a platform that empowers educators, inspires learners, and shapes the future of digital education!</p>
                    ",
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(5),
                    ProjectPriorityId = priorityLow,
                    ImageFileData = onlineLearningImage,
                    ImageFileName = "onlineLearning.png",
                    ImageContentType = "image/png",
                };
                
                Project blog = new()
                {
                    CompanyId = company1Id,
                    Name = "Build a Blog Web Application",
                    Description=@"
                    <p>Embark on a journey to create a dynamic and feature-rich blog web application that not only captures attention but also provides a seamless user experience. This project focuses on developing a platform where content creators can share their thoughts, stories, and expertise with a global audience. The goal is to build a robust and engaging blogging solution that stands out in the digital landscape.</p>

                    <h2>Key Features:</h2>
                    <ul>
                        <li><strong>User-Friendly Interface:</strong> Intuitive design for easy navigation and content consumption.</li>
                        <li><strong>Content Management System (CMS):</strong> Empower authors with a user-friendly CMS to create, edit, and manage blog posts effortlessly.</li>
                        <li><strong>Responsive Design:</strong> Ensure a seamless experience across various devices with a fully responsive web application.</li>
                        <li><strong>Commenting System:</strong> Foster community engagement with a robust commenting system for reader interaction.</li>
                        <li><strong>Search Functionality:</strong> Implement a powerful search feature for users to find relevant content quickly.</li>
                        <li><strong>Social Media Integration:</strong> Enhance content sharing and user reach by integrating social media sharing capabilities.</li>
                    </ul>

                    <h2>Technologies Used:</h2>
                    <ul>
                        <li>HTML5</li>
                        <li>CSS3</li>
                        <li>JavaScript</li>
                        <li>Node.js</li>
                        <li>Express.js</li>
                        <li>MongoDB</li>
                    </ul>

                    <p>Join us on this exciting venture to revolutionise the blogging experience. Let's create a platform that not only reflects your brand but also empowers content creators in the digital age!</p>
                    ",
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now.AddDays(-7).AddMonths(3),
                    ProjectPriorityId = priorityMedium,
                    ImageFileData = blogImage,
                    ImageFileName = "blog.png",
                    ImageContentType = "image/png"
                    };

                Project ecommerce = new()
                {
                    CompanyId = company1Id,
                    Name = "E-commerce Website",
                    Description = @"
                    <p>Embark on the journey of creating a powerful and user-friendly e-commerce website that not only showcases your products but also provides a seamless shopping experience for your customers. This project is focused on building a robust online store that reflects your brand identity, attracts customers, and drives sales in the competitive digital marketplace.</p>

                    <h2>Key Features:</h2>
                    <ul>
                        <li><strong>Product Catalogue:</strong> Showcase your products with high-quality images, detailed descriptions, and categorisation for easy navigation.</li>
                        <li><strong>Shopping Cart and Checkout:</strong> Implement a secure and user-friendly shopping cart system with a seamless checkout process.</li>
                        <li><strong>Payment Gateway Integration:</strong> Integrate popular payment gateways to offer a variety of secure payment options for customers.</li>
                        <li><strong>Responsive Design:</strong> Ensure a consistent and engaging shopping experience across devices with a responsive design.</li>
                        <li><strong>Customer Accounts:</strong> Allow customers to create accounts, track orders, and manage their preferences for personalised shopping.</li>
                        <li><strong>Search Functionality:</strong> Implement a robust search feature for customers to easily find products based on keywords or categories.</li>
                    </ul>

                    <h2>Technologies Used:</h2>
                    <ul>
                        <li>HTML5</li>
                        <li>CSS3</li>
                        <li>JavaScript</li>
                        <li>React.js (or Angular/Vue.js)</li>
                        <li>Node.js (or Django/Flask for Python)</li>
                        <li>MongoDB (or MySQL/PostgreSQL)</li>
                    </ul>

                    <p>Join us in creating an e-commerce website that not only meets but exceeds the expectations of your customers. Let's build an online shopping destination that showcases your products and elevates your brand presence!</p>
                    ",
                    StartDate = DateTime.Now.AddMonths(-2).AddDays(-5),
                    EndDate = DateTime.Now.AddDays(-5).AddMonths(1),
                    ProjectPriorityId = priorityHigh,
                    ImageFileData = ecommerceImage,
                    ImageFileName = "ecommerce.png",
                    ImageContentType = "image/png"
                };
                
                Project crm = new()
                {
                    CompanyId = company1Id,
                    Name = "Customer Relationship Management (CRM) System",
                    Description = @"
                    <p>Embark on the creation of a comprehensive Customer Relationship Management (CRM) System designed to empower your business in managing customer interactions, leads, and communication effectively. This project focuses on building a centralised platform that enhances customer relationships, streamlines communication, and provides valuable insights to drive business growth.</p>

                    <h2>Key Features:</h2>
                    <ul>
                        <li><strong>Customer Database:</strong> Centralised storage for customer information, interactions, and preferences.</li>
                        <li><strong>Lead Management:</strong> Efficiently track and manage leads from acquisition to conversion.</li>
                        <li><strong>Communication Tools:</strong> Integrated tools for seamless communication with customers via email, phone, and other channels.</li>
                        <li><strong>Task and Appointment Management:</strong> Schedule tasks, appointments, and follow-ups to ensure timely and organised interactions.</li>
                        <li><strong>Analytics and Reporting:</strong> Generate insightful reports and analytics to understand customer behaviour and make informed business decisions.</li>
                        <li><strong>Integration Capabilities:</strong> Integrate with other business systems and tools for a unified workflow.</li>
                    </ul>

                    <h2>Technologies Used:</h2>
                    <ul>
                        <li>HTML5</li>
                        <li>CSS3</li>
                        <li>JavaScript</li>
                        <li>React.js (or Angular/Vue.js)</li>
                        <li>Node.js (or Django/Flask for Python)</li>
                        <li>MongoDB (or MySQL/PostgreSQL)</li>
                    </ul>

                    <p>Join us in creating a CRM System that goes beyond managing data; it's a strategic tool to enhance customer relationships and drive business success. Let's build a system that grows with your business!</p>
                    ",
                    StartDate = DateTime.Now.AddMonths(-2).AddDays(-3),
                    EndDate = DateTime.Now.AddDays(-3).AddMonths(6),
                    ProjectPriorityId = priorityUrgent,
                    ImageFileData = crmImage,
                    ImageFileName = "crm.png",
                    ImageContentType = "image/png"
                };

                // online learning portal members
                // project manager
                onlineLearningPortal.Members.Add(demoPm);
                // developers
                onlineLearningPortal.Members.Add(lucasJackson);
                onlineLearningPortal.Members.Add(ameliaWilson);
                onlineLearningPortal.Members.Add(demoDev);
                // submitters
                onlineLearningPortal.Members.Add(ethanMarshallId);
                onlineLearningPortal.Members.Add(marcusDonovanId);
                onlineLearningPortal.Members.Add(demoSubmitterId);

                // blog members
                // project manager
                blog.Members.Add(oliviaJones);
                // developers
                blog.Members.Add(leoMartin);
                blog.Members.Add(demoDev);
                blog.Members.Add(sophieRoberts);
                // submitters
                blog.Members.Add(ethanMarshallId);
                blog.Members.Add(marcusDonovanId);
                blog.Members.Add(demoSubmitterId);

                // ecommerce members
                // project manager
                ecommerce.Members.Add(natalieCarter);
                // developers
                ecommerce.Members.Add(liamHarrison);
                ecommerce.Members.Add(isabellaMurray);
                ecommerce.Members.Add(leoMartin);
                // submitters
                ecommerce.Members.Add(ethanMarshallId);
                ecommerce.Members.Add(marcusDonovanId);

                // crm members
                // project manager
                crm.Members.Add(demoPm);
                // developers
                crm.Members.Add(lucasJackson);
                crm.Members.Add(demoDev);
                crm.Members.Add(ameliaWilson);
                crm.Members.Add(liamHarrison);
                // submitters
                crm.Members.Add(ethanMarshallId);
                crm.Members.Add(marcusDonovanId);
                crm.Members.Add(demoSubmitterId);

                // list of projects
                var projects = new List<Project> { onlineLearningPortal, blog, ecommerce, crm };

                foreach (Project project in projects)
                {
                    // check if the project already exists in the database
                    Project? existingProject = await context.Projects
                        .FirstOrDefaultAsync(p => p.Name == project.Name);

                    // if the project doesn't exist, add it
                    if (existingProject == null) context.Projects.Add(project);
                }

                // save changes
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

        public static async Task SeedDefaultTicketsAsync(ApplicationDbContext context)
        {
            //Get project Ids
            int onlineLearningId = (context.Projects.FirstOrDefault(p => p.Name == "Online Learning Platform")?.Id) ?? throw new Exception("Failed to get projectId.");
            int blogId = context.Projects.FirstOrDefault(p => p.Name == "Build a Blog Web Application")?.Id ?? throw new Exception("Failed to get projectId.");
            int ecommerceId = context.Projects.FirstOrDefault(p => p.Name == "E-commerce Website")?.Id ?? throw new Exception("Failed to get projectId.");
            int crmId = context.Projects.FirstOrDefault(p => p.Name == "Customer Relationship Management (CRM) System")?.Id ?? throw new Exception("Failed to get projectId.");

            // get developer Ids
            string lucasJacksonId = context.Users.FirstOrDefault(p => p.FirstName == "Lucas" && p.LastName == "Jackson")?.Id ?? throw new Exception("Failed to get lucasJacksonId");
            string ameliaWilsonId = context.Users.FirstOrDefault(p => p.FirstName == "Amelia" && p.LastName == "Wilson")?.Id ?? throw new Exception("Failed to get ameliaWilsonId");
            string leoMartinId = context.Users.FirstOrDefault(p => p.FirstName == "Leo" && p.LastName == "Martin")?.Id ?? throw new Exception("Failed to get leoMartinId");
            string sophieRobertsId = context.Users.FirstOrDefault(p => p.FirstName == "Sophie" && p.LastName == "Roberts")?.Id ?? throw new Exception("Failed to get sophieRobertsId");
            string liamHarrisonId = context.Users.FirstOrDefault(p => p.FirstName == "Liam" && p.LastName == "Harrison")?.Id ?? throw new Exception("Failed to get liamHarrisonId");
            string isabellaMurrayId = context.Users.FirstOrDefault(p => p.FirstName == "Isabella" && p.LastName == "Murray")?.Id ?? throw new Exception("Failed to get isabellaMurrayId");
            string demoDevId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Developer")?.Id ?? throw new Exception("Failed to get demoDevId");
            
            // get submitter Ids
            string ethanMarshallId = context.Users.FirstOrDefault(p => p.FirstName == "Ethan" && p.LastName == "Marshall")?.Id ?? throw new Exception("Failed to get ethanMarshallId");
            string marcusDonovanId = context.Users.FirstOrDefault(p => p.FirstName == "Marcus" && p.LastName == "Donovan")?.Id ?? throw new Exception("Failed to get marcusDonovanId");
            string demoSubmitterId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Submitter")?.Id ?? throw new Exception("Failed to get demoSubmitterId");

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
                Random random = new();
                int? priorityLow1 = priorityLow;
                IList<Ticket> tickets = new List<Ticket>() {
                                // online learning
                                // live tickets
                                new() { 
                                    Title = "Create Welcome Page",
                                    Description = "<p>Create a welcoming and informative home page for the online learning platform. Include an introduction to the platform's features and benefits.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new() { 
                                    Title = "Implement Video Lecture Feature",
                                    Description = "<p>Develop the functionality to support video lectures. Ensure compatibility with various video formats and provide a user-friendly interface for students and instructors.</p>",
                                    Created = DateTimeOffset.Now,
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = ethanMarshallId
                                },
                                new() {
                                    Title = "Fix Quiz Grading Bug",
                                    Description = "<p>Investigate and fix a bug in the quiz grading system. Ensure accurate grading and feedback for students taking quizzes on the platform.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusTest,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = lucasJacksonId
                                },
                                new() {
                                    Title = "Add Discussion Forum",
                                    Description = "<p>Integrate a discussion forum feature where students and instructors can engage in meaningful discussions. Implement moderation tools to ensure a positive learning environment.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new() {
                                    Title = "Enhance User Profile Page",
                                    Description = "<p>Improve the user profile page to include additional details about students and instructors. Enhance the design for a more engaging and personalised user experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = demoDevId
                                },
                                new() {
                                    Title = "Optimise Video Streaming Performance",
                                    Description = "<p>Optimise the performance of video streaming to ensure smooth playback for users with various internet speeds. Implement adaptive streaming technologies for a seamless experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = lucasJacksonId
                                },
                                new() {
                                    Title = "Fix Accessibility Issues in Course Content",
                                    Description = "<p>Address and resolve accessibility issues in course content. Ensure that all content, including text, images, and multimedia, is accessible to users with disabilities.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusTest,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                // resolved tickets
                                new()
                                {
                                    Title = "Investigate Video Playback Issues",
                                    Description = "<p>Explore and resolve issues affecting the playback of videos on the online learning platform. Ensure a seamless and high-quality video viewing experience for users.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                new()
                                {
                                    Title = "Enhance Course Enrollment Process",
                                    Description = "<p>Implement improvements to the course enrollment process. Streamline the steps for users to enroll in courses and access educational content with ease.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = demoDevId
                                },

                                new()
                                {
                                    Title = "Fix Responsive Design Issues in Course Pages",
                                    Description = "<p>Address and fix responsive design issues affecting the display of course pages. Ensure a consistent and visually appealing layout across various devices.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = lucasJacksonId
                                },

                                new()
                                {
                                    Title = "Optimise Quiz Performance",
                                    Description = "<p>Optimise the performance of quizzes on the learning platform. Implement enhancements to ensure a smooth and efficient quiz-taking experience for students.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                new()
                                {
                                    Title = "Resolve Login Authentication Issues",
                                    Description = "<p>Investigate and resolve issues related to login authentication on the online learning platform. Ensure that users can securely log in and access their accounts without difficulties.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = demoDevId
                                },

                                new()
                                {
                                    Title = "Implement Course Rating System",
                                    Description = "<p>Implement a course rating system for users to provide feedback on the quality of courses. Enhance the overall user experience by incorporating user ratings.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = lucasJacksonId
                                },

                                new()
                                {
                                    Title = "Fix Certificate Generation Bug",
                                    Description = "<p>Address and fix bugs related to the generation of course completion certificates. Ensure that users receive accurate and timely certificates upon completing courses.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = onlineLearningId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                // blog
                                // live tickets
                                new()
                                {
                                    Title = "Create Welcome Blog Post",
                                    Description = "<p>Create a welcoming and engaging blog post to introduce users to the blog platform. Highlight the key features and encourage readers to explore various topics.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeDefect,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Implement Image Gallery Feature",
                                    Description = "<p>Develop a feature to support image galleries within blog posts. Ensure a user-friendly interface for bloggers to easily add and manage images in their content.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeEnhancement,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },
                                new()
                                {
                                    Title = "Fix Blog Post Formatting Issue",
                                    Description = "<p>Investigate and fix a formatting issue affecting the appearance of blog posts. Ensure consistent and visually appealing formatting across all posts.</p>",
                                    Created = DateTimeOffset.Now,
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = marcusDonovanId
                                },
                                new()
                                {
                                    Title = "Add Comment Section to Blog Posts",
                                    Description = "<p>Integrate a comment section for readers to engage with blog posts. Implement moderation tools to ensure a positive and respectful commenting environment.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    DeveloperUserId = demoDevId,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Enhance Author Profile Page",
                                    Description = "<p>Improve the author profile page to include additional details about bloggers. Enhance the design for a more personalized and appealing user experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeDefect,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },
                                new()
                                {
                                    Title = "Optimise Blog Loading Speed",
                                    Description = "<p>Optimize the loading speed of blog pages to ensure a smooth reading experience for users. Implement caching and other performance enhancements for efficient page loading.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeEnhancement,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = marcusDonovanId
                                },
                                new()
                                {
                                    Title = "Fix Broken Links in Blog Posts",
                                    Description = "<p>Address and fix broken links within blog posts. Ensure all links are functional and direct readers to the intended content.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeChangeRequest,
                                    DeveloperUserId = demoDevId,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Improve Blog Search Functionality",
                                    Description = "<p>Enhance the search functionality within the blog platform. Implement advanced search features for users to find specific topics and articles more easily.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },
                                new()
                                {
                                    Title = "Fix Responsive Design Issue",
                                    Description = "<p>Address and fix responsive design issues affecting the display of blog content on various devices. Ensure a consistent and visually appealing layout across all screen sizes.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeDefect,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = marcusDonovanId
                                },
                                new()
                                {
                                    Title = "Enhance Blog Category Navigation",
                                    Description = "<p>Improve the category navigation system within the blog platform. Make it easier for users to browse and discover content based on specific topics.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeEnhancement,
                                    DeveloperUserId = demoDevId,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Optimise Blog Commenting System",
                                    Description = "<p>Optimize the commenting system for blog posts. Implement features to enhance user engagement and ensure a smooth and enjoyable commenting experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeChangeRequest,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },
                                new()
                                {
                                    Title = "Fix Blog Post Pagination Issue",
                                    Description = "<p>Investigate and fix an issue with blog post pagination. Ensure that readers can navigate through multiple pages of blog content seamlessly.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = marcusDonovanId
                                },
                                new()
                                {
                                    Title = "Address Accessibility Issues in Blog Platform",
                                    Description = "<p>Address and resolve accessibility issues within the blog platform. Ensure that all users, including those with disabilities, can access and enjoy the content without any barriers.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeDefect,
                                    DeveloperUserId = demoDevId,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Improve Blog Post Sharing Options",
                                    Description = "<p>Enhance the sharing options for blog posts. Implement additional social media integration and sharing features to expand the reach of the blog content.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeEnhancement,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },
                                new()
                                {
                                    Title = "Fix Blog Author Attribution Issue",
                                    Description = "<p>Investigate and fix an issue with author attribution on blog posts. Ensure that the correct authors are credited for their contributions.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeChangeRequest,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = marcusDonovanId
                                },
                                new()
                                {
                                    Title = "Enhance Blog Subscription Features",
                                    Description = "<p>Improve the subscription features within the blog platform. Provide users with more options to subscribe to specific authors, categories, or the entire blog.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    DeveloperUserId = demoDevId,
                                    OwnerUserId = demoSubmitterId
                                },

                                // resolved tickets
                                new()
                                {
                                    Title = "Image Gallery Loading Issue",
                                    Description = "<p>Users reported difficulties loading images in the blog's gallery feature. Investigate and resolve to ensure smooth image loading.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 7)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 3)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeDefect,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = demoSubmitterId
                                },

                                new()
                                {
                                    Title = "Responsive Design Enhancement",
                                    Description = "<p>Issues with the responsive design of the blog on certain devices. Implement improvements to provide a consistent and visually appealing layout.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 7)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 3)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeChangeRequest,
                                    DeveloperUserId = sophieRobertsId,
                                    OwnerUserId = ethanMarshallId
                                },

                                new()
                                {
                                    Title = "Blog Search Functionality Upgrade",
                                    Description = "<p>Enhance the search functionality within the blog platform. Users have requested advanced search features to find specific topics more easily.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 7)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 3)),
                                    ProjectId = blogId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    DeveloperUserId = leoMartinId,
                                    OwnerUserId = marcusDonovanId
                                },
                                
                                // ecommerce website
                                // live tickets
                                new()
                                {
                                    Title = "Resolve Product Display Issue",
                                    Description = "<p>Investigate and resolve an issue affecting the display of product images on the website. Ensure that product images are correctly showcased for an optimal shopping experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Enhance Shopping Cart Functionality",
                                    Description = "<p>Implement improvements to the shopping cart functionality. Enhance features such as item quantity adjustments, removal of items, and a seamless checkout process.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },
                                new()
                                {
                                    Title = "Fix Checkout Page Responsiveness",
                                    Description = "<p>Address and fix issues related to the responsiveness of the checkout page. Ensure a consistent and user-friendly experience for customers accessing the website on various devices.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },
                                new()
                                {
                                    Title = "Optimise Product Search Functionality",
                                    Description = "<p>Optimise the product search functionality on the website. Implement advanced search features and filters to help users find specific products more efficiently.</p>",
                                    Created = DateTimeOffset.Now,
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                },
                                new()
                                {
                                    Title = "Fix Broken Links in Product Pages",
                                    Description = "<p>Address and fix broken links within product pages. Ensure that all links directing customers to product details and related pages are functional.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },
                                new()
                                {
                                    Title = "Improve Product Recommendations",
                                    Description = "<p>Enhance the product recommendation system on the website. Implement algorithms to suggest relevant products based on customer preferences and browsing history.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },
                                new()
                                {
                                    Title = "Fix Payment Gateway Integration Issue",
                                    Description = "<p>Investigate and fix issues related to payment gateway integration. Ensure a smooth and secure transaction process for customers making purchases on the website.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Implement Product Ratings and Reviews",
                                    Description = "<p>Implement a ratings and reviews system for products. Allow customers to provide feedback and ratings for products, enhancing the credibility of the online store.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },
                                new()
                                {
                                    Title = "Fix Shipping Calculation Error",
                                    Description = "<p>Address and fix errors in the calculation of shipping costs. Ensure accurate shipping cost calculations for customers during the checkout process.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },
                                new()
                                {
                                    Title = "Enhance User Account Management",
                                    Description = "<p>Implement improvements to the user account management system. Enhance features such as account registration, password recovery, and user profile management.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Fix Product Page Loading Speed",
                                    Description = "<p>Optimise the loading speed of product pages. Implement caching and other performance enhancements to ensure fast and efficient page loading for a better user experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },
                                new()
                                {
                                    Title = "Resolve Account Login Issues",
                                    Description = "<p>Investigate and resolve issues related to account login. Ensure that customers can log in securely and access their accounts without any difficulties.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },
                                new()
                                {
                                    Title = "Implement Product Stock Notifications",
                                    Description = "<p>Implement a system to notify customers when a product is back in stock. Enhance customer experience by providing timely updates on product availability.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Fix Checkout Page Validation Errors",
                                    Description = "<p>Address and fix validation errors occurring on the checkout page. Ensure that customer information is validated accurately during the checkout process.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },
                                new()
                                {
                                    Title = "Optimise Website for Mobile Devices",
                                    Description = "<p>Optimise the website for seamless performance on mobile devices. Ensure a responsive design that provides a consistent and user-friendly experience across various screen sizes.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },
                                new()
                                {
                                    Title = "Improve Product Page Navigation",
                                    Description = "<p>Enhance the navigation of product pages for improved user experience. Implement features such as breadcrumb navigation and filters for easy exploration of products.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                // resolved tickets
                                new()
                                {
                                    Title = "Investigate Product Page Display Error",
                                    Description = "<p>Explore and resolve an issue affecting the display of product images on the website. Ensure that product images are correctly showcased for an optimal shopping experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                new()
                                {
                                    Title = "Implement One-Click Checkout Feature",
                                    Description = "<p>Introduce a one-click checkout feature to enhance the shopping cart functionality. Simplify the checkout process for a smoother and more efficient shopping experience.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },

                                new()
                                {
                                    Title = "Address Checkout Page Responsiveness Issue",
                                    Description = "<p>Tackle and fix issues related to the responsiveness of the checkout page. Ensure a consistent and user-friendly experience for customers accessing the website on various devices.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },

                                new()
                                {
                                    Title = "Enhance Product Search Filters",
                                    Description = "<p>Optimise the product search functionality on the website. Implement advanced search features and filters to help users find specific products more efficiently.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                new()
                                {
                                    Title = "Fix Broken Links in Product Categories",
                                    Description = "<p>Address and fix broken links within product categories. Ensure that all links directing customers to product details and related pages are functional.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },

                                new()
                                {
                                    Title = "Revamp Product Recommendation Algorithms",
                                    Description = "<p>Enhance the product recommendation system on the website. Implement advanced algorithms to suggest relevant products based on customer preferences and browsing history.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },

                                new()
                                {
                                    Title = "Fix Payment Gateway Connection Error",
                                    Description = "<p>Investigate and fix errors related to payment gateway connection. Ensure a smooth and secure transaction process for customers making purchases on the website.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                new()
                                {
                                    Title = "Implement Customer-Driven Product Ratings",
                                    Description = "<p>Implement a customer-driven ratings and reviews system for products. Allow customers to provide feedback and ratings, enhancing the credibility of the online store.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = isabellaMurrayId
                                },

                                new()
                                {
                                    Title = "Fix Shipping Cost Calculation Bug",
                                    Description = "<p>Address and fix bugs in the calculation of shipping costs. Ensure accurate shipping cost calculations for customers during the checkout process.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = leoMartinId
                                },

                                new()
                                {
                                    Title = "Optimise User Account Registration",
                                    Description = "<p>Implement improvements to the user account registration system. Enhance features such as account creation, password recovery, and user profile management.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 45)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = ecommerceId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                // crm
                                // live tickets
                                new()
                                {
                                    Title = "Resolve Customer Data Import Issue",
                                    Description = "<p>Investigate and resolve issues related to the import of customer data into the CRM system. Ensure that customer information is accurately imported and accessible within the system.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new()
                                {
                                    Title = "Enhance Contact Management Functionality",
                                    Description = "<p>Implement improvements to the contact management functionality in the CRM system. Enhance features such as contact creation, updating, and segmentation for better organization.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = demoDevId
                                },
                                new()
                                {
                                    Title = "Implement Advanced Reporting Module",
                                    Description = "<p>Develop and implement an advanced reporting module within the CRM system. Provide users with comprehensive reporting tools to analyze customer data and generate insights.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Fix Urgent User Permission Issue",
                                    Description = "<p>Address and fix urgent issues related to user permissions in the CRM system. Ensure that users have the appropriate access levels and permissions for their roles.</p>",
                                    Created = DateTimeOffset.Now,
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId
                                },
                                new()
                                {
                                    Title = "Enhance Customer Interaction Tracking",
                                    Description = "<p>Improve the tracking of customer interactions within the CRM system. Implement features to log and analyze customer communications, ensuring a more comprehensive customer history.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new()
                                {
                                    Title = "Implement Email Integration for Leads",
                                    Description = "<p>Integrate email functionality for lead management in the CRM system. Allow users to manage leads efficiently by linking relevant email communications to lead records.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },
                                new()
                                {
                                    Title = "Fix Calendar Synchronization Error",
                                    Description = "<p>Address and fix errors occurring in the synchronization of calendars within the CRM system. Ensure accurate and real-time updates across user calendars.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Implement Social Media Integration",
                                    Description = "<p>Integrate social media platforms into the CRM system for enhanced customer engagement. Allow users to view and interact with customer social media profiles directly within the system.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new()
                                {
                                    Title = "Resolve Customer Record Duplication Issue",
                                    Description = "<p>Investigate and resolve issues related to the duplication of customer records in the CRM system. Implement measures to prevent and correct duplicate customer entries.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },
                                new()
                                {
                                    Title = "Enhance User Experience in CRM Dashboard",
                                    Description = "<p>Improve the user experience in the CRM system's dashboard. Implement a more intuitive and user-friendly design for better navigation and data visibility.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Fix Urgent CRM System Performance Issue",
                                    Description = "<p>Address and fix urgent performance issues affecting the CRM system. Optimize system performance to ensure smooth and efficient operations.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new()
                                {
                                    Title = "Implement Automated Customer Surveys",
                                    Description = "<p>Implement automated customer surveys within the CRM system. Gather feedback from customers to improve services and overall customer satisfaction.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },
                                new()
                                {
                                    Title = "Resolve Urgent Email Notification Issue",
                                    Description = "<p>Investigate and resolve urgent issues related to email notifications within the CRM system. Ensure that users receive timely and accurate email notifications for important events.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityLow,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeDefect,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },
                                new()
                                {
                                    Title = "Enhance CRM System Data Security",
                                    Description = "<p>Implement enhanced security measures for data within the CRM system. Ensure that customer information is secure and protected from unauthorized access.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityMedium,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeEnhancement,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },
                                new()
                                {
                                    Title = "Implement Customer Segmentation Feature",
                                    Description = "<p>Develop and implement a customer segmentation feature within the CRM system. Allow users to categorize and target specific customer groups for personalized interactions.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusNew,
                                    TicketTypeId = typeChangeRequest,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },
                                new()
                                {
                                    Title = "Fix Urgent Contact Sync Error with External Systems",
                                    Description = "<p>Address and fix urgent errors in contact synchronization with external systems. Ensure seamless data exchange and synchronization with external platforms.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusDev,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                // resolved tickets
                                new()
                                {
                                    Title = "Investigate Contact Import Errors",
                                    Description = "<p>Investigate and resolve errors occurring during the import of contact data into the CRM system. Ensure accurate and seamless data import for effective customer management.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                new()
                                {
                                    Title = "Enhance Lead Management Interface",
                                    Description = "<p>Implement improvements to the lead management interface in the CRM system. Enhance features for creating, updating, and segmenting leads for better organisation.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },

                                new()
                                {
                                    Title = "Implement Customer Segmentation",
                                    Description = "<p>Develop and implement a customer segmentation feature within the CRM system. Allow users to categorise and target specific customer groups for personalised interactions.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                new()
                                {
                                    Title = "Resolve Urgent User Permission Issues",
                                    Description = "<p>Address and fix urgent issues related to user permissions in the CRM system. Ensure that users have the appropriate access levels and permissions for their roles.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },

                                new()
                                {
                                    Title = "Enhance Email Integration for Leads",
                                    Description = "<p>Integrate email functionality for lead management in the CRM system. Allow users to manage leads efficiently by linking relevant email communications to lead records.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = marcusDonovanId,
                                    DeveloperUserId = demoDevId
                                },

                                new()
                                {
                                    Title = "Fix Calendar Synchronisation Errors",
                                    Description = "<p>Address and fix errors occurring in the synchronisation of calendars within the CRM system. Ensure accurate and real-time updates across user calendars.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityHigh,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = demoSubmitterId,
                                    DeveloperUserId = liamHarrisonId
                                },

                                new()
                                {
                                    Title = "Implement Social Media Integration",
                                    Description = "<p>Integrate social media platforms into the CRM system for enhanced customer engagement. Allow users to view and interact with customer social media profiles directly within the system.</p>",
                                    Created = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ResolvedDate = DateTimeOffset.Now.AddDays(-random.Next(1, 30)),
                                    ProjectId = crmId,
                                    TicketPriorityId = priorityUrgent,
                                    TicketStatusId = statusResolved,
                                    TicketTypeId = typeNewDev,
                                    OwnerUserId = ethanMarshallId,
                                    DeveloperUserId = ameliaWilsonId
                                },

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

        public static async Task SeedDefaultNotificationsAsync(ApplicationDbContext context)
        {
            var random = new Random();

            // get member ids
            string demoAdminId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Admin")?.Id ?? throw new Exception("Failed to get demoAdminId");
            string demoPmId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Project-Manager")?.Id ?? throw new Exception("Failed to get demoPmId");
            string demoDevId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Developer")?.Id ?? throw new Exception("Failed to get demoDevId");
            string demoSubmitterId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Submitter")?.Id ?? throw new Exception("Failed to get demoSubmitterId");
            string ethanMarshallId = context.Users.FirstOrDefault(p => p.FirstName == "Ethan" && p.LastName == "Marshall")?.Id ?? throw new Exception("Failed to get ethanMarshallId");

            // get ticket ids
            int ticketId1 = context.Tickets.Where(t => t.Title == "Add Comment Section to Blog Posts").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId1");
            int ticketId2 = context.Tickets.Where(t => t.Title == "Enhance Course Enrollment Process").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId2");

            try
            {
                var notifications = new List<Notification>
                {
                    new()
                    {
                        Title = "Ethan Marshall joined the team",
                        Message = "<p>Ethan Marshall accepted your invite and joined the team as a submitter.</p>",
                        Created = DateTimeOffset.Now.AddMonths(-1),
                        RecipientId = demoAdminId,
                    },

                    new()
                    {
                        Title = "Thanks for the invite",
                        Message = "<p>Thank you for the opportunity. I'll get to work right away. 🤓</p>",
                        Created = DateTimeOffset.Now.AddMonths(-1).AddDays(1),
                        RecipientId = demoAdminId,
                        SenderId = ethanMarshallId,
                    },

                    new()
                    {
                        Title = "New Project Assignment",
                        Message = "<p>You have been assigned as the Project Manager for: Online Learning Platform.</p>",
                        Created = DateTimeOffset.Now.AddMonths(-1).AddDays(2),
                        RecipientId = demoPmId,
                    },

                    new()
                    {
                        Title = "New Ticked Assignment",
                        Message = "<p>You have been assigned as the developer on ticket: Enhance Course Enrollment Process.</p>",
                        TicketId = ticketId2,
                        Created = DateTimeOffset.Now.AddDays(-6),
                        RecipientId = demoDevId,
                    },

                    new()
                    {
                        Title = "Completed Ticket 🚀",
                        Message = "<p>Hi. Managed to get this one wrapped up quicker than expected 😎. Do you need me for the meeting this afternoon?</p>",
                        TicketId = ticketId2,
                        Created = DateTimeOffset.Now.AddDays(-2),
                        RecipientId = demoPmId,
                        SenderId = demoDevId,
                    },

                    new()
                    {
                        Title = "New Project Assignment",
                        Message = "<p>You have been assigned to the project: Customer Relationship Management (CRM) System.</p>",
                        Created = DateTimeOffset.Now.AddMonths(-1).AddDays(7),
                        RecipientId = demoSubmitterId,
                    },
                };

                var dbNotifications = context.Notifications
                    .Select(n => new { n.Title, n.Message, n.RecipientId })
                    .ToList();

                await context.Notifications.AddRangeAsync(notifications.Where(n => !dbNotifications.Any(dbn => dbn.Title == n.Title && dbn.Message == n.Message && dbn.RecipientId == n.RecipientId)));

                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Notifications.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketHistoriesAsync(ApplicationDbContext context)
        {
            // get member ids
            string demoAdminId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Admin")?.Id ?? throw new Exception("Failed to get demoAdminId");
            string demoPmId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Project-Manager")?.Id ?? throw new Exception("Failed to get demoPmId");
            string demoDevId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Developer")?.Id ?? throw new Exception("Failed to get demoDevId");
            string demoSubmitterId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Submitter")?.Id ?? throw new Exception("Failed to get demoSubmitterId");
            string oliviaJonesId = context.Users.FirstOrDefault(p => p.FirstName == "Olivia" && p.LastName == "Jones")?.Id ?? throw new Exception("Failed to get oliviaJonesId");
            string sophieRobertsId = context.Users.FirstOrDefault(p => p.FirstName == "Sophie" && p.LastName == "Roberts")?.Id ?? throw new Exception("Failed to get sophieRobertsId");

            // get ticket ids
            int ticketId1 = context.Tickets.Where(t => t.Title == "Implement Image Gallery Feature").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId1");
            int ticketId2 = context.Tickets.Where(t => t.Title == "Enhance Course Enrollment Process").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId2");

            try
            {
                var histories = new List<TicketHistory>
                {
                    new()
                    {
                        TicketId = ticketId1,
                        Property = "Developer",
                        OldValue = "Unassigned",
                        NewValue = "Sophie Roberts",
                        Created = DateTimeOffset.Now.AddDays(-6),
                        UserId = oliviaJonesId,
                        Description = "New ticket developer: Sophie Roberts",
                    },

                    new()
                    {
                        TicketId = ticketId1,
                        Property = "Status",
                        OldValue = "New",
                        NewValue = "Development",
                        Created = DateTimeOffset.Now.AddDays(-6),
                        UserId = oliviaJonesId,
                        Description = "Ticket status changed from 'New' to 'Development'",
                    },

                    new()
                    {
                        TicketId = ticketId1,
                        Property = "TicketComment",
                        OldValue = "",
                        NewValue = "",
                        Created = DateTimeOffset.Now.AddDays(-6),
                        UserId = sophieRobertsId,
                        Description = "Hi. I've started working on this ticket. I'll keep you updated on my progress."
                    },

                    new()
                    {
                        TicketId = ticketId1,
                        Property = "TicketComment",
                        OldValue = "",
                        NewValue = "",
                        Created = DateTimeOffset.Now.AddDays(-5),
                        UserId = oliviaJonesId,
                        Description = "Thanks for the update. Let me know if you need any help."
                    },
                };

                var dbHistories = context.TicketHistories
                    .Select(h => new { h.TicketId, h.Description, h.UserId })
                    .ToList();

                await context.TicketHistories.AddRangeAsync(histories.Where(h => !dbHistories.Any(dbh => dbh.TicketId == h.TicketId && dbh.Description == h.Description && dbh.UserId == h.UserId)));

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Histories.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketCommentsAsync(ApplicationDbContext context)
        {
            // get member ids
            string demoAdminId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Admin")?.Id ?? throw new Exception("Failed to get demoAdminId");
            string demoPmId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Project-Manager")?.Id ?? throw new Exception("Failed to get demoPmId");
            string demoDevId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Developer")?.Id ?? throw new Exception("Failed to get demoDevId");
            string demoSubmitterId = context.Users.FirstOrDefault(p => p.FirstName == "Demo" && p.LastName == "Submitter")?.Id ?? throw new Exception("Failed to get demoSubmitterId");
            string oliviaJonesId = context.Users.FirstOrDefault(p => p.FirstName == "Olivia" && p.LastName == "Jones")?.Id ?? throw new Exception("Failed to get oliviaJonesId");
            string sophieRobertsId = context.Users.FirstOrDefault(p => p.FirstName == "Sophie" && p.LastName == "Roberts")?.Id ?? throw new Exception("Failed to get sophieRobertsId");

            // get ticket ids
            int ticketId1 = context.Tickets.Where(t => t.Title == "Implement Image Gallery Feature").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId1");
            int ticketId2 = context.Tickets.Where(t => t.Title == "Enhance Course Enrollment Process").FirstOrDefault()?.Id ?? throw new Exception("Failed to get ticketId2");

            try
            {
                var comments = new List<TicketComment>()
                {
                    new()
                    {
                        TicketId = ticketId1,
                        Comment = "Hi. I've started working on this ticket. I'll keep you updated on my progress.",
                        Created = DateTimeOffset.Now.AddDays(-6),
                        UserId = sophieRobertsId,
                    },

                    new()
                    {
                        TicketId = ticketId1,
                        Comment = "Thanks for the update. Let me know if you need any help.",
                        Created = DateTimeOffset.Now.AddDays(-5),
                        UserId = oliviaJonesId,
                    }
                };

                var dbComments = context.TicketComments
                    .Select(c => new { c.UserId, c.Comment, c.TicketId })
                    .ToList();

                await context.TicketComments.AddRangeAsync(comments.Where(c => !dbComments.Any(cdb => cdb.UserId == c.UserId && cdb.Comment == c.Comment && cdb.TicketId == c.TicketId)));

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Comments.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }
    }
}
