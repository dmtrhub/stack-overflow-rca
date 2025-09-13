using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.AzureStorage
{
    public class UserTableEntity : TableEntity
    {
        public UserTableEntity() { }

        public UserTableEntity(User user)
        {
            PartitionKey = "User";
            RowKey = Guid.NewGuid().ToString();
            FullName = user.FullName;
            Gender = user.Gender;
            Country = user.Country;
            City = user.City;
            Address = user.Address;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
            ProfilePictureUrl = user.ProfilePictureUrl;
        }

        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}