
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Subnetter {
    class IPv3 {
        public uint RequiredHosts, SubnetBits, BitsBorrwedFromHost, UsableSubnets, UsableHosts, HostsNum, HostsBits;
        public string IPMaskBinary, IPMaskDecimal, SubnetIP, FirstHostIP, LastHostIP, BroadcastIP, DefaultMask, SubnetName;
        private uint Offset;
        private void SetIP(IPNetwork ip) {
            var mask = ip.Netmask;
            var bytes = mask.GetAddressBytes()
                .Select(x => Convert.ToString(x, 2).PadLeft(8, '0'));
            this.IPMaskBinary = string.Join(".", bytes);
            this.IPMaskDecimal = mask.ToString();

            this.UsableSubnets = (uint)Math.Pow(2, this.BitsBorrwedFromHost);
            this.UsableHosts = this.HostsNum - 2;
            this.SubnetIP = ip.ToStringWithoutCIDR();
            this.SubnetIP = IncrementIP(this.SubnetIP, this.Offset);
            this.FirstHostIP = ip.FirstUsable.ToStringWithoutCIDR();
            this.FirstHostIP = IncrementIP(this.FirstHostIP, this.Offset);
            this.LastHostIP = ip.LastUsable.ToStringWithoutCIDR();
            this.LastHostIP = IncrementIP(this.LastHostIP, this.Offset);
            this.BroadcastIP = ip.Broadcast.ToStringWithoutCIDR();
            this.BroadcastIP = IncrementIP(this.BroadcastIP, this.Offset);
        }

        public IPv3(IPNetwork ip, uint hostsRequired, string name, uint offset) {
            this.Offset = offset;
            this.SubnetName = name;
            uint hostsBit = hostsRequired.ClosestBit();
            uint hostsNum = (uint)Math.Pow(2, hostsBit);
            this.HostsNum = hostsNum;
            uint subnetBits = (uint)ip.Cidr + ((32 - (uint)ip.Cidr) - hostsBit);

            this.DefaultMask = ip.Netmask.ToString();
            this.HostsBits = hostsBit;
            this.SubnetBits = subnetBits;
            this.BitsBorrwedFromHost = (32 - (uint)ip.Cidr) - hostsBit;
            var newIP = IPNetwork.Parse($"{ip.ToString().Split('/')[0]}/{this.SubnetBits}");
            SetIP(newIP);
           

        }

        public string GetSolution() {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"\n\nSubnet {this.SubnetName}");
            result.AppendLine($"\nWe design for {this.HostsNum} hosts");
            result.AppendLine($"\n{this.HostsNum} = 2^{this.HostsBits} - 2 = {this.UsableHosts} usable hosts");
            result.AppendLine($"\nDefault mask: {this.DefaultMask} /{this.SubnetBits} ({this.SubnetBits - this.BitsBorrwedFromHost} + {this.BitsBorrwedFromHost})");
            result.AppendLine($"Borrowed bits: {this.BitsBorrwedFromHost}");
            result.AppendLine($"Subnets: 2^{this.BitsBorrwedFromHost} = {this.UsableSubnets}");
            result.AppendLine($"<strong>Number of bits in the subnet:</strong> {this.SubnetBits}");
            result.AppendLine($"<strong>Number of bits borrowed from host bits:</strong> {this.BitsBorrwedFromHost}");
            result.AppendLine($"<strong>New IP Mask (Binary):</strong> {this.IPMaskBinary}");
            result.AppendLine($"<strong>New IP Mask (Decimal):</strong> {this.IPMaskDecimal}");
            result.AppendLine($"<strong>Maximum number of usable subnets (Including 0th subnet):</strong> {this.UsableSubnets}");
            result.AppendLine($"<strong>Number of usable hosts per subnet:</strong> {this.HostsNum} - 2 = {this.UsableHosts}");
            result.AppendLine($"<strong>Subnet IP address:</strong> {this.SubnetIP}");
            result.AppendLine($"<strong>First Host IP address: </strong>{this.FirstHostIP}");
            result.AppendLine($"<strong>Last Host IP address:</strong> {this.LastHostIP}");
            result.AppendLine($"<strong>Broadcast IP address:</strong> {this.BroadcastIP}");

            return result.ToString();
        }

        private IPAddress IncrementIP(IPAddress ip, uint amount) {
            byte[] bytes = ip.GetAddressBytes();
            bytes[3] += (byte)amount;
            return IPAddress.Parse($"{new IPAddress(bytes).ToString()}/{this.SubnetBits}");
        }
        //private string IncrementIP(string input, uint amount) {
        //    byte[] bytes = IPAddress.Parse(input).GetAddressBytes();
        //    bytes[3] += (byte)amount;
        //    return new IPAddress(bytes).ToString();
        //}

        private string IncrementIP(string input, uint amount) {
            byte[] addressBytes = IPAddress.Parse(input).GetAddressBytes().Reverse().ToArray();
            uint ipAsUint = BitConverter.ToUInt32(addressBytes, 0);
            var nextAddress = BitConverter.GetBytes((uint)(ipAsUint + amount));
            return String.Join(".", nextAddress.Reverse());
        }
    }
}
