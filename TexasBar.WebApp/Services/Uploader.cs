using Amazon.S3;
using SharedUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TexasBar.Services
{
    public class Uploader
    {

        public Uploader()
        {
                
        }


        public GenericResponse CreateBucket(string bucketName)
        {

            GenericResponse result = new GenericResponse();

            try
            {

            }
            catch(AmazonS3Exception awsEx)
            {
               
            }
            catch (Exception ex)
            {

              
            }

            return result;
        }


    }
}
