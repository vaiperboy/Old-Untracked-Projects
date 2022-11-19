using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Responses {
    public class ReadWriteResponse {
        public bool IsSucess { get; set; }
        public string Message { get; set; }
        public ReadWriteResponse(bool success, string message = "") {
            this.IsSucess = success;
            this.Message = message;
        }
    }
}
