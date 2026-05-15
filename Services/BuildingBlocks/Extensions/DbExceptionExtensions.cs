using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildingBlocks.Extensions;

public static class DbExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this Exception ex, string? constraintName = null)
    {
        var baseException = ex.GetBaseException();

        if (baseException is PostgresException pgEx)
        {
            if (pgEx.SqlState != "23505")
                return false;

            if (constraintName != null)
                return string.Equals(pgEx.ConstraintName, constraintName, StringComparison.OrdinalIgnoreCase);

            return true;
        }

        return false;
    }

    public static bool IsForeignKeyViolation(this Exception ex)
    {
        var baseException = ex.GetBaseException();
        return baseException is PostgresException { SqlState: "23503" };
    }
}
