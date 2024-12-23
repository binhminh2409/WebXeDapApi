﻿using WebXeDapAPI.Models.Enum;

namespace WebXeDapAPI.Dto
{
    public class OrderDto
    {
        public int? Id { get; set; }
        public int? UserID { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipEmail { get; set; }
        public string ShipPhone { get; set; }
        public List<int> Cart { get; set; }

        public string? Status { get; set; }

        public string? No_  { get; set; }

        public string? Guid { get; set; }

        public override string ToString()
        {
            return $"OrderDto [UserID={UserID}, ShipName={ShipName}, ShipAddress={ShipAddress}, ShipEmail={ShipEmail}, ShipPhone={ShipPhone}, Cart=[{string.Join(",", Cart)}]]";
        }
    }
}
