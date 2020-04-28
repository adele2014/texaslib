using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Util;
using SharedUtility;
using TexasBar.Domain.Models;
using TexasBar.Persistence.Repo;

namespace TexasBar.Services
{
    public class AmazonActivities : IAmazonActivities
    {
        private readonly IAmazonS3 _client;
        private readonly IAmazonSQS _sqsClient;

        private readonly IUnitOfWork uow = null;

        private const string bucketName = "*** bucket name ***";
        private const string indexDocumentSuffix = "*** index object key ***"; // For example, index.html.
        private const string errorDocument = "*** error object key ***"; // For example, error.html.


        private readonly IRepository<UploadLog> repoUpload = null;
        private readonly IRepository<Chapters> repoChapters = null;
        public AmazonActivities(IAmazonS3 client, IAmazonSQS sqsClient)
        {
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJI5RF4RFXM7U3ELQ", "4GH/cZnUMg9a+Tmwr6EJ5DQOplrduK3gTHYYb2D+");
            client = new AmazonS3Client(awsCredentials, RegionEndpoint.USEast2);
            sqsClient = new AmazonSQSClient(awsCredentials, RegionEndpoint.USEast2);
            _client = client;
            _sqsClient = sqsClient;
            uow = new UnitOfWork();
            repoUpload = new Repository<UploadLog>(uow);
            repoChapters = new Repository<Chapters>(uow);

        }

        public async Task<GenericResponse> CreateBucket(string bucketName)
        {

            GenericResponse result = new GenericResponse();

            try
            {

                if (await _client.DoesS3BucketExistAsync(bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest() { BucketName = bucketName, UseClientRegion = true };
                   var w= await _client.PutBucketAsync(putBucketRequest);
                }
            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.ErrorCode;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
            }

            return result;
        }

        public async Task<GenericResponse> CreateFolder(string folderName, string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();

            try
            {


                //  string folderPath = "my-folder/sub-folder/";

                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = folderPath
                };

                PutObjectResponse response = await _client.PutObjectAsync(request);
                result.Value = response.ResponseMetadata.RequestId;

            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.Message;
            }
            catch (Exception ex)
            {

                var errorMessage = ex.Message;
            }

            return result;
        }


        public async Task<GenericResponse> WriteToBucket(string filePath, string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();

            try
            {

                FileInfo file = new FileInfo(filePath);
                //string path = "my-folder/sub-folder/test.txt";

                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = file.OpenRead(),
                    BucketName = bucketName,
                    Key = folderPath.ToUpper()
                };

                PutObjectResponse response = await _client.PutObjectAsync(request);

                result.Value = response.ResponseMetadata.RequestId;

                char[] separator = { '/' };
                //save to db
                var uploadLog = new UploadLog();
                var folderPathArray = folderPath.Split(separator);

                uploadLog.BookValue = folderPathArray[0];
                uploadLog.UploadedFileName = file.Name;
                uploadLog.LogId = result.Value;
                uploadLog.IsCurrent = true;
                uploadLog.Path = folderPath;
                uploadLog.CreatedDate = DateTime.UtcNow;
                uploadLog.Version = Int32.Parse(folderPathArray[1]);

                repoUpload.Insert(uploadLog);
                var persist = uow.Save("David");
                if (persist > 0)
                    result.Status = "OK";


            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.ErrorCode;
            }
            catch (Exception ex)
            {

                var errorMessage = ex.Message;
            }

            return result;
        }


        public async Task<GenericResponse> ListFiles(string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();

            try
            {

                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = folderPath
                };

                ListObjectsResponse response = await _client.ListObjectsAsync(request);
                foreach (S3Object obj in response.S3Objects)
                {
                    var sd = obj.ETag;
                }

                result.Value = response.ResponseMetadata.RequestId;
            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.ErrorCode;
            }
            catch (Exception ex)
            {

                var errorMessage = ex.Message;
            }

