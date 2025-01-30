using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebAPIAutores.Data;
using WebAPIAutores.Filters;
using WebAPIAutores.Middlewares;
using WebAPIAutores.Services;
using WebAPIAutores.Utilities;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddControllers(opciones =>
{
    opciones.Filters.Add(typeof(ExceptionFilter));
    opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
}).AddJsonOptions(x => 
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
).AddNewtonsoftJson(); 

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"])),
            ClockSkew = TimeSpan.Zero
        };

        opciones.MapInboundClaims = false;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "WebAPIAutores", 
        Version = "v1",
        Description = "Este es un web api para trabajar con autores y libros",
        Contact = new OpenApiContact
        {
            Name = "Felipe Basini",
            Email = "felipe@hotmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
        },
    });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });

    c.OperationFilter<AgregarParametroHATEOAS>();
    c.OperationFilter<AgregarParametroXVersion>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });

    var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
    c.IncludeXmlComments(rutaXML);
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
});

builder.Services.AddDataProtection();

builder.Services.AddTransient<HashService>();

builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(builder =>
    {
        //builder.AllowAnyOrigin()
        builder.WithOrigins("https://www.apirequest.io")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders(new string[] { "cantidadTotalRegistros" });
    });
});

builder.Services.AddTransient<GeneradorEnlaces>();
builder.Services.AddTransient<HATEOASAutorFilterAttribute>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


var app = builder.Build();

app.UseLogHTTPResponse();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");
});

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
