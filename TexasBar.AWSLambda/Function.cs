using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using SharedUtility;
using Amazon;
using Amazon.SQS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TexasBar.AWSLambda
{
    public class Function
    {

        private readonly string[] _supportedArchiveTypes = new string[] { ".zip" };
        private readonly AmazonS3Client _s3Client;
        private readonly IAmazonSQS _sqs;

        public Function()
        {
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJI5RF4RFXM7U3ELQ", "4GH/cZnUMg9a+Tmwr6EJ5DQOplrduK3gTHYYb2D+");
            _s3Client = new AmazonS3Client();
            _sqs = new AmazonSQSClient(awsCredentials, RegionEndpoint.USEast2);
        }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>


        public async Task<GenericResponse> FunctionHandler(S3Event s3Event, ILambdaContext context)
        {
            GenericResponse gr = new GenericResponse();
            try
            {
                foreach (var record in s3Event.Records)
                {
                    var bucketName = record.S3.Bucket.Name;
                    var filePath = record.S3.Object.Key;
                    if (_supportedArchiveTypes.Contains(Path.GetExtension(filePath).ToLower()))
                    {
                        // Console.WriteLine($"Object {bucketName}:{filePath} is not a supported archive type");
                        // continue;


                      //  Console.WriteLine($"Determining whether file {bucketName}:{filePath} has been compressed");

                        // Get the existing tag set
                        //var taggingResponse = await _s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                        //{
                        //    BucketName = bucketName,
                        //    Key = record.S3.Object.Key
                        //});

                        //if (taggingResponse.Tagging.Any(tag => tag.Key == "Compressed" && tag.Value == "true"))
                        //{
                        //    Console.WriteLine(
                        //        $"Archive {bucketName}:{filePath} has already been compressed");
                        //    continue;
                        //}

                        // Get the existing zip
                        using (var objectResponse = await _s3Client.GetObjectAsync(bucketName, filePath))
                        using (Stream responseStream = objectResponse.ResponseStream)
                        {

                            Console.WriteLine($"start to unzip archive {bucketName}:{filePath}");
                            using (ZipArchive archive = new ZipArchive(responseStream, ZipArchiveMode.Read))
                            {
                                char[] separator = { '/' };
                                string chapters = string.Empty;
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    var entryFName = entry.FullName;
                                    if (entryFName.Contains("/", StringComparison.OrdinalIgnoreCase))
                                    {

                                        //get directories of chapters
                                        String[] dirArray = entryFName.Split(separator);
                                        var splitting = SplitString(dirArray);
                                        if (splitting.Status == "OK")
                                        {
                                            var chapterName = SplitString(dirArray).Text;
                                            var chapterNo = SplitString(dirArray).Value;
                                            chapters += chapterNo + "*" + chapterName + "*" + filePath +"|" ;
                                        //    Console.WriteLine($"The CHAPTERS VALUE IS NOW {chapters}");
                                        }
                                        // call sqs a
                                    }

                                    var entryStream = entry.Open();

                                    String[] strlist = filePath.Split(separator);

                                    // Gets the full path to ensure that relative segments are removed.
                                    string destinationPath = $"{strlist[0]}/{strlist[1]}/" + entryFName;
                               //     Console.WriteLine($"start to unzip archive into the detinantion  {bucketName}:{destinationPath}");

                                    var extractResponse = await ExtractInBucket(entryStream, bucketName, destinationPath);
                                    //gr.Text = extractResponse.Text;
                                    //gr.Value = extractResponse.Value;
                                    //gr.Status = extractResponse.Status;
                                }

                                var sqsResult = new GenericResponse();
                                if (QueueExists())
                                {
                                    sqsResult = await SendSQSMessage(chapters, filePath);
                                }
                                else
                                {
                                     sqsResult = await CreateSQSMessage(chapters, filePath);
                                }
                                
                           //     Console.WriteLine($"The SQS status is {sqsResult.Status} with value {sqsResult.Value} and TEXT {sqsResult.Text}");
                            }
                        }


                    }
                }
                LambdaLogger.Log("Function name: " + context.FunctionName + "\n");
                context.Logger.Log("RemainingTime: " + context.RemainingTime + "\n");
                LambdaLogger.Log("LogGroupName: " + context.LogGroupName + "\n");
            }
            catch (AmazonS3Exception seEx)
            {
                Console.WriteLine($"Error occurred:  {seEx.Message}, STACKTRACE : {seEx.StackTrace}");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                Console.WriteLine($"Error occurred:  {errorMessage}, STACKTRACE : {ex.StackTrace}");
            }
            return gr;
        }

        public async Task<GenericResponse> ExtractInBucket(Stream fileStream, string bucketName, string folderPath)
        {

            GenericResponse result = new GenericResponse();
            // Console.WriteLine($"writing files from filepath {filePath}");
          //  Console.WriteLine($"writing files into {bucketName}:{folderPath}");
            Stream decodedStream = new MemoryStream();
            byte[] buffer = new byte[4096];
            try
            {

                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    decodedStream.Write(buffer, 0, bytesRead);

               // Console.WriteLine($"Decoded Stream size is {decodedStream.Length}");
                //  FileInfo file = new FileInfo(filePath);
                //string path = "my-folder/sub-folder/test.txt";
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = decodedStream,
                    BucketName = bucketName,
                    Key = folderPath
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(request);

                result.Value = response.ResponseMetadata.RequestId;
               // Console.WriteLine($"writing successful for {bucketName}:{folderPath} with {result.Value}");
            }
            catch (AmazonS3Exception awsEx)
            {
                var errorMessage = awsEx.Message;
                //var errorType =  awsEx.ErrorType. is null ? awsEx.ErrorType.ToString() : null;
              //  Console.WriteLine($"Error writing file for {awsEx.ResponseBody} with {errorMessage}");
            }
            catch (Exception ex)
            {

               var errorMessage = ex.Message;
                Console.WriteLine($"Error writing file for with {errorMessage}");
            }

            return result;
        }

        public GenericResponse SplitString(string[] dirArray)
        {

            var result = new GenericResponse();
            try
            {
                char[] separator = { '_' };

                if (dirArray != null)
                {
                    // based on folder structure of 
                    var publicHtml = dirArray[0];
                    var likelyChapter = dirArray[1];
                    var firstChar = likelyChapter.Substring(0, 1);
                    if (firstChar.All(char.IsDigit))
                    {
                        //it's a chapter
                        var chapterArray = likelyChapter.Split(separator);
                        if (chapterArray != null)
                        {
                            result.Value = chapterArray[0];
                            result.Text = chapterArray[1];
                            result.Status = "OK";
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                result.Text = ex.Message;
                result.Value = ex.StackTrace;
                result.Status = "ERROR";

            }

            return result;

        }



        public async Task<GenericResponse> CreateSQSMessage(string messageBody, string filePath)
        {
            var result = new GenericResponse();
            try
            {
                Console.WriteLine($"about to create the chaptersQueue");
                var sqsRequest = new CreateQueueRequest { QueueName = "ChaptersInQueue" };
                var createQueueResponse = _sqs.CreateQueueAsync(sqsRequest).Result;
                var myQueueUrl = createQueueResponse.QueueUrl;

                //foreach (var url in listQueueResponse.Result.QueueUrls)
                //{
                //    Console.WriteLine($"{url}");
                //}

                var sqsMessageReq = new SendMessageRequest();
                sqsMessageReq.QueueUrl = myQueueUrl;
                sqsMessageReq.MessageBody = messageBody;

                await _sqs.SendMessageAsync(sqsMessageReq);
                Console.WriteLine($"Message was sent successfully to SQS. \n");
                result.Text = messageBody;
                result.Value = myQueueUrl;
                result.Status = "OK";
                
            }
            catch (AmazonSQSException sqsEx)
            {
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

        public async Task<GenericResponse> SendSQSMessage(string messageBody, string filePath)
        {
            var result = new GenericResponse();
            try
            {

                Console.WriteLine($"about to create the chaptersQueue");
                var request = new GetQueueUrlRequest
                {
                    QueueName = "ChaptersInQueue"
                 //  , QueueOwnerAWSAccountId = "80398EXAMPLE"
                };

                var response = await _sqs.GetQueueUrlAsync(request);
                var myQueueUrl = response.QueueUrl;

                var sqsMessageReq = new SendMessageRequest();
                sqsMessageReq.QueueUrl = myQueueUrl;
                sqsMessageReq.MessageBody = messageBody;

                await _sqs.SendMessageAsync(sqsMessageReq);
                Console.WriteLine($"Message was sent successfully to SQS. \n");
                result.Text = messageBody;
                result.Value = myQueueUrl;
                result.Status = "OK";

            }
            catch (AmazonSQSException sqsEx)
            {
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

        public bool QueueExists()
        {
            var ret = false;
            try
            {
                var listQueueRequest = new ListQueuesRequest();
                var listQueueResponse = _sqs.ListQueuesAsync(listQueueRequest);
                var w = listQueueResponse.Result.QueueUrls.Where(x => x.Equals("ChaptersInQueue")).Count();
                if (w > 0)
                    ret = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred:  {ex.Message}, STACKTRACE : {ex.StackTrace}");

            }
            return ret;
        }
    }


}

