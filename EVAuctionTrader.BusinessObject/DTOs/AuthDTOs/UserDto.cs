using EVAuctionTrader.BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVAuctionTrader.BusinessObject.DTOs.AuthDTOs
{
    public class UserDto
    {
        public string FullName { get; set; } 
        public string Email { get; set; }
        public string Phone { get; set; }
        public RoleType Role { get; set; }
    }
}
