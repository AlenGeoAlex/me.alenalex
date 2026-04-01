using Bloggi.Backend.Api.Database;
using Bloggi.Backend.Api.Database.Glossary;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Database.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bloggi.Backend.Api.Web.Database.DbContext;

public class BloggiDbContext : Microsoft.EntityFrameworkCore.DbContext, IBloggiDbContext
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostHead> PostHeads { get; set; }
    public DbSet<PostBlock> PostBlocks { get; set; }
    public DbSet<PostRevision> PostRevisions { get; set; }
    public DbSet<PostMeta> PostMetas { get; set; }
    public DbSet<Tags> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<PostFile> PostFiles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Glossary> Glossary { get; set; }

    public BloggiDbContext(DbContextOptions<BloggiDbContext> options) : base(options)
    {
    }

    public BloggiDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IEntity).Assembly);
    }
}