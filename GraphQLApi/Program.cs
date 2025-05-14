using GraphQLApi.Data;
using GraphQLApi.Schema;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Auth0 configuration
var domain = builder.Configuration["Auth0:Domain"];
var audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{domain}/";
        options.Audience = audience;

        // Optional: Validate token even if no audience match
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{domain}/",
            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// ====== 2. Add CORS ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ====== 3. GraphQL setup ======
builder.Services
    .AddGraphQLServer()
    .AddAuthorization() // <- This enables [Authorize] attribute
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>() // 👈 Add this
    .AddInMemorySubscriptions();

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseRouting(); // optional in minimal hosting model, but fine

// 1. Enable CORS *before* auth
app.UseCors("AllowReactApp");

// 2. Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

// 3. Map endpoints (GraphQL, etc.)
app.MapGraphQL();
app.MapGet("/", () => "GraphQL is running at /graphql");
app.Run();
