using System.Configuration;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using tX.Data;
using tX.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TxContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TxContext"))
    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .EnableSensitiveDataLogging()
);

builder.Services.AddHangfire(hangfire =>
{
    hangfire.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    hangfire.UseSimpleAssemblyNameTypeSerializer();
    hangfire.UseRecommendedSerializerSettings();
    hangfire.UseColouredConsoleLogProvider();
    hangfire.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HfContext"));
    JobStorage.Current = new PostgreSqlStorage(builder.Configuration.GetConnectionString("HfContext"));
    //hangfire.UseSqlServerStorage(
    //             configuration.GetConnectionString("HangfireConn"),
    //    new SqlServerStorageOptions
    //    {
    //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
    //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
    //        QueuePollInterval = TimeSpan.Zero,
    //        UseRecommendedIsolationLevel = true,
    //        DisableGlobalLocks = true
    //    });

    var server = new BackgroundJobServer(new BackgroundJobServerOptions
    {
        ServerName = "hangfire-test",
    });
});

builder.Services.AddHangfireServer();

IRecurringJobManager manager = new RecurringJobManager();

manager.RemoveIfExists("c2");
manager.AddOrUpdate<IMyRecurringJob>("c2",
   job => job.DoSomethingReentrant(), Cron.Minutely);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<TxContext>();
    context.Database.Migrate();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred creating the DB.");
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHangfireServer();
app.UseHangfireDashboard();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();


app.Run();
