using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Amazon.Util;
using SharedUtility;

namespace TexasBar.Services
{
    public class AmazonActivities : IAmazonActivities
    {
        private readonly IAmazonS3 _client;
        public AmazonActivities(IAmazonS3 client)
        {
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJI5RF4RFXM7U3ELQ", "4GH/cZnUMg9a+Tmwr6EJ5DQOplrduK3gTHYYb2D+");
            client = new AmazonS3Client(awsCredentials, RegionEndpoint.USEast2);
            _client = client;

        }

        public async Task<GenericResponse> CreateBucket(string bucketName)
        {

            GenericResponse result = new GenericResponse();

            try
            {

                if (await _client.DoesS3BucketExistAsync(bucketName) == false)
                {
                    var putBucketRequest = new PutBucketRequest() { BucketName = bucketName, UseClientRegion = true };
                    await _client.PutBucketAsync(putBucketRequest);

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
                    Key = folderPath
                };

                PutObjectResponse response = await _client.PutObjectAsync(request);

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


        public async Task<GenericResponse> ListFiles( string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();

            try
            {

                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = folderPath
                };

                ListObjectsResponse response =  await _client.ListObjectsAsync(request);
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



    }
}
