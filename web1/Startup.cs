using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using web1.Models.Blocks;

namespace web1;

public class Startup
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="configuration">The current configuration</param>
    public Startup(IConfiguration configuration)
    {
        _config = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // Service setup
        services.AddPiranha(options =>
        {
            options.UseCms();
            options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
            options.UseImageSharp();
            options.UseManager();
            options.UseTinyMCE();
            options.UseMemoryCache();
            options.UseEF<SQLiteDb>(db =>
                db.UseSqlite(_config.GetConnectionString("piranha")));
            options.UseIdentityWithSeed<IdentitySQLiteDb>(db =>
                db.UseSqlite(_config.GetConnectionString("piranha")));
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApi api, Config config)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        config.CommentsEnabledForPosts = false;
        // Initialize Piranha
        App.Init(api);

        // Build content types
        new ContentTypeBuilder(api)
            .AddAssembly(typeof(Startup).Assembly)
            .Build()
            .DeleteOrphans();
        App.Blocks.Register<YoutubeBlock>();
        App.Blocks.Register<ActionBlock>();
        App.Blocks.Register<EventBlock>();
        App.MediaTypes.Documents.Add(".css", "text/css");
        // Configure Tiny MCE
        EditorConfig.FromFile("editorconfig.json");

        // Middleware setup
        app.UsePiranha(options =>
        {
            options.UseManager();
            options.UseTinyMCE();
            options.UseIdentity();
        });
    }
}
