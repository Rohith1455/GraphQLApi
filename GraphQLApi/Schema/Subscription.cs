using GraphQLApi.Models;

public class Subscription
{
    [Subscribe]
    [Topic]
    public Book BookAdded([EventMessage] Book book) => book;

    [Subscribe]
    [Topic]
    public Book BookUpdated([EventMessage] Book book) => book;

    [Subscribe] 
    [Topic]
    public Book BookDeleted([EventMessage] Book book) => book;
}
