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
    public class ProfilePictureBlobService
    {
        private readonly CloudBlobContainer _container;

        public ProfilePictureBlobService()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference("profilepictures");
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
            var blobName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var blob = _container.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }
    }
}