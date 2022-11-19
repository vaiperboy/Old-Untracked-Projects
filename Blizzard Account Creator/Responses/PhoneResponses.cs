using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Blizzard_Account_Creator.Responses {
    interface IResponse {
        bool isSuccess { get; }
        string Message { get; }
    }
    public class GeneratePhoneResponse : IResponse {
        public bool isSuccess { get; }
        public string Phone { get; }
        public string OrderID { get; }

        public string Message { get; }
        public static GeneratePhoneResponse Empty = new GeneratePhoneResponse();

        public GeneratePhoneResponse(bool success, string phone, string order, string msg = "") {
            this.isSuccess = success;
            this.Phone = phone;
            this.OrderID = order;
            this.Message = msg;
        }


        public GeneratePhoneResponse(bool success, string msg) {
            this.isSuccess = success;
            this.Message = msg;
        }

        public GeneratePhoneResponse() {

        }

        public override string ToString() {
            return this.Message;
        }
    }

    public class ActivationCodeResponse : IResponse {
        public bool isSuccess { get; }
        public bool Timeout { get; }
        public string Code { get; }
        public string Message { get; }
        public static ActivationCodeResponse Empty = new ActivationCodeResponse();

        public ActivationCodeResponse(bool sucess, string code, string msg) {
            this.isSuccess = sucess;
            this.Code = code;
            this.Message = msg;
        }

        public ActivationCodeResponse(bool success, string msg, bool timeout) {
            this.isSuccess = success;
            this.Message = msg;
            this.Timeout = timeout;
        }
        public ActivationCodeResponse(bool success, string msg) {
            this.isSuccess = success;
            this.Message = msg;
        }
        public ActivationCodeResponse() {

        }
    }

    public class SetActivationCodeResponse : IResponse {
        public bool isSuccess { get; }

        public string Message { get; }
        public static SetActivationCodeResponse Empty = new SetActivationCodeResponse();
        public SetActivationCodeResponse(bool success, string msg) {
            this.isSuccess = success;
            this.Message = msg;
        }
        public SetActivationCodeResponse() {

        }
    }
}
