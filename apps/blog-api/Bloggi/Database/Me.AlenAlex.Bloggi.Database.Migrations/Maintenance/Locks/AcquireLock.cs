using FluentMigrator;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Maintenance.Locks;

public partial class AdvisoryLock
{
    [Maintenance(MigrationStage.BeforeAll, TransactionBehavior.None)]
    public class AcquireLock : Migration
    {
        public override void Up()
        {
            Execute.Sql($"SELECT pg_advisory_lock({LockId});");
        }

        public override void Down() { }
    }
}