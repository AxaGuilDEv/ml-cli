using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ml.Cli.WebApp.Server.Audits;
using Ml.Cli.WebApp.Server.Datasets;
using Ml.Cli.WebApp.Server.Groups.Oidc;
using Ml.Cli.WebApp.Server.Oidc;
using Ml.Cli.WebApp.Server.Groups;
using Ml.Cli.WebApp.Server.Groups.Database.Users;
using Ml.Cli.WebApp.Server.Projects;
using Serilog;
using ConfigureExtension = Ml.Cli.WebApp.Server.Groups.ConfigureExtension;

namespace Ml.Cli.WebApp.Server
{
    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            if (services.IsReadOnly)
            {
                throw new ReadOnlyException($"{nameof(services)} is read only");
            }

            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
            if (serviceDescriptor != null) services.Remove(serviceDescriptor);

            return services;
        }
    }
    [ExcludeFromCodeCoverage]
    public class StartupServer
    {
        public StartupServer(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        private IWebHostEnvironment CurrentEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OidcSettings>(
                Configuration.GetSection(OidcSettings.Oidc));
            services.Configure<DatasetsSettings>(
                Configuration.GetSection(DatasetsSettings.Datasets));
            var oidcSettings = Configuration.GetSection(OidcSettings.Oidc).Get<OidcSettings>();
            var oidcUserSettings = Configuration.GetSection(OidcUserSettings.OidcUser).Get<OidcUserSettings>();
            var httpClientService = services.AddHttpClient(NamedHttpClients.ProxiedClient);
            services.AddMemoryCache();
            if (!string.IsNullOrEmpty(oidcSettings.ProxyUrl))
            {
                httpClientService.ConfigurePrimaryHttpMessageHandler(() => 
                    new SocketsHttpHandler()
                    {
                        UseProxy = true,
                        Proxy =  new WebProxy(oidcSettings.ProxyUrl)
                        {
                            Credentials = CredentialCache.DefaultCredentials
                        }
                    });
            }
                
            services.AddScoped<OidcUserInfoService>();
            services.AddOptions();
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddResponseCaching();
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
            services.ConfigureGroups(Configuration);
            services.ConfigureDatasets(Configuration);
            services.ConfigureProjects(Configuration);
            services.ConfigureServiceAudits(Configuration);
            
             services.AddAuthorization(options =>
                  {
                      var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                          JwtBearerDefaults.AuthenticationScheme);
                      defaultAuthorizationPolicyBuilder =
                          defaultAuthorizationPolicyBuilder
                              .RequireAuthenticatedUser();
                      if (oidcUserSettings.RequireScopes != null)
                      {
                          var requireScopes = oidcUserSettings.RequireScopes.ToArray();
                          if (requireScopes.Length > 0)
                          {
                              defaultAuthorizationPolicyBuilder.RequireScope(oidcUserSettings.RequireScopes.ToArray());
                          }
                      }

                      options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                  });
              
            services.AddSingleton<IDateTime, SystemDateTime>();
            services
                .AddControllersWithViews();

            services.Remove<Local.AnnotationsController>();
            services.Remove<Local.ComparesController>();
            services.Remove<Local.DatasetsController>();
            services.Remove<Local.FilesController>();
            services.Remove<Local.GalleryController>();
            
            var oidcMode = Configuration[$"OidcMode"];
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (oidcMode == "AXA_FRANCE")
                            {
                                var profiles = context.Principal.Identity.GetProfiles();
                                var claims = profiles.Select(profile => new Claim(ClaimTypes.Role, profile));
                                ((ClaimsIdentity)context.Principal.Identity).AddClaims(claims);
                                return Task.CompletedTask;
                            }
                            else
                            {
                                var usersRepository = context.HttpContext.RequestServices.GetRequiredService<UsersRepository>();
                                var nameIdentifier = context.Principal.Identity.GetNameIdentifier();
                                //var userTask = usersRepository.GetUserByNameIdentifierAsync(nameIdentifier);
                                //userTask.Wait();
                                var role = "";//userTask.Result.Role;
                                var roles = new List<string>();
                                if (role == Roles.DataScientist)
                                {
                                    roles.Add(Roles.DataAnnoteur);
                                    roles.Add(Roles.DataScientist);
                                }
                                else if (role == Roles.DataAdministateur)
                                {
                                    roles.Add(Roles.DataScientist);
                                    roles.Add(Roles.DataAnnoteur);
                                    roles.Add(Roles.DataAdministateur);
                                }
                                roles.Add(Roles.DataScientist);
                                roles.Add(Roles.DataAnnoteur);
                                roles.Add(Roles.DataAdministateur);
                            
                                var claims = roles.Select(profile => new Claim(ClaimTypes.Role, profile));
                                ((ClaimsIdentity)context.Principal.Identity).AddClaims(claims);
                            
                                // If your authentication logic is based on users then add your logic here
                                return Task.CompletedTask;
                            }
                        }
                    };
                    var isValidateAudience = !String.IsNullOrEmpty(oidcUserSettings.RequireAudience);
                    if (isValidateAudience)
                    {
                        options.Audience = oidcUserSettings.RequireAudience;
                    }

                    var proxyUrl = oidcSettings.ProxyUrl;
                    if (!string.IsNullOrEmpty(proxyUrl))
                    {
                        var handler = new HttpClientHandler
                        {
                            Proxy = new WebProxy(proxyUrl)
                            {
                                Credentials = CredentialCache.DefaultCredentials
                            }
                        };
                        options.BackchannelHttpHandler = handler;
                    }

                    options.Authority = oidcSettings.Authority;
                    options.RequireHttpsMetadata = oidcSettings.RequireHttpsMetadata;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = isValidateAudience,
                        ValidateLifetime = true,
                        RequireSignedTokens = true
                    };
                });
            
            services.Configure<CorsSettings>(
                Configuration.GetSection(CorsSettings.Cors));
            var corsSettings = GetCorsSettings();
              if (!string.IsNullOrEmpty(corsSettings.Origins))
                  services.AddCors(options =>
                  {
                      options.AddPolicy("CorsPolicy",
                          builder =>
                          {
                              builder.WithOrigins(corsSettings.Origins.Split(";"))
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials()
                                  .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
                          });
                  });

            // In production, the React files will be served from this directory
            if (IsSpaStaticFiles())
            {
                services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build-server"; });
            }
            if (IsSwagger())
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecotag", Version = "v1" });
                });
            }
        }

        private bool IsSwagger()
        {
            return Configuration["SwaggerActive"] == "True";
        }

        private CorsSettings GetCorsSettings()
        {
            var corsSettings = Configuration.GetSection(CorsSettings.Cors).Get<CorsSettings>();
            return corsSettings;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,  IServiceProvider serviceProvider)
        {
            app.UseSerilogRequestLogging();
            app.UseErrorLogging();
            AuditsService.ConfigureAudits(serviceProvider);
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "sameorigin");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                await next();
            });
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var isSpaStaticFiles = IsSpaStaticFiles();
            if (isSpaStaticFiles)
            {
                app.UseStaticFiles();
                app.UseSpaStaticFiles();
            }
            var corsSettings = GetCorsSettings();
            if (!string.IsNullOrEmpty(corsSettings.Origins))
            {
                app.UseCors("CorsPolicy");
            }
            
            app.UseAuthentication();
            if (IsSwagger())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecotag"); });
            }

            app.UseRouting();
            app.UseResponseCaching();
            app.UseAuthorization();
            app.UseCreateUser();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/isalive", async (HttpContext context) =>
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("ok");
                });
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapControllerRoute(
                    "default", "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
            });

            if (isSpaStaticFiles)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    if (env.IsDevelopment())
                    {
                        spa.UseReactDevelopmentServer(npmScript: "start:server");
                    }
                });
            }
        }
        
        private bool IsSpaStaticFiles()
        {
            var isSpaStaticFiles = Configuration.GetSection("SpaStaticFiles").Get<bool>();
            return isSpaStaticFiles;
        }
    }
}