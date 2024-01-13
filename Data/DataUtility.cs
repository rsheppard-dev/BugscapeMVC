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
            await SeedDefaultTicketsAsync(dbContextSvc);
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

                        <p>Welcome to Democorp, where innovation meets technology excellence. As a leading software and web development company, we specialize in crafting digital solutions that empower businesses and elevate online experiences. Our commitment to quality, creativity, and client satisfaction sets us apart in the competitive landscape of technology.</p>

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

        public static async Task SeedDefaultProjectsAsync(ApplicationDbContext context)
        {
            try
            {
                // get project images
                byte[] blogImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/blog.png");
                byte[] onlineLearningImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/onlineLearning.png");
                byte[] ecommerceImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/ecommerce.png");
                byte[] crmImage = await File.ReadAllBytesAsync("wwwroot/images/demo/projects/crm.png");

                // get project priority Ids
                int priorityLow = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Low.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");
                int priorityMedium = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Medium.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
                int priorityHigh = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.High.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
                int priorityUrgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == Priorities.Urgent.ToString())?.Id ?? throw new Exception("Failed to get projectPriorityId");;
           
                IList<Project> projects = new List<Project>() {
                    new()
                    {
                        CompanyId = company1Id,
                        Name = "Online Learning Platform",
                        Description = @"
                        <p>Embark on the creation of a dynamic Online Learning Platform designed to revolutionize education delivery and provide a seamless learning experience. This project focuses on building an engaging platform that empowers educators, facilitates student learning, and fosters collaboration in the digital education landscape.</p>

                        <h2>Key Features:</h2>
                        <ul>
                            <li><strong>Course Management:</strong> Easily create, manage, and organize courses with diverse content types.</li>
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
                        ImageContentType = "image/png"
                    },
                     new()
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

                        <p>Join us on this exciting venture to revolutionize the blogging experience. Let's create a platform that not only reflects your brand but also empowers content creators in the digital age!</p>
                        ",
                        StartDate = DateTime.Now.AddDays(-7),
                        EndDate = DateTime.Now.AddDays(-7).AddMonths(3),
                        ProjectPriorityId = priorityMedium,
                        ImageFileData = blogImage,
                        ImageFileName = "blog.png",
                        ImageContentType = "image/png"
                     },
                     new()
                     {
                        CompanyId = company1Id,
                        Name = "E-commerce Website",
                        Description = @"
                        <p>Embark on the journey of creating a powerful and user-friendly e-commerce website that not only showcases your products but also provides a seamless shopping experience for your customers. This project is focused on building a robust online store that reflects your brand identity, attracts customers, and drives sales in the competitive digital marketplace.</p>

                        <h2>Key Features:</h2>
                        <ul>
                            <li><strong>Product Catalog:</strong> Showcase your products with high-quality images, detailed descriptions, and categorization for easy navigation.</li>
                            <li><strong>Shopping Cart and Checkout:</strong> Implement a secure and user-friendly shopping cart system with a seamless checkout process.</li>
                            <li><strong>Payment Gateway Integration:</strong> Integrate popular payment gateways to offer a variety of secure payment options for customers.</li>
                            <li><strong>Responsive Design:</strong> Ensure a consistent and engaging shopping experience across devices with a responsive design.</li>
                            <li><strong>Customer Accounts:</strong> Allow customers to create accounts, track orders, and manage their preferences for personalized shopping.</li>
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
                     },
                     new()
                     {
                        CompanyId = company1Id,
                        Name = "Customer Relationship Management (CRM) System",
                        Description = @"
                        <p>Embark on the creation of a comprehensive Customer Relationship Management (CRM) System designed to empower your business in managing customer interactions, leads, and communication effectively. This project focuses on building a centralized platform that enhances customer relationships, streamlines communication, and provides valuable insights to drive business growth.</p>

                        <h2>Key Features:</h2>
                        <ul>
                            <li><strong>Customer Database:</strong> Centralized storage for customer information, interactions, and preferences.</li>
                            <li><strong>Lead Management:</strong> Efficiently track and manage leads from acquisition to conversion.</li>
                            <li><strong>Communication Tools:</strong> Integrated tools for seamless communication with customers via email, phone, and other channels.</li>
                            <li><strong>Task and Appointment Management:</strong> Schedule tasks, appointments, and follow-ups to ensure timely and organized interactions.</li>
                            <li><strong>Analytics and Reporting:</strong> Generate insightful reports and analytics to understand customer behavior and make informed business decisions.</li>
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
            // Seed Default Admin User
            var defaultUser = new AppUser
            {
                UserName = "scott.miles@democorp.com",
                Email = "scott.miles@democorp.com",
                FirstName = "Scott",
                LastName = "Miles",
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

            // Seed Default Admin User
            defaultUser = new AppUser
            {
                UserName = "michael.brown@democorp.com",
                Email = "michael.brown@democorp.com",
                FirstName = "Michael",
                LastName = "Brown",
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


            // Seed Default Project_Manager User
            defaultUser = new AppUser
            {
                UserName = "olivia.jones@democorp.com",
                Email = "olivia.jones@democorp.com",
                FirstName = "Olivia",
                LastName = "Jones",
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


            // Seed Default Project_Manager User
            defaultUser = new AppUser
            {
                UserName = "natalie.carter@democorp.com",
                Email = "natalie.carter@democorp.com",
                FirstName = "Natalie",
                LastName = "Carter",
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "amelia.wilson@democorp.com",
                Email = "amelia.wilson@democorp.com",
                FirstName = "Amelia",
                LastName = "Wilson",
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


            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "sophie.roberts@democorp.com",
                Email = "sophie.roberts@democorp.com",
                FirstName = "Sophie",
                LastName = "Roberts",
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

            // Seed Default Developer User
            defaultUser = new AppUser
            {
                UserName = "isabella.murray@democorp.com",
                Email = "isabella.murray@democorp.com",
                FirstName = "Isabella",
                LastName = "Murray",
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

            // Seed Default Submitter User
            defaultUser = new AppUser
            {
                UserName = "liam.harrison@democorp.com",
                Email = "liam.harrison@democorp.com",
                FirstName = "Liam",
                LastName = "Harrison",
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


            // Seed Default Submitter User
            defaultUser = new AppUser
            {
                UserName = "eva.jenkins@democorp.com",
                Email = "eva.jenkins@democorp.com",
                FirstName = "Eva",
                LastName = "Jenkins",
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

        public static async Task SeedDefaultTicketsAsync(ApplicationDbContext context)
        {
            //Get project Ids
            int onlineLearningId = (context.Projects.FirstOrDefault(p => p.Name == "Online Learning Platform")?.Id) ?? throw new Exception("Failed to get projectId.");
            int blogId = context.Projects.FirstOrDefault(p => p.Name == "Build a Blog Web Application")?.Id ?? throw new Exception("Failed to get projectId.");
            int ecommerceId = context.Projects.FirstOrDefault(p => p.Name == "E-commerce Website")?.Id ?? throw new Exception("Failed to get projectId.");
            int crmId = context.Projects.FirstOrDefault(p => p.Name == "Customer Relationship Management (CRM) System")?.Id ?? throw new Exception("Failed to get projectId.");

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
                                new Ticket() { Title = "Portfolio Ticket 1", Description = "Ticket details for portfolio ticket 1", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Portfolio Ticket 2", Description = "Ticket details for portfolio ticket 2", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Portfolio Ticket 4", Description = "Ticket details for portfolio ticket 4", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Portfolio Ticket 5", Description = "Ticket details for portfolio ticket 5", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Portfolio Ticket 6", Description = "Ticket details for portfolio ticket 6", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Portfolio Ticket 7", Description = "Ticket details for portfolio ticket 7", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Portfolio Ticket 8", Description = "Ticket details for portfolio ticket 8", Created = DateTimeOffset.Now, ProjectId = onlineLearningId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
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
                                new Ticket() { Title = "Bug Tracker Ticket 1", Description = "Ticket details for bug tracker ticket 1", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 2", Description = "Ticket details for bug tracker ticket 2", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 3", Description = "Ticket details for bug tracker ticket 3", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 4", Description = "Ticket details for bug tracker ticket 4", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 5", Description = "Ticket details for bug tracker ticket 5", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 6", Description = "Ticket details for bug tracker ticket 6", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 7", Description = "Ticket details for bug tracker ticket 7", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 8", Description = "Ticket details for bug tracker ticket 8", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 9", Description = "Ticket details for bug tracker ticket 9", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 10", Description = "Ticket details for bug tracker 10", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 11", Description = "Ticket details for bug tracker 11", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 12", Description = "Ticket details for bug tracker 12", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 13", Description = "Ticket details for bug tracker 13", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 14", Description = "Ticket details for bug tracker 14", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 15", Description = "Ticket details for bug tracker 15", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 16", Description = "Ticket details for bug tracker 16", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 17", Description = "Ticket details for bug tracker 17", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 18", Description = "Ticket details for bug tracker 18", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 19", Description = "Ticket details for bug tracker 19", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 20", Description = "Ticket details for bug tracker 20", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 21", Description = "Ticket details for bug tracker 21", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 22", Description = "Ticket details for bug tracker 22", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 23", Description = "Ticket details for bug tracker 23", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 24", Description = "Ticket details for bug tracker 24", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 25", Description = "Ticket details for bug tracker 25", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 26", Description = "Ticket details for bug tracker 26", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 27", Description = "Ticket details for bug tracker 27", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 28", Description = "Ticket details for bug tracker 28", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 29", Description = "Ticket details for bug tracker 29", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Bug Tracker Ticket 30", Description = "Ticket details for bug tracker 30", Created = DateTimeOffset.Now, ProjectId = ecommerceId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                //MOVIE
                                new Ticket() { Title = "Movie Ticket 1", Description = "Ticket details for movie ticket 1", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 2", Description = "Ticket details for movie ticket 2", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 3", Description = "Ticket details for movie ticket 3", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 4", Description = "Ticket details for movie ticket 4", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 5", Description = "Ticket details for movie ticket 5", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 6", Description = "Ticket details for movie ticket 6", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 7", Description = "Ticket details for movie ticket 7", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 8", Description = "Ticket details for movie ticket 8", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 9", Description = "Ticket details for movie ticket 9", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 10", Description = "Ticket details for movie ticket 10", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 11", Description = "Ticket details for movie ticket 11", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 12", Description = "Ticket details for movie ticket 12", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 13", Description = "Ticket details for movie ticket 13", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() { Title = "Movie Ticket 14", Description = "Ticket details for movie ticket 14", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 15", Description = "Ticket details for movie ticket 15", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 16", Description = "Ticket details for movie ticket 16", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 17", Description = "Ticket details for movie ticket 17", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() { Title = "Movie Ticket 18", Description = "Ticket details for movie ticket 18", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() { Title = "Movie Ticket 19", Description = "Ticket details for movie ticket 19", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() { Title = "Movie Ticket 20", Description = "Ticket details for movie ticket 20", Created = DateTimeOffset.Now, ProjectId = crmId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},

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
