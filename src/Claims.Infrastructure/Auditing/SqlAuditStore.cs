using Claims.Application.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Auditing;

public sealed class SqlAuditStore(IDbContextFactory<AuditContext> contexts, ILogger<SqlAuditStore> logger) : IAuditStore
{
    public async Task SaveAsync(AuditEvent auditEvent)
    {
        await using var db = await contexts.CreateDbContextAsync();
        db.Record(auditEvent);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            logger.LogWarning("Audit event {EventId} was already stored.", auditEvent.EventId);
        }
    }
}
