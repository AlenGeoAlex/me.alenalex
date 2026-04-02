using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;

namespace Bloggi.Backend.Api.Web.Infrastructure.Services;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Bloggi.Backend.Api.Web.Options;

/// <summary>
/// S3-compatible implementation of <see cref="IFileService"/>.
/// Works with AWS S3, Cloudflare R2, MinIO, Backblaze B2, and any other
/// S3-compatible provider by setting <see cref="S3FileServiceOptions.Endpoint"/>.
/// </summary>
public sealed class S3FileService(IOptionsSnapshot<S3FileServiceOptions> options) : IFileService, IAsyncDisposable
{
    private readonly S3FileServiceOptions _options = options.Value;

    private IAmazonS3 S3
    {
        get
        {
            if (field is not null) return field;

            var credentials = new BasicAWSCredentials(_options.ClientId, _options.ClientSecret);
            var hasCustomEndpoint = !string.IsNullOrWhiteSpace(_options.Endpoint);

            var config = new AmazonS3Config
            {
                ForcePathStyle = hasCustomEndpoint
            };

            if (hasCustomEndpoint)
            {
                config.ServiceURL = _options.Endpoint;

                if (Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out var uri))
                {
                    config.UseHttp = uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region);
            }

            field = new AmazonS3Client(credentials, config);
            return field;
        }
    }

    /// <inheritdoc />
    public async Task<IFileService.DeleteFileResponse> DeleteAsync(
        IFileService.DeleteFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileKey);

        try
        {
            await S3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.Bucket,
                Key = request.FileKey
            }, cancellationToken);

            return new IFileService.DeleteFileResponse(Success: true);
        }
        catch (AmazonS3Exception ex)
        {
            return new IFileService.DeleteFileResponse(Success: false, ErrorMessage: ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<IFileService.GetPresignedUrlForUploadResponse> GetPresignedUrlForUploadAsync(
        IFileService.GetPresignedUrlForUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FilePathKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);

        var expiry = request.Expiry ?? _options.DefaultPresignedUrlExpiry;
        var fileKey = request.FilePathKey;
        var expiresAt = DateTimeOffset.UtcNow.Add(expiry);

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = fileKey,
            Verb = HttpVerb.PUT,
            ContentType = request.ContentType,
            Expires = expiresAt.UtcDateTime
        };

        var url = S3.GetPreSignedURL(presignRequest);
        if(string.IsNullOrWhiteSpace(url))
            throw new Exception("Failed to get presigned URL");
        
        url = NormalizePresignedUrlScheme(url);
        return Task.FromResult(new IFileService.GetPresignedUrlForUploadResponse(
            PresignedUrl: url,
            FileKey: fileKey,
            PublicUrl: BuildPublicUrl(fileKey),
            ExpiresAt: expiresAt)
        );
    }

    /// <inheritdoc />
    public async Task<IFileService.GetFileResponse> GetFileAsync(
        IFileService.GetFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileKey);

        var s3Response = await S3.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _options.Bucket,
            Key = request.FileKey
        }, cancellationToken);

        return new IFileService.GetFileResponse(
            Content: s3Response.ResponseStream,
            ContentType: s3Response.Headers.ContentType,
            FileName: s3Response.Metadata.Keys.Contains("x-amz-meta-filename")
                ? s3Response.Metadata["x-amz-meta-filename"]
                : null,
            ContentLength: s3Response.ContentLength > 0 ? s3Response.ContentLength : null
        );
    }
    
    public async Task<IFileService.GetPublicUrlIfExistsResponse?> GetPublicUrlIfExistsAsync(
        IFileService.GetPublicUrlIfExistsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileKey);

        try
        {
            await S3.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _options.Bucket,
                Key = request.FileKey
            }, cancellationToken);

            var publicUrl = BuildPublicUrl(request.FileKey);

            return new IFileService.GetPublicUrlIfExistsResponse(
                FileKey: request.FileKey,
                PublicUrl: publicUrl
            );
        }
        catch (AmazonS3Exception ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.NotFound ||
            ex.ErrorCode == "NoSuchKey" ||
            ex.ErrorCode == "NotFound")
        {
            return null;
        }
    }
    
    public string BuildPublicUrl(string fileKey)
    {
        var escapedKey = string.Join("/", fileKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

        if (_options.Endpoint is not null)
        {
            var endpoint = _options.Endpoint.TrimEnd('/');

            return _options.PublicUrl is not null ? $"{_options.PublicUrl.TrimEnd('/')}/{escapedKey}" : $"{endpoint}/{_options.Bucket}/{escapedKey}";
        }

        return $"https://{_options.Bucket}.s3.{_options.Region}.amazonaws.com/{escapedKey}";
    }

    public async Task<IFileService.SaveFileResponse> SaveFileAsync(
        IFileService.SaveFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);

        using var stream = new MemoryStream(request.Bytes);

        await S3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = request.FileKey,
            InputStream = stream,
            ContentType = request.ContentType,
            Metadata =
            {
                ["x-amz-meta-filename"] = request.FileName,
            }
        }, cancellationToken);

        return new IFileService.SaveFileResponse(
            FileKey: request.FileKey,
            PublicUrl: BuildPublicUrl(request.FileKey)
        );
    }
    
    private string NormalizePresignedUrlScheme(string url)
    {
        var endpoint = _options.Endpoint;

        if (string.IsNullOrWhiteSpace(endpoint))
            return url;

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
            return url;

        if (!endpointUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            return url;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var signedUri))
            return url;

        return new UriBuilder(signedUri)
        {
            Scheme = Uri.UriSchemeHttp,
            Port = endpointUri.Port
        }.Uri.ToString();
    }
    

    public ValueTask DisposeAsync()
    {
        S3.Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        S3.Dispose();
    }
}