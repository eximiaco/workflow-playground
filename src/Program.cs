using Microsoft.EntityFrameworkCore;
using Serilog;
using WorkflowCore.Interface;
using WorkflowPlayground.Domain;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;
using WorkflowPlayground.Workflow;
using WorkflowPlayground.Workflow.Steps;

var builder = WebApplication.CreateBuilder(args);
var baseUrl = builder.Configuration.GetValue<string>("ExternalServicesBaseUrl")!;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger(); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddSerilog();

builder.Services.AddDbContext<PlaygroundDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

builder.Services.AddWorkflow(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("WorkflowDatabase"), true, true);
    //x.UseRedisLocking("localhost:6379");
});

builder.Services.AddScoped<ReserveStockStep>();
builder.Services.AddScoped<ProcessPaymentStep>();
builder.Services.AddScoped<ConfirmStockStep>();
builder.Services.AddScoped<ScheduleShippingStep>();
builder.Services.AddScoped<NotifyCostumerStep>();
builder.Services.AddScoped<FinishOrderStep>();
builder.Services.AddScoped<UndoProcessingStep>();
builder.Services.AddWorkflowStepMiddleware<PrintStepMiddleware>();
builder.Services.AddWorkflowMiddleware<PrintStartWorkflowMiddleware>();
builder.Services.AddWorkflowMiddleware<PrintEndWorkflowMiddleware>();

builder.Services.AddHttpClient<PaymentService>(x => x.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<StockService>(x => x.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<ShippingService>(x => x.BaseAddress = new Uri(baseUrl));
builder.Services.AddScoped<EmailService>();

builder.Services.AddHostedService<OrderProcessingBackgroundService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

app.Services.GetRequiredService<IWorkflowHost>().RegisterWorkflow<OrderWorkflowDefinition, OrderWorflowData>();
app.Services.GetRequiredService<IWorkflowHost>().Start();

using var scope = app.Services.CreateScope();
scope.ServiceProvider.GetRequiredService<PlaygroundDbContext>().Database.MigrateAsync().Wait();

app.MapControllers();

app.Run();
