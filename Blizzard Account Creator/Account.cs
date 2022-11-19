using Blizzard_Account_Creator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blizzard_Account_Creator {
    public class Account {
        public string FirstName { get;  set; }
        public string LastName { get;  set; }
        public string Email { get;  set; }
        public string Password { get;  set; }
        public string Country { get;  set; }
        private DateTime DOB { get; set; }
        public string DOB_Formatted { get {
                return this.DOB.ToString("MM/dd/yyyy");
            } set {
                DOB_Formatted = value;
            }
        }
        public string DateCreated { get; set; }
        public int RecoveryQuestion { get;  set; }
        public string RecoveryAnswer { get;  set; }
        public string Phone { get; set; }
        public bool IsPhoneVerified { get {
                return !string.IsNullOrEmpty(this.Phone);
            } }
        public ProxyV2 ProxyCreated { get; set; }
        public EmailProvider OriginalEmail { get; set; }
        private readonly List<string> Names;
        private static readonly Random rand = new Random();
        public Account(List<string> names) {
            this.Names = names;
        }
        private Account() {

        }
        public Account GenerateAccount(EmailProvider ogEmail, string emailFormat, string password = "", int emailLength = 8, string country="KAZ") {
            return new Account {
                FirstName = this.Names[rand.Next(0, this.Names.Count)],
                LastName = this.Names[rand.Next(0, this.Names.Count)],
                Email = emailLength != 0 ? String.Format(emailFormat, Helper.RandomString(emailLength)) : emailFormat,
                Password = String.IsNullOrEmpty(password) ? GenPass(ogEmail, 10) : password,
                Country = country,
                DOB = new DateTime(day: 1, month: 1, year: 1980),
                RecoveryQuestion = 19,
                RecoveryAnswer = "none",
                OriginalEmail = ogEmail
            };
        }

        private string GenPass(EmailProvider ogEmail, int length) {
            return isPassFormatValid(ogEmail.Password) ? ogEmail.Password
                : Helper.RandomString(length);
        }
        private bool isPassFormatValid(string pass = "") {
            return pass.Length >= 8 && pass.Any(char.IsLetter) && pass.Any(char.IsNumber);
        }
        /// <summary>
        /// This should be at the end of the payload. REMEMBER!!!!!!!
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"&country={this.Country}&" +
                $"firstName={this.FirstName}&" +
                $"lastName={this.LastName}&" +
                $"dobMonth={(int)this.DOB.Month}&" +
                $"dobDay={(int)this.DOB.Day}&" +
                $"dobYear={(int)this.DOB.Year}&" +
                $"emailAddress={this.Email}&" +
                $"password={this.Password}&" +
                $"question1={this.RecoveryQuestion}&" +
                $"answer1={this.RecoveryAnswer}";
        }

        /// <summary>
        /// This should be at the end of the payload. REMEMBER!!!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> AsHeaders() {
            return new Dictionary<string, string> {
                {"country", "112" },
                {"dobMonth", this.DOB.Month.ToString() },
                {"dobDay", this.DOB.Day.ToString() },
                {"dobYear", this.DOB.Year.ToString() },
                {"capture-first-name", this.FirstName },
                {"capture-last-name", this.LastName },
                {"capture-email", this.Email }
        };
        }
    }
}
