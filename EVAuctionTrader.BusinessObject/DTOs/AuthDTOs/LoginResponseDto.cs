using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVAuctionTrader.BusinessObject.DTOs.AuthDTOs
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
    }
}
