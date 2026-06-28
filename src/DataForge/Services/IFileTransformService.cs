using DataForge.Models;

namespace DataForge.Services;

public interface IFileTransformService
{
    Task<TransformResponse> TransformAsync(Stream fileStream, string fileName, TransformRequest request);
}
