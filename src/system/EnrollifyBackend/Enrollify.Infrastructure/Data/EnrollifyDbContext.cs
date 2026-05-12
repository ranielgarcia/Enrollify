using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.BuildingAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.Data.Config;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicYearConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.BuildingConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.CollegeConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.CourseConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.CurriculumConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.DepartmentConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.PermissionScopeConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoleConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomTypeConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectEquivalenceGroupConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.TeacherConfigs;
using Enrollify.Infrastructure.Data.Config.AggregateConfigs.UserConfigs;
using SmartEnum.EFCore;

namespace Enrollify.Infrastructure.Data;

public class EnrollifyDbContext: DbContext
{
    public EnrollifyDbContext(DbContextOptions<EnrollifyDbContext> options) : base(options)
    {
    }

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<College> Colleges => Set<College>();
    public DbSet<Room> Rooms => Set<Room> ();

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Course> Courses => Set<Course>(); 
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Curriculum> Curriculums => Set<Curriculum>();
    public DbSet<SubjectEquivalenceGroup> SubjectEquivalenceGroups => Set<SubjectEquivalenceGroup>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.AddSoftDeleteQueryFilter();
    }

    public override int SaveChanges() =>
          SaveChangesAsync().GetAwaiter().GetResult();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.ConfigureSmartEnum();

        // Do not forget to add using statement when adding new entry
        configurationBuilder.RegisterAllInCollegeVogenEfCoreConverters();
        configurationBuilder.RegisterAllInBuildingVogenEfCoreConverters();
        configurationBuilder.RegisterAllInPermissionScopeVogenEfCoreConverters();
        configurationBuilder.RegisterAllInRoomVogenEfCoreConverters();
        configurationBuilder.RegisterAllInRoleVogenEfCoreConverters();
        configurationBuilder.RegisterAllInUserVogenEfCoreConverters();
        configurationBuilder.RegisterAllInRoomTypeVogenEfCoreConverters();
        configurationBuilder.RegisterAllInDepartmentVogenEfCoreConverters();
        configurationBuilder.RegisterAllInCourseVogenEfCoreConverters();
        configurationBuilder.RegisterAllInSubjectVogenEfCoreConverters();
        configurationBuilder.RegisterAllInCurriculumVogenEfCoreConverters();
        configurationBuilder.RegisterAllInSubjectEquivalenceGroupVogenEfCoreConverters();
        configurationBuilder.RegisterAllInTeacherVogenEfCoreConverters();
        configurationBuilder.RegisterAllInSharedValueObjectsVogenEfCoreConverters();
        configurationBuilder.RegisterAllInAcademicYearVogenEfCoreConverters();
        configurationBuilder.RegisterAllInClassSectionVogenEfCoreConverters();

    }
}
