using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.SQS;
using Hangfire;
using SharedUtility;
using TexasBar.Services;

namespace TexasBar.Services
{
    public class BackgroundTask
    {

        private  IBackgroundJobClient _backgroungJobClient = null;
        private IAmazonActivities _activity = null;

        // private AmazonActivities activity = null;
        // private BackgroundJobClient backgroungJobClient = null;

        private IAmazonS3 IAS3;
        private IAmazonSQS ISQS;
        

        //public BackgroundTask(IBackgroundJobClient backgroundJobClient, IAmazonActivities activities)
        //{
        //    _activity =  activities;
        //    _backgroungJobClient = backgroundJobClient;
        //}

        public BackgroundTask()
        {
            //var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("AKIAJI5RF4RFXM7U3ELQ", "4GH/cZnUMg9a+Tmwr6EJ5DQOplrduK3gTHYYb2D+");
            //IAS3 = new AmazonS3Client(awsCredentials, RegionEndpoint.USEast2);
            //ISQS = new AmazonSQSClient(awsCredentials, RegionEndpoint.USEast2);
            _activity = new AmazonActivities(IAS3, ISQS);
            _backgroungJobClient = new BackgroundJobClient();
        }

        //public BackgroundTask(): this(new BackgroundJobClient(),  new AmazonActivities(IAS3,ISQS))
        //{
        //}

        public GenericResponse CreateBucketTask(string bucketName)
        {
            GenericResponse gr = new GenericResponse();
            
            try
            {
                // var ans=  _backgroungJobClient.Enqueue(() => _activity.CreateBucket(bucketName));
             var  k=   _activity.CreateBucket(bucketName).Result;
            }
            catch (Exception ex)
            {

                var sa = ex.Message;
            }

            return gr;

        }

        public GenericResponse CreateFolderInBucketTask(string folderName, string bucketName, string folderPath)
        {
            GenericResponse gr = new GenericResponse();

            try
            {
                var ans = _backgroungJobClient.Enqueue(() => _activity.CreateFolder(folderName,bucketName, folderPath));
            }
            catch (Exception ex)
            {

                var sa = ex.Message;
            }

            return gr;

        }

        public GenericResponse WriteToBucketTask(string filePath, string bucketName, string folderPath)
        {
            GenericResponse gr = new GenericResponse();

            try
            {
                //var ans = _backgroungJobClient.Enqueue(() => _activity.WriteToBucket( filePath,  bucketName,  folderPath));
               var ans = _activity.WriteToBucket(filePath, bucketName, folderPath);
            }
            catch (Exception ex)
            {

                var sa = ex.Message;
            }

            return gr;

        }

        public GenericResponse ReceiveSQSMesaageTask(string queueName, string logId)
        {
            GenericResponse gr = new GenericResponse();

            try
            { 
               //var ans = _backgroungJobClient.Enqueue(() => _activity.ReceiveMessage(queueName, version, value));
              gr = _activity.ReceiveMessage(queueName, logId);
            }
            catch (Exception ex)
            {

                gr.Text = ex.Message;
                gr.Status = "ERROR";
            }

            return gr;

        }

        public async Task<GenericResponse> ReadObjectDataAsyncTask(string bucketName, string keyName)
        {
            GenericResponse gr = new GenericResponse();

            try
            {
                //var ans = _backgroungJobClient.Enqueue(() => _activity.ReceiveMessage(queueName, version, value));
                gr = await  _activity.ReadObjectDataAsync(bucketName, keyName);
            }
            catch (Exception ex)
            {

                gr.Text = ex.Message;
                gr.Status = "ERROR";
            }

            return gr;

        }

        public GenericResponse ConvertToSite(string bucketName, string index, string error)
        {
            GenericResponse gr = new GenericResponse();

            try
            {
              
                gr =  _activity.AddWebsiteConfigurationAsync(bucketName,index,error).Result;
                gr.Status = "SUCCESS";

            }
            catch (Exception ex)
            {

                gr.Text = ex.Message;
                gr.Status = "ERROR";
            }

            return gr;

        }

    }


}
