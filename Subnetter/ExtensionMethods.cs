using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Subnetter {
    public static class ExtensionMethods {
        public static string SeperateEveryN(this string text, int N, char seperator) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {
                sb.Append(text[i]);
                if (i != 0 && (i + 1) < text.Length && (i + 1) % N == 0) {
                    sb.Append(seperator);
                }
            }
            return sb.ToString();
        }

        public static uint ClosestBit(this uint hosts, uint N = 0) {
            if (N > 32) return default;
            uint closestBit = (uint)Math.Pow(2, N);
            if (closestBit >= hosts) return N;
            return ClosestBit(hosts, ++N);
        }

        public static string ToStringWithoutCIDR(this IPNetwork ip) {
            return ip.Network.ToString().Split('/')[0];
        }

        public static string ToStringWithoutCIDR(this IPAddress ip) {
            return ip.ToString().Split('/')[0];
        }

        public static IPNetwork IncrementIP(this IPNetwork ip, uint cidr) {
            IPNetwork tmpIP = IPNetwork.Parse(ip.ToStringWithoutCIDR(), Convert.ToByte(cidr));
            byte[] bytes = tmpIP.Broadcast.GetAddressBytes();
            bytes[3]++;
            string text = "";
            for (int b = 0; b < bytes.Length; b++) text += bytes[b].ToString() + (b + 1 < bytes.Length ? "." : "");
            return IPNetwork.Parse(text + "/" + ip.Cidr);
        }

        private static string GetNextIpAddress(string ipAddress, uint increment) {
            byte[] addressBytes = IPAddress.Parse(ipAddress).GetAddressBytes().Reverse().ToArray();
            uint ipAsUint = BitConverter.ToUInt32(addressBytes, 0);
            var nextAddress = BitConverter.GetBytes(ipAsUint + increment);
            return String.Join(".", nextAddress.Reverse());
        }
    }
}
