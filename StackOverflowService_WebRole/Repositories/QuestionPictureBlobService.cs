using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowService_WebRole.Repositories
{
    public class QuestionPictureBlobService
    {
        private readonly CloudBlobContainer _container;

        public QuestionPictureBlobService()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference("questionspictures");
            _container.CreateIfNotExists();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            _container.SetPermissionsAsync(permissions);
        }

        public async Task<string> UploadFileAsync(HttpPostedFile file)
        {
            var blob = _container.GetBlockBlobReference(Guid.NewGuid() + Path.GetExtension(file.FileName));
            await blob.UploadFromStreamAsync(file.InputStream);
            return blob.Uri.ToString();
        }
        public async Task DeleteFileAsync(string fileUrl)
        {
            var blobName = new Uri(fileUrl).Segments.Last();
            var blob = _container.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }
    }
}