﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose
{
    public static class WebClientExtensions
    {
            public static async Task DownloadDataAsync(this HttpClient client, string requestUrl, Stream destination, IProgress<(float progress, long bytesDownloaded, long totalBytes)> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    var contentLength = response.Content.Headers.ContentLength;
                    using (var download = await response.Content.ReadAsStreamAsync())
                    {
                        if (progress is null || !contentLength.HasValue)
                        {
                            await download.CopyToAsync(destination);
                            return;
                        }
                        // Such progress and contentLength much reporting Wow!
                        var progressWrapper = new Progress<long>(totalBytes => progress.Report((GetProgressPercentage(totalBytes, contentLength.Value), contentLength.Value, totalBytes)));
                        await download.CopyToAsync(destination, 81920, progressWrapper, cancellationToken);
                    }
                }

                float GetProgressPercentage(float totalBytes, float currentBytes) => (totalBytes / currentBytes) * 100f;
            }

            static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (bufferSize < 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (!source.CanRead)
                    throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
                if (destination == null)
                    throw new ArgumentNullException(nameof(destination));
                if (!destination.CanWrite)
                    throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

                var buffer = new byte[bufferSize];
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    progress?.Report(totalBytesRead);
                }
            }
    }
}
