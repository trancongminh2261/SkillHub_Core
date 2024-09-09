using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace LMSCore.Services
{
    public class PythonService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static IWebHostEnvironment _hostingEnvironment;
        public PythonService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public class ReadTextModel
        { 
            /// <summary>
            /// 0 Nam
            /// 1 Nữ
            /// </summary>
            public int TypeVoice { get; set; }
            public string Content { get; set; }
        }
        public static (string,string) ConvertTextToSpeech(ReadTextModel itemModel, string domain)
        {
            try
            {
                string domainPython = configuration.GetSection("MySettings:DomainPython").Value.ToString();
                var client = new RestClient($"{domainPython}/text-to-speech");
                var request = new RestRequest(Method.POST);
                request.RequestFormat = DataFormat.Json;
                var time = DateTime.Now.Subtract(new DateTime(2020, 01, 01));
                string fileName = $"voice{Math.Round(time.TotalSeconds, 0)}.mp3";

                var body = new
                {
                    typeVoice = itemModel.TypeVoice,
                    text = itemModel.Content,
                    fileName = $"Speech/{fileName}",
                };

                request.AddJsonBody(body);
                request.AddHeader("Content-Type", "application/json");

                IRestResponse restResponse = client.Execute(request);
                HttpStatusCode statusCode = restResponse.StatusCode;
                int numericStatusCode = (int)statusCode;

                return (domain + "TextToSpeech/Speech/" + fileName, restResponse.Content);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //public static (string, string) ConvertTextToSpeech(ReadTextModel itemModel, string pathDist, string pathUpload, string domain)
        //{
        //    try
        //    {
        //        var psi = new ProcessStartInfo();
        //        int typeVoice = itemModel.TypeVoice;//1 Nữ 0 Nam
        //        string content = itemModel.Content;
        //        var time = DateTime.Now.Subtract(new DateTime(2020, 01, 01));
        //        string fileName = $"voice{Math.Round(time.TotalSeconds, 0)}.mp3";
        //        psi.FileName = $"{pathDist}\\ReadText.exe";
        //        psi.UseShellExecute = false;
        //        psi.Arguments = string.Format("{0} \"{1}\" {2}", typeVoice, content, fileName);
        //        psi.CreateNoWindow = true;
        //        psi.WorkingDirectory = $"{pathUpload}\\PythonToMp3\\";
        //        psi.RedirectStandardOutput = true;
        //        psi.RedirectStandardError = true;

        //        var result = "";
        //        string stderr = "";
        //        using (var process = Process.Start(psi))
        //        {
        //            result = process.StandardOutput.ReadToEnd();
        //            stderr = process.StandardError.ReadToEnd();
        //        }
        //        return (domain + "Upload/PythonToMp3/" + fileName, result + "***" + stderr + "***" + psi.FileName + "***" + psi.Arguments);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
    }
}