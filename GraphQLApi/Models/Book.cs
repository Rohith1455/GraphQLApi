using System;

namespace GraphQLApi.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public DateTime CreatedTime { get; set; }
}

