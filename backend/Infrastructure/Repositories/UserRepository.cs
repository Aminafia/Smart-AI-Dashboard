/*
    Application = Manager
    UserRepository = Worker
    Database = Storage 

    DbContext = connection + translator to database
*/

using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implements IUserRepository using Entity Framework Core to interact with the database.
/// This class is responsible for fetching and manipulating user data in the database.
/// It serves as the bridge between the Application layer (which defines the IUserRepository interface) and the database (via AppDbContext).
/// The UserRepository class provides methods to add a new user, retrieve all users, check if an email exists, and get a user by email. These methods are used by the Application layer to perform user-related operations without needing to know the details of how the data is stored or accessed.
/// The UserRepository class is registered in the dependency injection container and injected into services that require user data access, such as the LoginCommandHandler for authentication and user management features. 
/// This separation of concerns allows for a clean architecture where the Application layer defines the business logic and interfaces, while the Infrastructure layer handles the actual data access implementation.
/// </summary>
public class UserRepository : IUserRepository
{
    //The UserRespository class needs access to Database, so it takes DbContext as input and stores it for later use. 
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    //AddUserAsync(User, CancellationToken) - POST /api/users
    // 1. AppDbContext.Users DbSet.AddAsync() adds input user to DB
    // 2. AppDbContext.SaveChangesAsync() actually executes the query and INSERTS user to db
    // Behind the scenes, an INSERT INTO Users (columns) VALUES (user properties) query is executed in the database to persist the new user record.

    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken); // Adds the user entity to the Users DbSet, marking it for insertion into the database. However, at this point, the user is not yet saved to the database.
        await _context.SaveChangesAsync(cancellationToken); // Generally, no database interaction will be performed until DbContext.SaveChanges() is called.
    }
    //GetAllUsersAsync(CancellationToken) - GET /api/users
    // 1. AppDbContext.Users DbSet.ToListAsync() 
    //   a. go to Users table in Db, 
    //   b. fetches all rows, 
    //   c. converts each row into User entity, 
    //   d. returns list of User entities to caller (IUserRepository interface in Application layer)
    // Behind the scenes, SELECT * FROM Users is executed in db and results are mapped to List<User>. 
    public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    // EmailExistsAsync(email, CancellationToken) - used in CreateUserCommandHandler to check if email is already registered
    // 1. AppDbContext.Users DbSet.AnyAsync()
    //   a. go to Users table in Db,
    //   b. check if any Email matches input email,
    //   c. return true if found, false if not found
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(
            u => u.Email == email,
            cancellationToken
        );
    }

    // GetUserByEmailAsync(email, CancellationToken) - used in LoginCommandHandler to fetch user by email for authentication
    // 1. AppDbContext.Users DbSet.FirstOrDefaultAsync()
    //   a. go to Users table in Db, 
    //   b. find first row where Email matches input email,
    //   c. convert that row into User entity,
    //   d. return User entity if found, or null if not found
    // Behind the scenes, SELECT TOP 1 * FROM Users WHERE Email = 'input email' is executed in db and result is mapped to User entity.
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
