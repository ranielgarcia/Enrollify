using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoleConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RolePermissionConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomTypeConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.UserConfigs;

namespace Enrollify.Infrastructure.Data;

public class EnrollifyDbContext: DbContext
{
    public EnrollifyDbContext(DbContextOptions<EnrollifyDbContext> options) : base(options)
    {
    }




    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room> ();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();





    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges() =>
          SaveChangesAsync().GetAwaiter().GetResult();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.RegisterAllInRoomVogenEfCoreConverters();
        configurationBuilder.RegisterAllInRoleVogenEfCoreConverters();
        configurationBuilder.RegisterAllInUserVogenEfCoreConverters();
        configurationBuilder.RegisterAllInRoomTypeVogenEfCoreConverters();
        configurationBuilder.RegisterAllInPermissionVogenEfCoreConverters();
    }
}
