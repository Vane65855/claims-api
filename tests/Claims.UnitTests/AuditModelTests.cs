using Claims.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.UnitTests;

public class AuditModelTests
{
    [Fact]
    public void Audit_model_has_no_pending_changes()
    {
        using var db = new AuditContext(new DbContextOptionsBuilder<AuditContext>().UseSqlServer().Options);

        Assert.False(db.Database.HasPendingModelChanges());
    }
}
