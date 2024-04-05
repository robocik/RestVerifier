using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RestVerifier.AspNetCore;

public class NotDisposableFileStreamResult : FileResult
{
    // default buffer size as defined in BufferedStream type
    private const int BufferSize = 0x1000;


    public NotDisposableFileStreamResult(Stream fileStream, string contentType)
        : base(contentType)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException("fileStream");
        }

        FileStream = fileStream;
    }

    public Stream FileStream { get; private set; }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        // grab chunks of data and write to the output stream
        Stream outputStream = context.HttpContext.Response.Body;
        byte[] buffer = new byte[BufferSize];

        int bytesRead = await FileStream.ReadAsync(buffer, 0, BufferSize);
        if (bytesRead == 0)
        {
            // no more data
            return;
        }

        await outputStream.WriteAsync(buffer, 0, bytesRead);
    }

}