            return result;
        }


        public async Task<GenericResponse> DeleteFolder(string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();

            try
            {


                var deleteFolderRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderPath
                };
                DeleteObjectResponse folderDeleteResponse = await _client.DeleteObjectAsync(deleteFolderRequest);

                result.Value = folderDeleteResponse.ResponseMetadata.RequestId;
                result.Text = $"Success {folderPath} has been deleted";
            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.ErrorCode;
            }
            catch (Exception ex)
            {

                var errorMessage = ex.Message;
            }

            return result;
        }

        public async Task<GenericResponse> DeleteFile(string filePath, string bucketName)
        {

            GenericResponse result = new GenericResponse();

            try
            {


                var deleteFileRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = filePath
                };
                DeleteObjectResponse fileDeleteResponse = await _client.DeleteObjectAsync(deleteFileRequest);



                result.Value = fileDeleteResponse.ResponseMetadata.RequestId;
                result.Text = $"Success {filePath} has been deleted";

            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.ErrorCode;
            }
            catch (Exception ex)
            {

                var errorMessage = ex.Message;
            }

            return result;
        }

        public async Task<GenericResponse> ReadObjectDataAsync(string bucketName, string keyName)
        {
            string responseBody = "";
            var result = new GenericResponse();
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await _client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                   // string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                    string contentType = response.Headers["Content-Type"];
                  
                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                    result.Text = "SUCCESS";
                    result.Value = responseBody;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
                result.Text = "ERROR";
                
                result.Value = e.Message;
            }
            catch (Exception e)
            {
                result.Text = "ERROR";
                
                result.Value = e.Message;
            }

            return result;
        }

        public GenericResponse ReceiveMessage(string queueName, string logId)
        {

            GenericResponse result = new GenericResponse();

            try
            {
                var queueUrl = _sqsClient.GetQueueUrlAsync(queueName).Result.QueueUrl;
                var receiveMessageReq = new ReceiveMessageRequest { QueueUrl = queueUrl };
                var receiveMessageResp = _sqsClient.ReceiveMessageAsync(receiveMessageReq).Result;

                foreach (var message in receiveMessageResp.Messages)
                {
                    result.Text = message.Body;
                    result.Value = message.MessageId;


                    foreach (var attrib in message.Attributes)
                    {
                        var atName = attrib.Key;
                        var atValue = attrib.Value;
                    }
                    DeleteMessage(queueUrl, message.ReceiptHandle);
                }

                result.Status = "OK";
                var res = SaveMessageToChapters(result.Text, logId);

            }
            catch (AmazonSQSException sqsEx)
            {
                var errorMessage = sqsEx.ErrorCode;
                result.Text = sqsEx.Message;
                result.Value = sqsEx.StackTrace;
                result.Status = "ERROR";
            }
            catch (Exception ex)
            {
                result.Text = ex.Message;
                result.Value = ex.StackTrace;
                result.Status = "ERROR";
            }

            return result;
        }

        public void DeleteMessage(string queueUrl, string receiptHandle)
        {
            try
            {
                var deleteMessageRequest = new DeleteMessageRequest();

                deleteMessageRequest.QueueUrl = queueUrl;
                deleteMessageRequest.ReceiptHandle = receiptHandle;
                var response = _sqsClient.DeleteMessageAsync(deleteMessageRequest);
            }
            catch (AmazonSQSException sqsEx)
            {
                var ws = sqsEx.Message;
            }


        }

        public GenericResponse SaveMessageToChapters(string messagebody, string logId)
        {

            var res = new GenericResponse();
            try
            {
                char[] seperator = { '|' };
                var chapters = messagebody.Split(seperator);
                foreach (var item in chapters)
                {
                    char[] seperator2 = { '*' };
                    var chapterNo = item.Split(seperator2)[0];
                    var chapterName = item.Split(seperator2)[1];
                    var filePath = item.Split(seperator2)[2];

                    var chapter = new Chapters();
                    chapter.BookCode = logId;
                    chapter.Description = chapterName;
                    chapter.ChapterNo = chapterNo;
                    chapter.CreateDate = DateTime.UtcNow;
                    chapter.CreatedBy = "David";
                    repoChapters.Insert(chapter);

                }

                if (uow.Save("david") > 0)
                {
                    res.Text = "SUCCESS";
                    res.Status = "OK";
                };
            }
            catch (Exception ex)
            {
                var ws = ex.Message;
                res.Status = "ERROR";
                res.Text = ws;
            }

            return res;
        }

        public async Task<GenericResponse> AddWebsiteConfigurationAsync(string bucketName, string indexDocumentSuffix, string errorDocument)
        {
                var result = new GenericResponse();
            try
            {
                // 1. Put the website configuration.
                PutBucketWebsiteRequest putRequest = new PutBucketWebsiteRequest()
                {
                    BucketName = bucketName,
                    WebsiteConfiguration = new WebsiteConfiguration()
                    {
                        IndexDocumentSuffix = indexDocumentSuffix,
                        ErrorDocument = errorDocument
                    }
                };
                PutBucketWebsiteResponse response = await _client.PutBucketWebsiteAsync(putRequest);

                // 2. Get the website configuration.
                GetBucketWebsiteRequest getRequest = new GetBucketWebsiteRequest()
                {
                    BucketName = bucketName
                };
                GetBucketWebsiteResponse getResponse = await _client.GetBucketWebsiteAsync(getRequest);
                Console.WriteLine("Index document: {0}", getResponse.WebsiteConfiguration.IndexDocumentSuffix);
                Console.WriteLine("Error document: {0}", getResponse.WebsiteConfiguration.ErrorDocument);
                result.Text = "SUCCESS";
                result.Value = bucketName;
                result.Status = "OK";
            }
            catch (AmazonS3Exception e)
            {
                result.Text = e.Message;
                result.Value = e.StackTrace;
                result.Status = "ERROR";
            }
            catch (Exception e)
            {
                result.Text = e.Message;
                result.Value = e.StackTrace;
                result.Status = "ERROR";
               
            }
            return result;
        }
    }


}
