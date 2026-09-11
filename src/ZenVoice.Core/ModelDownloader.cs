using System.Security.Cryptography;

namespace ZenVoice.Core;

public sealed class ModelDownloader
{
    private readonly HttpClient _http;

    public ModelDownloader(HttpClient? http = null) =>
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

    public async Task<string> DownloadAsync(
        Model model,
        string destDir,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(destDir);
        var dest = Path.Combine(destDir, model.Filename);
        var tmp = dest + ".part";
        using var resp = await _http.GetAsync(
            model.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? model.SizeBytes;
        await using var input = await resp.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (var output = File.Create(tmp))
        {
            var buf = new byte[1024 * 256];
            long read = 0;
            int n;
            while ((n = await input.ReadAsync(buf, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await output.WriteAsync(buf.AsMemory(0, n), cancellationToken).ConfigureAwait(false);
                read += n;
                progress?.Report(total == 0 ? 0 : (double)read / total);
            }

            if (read != model.SizeBytes)
            {
                throw new InvalidDataException($"Size {read} != {model.SizeBytes}");
            }
        }

        await using (var hashed = File.OpenRead(tmp))
        {
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(hashed, cancellationToken)
                .ConfigureAwait(false)).ToLowerInvariant();
            if (hash != model.Sha256)
            {
                File.Delete(tmp);
                throw new InvalidDataException("SHA-256 mismatch");
            }
        }

        File.Move(tmp, dest, overwrite: true);
        return dest;
    }
}
