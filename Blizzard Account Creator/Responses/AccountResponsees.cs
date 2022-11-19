using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Blizzard_Account_Creator.Responses {
    class RequestResult : IResponse {
        public bool isSuccess { get; }
        public string Message { get; }
        public HttpResponseMessage HttpResponse { get; }
        public Account? Account { get; }
        public static RequestResult Empty = new RequestResult();

        public RequestResult(bool sucess, string msg, Account? acc = null, HttpResponseMessage httpMsg = null) {
            this.isSuccess = sucess;
            this.Message = msg;
            this.Account = acc;
            this.HttpResponse = httpMsg;
        }

        public RequestResult() {

        }
    }

    class VerificationResult : IResponse {
        public bool isSuccess { get; }

        public string Message { get; }
        public string PhoneNo { get; set; }
        public ActivationCodeResponse activationCodeResponse { get; }
        public static VerificationResult Empty = new VerificationResult();

        public VerificationResult(bool success, string msg, string phoneNo, ActivationCodeResponse code) {
            this.isSuccess = success;
            this.Message = msg;
            this.activationCodeResponse = code;
            this.PhoneNo = phoneNo;
        }
        public VerificationResult() {

        }
    }
}
