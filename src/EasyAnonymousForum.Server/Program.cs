using EasyAnonymousForum.Extensions.ServiceExtensions;
using FluentValidation.AspNetCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NodaTime.Serialization.SystemTextJson;
using NodaTime;
using System.Text.Json.Serialization;
using EasyAnonymousForum.Server.Extensions.ServiceExtensions;
using EasyAnonymousForum.Server.Exceptions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Add Services
// Add services to the container.
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
});

builder.Services.AddSwaggerGen(c =>
{
    c.MapType<Instant>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<LocalDate>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<LocalDateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<OffsetDateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<ZonedDateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<Duration>(() => new OpenApiSchema { Type = "string", Format = "duration" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins((builder.Configuration.GetValue<string>("AllowedOrigins", "") ?? "").Split(";"))
            .WithHeaders(HeaderNames.Origin, HeaderNames.XRequestedWith, HeaderNames.ContentType, HeaderNames.Accept);
    });
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
    });

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCustomProblemDetails();

builder.Services.AddEndpointsApiExplorer();

// New validators should be registered inside these service extensions.
builder.Services.AddValidators();

builder.Services.AddDataContext(builder.Configuration);
#endregion

#region Build App
var app = builder.Build();

app.Logger.LogInformation("Starting up server");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();
