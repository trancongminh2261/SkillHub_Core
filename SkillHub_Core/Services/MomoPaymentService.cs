using LMSCore.Models;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class MomoPaymentService : DomainService
    {
        public MomoPaymentService(lmsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<string> PaymentByMomo(MomoPayment itemModel)
        {
            var model = await dbContext.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == itemModel.PaymentMethodId
                                                                          && x.Active == true
                                                                          && x.Enable == true);
            if (model == null)
                throw new Exception("Không tìm thấy phương thức thanh toán");
            //request params need to request to MoMo system
            string endpoint = model.UrlPay;
            string partnerCode = model.PartnerCode;
            string accessKey = model.AccessKey;
            string secretkey = model.Secretkey;
            string orderInfo = $"Thanh toán mã hóa đơn {itemModel.BillCode}";//mô tả
            string returnUrl = itemModel.ReturnUrl;
            string notifyurl = itemModel.NotifyUrl; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test
            int money = (int)itemModel.Amount;
            string amount = money.ToString();
            string orderid = itemModel.BillCode; //mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string requestType = model.Command;
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            //sign signature SHA256
            string signature = MomoLibraryService.signSHA256(rawHash, secretkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };

            string responseFromMomo = MomoLibraryService.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return jmessage.GetValue("payUrl").ToString();
        }
        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public async Task<bool> ConfirmPaymentClient(MomoPaymentResult result)
        {
            ////lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            //string rMessage = result.Message;
            //string rOrderId = result.OrderId;
            //string rErrorCode = result.ErrorCode; // = 0: thanh toán thành công 

            bool isSuccess = false;
            tbl_PaymentDetail paymentDetail = new tbl_PaymentDetail();
            if (result.ErrorCode == "0" || result.ErrorCode == "9000") // Momo thanh toán thành công
            {
                //Thanh toan thanh cong  
                paymentDetail.Status = 1;
                paymentDetail.PayInfo = "Giao dịch được thực hiện thành công";
                paymentDetail.PaymentType = "VnPay";
                paymentDetail.BillCode = result.OrderInfo;
                paymentDetail.Amount = result.Amount;
                paymentDetail.PayStatus = result.ErrorCode;
                paymentDetail.PayInfo = result.LocalMessage;
                paymentDetail.WebsiteCode = null;
                paymentDetail.PurcharCode = result.RequestId;
                paymentDetail.BankCode = null;
                dbContext.tbl_PaymentDetail.Add(paymentDetail);
                await dbContext.SaveChangesAsync();
                isSuccess = true;
            }
            else
            {
                // Thanh toán thất bại 
                paymentDetail.Status = 2;
                paymentDetail.PayInfo = "Giao dịch thực hiện không thành công";
                paymentDetail.PaymentType = "VnPay";
                paymentDetail.BillCode = result.OrderInfo;
                paymentDetail.Amount = result.Amount;
                paymentDetail.PayStatus = result.ErrorCode;
                paymentDetail.PayInfo = result.LocalMessage;
                paymentDetail.WebsiteCode = null;
                paymentDetail.PurcharCode = result.RequestId;
                paymentDetail.BankCode = null;
                dbContext.tbl_PaymentDetail.Add(paymentDetail);
                await dbContext.SaveChangesAsync();
                isSuccess = false;
            }
            return isSuccess;
        }
        public class MomoPaymentResult
        {
            public string PartnerCode { get; set; }
            public string AccessKey { get; set; }
            public string RequestId { get; set; }
            public string Amount { get; set; }
            public string OrderId { get; set; }
            public string OrderInfo { get; set; }
            public string OrderType { get; set; }
            public string TransId { get; set; }
            public string Message { get; set; }
            public string LocalMessage { get; set; }
            public string ResponseTime { get; set; }
            public string ErrorCode { get; set; }
            public string PayType { get; set; }
            public string ExtraData { get; set; }
            public string Signature { get; set; }
        }
    }
}