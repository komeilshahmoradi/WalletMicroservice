using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Wallet.Application.Helper;

public static class DatabaseExceptionHelper
{
  public static bool IsUniqueViolation(DbUpdateException exception)
  {
    if (exception.InnerException is null)
    {
      return false;
    }

    // PostgreSQL
    if (exception.InnerException is PostgresException postgresException)
    {
      return postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }

    // SQL Server
    if (exception.InnerException is SqlException sqlException)
    {
      // Unique Constraint , Unique Index
      var isUniqueError = sqlException.Number == 2627 || sqlException.Number == 2601;

      return isUniqueError;
    }

    return false;
  }

  public static bool IsUniqueViolation(DbUpdateException exception, string constraintName)
  {
    if (exception.InnerException is null)
    {
      return false;
    }

    // PostgreSQL
    if (exception.InnerException is PostgresException postgresException)
    {
      return postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
             string.Equals(postgresException.ConstraintName, constraintName, StringComparison.Ordinal);
    }

    // SQL Server
    if (exception.InnerException is SqlException sqlException)
    {
      // Unique Constraint , Unique Index
      var isUniqueError = sqlException.Number == 2627 || sqlException.Number == 2601;

      return isUniqueError &&
             sqlException.Message.Contains(constraintName, StringComparison.OrdinalIgnoreCase);
    }

    return false;
  }
}
