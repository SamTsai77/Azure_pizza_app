using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly BlobContainerClient _containerClient;

    public UploadController(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var blobClient = _containerClient.GetBlobClient(file.FileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        return Ok(new { url = blobClient.Uri.ToString() });
    }
}
