"use strict";

// --- DOM Elements ---
const loginView = document.getElementById("login-view");
const chatView = document.getElementById("chat-view");
const usernameInput = document.getElementById("username-input");
const loginButton = document.getElementById("login-button");
const userList = document.getElementById("user-list");
const chatHeader = document.getElementById("chat-header");
const chatBox = document.getElementById("chat-box"); // THÊM MỚI: Lấy phần tử khung chat
const messageList = document.getElementById("message-list");
const messageForm = document.getElementById("message-form");
const messageInput = document.getElementById("message-input");
const sendButton = document.getElementById("send-button");

// --- State ---
let currentUser = "";
let activeChatUser = "";
let connection = null;

// --- Functions ---

// THÊM MỚI: Hàm tự động cuộn xuống dưới cùng
function scrollToBottom() {
    chatBox.scrollTop = chatBox.scrollHeight;
}

function setupConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7171/chatHub") // Đảm bảo URL này đúng
        .build();

    // Lắng nghe danh sách người dùng được cập nhật từ server
    connection.on("UpdateUserList", (users) => {
        const currentlySelected = activeChatUser;
        userList.innerHTML = ""; // Xóa danh sách cũ
        users.forEach(user => {
            if (user !== currentUser) { // Không hiển thị chính mình
                const li = document.createElement("li");
                li.textContent = user;
                li.onclick = () => selectUser(user);
                // Giữ lại trạng thái active nếu user đó đang được chọn
                if (user === currentlySelected) {
                    li.classList.add('active');
                    activeChatUser = user;
                }
                userList.appendChild(li);
            }
        });
    });

    // Lắng nghe tin nhắn riêng từ server
    connection.on("ReceivePrivateMessage", (sender, message) => {
        // Chỉ hiển thị tin nhắn nếu nó thuộc về cuộc trò chuyện đang mở
        if (sender === activeChatUser || sender === currentUser) {
            const li = document.createElement("li");
            // Phân biệt tin nhắn gửi và nhận
            if (sender === currentUser) {
                li.className = 'sent';
                li.innerHTML = `<strong>Bạn:</strong><br>${message}`;
            } else {
                li.className = 'received';
                li.innerHTML = `<strong>${sender}:</strong><br>${message}`;
            }
            // THAY ĐỔI: Dùng appendChild để thêm tin nhắn mới vào cuối
            messageList.appendChild(li);
            // THÊM MỚI: Tự động cuộn xuống
            scrollToBottom();
        }
    });

    // Lắng nghe lịch sử cuộc trò chuyện được gửi từ server
    connection.on("LoadHistory", (history) => {
        messageList.innerHTML = ""; // Xóa các tin nhắn tạm thời
        history.forEach(msg => {
            const li = document.createElement("li");
            if (msg.sender === currentUser) {
                li.className = 'sent';
                li.innerHTML = `<strong>Bạn:</strong><br>${msg.message}`;
            } else {
                li.className = 'received';
                li.innerHTML = `<strong>${msg.sender}:</strong><br>${msg.message}`;
            }
            // Dùng appendChild để hiển thị theo đúng thứ tự thời gian (cũ nhất ở trên cùng)
            messageList.appendChild(li);
        });
        // THÊM MỚI: Tự động cuộn xuống sau khi tải lịch sử
        scrollToBottom();
    });

    // Bắt đầu kết nối
    connection.start().catch(err => console.error(err.toString()));
}

// CẬP NHẬT: Hàm chọn người dùng để bắt đầu chat
function selectUser(username) {
    activeChatUser = username;
    chatHeader.textContent = `Trò chuyện với ${username}`;
    messageList.innerHTML = ""; // Xóa tin nhắn cũ ngay lập tức
    sendButton.disabled = false;

    // Đánh dấu người dùng đang được chọn
    document.querySelectorAll('#user-list li').forEach(li => {
        li.classList.remove('active');
        if (li.textContent === username) {
            li.classList.add('active');
        }
    });

    // MỚI: Yêu cầu server gửi lịch sử cuộc trò chuyện cho người dùng vừa chọn
    if (connection && connection.state === "Connected") {
        connection.invoke("GetConversationHistory", activeChatUser).catch(err => console.error(err.toString()));
    }
}

// --- Event Listeners ---

loginButton.addEventListener("click", (event) => {
    event.preventDefault();
    const username = usernameInput.value.trim();
    if (username) {
        currentUser = username;

        // Bắt đầu kết nối và đăng ký người dùng
        setupConnection();

        // Đợi một chút để kết nối được thiết lập rồi mới đăng ký
        setTimeout(() => {
            if (connection.state === "Connected") {
                connection.invoke("RegisterUser", currentUser).catch(err => console.error(err.toString()));
                loginView.style.display = 'none';
                chatView.style.display = 'flex';
            } else {
                alert("Không thể kết nối tới server. Vui lòng thử lại.");
            }
        }, 500);
    }
});

messageForm.addEventListener("submit", (event) => {
    event.preventDefault();
    const message = messageInput.value.trim();
    if (message && activeChatUser) {
        // Gửi tin nhắn riêng đến người dùng đang được chọn
        connection.invoke("SendPrivateMessage", activeChatUser, message)
            .catch(err => console.error(err.toString()));
        messageInput.value = "";
    }
});