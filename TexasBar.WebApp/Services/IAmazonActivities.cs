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
        GenericResponse ReceiveMessage(string queueName, string logId);
        Task<GenericResponse> AddWebsiteConfigurationAsync(string bucketName,string indexDocumentSuffix, string errorDocument);
        GenericResponse SaveMessageToChapters(string messagebody, string logId);
        Task<GenericResponse> ReadObjectDataAsync(string bucketName, string keyName);
        GenericResponse GetPreSignedURL(string bucketName, string keyName);
    }
}