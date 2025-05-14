using GraphQLApi.Data;
using GraphQLApi.Models;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System;


namespace GraphQLApi.Schema
{
    public class Mutation
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ITopicEventSender _eventSender;

        public Mutation(IDbContextFactory<AppDbContext> dbFactory, ITopicEventSender eventSender)
        {
            _dbFactory = dbFactory;
            _eventSender = eventSender;
        }

        [Authorize]
        public async Task<Book> AddBookAsync(string title, string author)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                    throw new GraphQLException("At least one of 'title' or 'author' must be provided.");

                using var context = await _dbFactory.CreateDbContextAsync();

                var existingBook = await context.Books
                    .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower());

                if (existingBook != null)
                    throw new GraphQLException($"Book with title '{title}' already exists.");

                var input = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Author = author,
                    CreatedTime = DateTime.UtcNow,
                };

                context.Books.Add(input);
                await context.SaveChangesAsync();

                //Publish bookAdded event
                await _eventSender.SendAsync(nameof(Subscription.BookAdded), input);

                return input;
            }
            catch (GraphQLException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new GraphQLException("An unexpected error occurred: " + ex.Message);
            }
        }

        [Authorize]
        public async Task<Book> UpdateBookAsync(Guid id, string? title, string? author)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(author))
                    throw new GraphQLException("At least one of 'title' or 'author' must be provided.");

                using var context = await _dbFactory.CreateDbContextAsync();
                var book = await context.Books.FindAsync(id) ?? throw new GraphQLException($"Book with ID {id} not found.");

                var existingBook = await context.Books
                    .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower() && b.Id != id);

                if (existingBook != null)
                    throw new GraphQLException($"Book with title '{title}' already exists.");

                if (!string.IsNullOrWhiteSpace(title))
                    book.Title = title;

                if (!string.IsNullOrWhiteSpace(author))
                    book.Author = author;

                await context.SaveChangesAsync();
                await _eventSender.SendAsync(nameof(Subscription.BookUpdated), book);

                return book;

            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GraphQLException("An unexpected error occurred: " + ex.Message);
            }
        }

        [Authorize]
        public async Task<bool> DeleteBookAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    throw new GraphQLException("Id cannot be empty.");

                using var context = await _dbFactory.CreateDbContextAsync();
                var book = await context.Books.FindAsync(id) ?? throw new GraphQLException($"Book with ID {id} not found.");
                context.Books.Remove(book);
                await context.SaveChangesAsync();
                await _eventSender.SendAsync(nameof(Subscription.BookDeleted), book);

                return true;
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ex.ToString());
            }
        }
    }
}
