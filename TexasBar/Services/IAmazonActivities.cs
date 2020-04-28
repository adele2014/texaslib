using SharedUtility;
using System.Threading.Tasks;

namespace TexasBar.Services
{
    public interface IAmazonActivities
    {
        Task<GenericResponse> CreateBucket(string bucketName);
        Task<GenericResponse> CreateFolder(string folderName, string bucketName, string folderPath);

        Task<GenericResponse> WriteToBucket(string folderName, string bucketName, string folderPath);

        Task<GenericResponse> ListFiles(string bucketName, string folderPath);
    }
}