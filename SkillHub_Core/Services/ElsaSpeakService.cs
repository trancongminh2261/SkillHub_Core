using LMSCore.Areas.Models;
using LMSCore.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using LMSCore.Areas.Request;
using System.Reflection;

namespace LMSCore.Services
{
    public class ElsaSpeakService : DomainService
    {
        private readonly string _token = "Elsa DX+gsaRdJgTQN7K3XtShZ5b6x/KnAn7pa7QPf777QVFj0zz3B0WzZBU4zno/EFt4ws7DNHLanjovnzTsa++qGDPglfdOJQoAXR+tCS4L8wc=";
        private readonly string URL = "https://api.elsanow.io/api/v3/score_audio";
        public ElsaSpeakService(lmsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ElsaApiResponse> ScriptedAsync(ElsaSpeakRequest request)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            if (string.IsNullOrEmpty(request.filePath) || string.IsNullOrEmpty(request.sentence))
                throw new Exception("Request not found");
            var result = new ElsaApiResponse();
            using (var httpClient = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                var audioResponse = await httpClient.GetAsync(request.filePath);
                if (audioResponse.IsSuccessStatusCode)
                {
                    var audioStream = await audioResponse.Content.ReadAsStreamAsync();
                    var audioContent = new StreamContent(audioStream);
                    audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
                    formData.Add(audioContent, "audio_file", "testRecord.mp3"); 
                }
                else
                {
                    throw new Exception("File not found");
                }
                formData.Add(new StringContent(request.sentence), "sentence");
                formData.Add(new StringContent("premium"), "api_plan");

                httpClient.DefaultRequestHeaders.Add("Authorization", _token);

                using (var response = await httpClient.PostAsync(URL, formData))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<ElsaApiResponse>(responseString);
                        
                        //if(result.Utterances != null && result.Utterances.Count > 0)
                        //{
                        //    var detail = result.Utterances.FirstOrDefault();
                        //    var words = result.Utterances.FirstOrDefault().Words;
                        //    //save data to tbl_UtteranceInfo
                        //    using (var db = new lmsDbContext())
                        //    {
                        //        var entity = await db.tbl_UserInformation.FindAsync(request.studentId);
                        //        if (entity == null)
                        //            throw new Exception("Không tìm thấy học viên");

                        //        tbl_UtteranceInfo utteranceInfo = new tbl_UtteranceInfo();
                        //        foreach (PropertyInfo me in utteranceInfo.GetType().GetProperties())
                        //        {
                        //            foreach (PropertyInfo item in detail.GetType().GetProperties())
                        //            {
                        //                if (me.PropertyType.Name == item.PropertyType.Name)
                        //                {
                        //                    me.SetValue(utteranceInfo, item.GetValue(detail));
                        //                }
                        //            }
                        //        }

                        //        //another json
                        //        string jsonString = JsonConvert.SerializeObject(detail.SpeedMetrics);
                        //        utteranceInfo.SpeedMetrics = jsonString;
                        //        jsonString = JsonConvert.SerializeObject(detail.PronunciationRateMetrics);
                        //        utteranceInfo.PronunciationRateMetrics = jsonString;
                        //        jsonString = JsonConvert.SerializeObject(detail.PausingMetrics);
                        //        utteranceInfo.PausingMetrics = jsonString;

                        //        db.tbl_UtteranceInfo.Add(utteranceInfo);

                        //        //word list
                        //        if (words != null && words.Count > 0)
                        //        {
                        //            foreach(var word in words)
                        //            {
                        //                tbl_WordInfo wordInfo = new tbl_WordInfo();
                        //                foreach (PropertyInfo me in wordInfo.GetType().GetProperties())
                        //                {
                        //                    foreach (PropertyInfo item in word.GetType().GetProperties())
                        //                    {
                        //                        if (me.PropertyType.Name == item.PropertyType.Name)
                        //                        {
                        //                            me.SetValue(wordInfo, item.GetValue(word));
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }

                        //        await db.SaveChangesAsync();
                        //    }
                        //}
                      
                    }
                    else
                    {
                        throw new Exception("Lỗi hệ thống. Vui lòng liên hệ nhà phát triển");
                    }
                }
            }

            return result;
        }
    }
}