using Microsoft.AspNetCore.Mvc;
using PaymentIntegration.Manager;
using PaymentIntegration.Model;
using PaymentIntegration.Net5Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentIntegration.Net5Example.Controllers
{
    public class PaymentController : Controller
    {
        //Ask for this information
        private const string ApiUrl = "";
        private const string ApiKey = "";
        private const string SecretKey = "";

        PaymentOptions paymentOptions;
        public PaymentController()
        {
            paymentOptions = new PaymentOptions(ApiUrl, ApiKey, SecretKey);
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ErrorViewModel() { ErrorMessage = "" });
        }
        [HttpPost]
        public async Task<IActionResult> Index(PaymentModel paymentModel)
        {

            var auth3DRequest = new Auth3DRequest();
            auth3DRequest.Amount = 500;//5.00
            auth3DRequest.CardNo = paymentModel.Cardnumber;
            auth3DRequest.Currency = 978;
            auth3DRequest.Cvv2 = paymentModel.Cvv;
            auth3DRequest.Ecommerce = true;
            auth3DRequest.Expiry = int.Parse(paymentModel.Expyear.Substring(2, 2) + paymentModel.Expmonth);
            auth3DRequest.InstallmentCount = 1;
            auth3DRequest.Lang = "TR";
            auth3DRequest.OrderId = DateTime.Now.Ticks.ToString();

            var payment = new Payment(paymentOptions);
            var response = await payment.Auth3D(auth3DRequest);
            if (response.IsConnectionSuccess)
                if (response.Result.State == PaymentState.Success)
                {
                    var html64 = response.Result.Result.HtmlContent;
                    var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(html64));
                    return Content(htmlContent, "text/html");
                }
                else
                    return View(new ErrorViewModel() { ErrorMessage = response.Result.Result.ResultMessage });
            return View(new ErrorViewModel() { ErrorMessage = "Connection Error:" + response.StatusCode });
        }
        [HttpGet]
        public async Task<IActionResult> ReturnUrl(string token)//Your return url must be defined.
        {
            var checkPaymentRequest = new CheckPaymentRequest();
            checkPaymentRequest.Token = token;
            var payment = new Payment(paymentOptions);
            var response = await payment.CheckPayment(checkPaymentRequest);


            var response2 = await payment.CheckByProcessId(new CheckByProcessIdRequest() { ProcessId = response.Result.Result.ProcessId });
            var response3 = await payment.CheckByToken(new CheckByTokenRequest() { Token = token });
            var response4 = await payment.CheckOrderId(new CheckByOrderIdRequest() { OrderId = response.Result.Result.OrderId });

            if (response.IsConnectionSuccess)
                return Ok(response.Result.Result);//Just a quick example :)
            return View();
        }
    }
}
