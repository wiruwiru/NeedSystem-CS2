using AnyBaseLib;
using AnyBaseLib.Bases;

using NeedSystem.Configs;
using NeedSystem.Utils;
using NeedSystem.Models;

namespace NeedSystem.Services;

public class DatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly bool _enabled;
    private IAnyBase? _db;
    private bool _isInitialized = false;

    public DatabaseService(DatabaseConfig config)
    {
        _config = config;
        _enabled = config.Enabled;

        if (_enabled)
        {
            try
            {
                _db = CAnyBase.Base("mysql");
                Logger.LogInfo("DatabaseService", "DatabaseService initialized and enabled");
            }
            catch (Exception ex)
            {
                Logger.LogError("DatabaseService", $"Failed to load AnyBaseLib: {ex.Message}");
                Logger.LogWarning("DatabaseService", "Database functionality will be disabled. Make sure AnyBaseLib.dll is present.");
                _enabled = false;
            }
        }
        else
        {
            Logger.LogInfo("DatabaseService", "DatabaseService initialized but disabled in configuration");
        }
    }

    public async Task InitializeDatabase()
    {
        if (!_enabled || _db == null)
        {
            Logger.LogInfo("DatabaseService", "Database is disabled, skipping initialization");
            return;
        }

        try
        {
            _db.Set(
                CommitMode.AutoCommit,
                _config.DatabaseName,
                $"{_config.Host}:{_config.Port}",
                _config.User,
                _config.Password
            );

            if (!_db.Init())
            {
                throw new Exception("Failed to initialize database connection");
            }

            await Task.Run(CreateTable);
            _isInitialized = true;
            Logger.LogInfo("DatabaseService", "Database connection established and table created");
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Failed to initialize database: {ex.Message}");
            throw;
        }
    }

    private void CreateTable()
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

        _db?.Query(createTableQuery, new List<string>(), true);
        Logger.LogInfo("DatabaseService", "Notifications table created/verified");
    }

    public async Task<bool> SaveNotification(NotificationRecord record)
    {
        if (!_enabled || _db == null || !_isInitialized)
        {
            return false;
        }

        try
        {
            var insertQuery = @"
                INSERT INTO need_notifications 
                (uuid, server_address, connected_players, max_players, map_name, timestamp, requested_by)
                VALUES 
                ('{ARG}', '{ARG}', {ARG}, {ARG}, '{ARG}', '{ARG}', '{ARG}')";

            var args = new List<string>
            {
                record.Uuid,
                record.ServerAddress,
                record.ConnectedPlayers.ToString(),
                record.MaxPlayers.ToString(),
                record.MapName,
                record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                record.RequestedBy
            };

            await Task.Run(() => _db.Query(insertQuery, args, true));

            Logger.LogInfo("DatabaseService", $"Notification saved to database - UUID: {record.Uuid}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Error saving notification to database: {ex.Message}");
            return false;
        }
    }

    public async Task<List<NotificationRecord>> GetRecentNotifications(int limit = 10)
    {
        if (!_enabled || _db == null || !_isInitialized)
        {
            return new List<NotificationRecord>();
        }

        try
        {
            var selectQuery = @"
                SELECT uuid, server_address, connected_players, max_players, map_name, timestamp, requested_by
                FROM need_notifications
                ORDER BY timestamp DESC
                LIMIT {ARG}";

            var args = new List<string> { limit.ToString() };

            var records = new List<NotificationRecord>();
            await Task.Run(() =>
            {
                var rows = _db.Query(selectQuery, args);
                if (rows != null && rows.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        records.Add(new NotificationRecord
                        {
                            Uuid = row[0],
                            ServerAddress = row[1],
                            ConnectedPlayers = int.Parse(row[2]),
                            MaxPlayers = int.Parse(row[3]),
                            MapName = row[4],
                            Timestamp = DateTime.Parse(row[5]),
                            RequestedBy = row[6]
                        });
                    }
                }
            });

            return records;
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Error retrieving notifications from database: {ex.Message}");
            return new List<NotificationRecord>();
        }
    }

    public async Task<int> GetNotificationCount()
    {
        if (!_enabled || _db == null || !_isInitialized)
        {
            return 0;
        }

        try
        {
            var countQuery = "SELECT COUNT(*) FROM need_notifications";

            int count = 0;
            await Task.Run(() =>
            {
                var result = _db.Query(countQuery, new List<string>());
                if (result != null && result.Count > 0 && result[0].Count > 0)
                {
                    count = int.Parse(result[0][0]);
                }
            });

            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Error getting notification count: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> TestConnection()
    {
        if (!_enabled || _db == null)
        {
            Logger.LogInfo("DatabaseService", "Database is disabled, skipping connection test");
            return false;
        }

        try
        {
            await Task.Run(() => _db.Query("SELECT 1", new List<string>()));
            Logger.LogInfo("DatabaseService", "Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Database connection test failed: {ex.Message}");
            return false;
        }
    }

    public bool IsEnabled() => _enabled && _db != null && _isInitialized;

    public void Dispose()
    {
        try
        {
            _db?.Close();
            Logger.LogInfo("DatabaseService", "Database connection closed");
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseService", $"Error closing database connection: {ex.Message}");
        }
    }
}