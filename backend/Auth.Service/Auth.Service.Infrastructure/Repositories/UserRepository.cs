using Auth.Service.Application.Interfaces;
using Auth.Service.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.Service.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        _logger = logger;
    }

    public ApplicationUser? FindByUsername(string username)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_GetUserByUsername", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new ApplicationUser
            {
                Id               = reader.GetString(reader.GetOrdinal("Id")),
                Username         = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash     = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Email            = reader.IsDBNull(reader.GetOrdinal("Email"))            ? null : reader.GetString(reader.GetOrdinal("Email")),
                ExternalProvider = reader.IsDBNull(reader.GetOrdinal("ExternalProvider")) ? null : reader.GetString(reader.GetOrdinal("ExternalProvider")),
                ExternalId       = reader.IsDBNull(reader.GetOrdinal("ExternalId"))       ? null : reader.GetString(reader.GetOrdinal("ExternalId"))
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in {Method} while looking up username '{Username}'.",
                nameof(FindByUsername), username);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in {Method} while looking up username '{Username}'.",
                nameof(FindByUsername), username);
            throw;
        }
    }

    // Returns true when the INSERT succeeds, false on duplicate username.
    public bool TryAddUser(ApplicationUser user)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_CreateUser", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id",              user.Id);
            command.Parameters.AddWithValue("@Username",         user.Username);
            command.Parameters.AddWithValue("@PasswordHash",     user.PasswordHash);
            command.Parameters.AddWithValue("@Email",            (object?)user.Email            ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExternalProvider", (object?)user.ExternalProvider ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExternalId",       (object?)user.ExternalId       ?? DBNull.Value);

            var rowsAffectedParam = new SqlParameter("@RowsAffected", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            command.Parameters.Add(rowsAffectedParam);

            command.ExecuteNonQuery();

            var rowsAffected = (int)rowsAffectedParam.Value;

            return rowsAffected == 1;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in {Method} while adding user '{Username}'.",
                nameof(TryAddUser), user.Username);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in {Method} while adding user '{Username}'.",
                nameof(TryAddUser), user.Username);
            throw;
        }
    }

    public ApplicationUser? FindByEmail(string email)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_GetUserByEmail", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Email", email);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new ApplicationUser
            {
                Id               = reader.GetString(reader.GetOrdinal("Id")),
                Username         = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash     = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Email            = reader.IsDBNull(reader.GetOrdinal("Email"))            ? null : reader.GetString(reader.GetOrdinal("Email")),
                ExternalProvider = reader.IsDBNull(reader.GetOrdinal("ExternalProvider")) ? null : reader.GetString(reader.GetOrdinal("ExternalProvider")),
                ExternalId       = reader.IsDBNull(reader.GetOrdinal("ExternalId"))       ? null : reader.GetString(reader.GetOrdinal("ExternalId"))
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in {Method} for email '{Email}'.",
                nameof(FindByEmail), email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in {Method} for email '{Email}'.",
                nameof(FindByEmail), email);
            throw;
        }
    }

    public ApplicationUser? FindByExternalId(string provider, string externalId)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_GetUserByExternalId", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Provider",   provider);
            command.Parameters.AddWithValue("@ExternalId", externalId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new ApplicationUser
            {
                Id               = reader.GetString(reader.GetOrdinal("Id")),
                Username         = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash     = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Email            = reader.IsDBNull(reader.GetOrdinal("Email"))            ? null : reader.GetString(reader.GetOrdinal("Email")),
                ExternalProvider = reader.IsDBNull(reader.GetOrdinal("ExternalProvider")) ? null : reader.GetString(reader.GetOrdinal("ExternalProvider")),
                ExternalId       = reader.IsDBNull(reader.GetOrdinal("ExternalId"))       ? null : reader.GetString(reader.GetOrdinal("ExternalId"))
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL error in {Method} for provider '{Provider}' / externalId '{ExternalId}'.",
                nameof(FindByExternalId), provider, externalId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in {Method} for provider '{Provider}' / externalId '{ExternalId}'.",
                nameof(FindByExternalId), provider, externalId);
            throw;
        }
    }
}
