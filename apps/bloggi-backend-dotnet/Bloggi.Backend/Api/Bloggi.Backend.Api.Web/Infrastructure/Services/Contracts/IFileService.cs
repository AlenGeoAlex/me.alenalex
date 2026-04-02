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

    /// <summary>
    /// Stores a file in the configured storage provider and returns metadata about the stored file.
    /// </summary>
    /// <param name="request">The request containing details of the file to be saved, such as its file key, name, content type, and binary content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the file key and public URL of the saved file.</returns>
    Task<IFileService.SaveFileResponse> SaveFileAsync(
        IFileService.SaveFileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Constructs a public URL for accessing a file in the storage service.
    /// </summary>
    /// <param name="fileKey">The unique identifier of the file whose public URL is to be generated.</param>
    /// <returns>A string representing the public URL of the specified file.</returns>
    string BuildPublicUrl(string fileKey);

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

    public record SaveFileRequest(
        string FileKey,
        string FileName,
        string ContentType,
        byte[] Bytes
    );

    public record SaveFileResponse(
        string FileKey,
        string PublicUrl
    );
}