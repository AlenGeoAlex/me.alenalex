using FluentMigrator;

namespace Me.AlenAlex.Bloggi.Database.Migrations.Maintenance.Locks;

public partial class AdvisoryLock
{
    [Maintenance(MigrationStage.AfterAll, TransactionBehavior.None)]
    public class ReleaseLock : Migration
    {
        public override void Up()
        {
            Execute.Sql($"SELECT pg_advisory_unlock({LockId});");
        }

        public override void Down() { }
    }
}