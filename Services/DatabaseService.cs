using MySqlConnector;
using NeedSystem.Configs;
using NeedSystem.Models;

namespace NeedSystem.Services;

public class DatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly bool _enabled;

    public DatabaseService(DatabaseConfig config)
    {
        _config = config;
        _enabled = config.Enabled;

        if (_enabled)
        {
            LogInfo("DatabaseService initialized and enabled");
        }
        else
        {
            LogInfo("DatabaseService initialized but disabled in configuration");
        }
    }

    public async Task InitializeDatabase()
    {
        if (!_enabled)
        {
            LogInfo("Database is disabled, skipping initialization");
            return;
        }

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            await CreateTable(connection);
            LogInfo("Database connection established and table created");
        }
        catch (Exception ex)
        {
            LogError($"Failed to initialize database: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTable(MySqlConnection connection)
    {
        var createTableQuery = @"
            CREATE TABLE IF NOT EXISTS need_notifications (
                uuid VARCHAR(36) PRIMARY KEY UNIQUE NOT NULL,
                server_address VARCHAR(64) NOT NULL,
                connected_players INT NOT NULL,
                max_players INT NOT NULL,
                map_name VARCHAR(64) NOT NULL,
                timestamp DATETIME NOT NULL,
                requested_by VARCHAR(128) NOT NULL,
                INDEX idx_timestamp (timestamp),
                INDEX idx_server (server_address)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        using var cmd = new MySqlCommand(createTableQuery, connection);
        await cmd.ExecuteNonQueryAsync();
        LogInfo("Notifications table created/verified");
    }

    public async Task<bool> SaveNotification(NotificationRecord record)
    {
        if (!_enabled)
        {
            return false;
        }

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var insertQuery = @"
                INSERT INTO need_notifications 
                (uuid, server_address, connected_players, max_players, map_name, timestamp, requested_by)
                VALUES 
                (@uuid, @server_address, @connected_players, @max_players, @map_name, @timestamp, @requested_by)";

            using var cmd = new MySqlCommand(insertQuery, connection);
            cmd.Parameters.AddWithValue("@uuid", record.Uuid);
            cmd.Parameters.AddWithValue("@server_address", record.ServerAddress);
            cmd.Parameters.AddWithValue("@connected_players", record.ConnectedPlayers);
            cmd.Parameters.AddWithValue("@max_players", record.MaxPlayers);
            cmd.Parameters.AddWithValue("@map_name", record.MapName);
            cmd.Parameters.AddWithValue("@timestamp", record.Timestamp);
            cmd.Parameters.AddWithValue("@requested_by", record.RequestedBy);

            await cmd.ExecuteNonQueryAsync();
            LogInfo($"Notification saved to database - UUID: {record.Uuid}");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error saving notification to database: {ex.Message}");
            return false;
        }
    }

    public async Task<List<NotificationRecord>> GetRecentNotifications(int limit = 10)
    {
        if (!_enabled)
        {
            return new List<NotificationRecord>();
        }

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var selectQuery = @"
                SELECT uuid, server_address, connected_players, max_players, map_name, timestamp, requested_by
                FROM need_notifications
                ORDER BY timestamp DESC
                LIMIT @limit";

            using var cmd = new MySqlCommand(selectQuery, connection);
            cmd.Parameters.AddWithValue("@limit", limit);

            var records = new List<NotificationRecord>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(new NotificationRecord
                {
                    Uuid = reader.GetString("uuid"),
                    ServerAddress = reader.GetString("server_address"),
                    ConnectedPlayers = reader.GetInt32("connected_players"),
                    MaxPlayers = reader.GetInt32("max_players"),
                    MapName = reader.GetString("map_name"),
                    Timestamp = reader.GetDateTime("timestamp"),
                    RequestedBy = reader.GetString("requested_by")
                });
            }

            return records;
        }
        catch (Exception ex)
        {
            LogError($"Error retrieving notifications from database: {ex.Message}");
            return new List<NotificationRecord>();
        }
    }

    public async Task<int> GetNotificationCount()
    {
        if (!_enabled)
        {
            return 0;
        }

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var countQuery = "SELECT COUNT(*) FROM need_notifications";
            using var cmd = new MySqlCommand(countQuery, connection);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            LogError($"Error getting notification count: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> TestConnection()
    {
        if (!_enabled)
        {
            LogInfo("Database is disabled, skipping connection test");
            return false;
        }

        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            LogInfo("Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Database connection test failed: {ex.Message}");
            return false;
        }
    }

    private MySqlConnection GetConnection()
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Database configuration is null");
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = _config.Host,
            Port = _config.Port,
            UserID = _config.User,
            Database = _config.DatabaseName,
            Password = _config.Password,
            Pooling = true,
            SslMode = MySqlSslMode.Preferred
        };

        return new MySqlConnection(builder.ConnectionString);
    }

    private void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[NeedSystem Database] {message}");
        Console.ResetColor();
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NeedSystem Database] {message}");
        Console.ResetColor();
    }

    public bool IsEnabled() => _enabled;
}