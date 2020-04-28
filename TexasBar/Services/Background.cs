using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
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

        public BackgroundTask(IBackgroundJobClient backgroundJobClient, IAmazonActivities activities)
        {
            _activity =  activities;
            _backgroungJobClient = backgroundJobClient;
        }

        public BackgroundTask()
        {
            _activity = new AmazonActivities(IAS3);
           _backgroungJobClient = new BackgroundJobClient();
        }

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

        

    }


}
