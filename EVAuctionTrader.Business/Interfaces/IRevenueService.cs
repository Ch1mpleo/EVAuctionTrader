using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IRevenueService
    {
        Task<RevenueSummaryDto> GetRevenueSummaryAsync(int? year = null, int? month = null);
        Task<Pagination<RevenueDetailDto>> GetRevenueDetailsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? year = null,
            int? month = null);
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenuesAsync(int year);
        Task<List<DailyRevenueDto>> GetDailyRevenuesAsync(int year, int month);
    }
}
