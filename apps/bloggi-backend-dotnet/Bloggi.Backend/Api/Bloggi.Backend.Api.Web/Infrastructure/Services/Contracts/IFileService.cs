namespace Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;

/// <summary>
/// Provider-agnostic abstraction for object / blob storage operations.
/// Implementations exist for AWS S3, Azure Blob Storage, GCS, etc.
/// </summary>
public interface IFileService : IDisposable, IAsyncDisposable
{
    /// <summary>Permanently removes a file from storage.</summary>
    Task<DeleteFileResponse> DeleteAsync(
        DeleteFileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a time-limited URL that allows the caller to upload a single
    /// file directly to storage without exposing long-lived credentials.
    /// </summary>
    Task<GetPresignedUrlForUploadResponse> GetPresignedUrlForUploadAsync(
        GetPresignedUrlForUploadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads a file and returns its content as a stream.</summary>
    Task<GetFileResponse> GetFileAsync(
        GetFileRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks whether a file exists and returns its public URL if available.
    /// Returns null when the file does not exist.
    /// </summary>
    Task<GetPublicUrlIfExistsResponse?> GetPublicUrlIfExistsAsync(
        GetPublicUrlIfExistsRequest request,
        CancellationToken cancellationToken = default);
    
    
    public sealed record GetPublicUrlIfExistsRequest(
        string FileKey
    );

    public sealed record GetPublicUrlIfExistsResponse(
        string FileKey,
        string PublicUrl
    );
    
    public sealed record DeleteFileRequest(
        string FileKey
    );

    public sealed record DeleteFileResponse(
        bool Success,
        string? ErrorMessage = null
    );


    public sealed record GetPresignedUrlForUploadRequest(
        string FilePathKey,
        string ContentType,
        TimeSpan? Expiry = null,
        string? AllowedIpAddress = null
    );

    public sealed record GetPresignedUrlForUploadResponse(
        string PresignedUrl,
        string FileKey,
        string PublicUrl,
        DateTimeOffset ExpiresAt
    );

    public sealed record GetFileRequest(
        string FileKey
    );

    public sealed record GetFileResponse(
        Stream Content,
        string ContentType,
        string? FileName = null,
        long? ContentLength = null
    );

}