namespace LMSCore.Models
{
    public class tbl_PaymentMethod : DomainEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        //thông tin key 
        public string Version { get; set; }//Phiên bản api mà merchant kết nối.
        public string PartnerCode { get; set; }// partner code,TmnCode
        public string AccessKey { get; set; }// không dùng cho vn pay
        public string Secretkey { get; set; }
        public string OrderType { get; set; } = "orther";//topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
        public string UrlPay { get; set; }// URL dẫn đến trang thanh toán--enpoint
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        // vnpay
        /// <summary>
        /// Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Đơn vị tiền tệ sử dụng thanh toán. VND
        /// </summary>
        public string CurrCode { get; set; }
        /// <summary>
        /// ngôn ngữ
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// publickey dùng cho momo
        /// </summary>
        public string PublicKey { get; set; }
        public tbl_PaymentMethod() : base() { }
        public tbl_PaymentMethod(object model) : base(model) { }
    }
    public class VnPayPayment
    {
        public int PaymentMethodId { get; set; }
        public string BillCode { get; set; }
        public double Amount { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        /// <summary>
        /// Cách 1: Chuyển hướng sang VNPAY chọn phương thức thanh toán
        /// 1:Cổng thanh toán VNPAYQR
        /// Cách 2: Tách phương thức thanh toán tại site của Merchant
        /// 2:Thanh toán qua ứng dụng hỗ trợ VNPAYQR
        /// 3:ATM-Tài khoản ngân hàng nội địa
        /// 4:Thanh toán qua thẻ quốc tế
        /// </summary>
        public int PaymentOption { get; set; } = 1;
        public string Location { get; set; } = "vn";
        public string OrderType { get; set; } = "other";
    }
    public class VnPayPaymentCofirm
    {
        public int paymentMethodId { get; set; }
        public string vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_CardType { get; set; }
        /// <summary>
        /// Có bill code trong đó
        /// </summary>
        public string vnp_OrderInfo { get; set; }
        public string vnp_PayDate { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TransactionNo { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_SecureHash { get; set; }
    }

    public class MomoPayment
    {
        public int PaymentMethodId { get; set; }
        public string BillCode { get; set; }
        public double Amount { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
    }
}