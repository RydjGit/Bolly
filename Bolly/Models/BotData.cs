using Bolly.Enums;
using System;
using System.Collections.Generic;

namespace Bolly.Models
{
    public class BotData
    {
        public Combo Combo { get; }
        public Status Status { get; set; }
        public int ResponseCode { get; set; }
        public string Address { get; set; }
        public string Source { get; set; }
        public Dictionary<string, string> Variables { get; }
        public Dictionary<string, string> Captues { get; }

        public BotData (Combo combo)
        {
            Combo = combo;
            Status = Status.None;
            Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Captues = new Dictionary<string, string>();
        }
    }
}