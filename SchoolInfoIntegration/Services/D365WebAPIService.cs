using ECC.Institute.CRM.IntegrationAPI.Models;
using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ECC.Institute.CRM.IntegrationAPI
{
    public interface ID365WebAPIService
    {
        HttpResponseMessage SendRetrieveRequestAsync(string query, bool formatted = false, int maxPageSize = 50);
        HttpResponseMessage SendCreateRequestAsync(string endPoint, string content);
        HttpResponseMessage SendCreateRequestAsyncRtn(string endPoint, string content);
        HttpResponseMessage SendCreateRequestAsync(HttpMethod httpMethod, string entitySetName, string body);
        HttpResponseMessage SendDeleteRequestAsync(string endPoint);
        HttpResponseMessage SendUpdateRequestAsync(string endPoint, string content);

        HttpResponseMessage SendUploadFileRequestAsync(string endPoint, Stream fileContent);
        HttpResponseMessage SendMessageAsync(HttpMethod httpMethod, string messageUri);
        public ID365AuthenticationService D365AuthenticationService { get; }
        public string Application { get; set; }
    }

    public class D365WebAPIService : ID365WebAPIService
    {
        private readonly ID365AuthenticationService _authenticationService;
        public string Application { get; set; }

        public ID365AuthenticationService D365AuthenticationService { get { return _authenticationService; } }
        public D365WebAPIService(ID365AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public HttpResponseMessage SendRetrieveRequestAsync(string query, Boolean formatted = false, int maxPageSize = 50)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());
            if (formatted)
                request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

            var client = _authenticationService.GetHttpClient(Application).Result;

            return client.SendAsync(request).Result;
        }

        public HttpResponseMessage SendCreateRequestAsync(string endPoint, string content)
        {
            return SendAsync(HttpMethod.Post, endPoint, content);
        }

        public HttpResponseMessage SendCreateRequestAsyncRtn(string endPoint, string content)
        {
            return SendAsyncRtn(HttpMethod.Post, endPoint, content);
        }

        public HttpResponseMessage SendUpdateRequestAsync(string endPoint, string body)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, endPoint);
            message.Headers.Add("Match", "*");
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = _authenticationService.GetHttpClient(Application).Result;
            return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendUploadFileRequestAsync(string endPoint, Stream fileContent)
        {

            var message = new HttpRequestMessage(HttpMethod.Patch, endPoint);
            message.Headers.Add("Match", "*");

            HttpContent? content = new StreamContent(fileContent);
            content.Headers.Add("Content-Type", "application/octet-stream");
            message.Content = content;

            var client = _authenticationService.GetHttpClient(Application).Result;
            return client.SendAsync(message).Result;

        }

        public HttpResponseMessage SendDeleteRequestAsync(string endPoint)
        {
            return _authenticationService.GetHttpClient(Application).Result.DeleteAsync(endPoint).Result;
        }

        public HttpResponseMessage SendCreateRequestAsync(HttpMethod httpMethod, string entitySetName, string body)
        {
            var message = new HttpRequestMessage(httpMethod, entitySetName);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var client = _authenticationService.GetHttpClient(Application).Result;

            return client.SendAsync(message).Result;
        }

        public HttpResponseMessage SendMessageAsync(HttpMethod httpMethod, string messageUri)
        {

            HttpRequestMessage message = new(httpMethod, messageUri);
            message.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
            var client = _authenticationService.GetHttpClient(Application).Result;
           

            // Send the message to the WebAPI. 
            return client.SendAsync(message).Result;
        }

        
        private HttpResponseMessage SendAsync(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint)  ;
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return _authenticationService.GetHttpClient(Application).Result.SendAsync(message).Result;
        }

        private HttpResponseMessage SendAsyncRtn(HttpMethod operation, string endPoint, string body)
        {
            var message = new HttpRequestMessage(operation, endPoint);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            message.Headers.Add("Prefer", "return=representation");

            return _authenticationService.GetHttpClient(Application).Result.SendAsync(message).Result;
        }
    }
}