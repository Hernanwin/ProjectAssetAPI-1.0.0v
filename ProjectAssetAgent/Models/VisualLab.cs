using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectAssetAgent.Models
{
    public class VisualLab
    {
        public int Id { get; set; }

        public string Hostname { get; set; }

        public string IPAddress { get; set; }

        public string OperatingSystem { get; set; }

        public string Status { get; set; }
    }
}
