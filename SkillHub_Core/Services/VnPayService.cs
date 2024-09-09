//using LMSCore.Areas.Models;
//using LMSCore.Models;
//using System;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;

//namespace LMSCore.Services
//{
//    public class VnPayService : DomainService
//    {
//        public VnPayService(lmsDbContext dbContext) : base(dbContext)
//        {
//        }
//        public async Task<string> Pay(VnPayPayment itemModel)
//        {
//            var model = await dbContext.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == itemModel.PaymentMethodId
//                                                                          && x.Active == true
//                                                                          && x.Enable == true);
//            if (model == null)
//                throw new Exception("Không tìm thấy phương thức thanh toán");
//            //Get Config Info
//            string VERSION = model.Version;
//            string vnp_Returnurl = itemModel.ReturnUrl; //URL nhan ket qua tra ve 
//            string vnp_Url = model.UrlPay; //URL thanh toan cua VNPAY 
//            string vnp_TmnCode = model.PartnerCode; //Ma định danh merchant kết nối (Terminal Id)
//            string vnp_HashSecret = model.Secretkey; //Secret Key, HashSerect bên VnPay
//            string command = model.Command;//pay

//            //Get payment input
//            tbl_PaymentDetail paymentInfo = new tbl_PaymentDetail();
//            var BillCode = itemModel.BillCode;
//            paymentInfo.BillCode = BillCode; //Mã giao dịch hệ thống merchant gửi sang VNPAY
//            var totalMoney = itemModel.Amount;
//            var longMoney = Convert.ToInt64(totalMoney);
//            paymentInfo.Amount = longMoney.ToString(); //Số tiền thanh toán hệ thống merchant gửi sang VNPAY 
//            paymentInfo.Status = 0; //0: Trạng thái thanh toán "chờ thanh toán" hoặc "Pending" khởi tạo giao dịch chưa có IPN
//            var dataTimeNow = DateTime.Now;
//            paymentInfo.CreatedOn=paymentInfo.ModifiedOn = dataTimeNow;
//            paymentInfo.CreatedBy = paymentInfo.ModifiedBy = "Hệ thống";
//            //Save order to db
//            dbContext.tbl_PaymentDetail.Add(paymentInfo);
//            await dbContext.SaveChangesAsync();
//            //Build URL for VNPAY
//            VnPayLibraryService vnpay = new VnPayLibraryService();

//            vnpay.AddRequestData("vnp_Version", VERSION);
//            vnpay.AddRequestData("vnp_Command", command);
//            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
//            vnpay.AddRequestData("vnp_Amount", (longMoney * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
//            int paymentOption = itemModel.PaymentOption;
//            if (paymentOption == 1)
//            {
//                vnpay.AddRequestData("vnp_BankCode", "");
//            }
//            else if (paymentOption == 2)
//            {
//                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
//            }
//            else if (paymentOption == 3)
//            {
//                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
//            }
//            else if (paymentOption == 4)
//            {
//                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
//            }
//            var dateTimeNow = dataTimeNow.ToString("yyyyMMddHHmmss");
//            vnpay.AddRequestData("vnp_CreateDate", dateTimeNow);
//            string CurrCode = model.CurrCode;
//            vnpay.AddRequestData("vnp_CurrCode", CurrCode);
//            var ipAdress = Utils.GetIpAddress();
//            vnpay.AddRequestData("vnp_IpAddr", ipAdress);
//            string location = itemModel.Location;
//            vnpay.AddRequestData("vnp_Locale", location); 
//            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + BillCode);// thông tin thanh toán
//            vnpay.AddRequestData("vnp_OrderType", itemModel.OrderType); //default value: other

//            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
//            vnpay.AddRequestData("vnp_TxnRef", paymentInfo.Id.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

//            //Add Params of 2.1.0 Version
//            //Billing

//            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
//            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
//            //Response.Redirect(paymentUrl); 
//            return paymentUrl;
//        }
//        public async Task<AppDomainResult> GetResultPayMent(VnPayPaymentCofirm cofirm)
//        { 
//            if (cofirm!=null)
//            {
//                var model = await dbContext.tbl_PaymentMethod.SingleOrDefaultAsync(x => x.Id == cofirm.paymentMethodId
//                                                                          && x.Active == true
//                                                                          && x.Enable == true);
//                if (model == null)
//                    throw new Exception("Không tìm thấy phương thức thanh toán lúc trả kết quả"); 
//                string vnp_HashSecret = model.Secretkey; //Chuoi bi mat
                  
//                VnPayLibraryService vnpay = new VnPayLibraryService();
//                vnpay.AddResponseData("vnp_Amount", cofirm.vnp_Amount);
//                vnpay.AddResponseData("vnp_BankCode", cofirm.vnp_BankCode);
//                vnpay.AddResponseData("vnp_CardType", cofirm.vnp_CardType);
//                vnpay.AddResponseData("vnp_OrderInfo", cofirm.vnp_OrderInfo);
//                vnpay.AddResponseData("vnp_PayDate ", cofirm.vnp_PayDate);
//                vnpay.AddResponseData("vnp_ResponseCode", cofirm.vnp_ResponseCode);
//                vnpay.AddResponseData("vnp_TmnCode", cofirm.vnp_TmnCode);
//                vnpay.AddResponseData("vnp_TransactionNo", cofirm.vnp_TransactionNo);
//                vnpay.AddResponseData("vnp_TransactionStatus", cofirm.vnp_TransactionStatus);
//                vnpay.AddResponseData("vnp_TxnRef", cofirm.vnp_TxnRef);
//                vnpay.AddResponseData("vnp_SecureHash", cofirm.vnp_SecureHash); 
                 
//                long billCode = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));//vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
//                var vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));//vnp_TransactionNo: Ma GD tai he thong VNPAY
//                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");//vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
//                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
//                String vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash"); //vnp_SecureHash: HmacSHA512 cua du lieu tra ve
//                String terminalID = vnpay.GetResponseData("vnp_TmnCode");
//                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
//                String bankCode = vnpay.GetResponseData("vnp_BankCode"); 

//                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
//                if (checkSignature)
//                {
//                    tbl_PaymentDetail paymentDetail = new tbl_PaymentDetail();
//                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
//                    {
//                        //Thanh toan thanh cong  
//                        paymentDetail.Status = 1;
//                        paymentDetail.PayInfo = "Giao dịch được thực hiện thành công";
//                    }
//                    else
//                    {
//                        // Thanh toán thất bại 
//                        paymentDetail.Status = 2; 
//                        paymentDetail.PayInfo = "Giao dịch thực hiện không thành công"; 
//                    }
//                    paymentDetail.PaymentType = "VnPay";
//                    paymentDetail.BillCode = cofirm.vnp_OrderInfo;
//                    paymentDetail.Amount = cofirm.vnp_Amount;
//                    paymentDetail.PayStatus = vnp_ResponseCode;
//                    paymentDetail.WebsiteCode = terminalID;
//                    paymentDetail.PurcharCode = vnpayTranId.ToString();
//                    paymentDetail.BankCode = bankCode;
//                    dbContext.tbl_PaymentDetail.Add(paymentDetail);
//                    await dbContext.SaveChangesAsync();
//                    return new AppDomainResult { Data = paymentDetail, ResultMessage=paymentDetail.StatusName };
//                }
//                else
//                {
//                    throw new Exception("Có lỗi xảy ra trong quá trình xử lý");
//                }
//            }
//            else
//            { 
//                return new AppDomainResult { Data = null, ResultMessage="Unknown" };
//            }
//        }
//    }
//}