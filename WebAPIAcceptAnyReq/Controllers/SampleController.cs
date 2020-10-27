using HttpMultipartParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WebAPIAcceptAnyReq.Controllers
{
    public class SampleController : ApiController
    {
        Logger logger = LogManager.GetLogger("universalLogger");

        [HttpPost]
        [Route("RouteStart/{routeId}/IncomingReq")]
        public HttpResponseMessage IncomingReq(string routeId)
        {
            HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.BadRequest);

            try
            {
                //取原始请求字串
                var notifyStream = this.Request.Content.ReadAsStreamAsync().Result;
                notifyStream.Seek(0, SeekOrigin.Begin);
                var notifyString = (new StreamReader(notifyStream)).ReadToEnd();

                var contentType = String.IsNullOrEmpty(Request.Content.Headers.ContentType?.MediaType) ? "" : Request.Content.Headers.ContentType?.MediaType;

                string parseResult = "";
                switch (contentType)
                {
                    case "multipart/form-data":
                        notifyStream.Seek(0, SeekOrigin.Begin);
                        var result = MultipartFormDataParser.Parse(notifyStream);
                        foreach(var item in result.Parameters)
                        {
                            parseResult += $"\n\t{item.Name}:{item.Data}";
                        }
                        break;
                    case "application/x-www-form-urlencoded":
                        NameValueCollection data = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(notifyString));
                        Dictionary<string, string> input = new Dictionary<string, string>();
                        foreach (var key in data.AllKeys)
                        {
                            parseResult += $"\n\t{key}:{data[key]}";
                        }
                        break;
                    case "text/plain":
                        parseResult = HttpUtility.UrlDecode(notifyString);
                        break;
                    case "application/javascript":
                        parseResult = HttpUtility.UrlDecode(notifyString);
                        break;
                    case "application/json":
                        JObject obj = JsonConvert.DeserializeObject<JObject>(HttpUtility.UrlDecode(notifyString));
                        foreach(var p in obj)
                        {
                            parseResult += $"\n\t{p.Key}:{p.Value}";
                        }
                        break;
                    case "text/html":
                        parseResult = HttpUtility.UrlDecode(notifyString);
                        break;
                    case "application/xml":
                        parseResult = HttpUtility.UrlDecode(notifyString);
                        break;
                    default:
                        parseResult = HttpUtility.UrlDecode(notifyString);
                        break;
                }

                response.Content = new StringContent($"contentType: {contentType}\nrouteId: {routeId}\nnotifyString: {notifyString}\nparseResult: {parseResult}");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Content = new StringContent("未知错误");
            }

            return response;
        }
    }
}
