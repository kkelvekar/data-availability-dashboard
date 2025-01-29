using DaDashboard.GraphQL.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Register Hot Chocolate services with your schema configuration
builder.Services
    .AddGraphQLServer()
    // This tells Hot Chocolate to look at the Query class for fields
    .AddQueryType<Query>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//app.UseAuthorization();

// 2. Map the GraphQL endpoint
app.MapGraphQL("/graphql");

app.Run();
