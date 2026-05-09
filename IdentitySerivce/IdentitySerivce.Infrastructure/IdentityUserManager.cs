using IdentitySerivce.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentitySerivce.Infrastructure;

public class IdentityUserManager : UserManager<User>
{
    public IdentityUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, 
        IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : 
        base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        
    }

    public IUserLoginStore<User> UserLoginStore
    {
        get
        {
            return (IUserLoginStore<User>)this.Store;
        }
    }
}