using Drawing.Shared.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddDefaultCors(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.MapReverseProxy();

app.Run();
