using GraphQLApi.Models;
using GraphQLApi.Data;
using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Authorization;

public class Query
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public Query(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [Authorize]
    public async Task<List<Book>> GetBooks()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Books.OrderByDescending(x => x.CreatedTime).ToListAsync();
    }

    [Authorize]
    public string Developer => "Rohith R";
}