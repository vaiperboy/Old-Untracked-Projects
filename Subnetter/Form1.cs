using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subnetter {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //int[] testValues = { 66, 34, 17, 4 };
            //for (int i = 0; i < testValues.Length; i++) {
            //    DataGridViewRow row = (DataGridViewRow)table.Rows[0].Clone();
            //    row.Cells[0].Value = (char)(64 + table.Rows.Count);
            //    row.Cells[1].Value = testValues[i];
            //    table.Rows.Add(row);
            //}
            hostsNum.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e) {
            DataGridViewRow row = (DataGridViewRow)table.Rows[0].Clone();
            row.Cells[0].Value = (char)(64 + table.Rows.Count);
            row.Cells[1].Value = hostsNum.Value.ToString();
            table.Rows.Add(row);
            hostsNum.Value = 0;
        }

        private void classBox_SelectedIndexChanged(object sender, EventArgs e) {
            //byte[] bytes = new byte[4];
            //if (classBox.Text[0] >= 'A')
            //    bytes[0] = 255;
            //if (classBox.Text[0] >= 'B')
            //    bytes[1] = 255;
            //if (classBox.Text[0] >= 'C')
            //    bytes[2] = 255;
            //string text = "";
            //for (int b = 0; b < bytes.Length; b++) text += bytes[b].ToString();
            //text = text.PadRight(3 * 4, '0');
            //text = text.SeperateEveryN(3, '.');
            //subnetText.Text = text;
        }


        private void button2_Click(object sender, EventArgs e) {
            uint offset = 0;
            if (IPNetwork.TryParse(ipText.Text, out IPNetwork ip)) {
                output.Clear();
                string totalSolution = string.Empty;
                for (int i = 0; i < table.Rows.Count - 1; i++) {
                    uint hostsRequired = uint.Parse(table.Rows[i].Cells[1].Value.ToString());
                    IPv3 result = new IPv3(ip, hostsRequired, table.Rows[i].Cells[0].Value.ToString(), offset);
                    offset = uint.Parse(result.BroadcastIP.Substring(result.BroadcastIP.LastIndexOf('.') + 1)) + 1;
                    string solution = result.GetSolution();
                    solution = solution.Replace("<strong>", @"{\b ").Replace("</strong>", "}").Replace("<br />", @"{\line}")
                        .Replace(System.Environment.NewLine, @"{\line}");
                    totalSolution += solution + @"{\line}";
                    ip =  IPNetwork.Parse(ip.IncrementIP(result.SubnetBits).ToString());
                }
                output.Rtf = @"{\rtf1\ansi " + totalSolution + "}";
            } else MessageBox.Show("Invalid IP format", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void table_ColumnAdded(object sender, DataGridViewColumnEventArgs e) {
            e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}
