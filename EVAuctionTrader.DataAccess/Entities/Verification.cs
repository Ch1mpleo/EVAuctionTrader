using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Verification : BaseEntity
    {
        public string SubjectType { get; set; } // user, listing, vehicle, battery, auction
        public Guid SubjectId { get; set; }

        public VerificationStatus Status { get; set; }
        public VerificationMethod Method { get; set; }
        public string Note { get; set; }

        public string FacilityName { get; set; }
        public string FacilityReportUrl { get; set; }
        public DateTime? FacilityVerifiedAt { get; set; }

        public Guid? VerifiedBy { get; set; }
        public User VerifiedByUser { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
