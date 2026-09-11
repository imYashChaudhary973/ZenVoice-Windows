using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class VaultTests
{
    [Fact]
    public void RoundTripAndDeleteAllRotatesKey()
    {
        var dir = Path.Combine(Path.GetTempPath(), "zenvoice-vault-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var keys = new InMemoryVaultKey();
        using (var vault = new TranscriptVault(Path.Combine(dir, "h.db"), keys))
        {
            vault.Add("Hello from ZenVoice", EngineIds.WhisperDistilLargeV3);
            var rows = vault.List();
            Assert.Single(rows);
            Assert.Equal("Hello from ZenVoice", rows[0].Text);
            vault.DeleteAll();
            Assert.Empty(vault.List());
        }

        Directory.Delete(dir, true);
    }
}
