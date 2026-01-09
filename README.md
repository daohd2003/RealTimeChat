# 💬 RealTimeChat - Hệ thống Chat Trực tuyến với SignalR & Firebase

**RealTimeChat** là một giải pháp chat trực tuyến thời gian thực (Real-time) được xây dựng trên nền tảng **.NET 8**. Dự án minh họa cách kết hợp sức mạnh của **SignalR** để truyền tải tin nhắn tức thì và **Firebase Realtime Database** để lưu trữ lịch sử cuộc trò chuyện.

Dự án được chia tách theo kiến trúc lớp (Layered Architecture), tách biệt rõ ràng giữa Giao diện (Client), API (Server), Logic nghiệp vụ (Core) và Hạ tầng (Infrastructure).



---

## 🏗 Kiến trúc Dự án

Hệ thống bao gồm 4 project chính:

| Project | Loại | Nhiệm vụ |
| :--- | :--- | :--- |
| **Core** | Class Library | Chứa các thực thể (`ChatMessage`), interfaces (`IChatService`) và các thành phần cốt lõi không phụ thuộc vào hạ tầng. |
| **Infrastructure** | Class Library | Triển khai các interface từ Core. Cụ thể là `FirebaseChatService` dùng để giao tiếp với Firebase qua REST API. |
| **WebApp** | Razor Pages | Đóng vai trò là **Client Frontend**. Chứa giao diện người dùng, file Javascript xử lý kết nối SignalR (`chat.js`) và định nghĩa lớp `ChatHub`. |
| **WebAppAPI** | Web API | Đóng vai trò là **Backend Server**. Nơi host SignalR Hub, cung cấp các API endpoints và xử lý xác thực/CORS. |

---

## 🛠 Công nghệ sử dụng

* **Framework:** .NET 8.0
* **Real-time Communication:** ASP.NET Core SignalR
* **Database:** Firebase Realtime Database (Google)
* **Frontend:** Razor Pages, JavaScript (Vanilla), jQuery, Bootstrap 5
* **HTTP Client:** IHttpClientFactory (Gọi REST API tới Firebase)
* **JSON Processing:** Newtonsoft.Json

---

## ⚙️ Cài đặt và Hướng dẫn chạy

Để chạy dự án này, bạn cần thiết lập để cả **Backend (WebAppAPI)** và **Frontend (WebApp)** chạy song song.

### 1. Yêu cầu tiên quyết
* Visual Studio 2022 hoặc VS Code.
* .NET 8.0 SDK.
* Tài khoản Firebase (Google) để tạo Realtime Database (nếu muốn dùng DB riêng).

### 2. Cấu hình Firebase
Mở file `Infrastructure/Services/FirebaseChatService.cs`. Hiện tại dự án đang để URL mặc định. Nếu bạn muốn dùng database của riêng mình, hãy thay đổi dòng sau:

```csharp
private const string FirebaseDbBaseUrl = "[https://your-firebase-id.firebasedatabase.app/](https://your-firebase-id.firebasedatabase.app/)";
```

### 3. Cấu hình CORS (Quan trọng)
Trong `WebAppAPI/Program.cs`, đảm bảo rằng URL của Frontend (`WebApp`) được phép truy cập. Mặc định Frontend chạy ở port `7106`:

```csharp
options.AddPolicy(name: MyAllowSpecificOrigins,
    policy => {
        policy.WithOrigins("https://localhost:7106") // URL của WebApp
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Bắt buộc cho SignalR
    });
```

### 4. Chạy dự án
Bạn cần chạy **Multiple Startup Projects** trong Visual Studio:

1.  Chuột phải vào **Solution** -> **Set Startup Projects**.
2.  Chọn **Multiple startup projects**.
3.  Đặt **Start** cho cả `WebAppAPI` và `WebApp`.
4.  Nhấn **F5**.

* **API Server (SignalR Host):** `https://localhost:7171`
* **Client UI:** `https://localhost:7106`

---

## 📖 Hướng dẫn sử dụng

### 1. Đăng nhập vào Chat
* Truy cập giao diện tại `https://localhost:7106`.
* Nhập **Username** bất kỳ (Ví dụ: `UserA`) và nhấn **Vào Chat**.
* Mở một trình duyệt khác (hoặc tab ẩn danh), truy cập lại và nhập **Username** khác (Ví dụ: `UserB`).

### 2. Gửi tin nhắn riêng (Private Chat)
* Trên màn hình của `UserA`, danh sách người dùng Online sẽ hiện ở cột bên trái.
* Click vào tên `UserB`.
* Lịch sử chat cũ (lưu từ Firebase) sẽ tự động tải về.
* Nhập tin nhắn và nhấn **Gửi**. Tin nhắn sẽ xuất hiện ngay lập tức bên phía `UserB` mà không cần tải lại trang.

### 3. API Endpoints (Dành cho Postman/Mobile App)
`WebAppAPI` cung cấp endpoint để gửi tin nhắn từ bên thứ 3 (ví dụ từ Mobile App hoặc hệ thống khác):

**POST** `/api/Chat/send`

**Body (JSON):**
```json
{
  "sender": "SystemAdmin",
  "recipient": "UserA",
  "message": "Đây là tin nhắn thông báo từ hệ thống."
}
```
*API này sẽ lưu tin nhắn vào Firebase và đẩy thông báo realtime tới `UserA` nếu họ đang online.*

---

## 📂 Cấu trúc thư mục

```text
daohd2003-realtimechat/
├── Core/                   # Lớp lõi (Interfaces, Models)
├── Infrastructure/         # Lớp hạ tầng (Firebase Service)
├── WebApp/                 # Lớp giao diện (Razor Pages, Chat.js, CSS)
│   ├── Hubs/               # Chứa ChatHub.cs (Logic xử lý SignalR)
│   └── wwwroot/js/         # Chứa chat.js (Client SignalR logic)
└── WebAppAPI/              # Lớp Server (API Controller, Program.cs config SignalR)
```

---

## 📝 Lưu ý quan trọng
1.  **Dependency Injection:** `WebAppAPI` tham chiếu tới `WebApp` để sử dụng class `ChatHub`. Đây là cấu hình đặc biệt để tách biệt UI và API Server nhưng vẫn dùng chung Logic Hub.
2.  **Lưu trữ:** Tin nhắn không lưu trong SQL Server mà lưu dạng JSON trên Firebase. Key của hội thoại được tạo theo quy tắc: `UserA-UserB` (sắp xếp theo bảng chữ cái để đảm bảo tính nhất quán).

---
