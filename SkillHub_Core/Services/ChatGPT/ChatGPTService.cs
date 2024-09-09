using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.ClassTranscript;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using LMSCore.DTO.ChatGPT;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using LMSCore.DTO.BandDescriptor;

namespace LMSCore.Services.ChatGPT
{
    public class ChatGPTService : DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        public static string apiKey = configuration.GetSection("ChatGPT:ApiKey").Value.ToString();
        public static string chatAPI = configuration.GetSection("ChatGPT:ChatAPI").Value.ToString();

        public ChatGPTService(lmsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IList<BandDescriptorOption>> GetBandDescriptor(int taskOrder)
        {
            return await dbContext.tbl_BandDescriptor.Where(x => x.Enable == true && x.TaskOrder == taskOrder)
                .Select(x => new BandDescriptorOption 
                { 
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
        }

        public async Task<tbl_CheckWritingResponse> CheckWriting(CheckWritingRequestPost request, tbl_UserInformation currentUser)
        {
            using(var tran = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var historyCheckWriting = await dbContext.tbl_HistoryCheckWriting.SingleOrDefaultAsync(x => x.Enable == true && x.Id == request.HistoryCheckWritingId);
                    if (historyCheckWriting == null)
                        throw new Exception("Không tìm thấy lịch sử chấm bài");

                    var bandDescriptor = await dbContext.tbl_BandDescriptor.SingleOrDefaultAsync(x => x.Enable == true && x.Id == request.BandDescriptorId);
                    if (bandDescriptor == null)
                        throw new Exception("Không tìm thấy tiêu chí đánh giá");

                    HttpClient client = new HttpClient();

                    // Nội dung JSON cần gửi
                    var requestBody = new
                    {
                        model = "gpt-4o",
                        messages = new[]
                        {
                                new
                                {
                                    role = "system",
                                    content = "You are an IELTS Writing expert with deep understanding of the IELTS Writing band descriptors. Your role is to evaluate student essays in a detailed manner, focusing on Task Achievement/Response, Coherence and Cohesion, Lexical Resource, and Grammatical Range and Accuracy. Highlight specific errors, give constructive feedback, and provide an estimated band score. Make sure to give clear and actionable advice for improvement."
                                },
                                new
                                {
                                    role = "user",
                                    content = $@"Please evaluate the following IELTS Writing Task {bandDescriptor.TaskOrder} essay in detail according to the criteria for {bandDescriptor.Name} {bandDescriptor.Description}."
                                },
                                new
                                {
                                    role = "user",
                                    content = $@"**Topic:** {historyCheckWriting.Question}"

                                },
                                new
                                {
                                    role = "user",
                                    content = $@"**Student's Essay:** {historyCheckWriting.Answer}"
                                }
                        }
                    };


                    // Chuyển đổi đối tượng C# thành chuỗi JSON
                    string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

                    // Tạo request message
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, chatAPI);
                    requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
                    requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Gửi request và nhận response
                    HttpResponseMessage response = await client.SendAsync(requestMessage);

                    if (response != null)
                    {
                        // Kiểm tra mã trạng thái của response
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // Đọc và hiển thị nội dung response
                            string responseContent = await response.Content.ReadAsStringAsync();

                            // Phân tích cú pháp JSON của phản hồi để lấy content
                            var openAiResponse = JsonConvert.DeserializeObject<ChatResponseDTO>(responseContent);

                            // Lấy nội dung của message trong phần tử đầu tiên của choices
                            string content = openAiResponse?.choices?[0]?.message?.content;

                            //lưu thông tin response từ gpt
                            var checkWritingResponse = new tbl_CheckWritingResponse();
                            checkWritingResponse.HistoryCheckWritingId = request.HistoryCheckWritingId;
                            checkWritingResponse.BandDescriptorId = request.BandDescriptorId;
                            var score = "";
                            var findScore = Regex.Match(content, @"\{([^}]+)\}");
                            if (findScore.Success)
                            {
                                score = findScore.Groups[1].Value;
                            }
                            checkWritingResponse.Score = score;
                            checkWritingResponse.GPTAnswer = ConvertTextToHTML(content);
                            checkWritingResponse.Enable = true;
                            checkWritingResponse.CreatedOn = DateTime.Now;
                            checkWritingResponse.CreatedBy = currentUser.FullName;
                            checkWritingResponse.ModifiedOn = DateTime.Now;
                            checkWritingResponse.ModifiedBy = currentUser.FullName;
                            dbContext.tbl_CheckWritingResponse.Add(checkWritingResponse);
                            await dbContext.SaveChangesAsync();

                            tran.Commit();
                            // Xử lý nội dung response ở đây
                            return checkWritingResponse;
                        }
                        else
                        {
                            // Xử lý khi mã trạng thái không phải là 200
                            string errorMessage = $"Yêu cầu chấm bài không thành công.";
                            throw new Exception(errorMessage);
                        }
                    }
                    else
                    {
                        throw new Exception("Có lỗi xảy ra trong quá trình chấm bài.");
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }                     
        }     
        
        public string ConvertTextToHTML(string input)
        {
            string output = input.Replace("#", "");
            // Replace **text** with <strong>text</strong>
            output = Regex.Replace(input, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            // Replace \n\n with </p><p> for paragraphs
            output = output.Replace("\n\n", "</p><p>");

            // Replace single \n with <br> for line breaks
            output = output.Replace("\n", "<br>");

            // Handle lists by replacing "- " with <li> and closing the list with </ul>
            output = Regex.Replace(output, @"\n\s*-\s", "<li>");
            output = Regex.Replace(output, @"(<li>.*?)(?=<p>)", "$1</ul>");

            // Add opening and closing <ul> tags around lists
            output = Regex.Replace(output, @"<li>", "<ul><li>");
            output = Regex.Replace(output, @"</ul>\s*<li>", "</ul><ul><li>");

            // Add the initial <p> and final </p> tags
            output = "<p>" + output + "</p>";
            return output;
        }
    }
}
