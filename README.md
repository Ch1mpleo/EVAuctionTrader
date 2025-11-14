# EVAuctionTrader

Nền tảng đấu giá và mua bán xe điện (Electric Vehicle) và pin đã qua sử dụng.

## 🚀 Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: PostgreSQL 15
- **Real-time**: SignalR (Auction bidding & Chat)
- **Payment**: Stripe Integration
- **Authentication**: JWT Token
- **Containerization**: Docker & Docker Compose

## 📁 Cấu trúc dự án

```
EVAuctionTrader/
├── EVAuctionTrader.Business/          # Business Logic & Services
├── EVAuctionTrader.BusinessObject/    # DTOs & Entities
├── EVAuctionTrader.DataAccess/        # Database Context & Repositories
├── EVAuctionTrader.Presentation/      # Razor Pages & UI
└── docker-compose.yml                 # Docker configuration
```

## ⚙️ Yêu cầu hệ thống

- Docker Desktop
- .NET 8.0 SDK (nếu chạy local)
- PostgreSQL 15 (nếu chạy local)

## 🏃 Hướng dẫn chạy ứng dụng

### Cách 1: Chạy với Docker (Khuyến nghị)

1. **Clone repository**
```bash
git clone https://github.com/Ch1mpleo/EVAuctionTrader.git
cd EVAuctionTrader
```

2. **Chạy với Docker Compose**
```bash
docker-compose up --build
```

3. **Truy cập ứng dụng**
- Web Application: http://localhost:5000
- PostgreSQL: localhost:5433
  - Database: `EVAuctionTraderDb`
  - Username: `postgres`
  - Password: `postgres`

### Cách 2: Chạy local (Development)

1. **Cài đặt PostgreSQL** và tạo database `EVAuctionTraderDb`

2. **Cập nhật connection string** trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EVAuctionTraderDb;Username=postgres;Password=your_password"
  }
}
```

3. **Chạy migration** (tự động khi khởi động app)

4. **Chạy ứng dụng**
```bash
cd EVAuctionTrader.Presentation
dotnet run
```

## 👥 Tài khoản mặc định (Seeded Data)

**Admin:**
- Email: `admin@gmail.com`
- Password: `1@`

**Member 1:**
- Email: `customer1@gmail.com`
- Password: `1@`
- Wallet Balance: 50,000 VND

**Member 2:**
- Email: `customer2@gmail.com`
- Password: `1@`
- Wallet Balance: 75,000 VND

## 🎯 Chức năng chính

### 1. Quản lý người dùng
- ✅ Đăng ký/Đăng nhập với JWT Authentication
- ✅ Phân quyền: Admin, Staff, Member
- ✅ Quản lý hồ sơ cá nhân
- ✅ Hệ thống ví điện tử (Wallet)

### 2. Quản lý bài đăng (Posts)
- ✅ Tạo bài đăng bán xe điện hoặc pin
- ✅ Upload nhiều ảnh sản phẩm
- ✅ Hai loại bài đăng:
  - **Standard**: Miễn phí, hiển thị 7 ngày
  - **VIP**: Trả phí, nổi bật, hiển thị 28 ngày
- ✅ Tìm kiếm và lọc bài đăng
- ✅ Bình luận trên bài đăng
- ✅ Quản lý trạng thái (Active, Sold, Removed, Banned)

### 3. Hệ thống đấu giá (Auction)
- ✅ Tạo phiên đấu giá cho xe điện hoặc pin
- ✅ Đặt giá khởi điểm, bước giá tối thiểu
- ✅ Yêu cầu đặt cọc (Deposit) để tham gia
- ✅ **Real-time bidding** với SignalR
- ✅ Tự động cập nhật giá cao nhất
- ✅ Thông báo kết thúc đấu giá
- ✅ Xử lý thanh toán cho người thắng

### 4. Hệ thống thanh toán
- ✅ Tích hợp **Stripe Payment Gateway**
- ✅ Nạp tiền vào ví
- ✅ Thanh toán phí VIP post
- ✅ Thanh toán đặt cọc đấu giá
- ✅ Lịch sử giao dịch chi tiết

### 5. Chat Real-time
- ✅ Nhắn tin trực tiếp giữa người dùng
- ✅ SignalR Hub cho chat thời gian thực
- ✅ Quản lý cuộc hội thoại

### 6. Quản trị viên (Admin)
- ✅ Dashboard quản lý tổng quan
- ✅ Quản lý người dùng (Ban/Unban)
- ✅ Quản lý bài đăng (Approve/Remove/Ban)
- ✅ Quản lý phí dịch vụ (VIP Post Fee)
- ✅ Báo cáo doanh thu
- ✅ Quản lý đấu giá

### 7. Tính năng khác
- ✅ Landing Page hiện đại
- ✅ Responsive design (Mobile-friendly)
- ✅ Dark theme UI
- ✅ Email notifications (planned)
- ✅ Search & Filter
- ✅ Pagination

## 🗄️ Database Schema

**Entities chính:**
- `Users` - Người dùng (Admin, Staff, Member)
- `Vehicles` - Thông tin xe điện
- `Batteries` - Thông tin pin
- `Posts` - Bài đăng bán hàng
- `Auctions` - Phiên đấu giá
- `Bids` - Lượt đặt giá
- `Wallets` - Ví điện tử
- `WalletTransactions` - Lịch sử giao dịch
- `Payments` - Thanh toán Stripe
- `Conversations` & `Messages` - Chat
- `PostComments` - Bình luận
- `Fees` - Phí dịch vụ

## 🔧 Cấu hình môi trường

File `docker-compose.yml` đã cấu hình sẵn:
- JWT Secret Key
- Stripe API Keys (Test mode)
- Database connection
- Port mappings

**⚠️ Lưu ý**: Thay đổi Stripe keys trong production!

## 📝 API Endpoints

- `/` - Landing Page
- `/Auth/Login` - Đăng nhập
- `/Auth/Register` - Đăng ký
- `/PostPages/*` - Quản lý bài đăng
- `/AuctionPages/*` - Quản lý đấu giá
- `/Admin/*` - Dashboard admin
- `/auctionHub` - SignalR Hub đấu giá
- `/chathub` - SignalR Hub chat

## 🛠️ Development

**Chạy tests:**
```bash
dotnet test
```

**Build project:**
```bash
dotnet build
```

**Apply migrations:**
```bash
dotnet ef database update
```

## 📄 License

This project is for educational purposes.

## 👨‍💻 Author

Ch1mpleo

---

**Happy Coding! 🚗⚡**
