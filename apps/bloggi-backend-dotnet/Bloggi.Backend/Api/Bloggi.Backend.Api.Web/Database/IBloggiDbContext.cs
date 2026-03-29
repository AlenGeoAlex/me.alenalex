using Bloggi.Backend.Api.Database.Glossary;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Database.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bloggi.Backend.Api.Web.Database;

public interface IBloggiDbContext
{
    #region Posts

    DbSet<Post> Posts { get; }
    
    DbSet<PostHead> PostHeads { get; }
    
    DbSet<PostBlock> PostBlocks { get; }
    
    DbSet<PostRevision> PostRevisions { get; }
    
    DbSet<PostMeta> PostMetas { get; }
    
    DbSet<Tags> Tags { get; }
    
    DbSet<PostTag> PostTags { get; }

    #endregion

    #region Users

    DbSet<User> Users { get; }

    #endregion

    #region Glossary

    DbSet<Glossary> Glossary { get; }

    #endregion
    
    
    DatabaseFacade Database { get;  }
    

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}