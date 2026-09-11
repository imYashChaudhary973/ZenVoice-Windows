using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace ZenVoice.Core;

public interface IVaultKey
{
    byte[] LoadOrCreate();
    void Delete();
}

public sealed class InMemoryVaultKey : IVaultKey
{
    private byte[] _key = RandomNumberGenerator.GetBytes(32);

    public byte[] LoadOrCreate() => _key;

    public void Delete() => _key = RandomNumberGenerator.GetBytes(32);
}

public sealed record HistoryRecord(long Id, DateTime CreatedAt, string EngineId, string Text);

public sealed class TranscriptVault : IDisposable
{
    private readonly IVaultKey _keys;
    private readonly SqliteConnection _db;

    public TranscriptVault(string dbPath, IVaultKey keys)
    {
        _keys = keys;
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _db = new SqliteConnection($"Data Source={dbPath}");
        _db.Open();
        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS records (
              id INTEGER PRIMARY KEY,
              created TEXT NOT NULL,
              engine TEXT NOT NULL,
              nonce BLOB NOT NULL,
              tag BLOB NOT NULL,
              ciphertext BLOB NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public void Add(string text, string engineId)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plain = System.Text.Encoding.UTF8.GetBytes(text);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];
        using var gcm = new AesGcm(_keys.LoadOrCreate(), 16);
        gcm.Encrypt(nonce, plain, cipher, tag);

        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "INSERT INTO records(created, engine, nonce, tag, ciphertext) VALUES ($c,$e,$n,$t,$x)";
        cmd.Parameters.AddWithValue("$c", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$e", engineId);
        cmd.Parameters.AddWithValue("$n", nonce);
        cmd.Parameters.AddWithValue("$t", tag);
        cmd.Parameters.AddWithValue("$x", cipher);
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<HistoryRecord> List()
    {
        var key = _keys.LoadOrCreate();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, created, engine, nonce, tag, ciphertext FROM records ORDER BY id DESC";
        using var r = cmd.ExecuteReader();
        var rows = new List<HistoryRecord>();
        while (r.Read())
        {
            var nonce = (byte[])r["nonce"];
            var tag = (byte[])r["tag"];
            var cipher = (byte[])r["ciphertext"];
            var plain = new byte[cipher.Length];
            using var gcm = new AesGcm(key, 16);
            gcm.Decrypt(nonce, cipher, tag, plain);
            rows.Add(new HistoryRecord(
                r.GetInt64(0),
                DateTime.Parse(r.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind),
                r.GetString(2),
                System.Text.Encoding.UTF8.GetString(plain)));
        }

        return rows;
    }

    public void DeleteAll()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM records";
        cmd.ExecuteNonQuery();
        _keys.Delete();
    }

    public void Dispose() => _db.Dispose();
}
