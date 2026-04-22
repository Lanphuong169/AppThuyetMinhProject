// Xóa các dòng import vì ta dùng script CDN rồi

// Helper: Loại bỏ dấu Tiếng Việt để tìm kiếm thông minh
function removeAccents(str) {
    if (!str) return "";
    return str.normalize('NFD')
              .replace(/[\u0300-\u036f]/g, '')
              .replace(/đ/g, 'd').replace(/Đ/g, 'D');
}

// Hàm đóng Modal toàn cục
window.closeModal = function(id) {
    const modal = document.getElementById(id);
    if(modal) {
        modal.classList.add("hidden");
        document.body.style.overflow = "auto";
        // Nếu là modal audio, dừng phát nhạc
        if(id === 'modal-audio' && typeof globalAudioPlayer !== 'undefined') {
            globalAudioPlayer.pause();
        }
    }
};

// ==========================================
// THÔNG TIN CONFIG CỦA USER ĐÃ NHẬP
// ==========================================
const firebaseConfig = {
  apiKey: "AIzaSyAzZ8WiAotJgKlyBd-QA4aQSXmpqHEzSeg",
  authDomain: "vinhkhanhtrip.firebaseapp.com",
  databaseURL: "https://vinhkhanhtrip-default-rtdb.asia-southeast1.firebasedatabase.app",
  projectId: "vinhkhanhtrip",
  storageBucket: "vinhkhanhtrip.firebasestorage.app",
  messagingSenderId: "591995929888",
  appId: "1:591995929888:web:74cb4c8a1df9d25412c345",
  measurementId: "G-R4PXC2WVGR"
};

const isConfigured = firebaseConfig.apiKey !== "DÁN_API_KEY_CỦA_BẠN_VÀO_ĐÂY";

const statusDot = document.getElementById("connection-status");
const statusText = document.getElementById("status-text");

if(isConfigured) {
    try {
        firebase.initializeApp(firebaseConfig);
        statusDot.className = "status-dot success";
        statusText.innerText = "Đã kết nối Firebase";
        document.getElementById("config-alert").classList.add("hidden");

        // KHỞI TẠO HỆ THỐNG AUTH & PHÂN QUYỀN
        initAuthSystem();
        
        // Seed dữ liệu giả nếu cần
        seedMockData();
    } catch(e) {
        statusDot.className = "status-dot error";
        statusText.innerText = "Lỗi kết nối: " + e.message;
    }
} else {
    statusDot.className = "status-dot warning";
    statusText.innerText = "Chưa có cấu hình Firebase";
    document.getElementById("config-alert").classList.remove("hidden");
}

let currentUser = null;
let userRole = 'guest'; // 'admin', 'owner', 'guest'
let ownedPois = {};     // Object chứa { poiId: true }
let currentSearch = "";
let currentSearchAudio = "";
let currentSearchTrans = "";
let currentFilter = "all";

// UI Elements (Global)
let tableBody = null;
let tableAudioBody = null;
let searchInput = null;
let filterChips = null;
let filterLangSelect = null;
let searchAudioInput = null;

function initAuthSystem() {
    firebase.auth().onAuthStateChanged(async (user) => {
        const authOverlay = document.getElementById("auth-overlay");
        const userProfile = document.getElementById("user-profile");
        
        if (user) {
            currentUser = user;
            // Lấy thông tin role từ Database
            const userSnap = await firebase.database().ref('users/' + user.uid).once('value');
            const userData = userSnap.val();

            if (userData) {
                // Kiểm tra tài khoản bị khóa
                if (userData.status === 'blocked' && userData.role !== 'admin') {
                    alert("Tài khoản của bạn đã bị khóa bởi Quản trị viên. Vui lòng liên hệ hỗ trợ.");
                    firebase.auth().signOut();
                    return;
                }

                userRole = userData.role || 'owner';
                ownedPois = userData.ownedPois || {};
                
                // Kiểm tra gói dịch vụ cho chủ quán
                if (userRole === 'owner' && userData.subscription) {
                    const now = Date.now();
                    if (userData.subscription.status === 'expired' || now > userData.subscription.expiry) {
                        document.getElementById("modal-subscription").classList.remove("hidden");
                    }
                }

                // Cập nhật UI Profile
                document.getElementById("display-user-name").innerText = userData.name || user.email.split('@')[0];
                document.getElementById("display-user-role").innerText = userRole === 'admin' ? 'Quản trị viên' : 'Chủ quán VIP';
                document.getElementById("user-avatar-initial").innerText = (userData.name || 'A').charAt(0).toUpperCase();
                
                authOverlay.classList.add("hidden");
                userProfile.classList.remove("hidden");
                
                const menuStats = document.getElementById("menu-stats");
                const menuPois = document.getElementById("menu-pois");
                const menuAudio = document.getElementById("menu-audio");
                const menuTranslations = document.getElementById("menu-translations");
                const menuAccounts = document.getElementById("menu-accounts");
                const menuPresence = document.getElementById("menu-presence");
                const menuProfile = document.getElementById("menu-profile");

                const sidebarPoiText = document.getElementById("sidebar-poi-text");
                const filterPendingBtn = document.getElementById("filter-pending-btn");
                const btnAddNew = document.getElementById("btn-add-new");

                if (userRole === 'owner') {
                    if(sidebarPoiText) sidebarPoiText.innerText = "Quán Ăn Của Tôi";
                    if(filterPendingBtn) filterPendingBtn.classList.add("hidden");
                    if(btnAddNew) {
                        btnAddNew.innerHTML = '<i class="fa-solid fa-paper-plane"></i> GỬI DUYỆT QUÁN MỚI';
                        btnAddNew.classList.remove("hidden");
                    }
                    // Ẩn tất cả mục admin-only
                    document.querySelectorAll(".admin-only").forEach(el => el.classList.add("hidden"));
                } else {
                    if(sidebarPoiText) sidebarPoiText.innerText = "Điểm Ẩm Thực";
                    if(filterPendingBtn) filterPendingBtn.classList.remove("hidden");
                    if(btnAddNew) {
                        btnAddNew.innerHTML = '<i class="fa-solid fa-plus"></i> THÊM QUÁN MỚI';
                        btnAddNew.classList.remove("hidden");
                    }
                    // Hiện tất cả mục admin-only
                    document.querySelectorAll(".admin-only").forEach(el => el.classList.remove("hidden"));
                }

                // Chuyển view
                if(menuAccounts) {
                    menuAccounts.addEventListener("click", () => {
                        switchView(menuAccounts, viewAccounts);
                        loadAccountsTable();
                    });
                }

                if(menuPresence) {
                    menuPresence.addEventListener("click", () => {
                        switchView(menuPresence, viewPresence);
                        loadPresenceTable();
                    });
                }
                
                if (userRole === 'owner') {
                    // Chủ quán vào thẳng trang Quán ăn, không xem Dashboard
                    switchView(menuPois, viewPois);
                } else if (userRole === 'admin') {
                    // Admin mặc định vào Dashboard
                    switchView(menuStats, viewStats);
                }

                // Tải dữ liệu và kiểm tra số lượng quán đã duyệt để tính tiền
                loadInitialData();
                initCharts(); // Khởi tạo biểu đồ và thống kê ngay sau khi login
            } else {
                firebase.auth().signOut();
            }
        } else {
            currentUser = null;
            userRole = 'guest';
            authOverlay.classList.remove("hidden");
            userProfile.classList.add("hidden");
        }
    });
}

// Hàm tải dữ liệu ban đầu
function loadInitialData() {
    firebase.database().ref('quanan').on('value', (snapshot) => {
        const data = snapshot.val();
        window.cachedPois = data;
        applyFilters(); 
        
        if(typeof renderStatsView === 'function') renderStatsView(); 
        if(typeof lastAudioData !== 'undefined' && lastAudioData) renderAudioRankings(lastAudioData);

        // Kiểm tra hợp đồng tự động cho Chủ quán
        if (typeof checkOwnerContracts === 'function' && userRole === 'owner') {
            checkOwnerContracts();
        }
    });
}

function seedMockData() {
    if(!isConfigured) return;
    const last7Days = [];
    for(let i=1; i<=7; i++) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        last7Days.push(d.toISOString().split('T')[0]);
    }
    last7Days.forEach(day => {
        firebase.database().ref('analytics/' + day).once('value', snap => {
            if(!snap.exists()) {
                firebase.database().ref('analytics/' + day).set({
                    revenue: Math.floor(Math.random() * 30) + 5,
                    payments: Math.floor(Math.random() * 20) + 10
                });
            }
        });
    });
}

// UI Elements Initialization
tableBody = document.getElementById("table-body");
tableAudioBody = document.getElementById("table-audio-body"); 
searchInput = document.getElementById("search-input");
searchAudioInput = document.getElementById("search-audio-input");
filterLangSelect = document.getElementById("filter-lang");
filterChips = document.querySelectorAll(".filter-chip");

const modalOverlay = document.getElementById("modal-overlay");
const modalAudio = document.getElementById("modal-audio"); 

const btnAddNew = document.getElementById("btn-add-new");
const btnCloseModal = document.getElementById("btn-close-modal");
const btnCancel = document.getElementById("btn-cancel");
const poiForm = document.getElementById("poi-form");

const menuPois = document.getElementById("menu-pois");
const menuAudio = document.getElementById("menu-audio");
const menuStats = document.getElementById("menu-stats");
const menuTranslations = document.getElementById("menu-translations");
const menuAccounts = document.getElementById("menu-accounts");
let menuPresence = document.getElementById("menu-presence");
let btnClearPresenceHistory = document.getElementById("btn-clear-presence-history");

// GẮN SỰ KIỆN ĐÓNG MODAL
if(btnCloseModal) btnCloseModal.addEventListener("click", () => closeModal('modal-overlay'));
if(btnCancel) btnCancel.addEventListener("click", () => closeModal('modal-overlay'));
if(document.getElementById("btn-close-preview")) {
    document.getElementById("btn-close-preview").addEventListener("click", () => closeModal('modal-preview'));
}
if(document.getElementById("btn-close-audio")) {
    document.getElementById("btn-close-audio").addEventListener("click", () => closeModal('modal-audio'));
}

// Views
let viewPois = document.getElementById("view-pois");
let viewAudio = document.getElementById("view-audio");
let viewStats = document.getElementById("view-stats");
let viewTranslations = document.getElementById("view-translations");
let viewAccounts = document.getElementById("view-accounts");
let viewPresence = document.getElementById("view-presence");
let viewProfile = document.getElementById("view-profile");

// Navigation Logic
function switchView(targetMenu, targetView) {
    if (!targetView) return;

    // Xóa active trên tất cả menu link
    document.querySelectorAll(".nav-menu a").forEach(m => m.classList.remove("active"));
    
    // Ẩn tất cả các view section trong main content
    document.querySelectorAll(".main-content > section").forEach(v => v.classList.add("hidden"));
    
    // Nếu có targetMenu, đặt nó làm active
    if (targetMenu) {
        targetMenu.classList.add("active");
    }
    
    targetView.classList.remove("hidden");
    
    if(targetMenu === menuStats) {
        transformDashboardForRole(userRole);
        renderStatsView();
        initCharts();
    }
    
    if(targetMenu === menuTranslations) {
        applyFilters(); 
    }
}

// Chuyển đổi giao diện Dashboard theo Vai Trò
function transformDashboardForRole(role) {
    const label1 = document.getElementById("stat-label-1");
    const label2 = document.getElementById("stat-label-2");
    const label3 = document.getElementById("stat-label-3");
    const icon1 = document.getElementById("stat-icon-1");
    const icon2 = document.getElementById("stat-icon-2");
    const icon3 = document.getElementById("stat-icon-3");
    const card4 = document.getElementById("stat-card-4");
    const grid = document.getElementById("stats-grid");

    if (role === 'owner') {
        if(label1) label1.innerText = "LƯỢT QUÉT (HÔM NAY)";
        if(icon1) { icon1.innerHTML = '<i class="fa-solid fa-qrcode"></i>'; icon1.style.background = "rgba(139, 92, 246, 0.2)"; icon1.style.color = "#8b5cf6"; icon1.className="stat-icon";}
        
        if(label2) label2.innerText = "GIỜ CAO ĐIỂM (TOÀN PHỐ)";
        if(icon2) { icon2.innerHTML = '<i class="fa-solid fa-clock"></i>'; icon2.style.background = "rgba(16, 185, 129, 0.2)"; icon2.style.color = "#10b981"; icon2.className="stat-icon";}
        
        if(label3) label3.innerText = "THỜI GIAN NGHE (TB)";
        if(icon3) { icon3.innerHTML = '<i class="fa-solid fa-headphones-simple"></i>'; icon3.style.background = "rgba(59, 130, 246, 0.2)"; icon3.style.color = "#3b82f6"; icon3.className="stat-icon";}
        
        if(card4) card4.classList.add("hidden");
        if(grid) {
            grid.classList.remove("hidden");
            grid.classList.add("owner-mode");
        }

        // Clear values to wait for new data
        [1,2,3].forEach(i => {
           const el = document.getElementById(`stat-value-${i}`);
           if(el) el.innerText = "...";
        });
    } else {
        if(label1) label1.innerText = "TỔNG SỐ QUÁN";
        if(icon1) { icon1.innerHTML = '<i class="fa-solid fa-hotel"></i>'; icon1.style.background = ""; icon1.style.color = ""; icon1.className="stat-icon";}
        
        if(label2) label2.innerText = "ĐANG MỞ CỬA";
        if(icon2) { icon2.innerHTML = '<i class="fa-solid fa-door-open"></i>'; icon2.style.background = ""; icon2.style.color = ""; icon2.className="stat-icon open";}
                
        if(label3) label3.innerText = "LƯỢT QUÉT (NGÀY)";
        if(icon3) { icon3.innerHTML = '<i class="fa-solid fa-qrcode"></i>'; icon3.style.background = "rgba(139, 92, 246, 0.2)"; icon3.style.color = "#8b5cf6"; icon3.className="stat-icon";}
        
        if(card4) card4.classList.remove("hidden");
        if(grid) grid.classList.remove("hidden");
        if(grid) grid.classList.remove("owner-mode");
    }
}

// Charts Instances
let revenueChart = null;
let hourlyChart = null;

// Chart Filters State
let filterMode = 'today'; // 'today', 'month', 'custom'
let startDate = null;
let endDate = null;
let selectedMonth = new Date().getMonth();
let selectedYear = new Date().getFullYear();

// Chart.js Initialization
async function initCharts() {
    try {
        let labels = [];
        const poiStats = {}; 

        const now = new Date();
        const today = now.toLocaleDateString('en-CA');
        
        if (filterMode === 'today') {
            labels = Array.from({length: 24}, (_, i) => `${i}h`);
            const snap = await firebase.database().ref(`analytics/hourly/${today}`).once('value');
            const hourlyData = snap.val() || {};
            
            const scansDataArr = Array.from({length: 24}, (_, i) => hourlyData[i] || 0);
            const revenueData = Array.from({length: 24}, () => 0);

            // Lấy dữ liệu quán cho ngày hôm nay
            const poiSnap = await firebase.database().ref(`analytics/${today}/pois`).once('value');
            const todayPois = poiSnap.val() || {};
            Object.entries(todayPois).forEach(([id, stats]) => {
                poiStats[id] = { scans: (stats.scans || 0), revenue: (stats.revenue || 0) };
            });

            renderAdvancedChart(labels, revenueData, scansDataArr);
        } else {
            // Chuẩn bị danh sách ngày
            if (filterMode === 'month') {
                const daysInMonth = new Date(selectedYear, selectedMonth + 1, 0).getDate();
                for (let i = 1; i <= daysInMonth; i++) {
                    labels.push(`${selectedYear}-${String(selectedMonth + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`);
                }
            } else if (filterMode === 'custom' && startDate && endDate) {
                let curr = new Date(startDate);
                const end = new Date(endDate);
                while (curr <= end) {
                    labels.push(curr.toISOString().split('T')[0]);
                    curr.setDate(curr.getDate() + 1);
                }
            } else {
                for (let i = 6; i >= 0; i--) {
                    const d = new Date();
                    d.setDate(d.getDate() - i);
                    labels.push(d.toISOString().split('T')[0]);
                }
            }

            // Tải dữ liệu SONG SONG (Parallel) để tối ưu tốc độ
            const promises = labels.map(day => firebase.database().ref('analytics/' + day).once('value'));
            const snapshots = await Promise.all(promises);
            
            const revenueDataArr = [];
            const scansDataArr = [];

            snapshots.forEach((snap, idx) => {
                const data = snap.val() || { revenue: 0, scans: 0, pois: {} };
                
                // PHÂN QUYỀN THỐNG KÊ BIỂU ĐỒ
                if (userRole === 'owner') {
                    let ownerRevenue = 0;
                    let ownerScans = 0;
                    const dayPois = data.pois || {};
                    Object.entries(dayPois).forEach(([id, stats]) => {
                        if (ownedPois[id] || (window.cachedPois[id] && window.cachedPois[id].ownerUid === currentUser.uid)) {
                            ownerRevenue += (stats.revenue || 0);
                            ownerScans += (stats.scans || 0);
                            
                            if (!poiStats[id]) poiStats[id] = { scans: 0, revenue: 0 };
                            poiStats[id].scans += (stats.scans || 0);
                            poiStats[id].revenue += (stats.revenue || 0);
                        }
                    });
                    revenueDataArr.push(ownerRevenue);
                    scansDataArr.push(ownerScans);
                } else {
                    revenueDataArr.push(data.revenue || 0);
                    scansDataArr.push(data.scans || 0);
                    const dayPois = data.pois || {};
                    Object.entries(dayPois).forEach(([id, stats]) => {
                        if (!poiStats[id]) poiStats[id] = { scans: 0, revenue: 0 };
                        poiStats[id].scans += (stats.scans || 0);
                        poiStats[id].revenue += (stats.revenue || 0);
                    });
                }
            });

            renderAdvancedChart(labels, revenueDataArr, scansDataArr);
        }

        renderStatsView(poiStats); 
        initHourlyChart();

        // Tải thêm dữ liệu Audio Rankings để cập nhật Thời gian nghe (THẺ 3 cho Owner)
        const audioSnap = await firebase.database().ref(`analytics/audio/${today}`).once('value');
        renderAudioRankings(audioSnap.val());
    } catch (error) {
        console.error("Lỗi khi tải dữ liệu thống kê:", error);
        // Fallback render để không bị trắng màn hình
        renderStatsView({});
    }
}

function renderAdvancedChart(labels, revData, scanData) {
    const ctx = document.getElementById('revenueChart').getContext('2d');
    if (revenueChart) revenueChart.destroy();
    
    revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels.map(l => l.includes('-') ? (l.split('-').slice(2).join('/') + (labels.length > 10 ? '' : '/' + l.split('-')[1])) : l),
            datasets: [
                {
                    label: 'Doanh thu ($)',
                    data: revData,
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    yAxisID: 'y',
                    fill: true,
                    tension: 0.3
                },
                {
                    label: 'Lượt quét',
                    data: scanData,
                    borderColor: '#3b82f6',
                    backgroundColor: 'transparent',
                    yAxisID: 'y1',
                    tension: 0.3,
                    borderDash: [5, 5]
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { type: 'linear', display: true, position: 'left', grid: { color: 'rgba(255,255,255,0.05)' } },
                y1: { type: 'linear', display: true, position: 'right', grid: { drawOnChartArea: false } },
                x: { grid: { display: false } }
            }
        }
    });
}

menuPois.addEventListener("click", (e) => {
    e.preventDefault();
    switchView(menuPois, viewPois);
});

menuAudio.addEventListener("click", (e) => {
    e.preventDefault();
    switchView(menuAudio, viewAudio);
});

menuStats.addEventListener("click", (e) => {
    e.preventDefault();
    switchView(menuStats, viewStats);
});

menuTranslations.addEventListener("click", (e) => {
    e.preventDefault();
    switchView(menuTranslations, viewTranslations);
    applyFilters();
});

if(menuAccounts) {
    menuAccounts.addEventListener("click", (e) => {
        e.preventDefault();
        switchView(menuAccounts, viewAccounts);
        loadAccountsTable();
    });
}

// Charts Instances
// Removed old chart instances

// ==========================================
// THIẾT LẬP AUTH HANDLERS & HELPERS
// ==========================================

function toggleAuthMode(mode) {
    const loginForm = document.getElementById("login-form");
    const regForm = document.getElementById("register-form");
    const title = document.getElementById("auth-title");
    
    if (mode === 'register') {
        loginForm.classList.add("hidden");
        regForm.classList.remove("hidden");
        title.innerText = "Đăng Ký Đối Tác";
    } else {
        loginForm.classList.remove("hidden");
        regForm.classList.add("hidden");
        title.innerText = "Đăng Nhập Hệ Thống";
    }
}

// Xử lý Đăng nhập
document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = document.getElementById("login-email").value;
    const pass = document.getElementById("login-password").value;
    const btn = e.target.querySelector("button");

    try {
        btn.disabled = true;
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> ĐANG XỬ LÝ...';
        await firebase.auth().signInWithEmailAndPassword(email, pass);
    } catch (err) {
        alert("Lỗi đăng nhập: " + err.message);
        btn.disabled = false;
        btn.innerHTML = 'ĐĂNG NHẬP <i class="fa-solid fa-arrow-right"></i>';
    }
});

// Xử lý Đăng ký & Thanh toán
document.getElementById("register-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const name = document.getElementById("reg-name").value;
    const email = document.getElementById("reg-email").value;
    const pass = document.getElementById("reg-password").value;
    const btn = e.target.querySelector("button");

    if (pass.length < 6) { alert("Mật khẩu phải từ 6 ký tự!"); return; }

    try {
        btn.disabled = true;
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> ĐANG THANH TOÁN (10$)...';
        
        // Mô phỏng độ trễ thanh toán
        await new Promise(r => setTimeout(r, 2000));
        
        // Tạo User Auth
        const cred = await firebase.auth().createUserWithEmailAndPassword(email, pass);
        
        // Tạo User Profile trong Database
        await firebase.database().ref('users/' + cred.user.uid).set({
            name: name,
            email: email,
            role: email.toLowerCase().includes('admin') ? 'admin' : 'owner',
            createdAt: Date.now(), 
            subscription: {
                status: 'active',
                expiry: Date.now() + (30 * 24 * 60 * 60 * 1000) // 30 ngày
            },
            ownedPois: {} // Sẽ được Admin gán ID quán sau
        });
        
        alert("Thanh toán 10$ thành công! Chào mừng đối tác VIP.");
    } catch (err) {
        alert("Lỗi đăng ký: " + err.message);
        btn.disabled = false;
        btn.innerHTML = 'THANH TOÁN & ĐĂNG KÝ <i class="fa-solid fa-credit-card"></i>';
    }
});

// Xử lý Đăng xuất
document.getElementById("btn-logout").addEventListener("click", () => {
    if(confirm("Bạn muốn đăng xuất khỏi hệ thống?")) {
        firebase.auth().signOut();
    }
});

// Mô phỏng gia hạn thanh toán
window.simulatePayment = async function() {
    if(!currentUser) return;
    try {
        const btn = event.target;
        
        // Tính nhẩm số lượng quán đã duyệt để hiện cảnh báo
        const approvedCount = Object.entries(window.cachedPois || {}).filter(([id, p]) => p.ownerUid === currentUser.uid && p.status === "approved").length;
        const totalFee = approvedCount * 10 || 10; // Tải thiểu 10$ nếu chưa có quán nào

        if(!confirm(`Xác nhận thanh toán gia hạn phí $${totalFee} cho ${approvedCount} quán ăn đã phê duyệt?`)) return;

        btn.disabled = true;
        btn.innerText = `Đang xử lý $${totalFee}...`;
        
        await new Promise(r => setTimeout(r, 2000));
        
        await firebase.database().ref('users/' + currentUser.uid + '/subscription').update({
            status: 'active',
            expiry: Date.now() + (30 * 24 * 60 * 60 * 1000)
        });
        
        document.getElementById("modal-subscription").classList.add("hidden");
        alert("Gia hạn thành công! Cảm ơn bạn.");
        location.reload();
    } catch(err) {
        alert("Lỗi thanh toán: " + err.message);
    }
};


// Audio Guide Modal Variable
let currentQrCode = null;

// Modal Functions
function openModal(poiId = null, poiData = null) {
    document.body.style.overflow = "hidden";
    if(poiId && poiData) {
        document.getElementById("modal-title").innerText = "Sửa Thông Tin Quán";
        document.getElementById("input-id").value = poiId;
        document.getElementById("input-ten").value = poiData.Ten || "";
        document.getElementById("input-loai").value = poiData.Loai || "Oc";
        document.getElementById("input-lat").value = poiData.Lat || "";
        document.getElementById("input-lng").value = poiData.Lng || "";
        document.getElementById("input-diachi").value = poiData.DiaChi || "";
        document.getElementById("input-sodienthoai").value = poiData.SoDienThoai || "";
        document.getElementById("input-email").value = poiData.Email || "";
        document.getElementById("input-rating").value = poiData.Rating || "";
        
        // Tách giờ mở cửa
        const hours = (poiData.OpeningHours || "08:00 - 22:00").split(" - ");
        document.getElementById("input-open-time").value = hours[0] || "08:00";
        document.getElementById("input-close-time").value = hours[1] || "22:00";

        document.getElementById("input-pricerange").value = poiData.PriceRange || "";
        document.getElementById("input-imageurl").value = poiData.ImageUrl || "";
        document.getElementById("input-imageurl2").value = poiData.ImageUrl2 || "";
        document.getElementById("input-imageurl3").value = poiData.ImageUrl3 || "";
        document.getElementById("input-menuitems").value = (poiData.MenuItems || []).join(", ");
        document.getElementById("input-mota").value = poiData.MoTa || "";
    } else {
        document.getElementById("modal-title").innerText = "Thêm Quán Ăn Mới";
        poiForm.reset();
        document.getElementById("input-id").value = "";
    }
    modalOverlay.classList.remove("hidden");
}

function closeModal() {
    modalOverlay.classList.add("hidden");
    modalPreview.classList.add("hidden");
    document.body.style.overflow = "auto";
}

btnAddNew.addEventListener("click", () => openModal(null, null));
btnCloseModal.addEventListener("click", closeModal);
btnCancel.addEventListener("click", closeModal);

// Preview Elements
const modalPreview = document.getElementById("modal-preview");
const btnClosePreview = document.getElementById("btn-close-preview");
btnClosePreview.addEventListener("click", () => modalPreview.classList.add("hidden"));

window.viewPoi = function(id) {
    if(window.cachedPois && window.cachedPois[id]) {
        const data = window.cachedPois[id];
        document.getElementById("preview-banner").style.backgroundImage = `url('${data.ImageUrl || 'https://via.placeholder.com/400x200'}')`;
        document.getElementById("preview-tag").innerText = (data.Loai || "Khác").toUpperCase();
        document.getElementById("preview-address").innerText = data.DiaChi || "Chưa cập nhật địa chỉ";
        document.getElementById("preview-time").innerText = data.OpeningHours || "00:00 - 00:00";
        document.getElementById("preview-phone").innerText = data.SoDienThoai || "N/A";
        document.getElementById("preview-price").innerText = data.PriceRange || "N/A";
        document.getElementById("preview-desc").innerText = data.MoTa || "Không có mô tả.";

        // --- Cập nhật Widget Hộ chiếu ---
        // 1. Tọa độ & Tag
        document.getElementById("preview-coords").innerText = `${data.Lat || 0}, ${data.Lng || 0}`;
        document.getElementById("preview-tag").innerText = (data.Loai || "Khác").toUpperCase();

        // 2. Tính toán trạng thái Đóng/Mở
        const now = new Date();
        const currentTime = now.getHours() * 60 + now.getMinutes();
        
        const hours = (data.OpeningHours || "08:00 - 22:00").split(" - ");
        const openStr = hours[0].split(":");
        const closeStr = hours[1].split(":");
        
        const openTime = parseInt(openStr[0]) * 60 + parseInt(openStr[1]);
        const closeTime = parseInt(closeStr[0]) * 60 + parseInt(closeStr[1]);

        const statusDot = document.getElementById("preview-status-dot");
        const statusText = document.getElementById("preview-status-text");

        if (currentTime >= openTime && currentTime < closeTime) {
            statusDot.className = "status-dot-led online";
            statusText.innerText = "ĐANG MỞ CỬA";
            statusText.style.color = "#00ff88";
        } else {
            statusDot.className = "status-dot-led offline";
            statusText.innerText = "ĐÃ ĐÓNG CỬA";
            statusText.style.color = "#ff4d4d";
        }
        // ---------------------------------

        // Rating
        const ratingContainer = document.getElementById("preview-rating");
        ratingContainer.innerHTML = "";
        const r = Math.round(data.Rating || 0);
        for(let i=1; i<=5; i++) {
            ratingContainer.innerHTML += `<i class="fa-${i <= r ? 'solid' : 'regular'} fa-star"></i>`;
        }

        // Menu
        const menuContainer = document.getElementById("preview-menu");
        menuContainer.innerHTML = "";
        if(data.MenuItems && data.MenuItems.length > 0) {
            data.MenuItems.forEach(url => {
                const item = document.createElement("div");
                item.className = "preview-menu-item";
                item.style.backgroundImage = `url('${url}')`;
                menuContainer.appendChild(item);
            });
        }

        document.body.style.overflow = "hidden";
        modalPreview.classList.remove("hidden");
    }
}

window.editPoi = function(id) {
    if(!isConfigured) { alert("Vui lòng cấu hình Firebase trước!"); return; }
    if(window.cachedPois && window.cachedPois[id]) {
        openModal(id, window.cachedPois[id]);
    }
}

window.deletePoi = function(id) {
    if(!isConfigured) { alert("Vui lòng cấu hình Firebase trước!"); return; }
    if(confirm("Bạn có chắc chắn muốn xóa điểm này không?")) {
        firebase.database().ref('quanan/' + id).remove()
            .then(() => alert("Đã xóa thành công!"))
            .catch(err => alert("Lỗi khi xóa: " + err.message));
    }
}

// Helper: Tải ảnh lẻ lên Firebase Storage (Có theo dõi tiến trình cho Audio)
async function uploadFile(file, folder="quanan", onProgress = null) {
    return new Promise((resolve, reject) => {
        try {
            const storageRef = firebase.storage().ref();
            const cleanFileName = removeAccents(file.name).replace(/\s+/g, '_');
            const fileName = `${Date.now()}_${cleanFileName}`;
            const fileRef = storageRef.child(`${folder}/${fileName}`);
            
            const uploadTask = fileRef.put(file);

            // Xử lý Timeout sau 15 giây nếu vẫn ở 0%
            let hasProgressed = false;
            const timeout = setTimeout(() => {
                if (!hasProgressed) {
                    uploadTask.cancel();
                    reject(new Error("KẾT NỐI QUÁ HẠN: Không thể bắt đầu tải lên sau 15 giây. Hãy kiểm tra lại Firebase Storage Rules hoặc đường truyền mạng."));
                }
            }, 15000);

            uploadTask.on('state_changed', 
                (snapshot) => {
                    hasProgressed = true;
                    const progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
                    if (onProgress) onProgress(progress);
                }, 
                (error) => {
                    clearTimeout(timeout);
                    console.error("Firebase Storage Error:", error.code, error.message);
                    let msg = error.message;
                    if (error.code === 'storage/unauthorized') {
                        msg = "BẠN CHƯA CÓ QUYỀN GHI: Hãy vào Firebase Console -> Storage -> Rules và đổi thành 'allow read, write: if true;'";
                    }
                    reject(new Error(msg));
                }, 
                async () => {
                    clearTimeout(timeout);
                    const downloadUrl = await uploadTask.snapshot.ref.getDownloadURL();
                    resolve(downloadUrl);
                }
            );
        } catch (error) {
            console.error("Lỗi trong uploadFile:", error);
            reject(error);
        }
    });
}

async function uploadImageInput(fileInputId) {
    const fileInput = document.getElementById(fileInputId);
    if(fileInput && fileInput.files.length > 0) {
        return await uploadFile(fileInput.files[0], "quanan-images");
    }
    return null;
}

// --- Search & Filter Logic ---
searchInput.addEventListener("input", (e) => {
    currentSearch = e.target.value.toLowerCase();
    applyFilters();
});

filterChips.forEach(chip => {
    chip.addEventListener("click", () => {
        // UI: toggle active class
        filterChips.forEach(c => c.classList.remove("active"));
        chip.classList.add("active");
        
        currentFilter = chip.getAttribute("data-filter");
        applyFilters();
    });
});

if(searchAudioInput) {
    searchAudioInput.addEventListener("input", (e) => {
        currentSearchAudio = e.target.value.toLowerCase();
        applyFilters();
    });
}

if(filterLangSelect) {
    filterLangSelect.addEventListener("change", applyFilters);
}

function applyFilters() {
    if (!window.cachedPois) return;
    
    // 1. Lọc cho Tab Danh sách Quán (POI)
    const searchNormalized = removeAccents(currentSearch.toLowerCase());
    const filteredEntries = [];
    
    for (const [id, value] of Object.entries(window.cachedPois)) {
        // PHÂN QUYỀN:
        if (userRole === 'owner') {
            // Cho phép Owner thấy quán của mình (dù approved hay pending)
            if (value.ownerUid !== currentUser.uid && !ownedPois[id]) continue;
        }

        const nameNormalized = removeAccents((value.Ten || "").toLowerCase());
        const addressNormalized = removeAccents((value.DiaChi || "").toLowerCase());
        
        const matchesSearch = nameNormalized.includes(searchNormalized) || 
                              addressNormalized.includes(searchNormalized);
        
        // Logic Filter Tab
        const status = value.status || "approved";
        let matchesFilter = false;
        
        if (currentFilter === "all") {
            // ADMIN THẤY TẤT CẢ QUÁN ĐÃ DUYỆT (với Admin thì "Tất cả" là ưu tiên xem quán đang hoạt động)
            // Hoặc nếu muốn Admin thấy tuyệt đối mọi thứ, có thể thêm userRole === 'admin'
            matchesFilter = status === "approved" || userRole === 'admin' || (userRole === 'owner' && value.ownerUid === currentUser.uid);
        } else if (currentFilter === "pending") {
            matchesFilter = status === "pending";
        } else {
            matchesFilter = value.Loai === currentFilter && status === "approved";
        }
        
        if (matchesSearch && matchesFilter) {
            filteredEntries.push([id, value]);
        }
    }
    
    filteredEntries.sort((a, b) => (a[1].Ten || "").localeCompare(b[1].Ten || "", 'vi'));
    const sortedData = Object.fromEntries(filteredEntries);
    renderFilteredTable(sortedData);
    updateDashboardStats(sortedData);

    // 2. Lọc cho Tab Audio Guide
    const searchAudioNormalized = removeAccents(currentSearchAudio.toLowerCase());
    const langFilter = filterLangSelect ? filterLangSelect.value : "all";

    const audioEntries = Object.entries(window.cachedPois).filter(([id, value]) => {
        // PHÂN QUYỀN CHO AUDIO
        if (userRole === 'owner') {
            if (!ownedPois[id] && value.ownerUid !== currentUser.uid) return false;
        }
        const nameNormalized = removeAccents((value.Ten || "").toLowerCase());
        const matchesName = nameNormalized.includes(searchAudioNormalized);
        const matchesLang = langFilter === "all" || (value.AudioUrls && value.AudioUrls[langFilter]);
        return matchesName && matchesLang;
    }).sort((a, b) => (a[1].Ten || "").localeCompare(b[1].Ten || "", 'vi'));
    
    renderAudioTable(Object.fromEntries(audioEntries));

    // 3. Lọc cho Tab Bản dịch
    const searchTransNormalized = removeAccents((currentSearchTrans || "").toLowerCase());
    const transEntries = Object.entries(window.cachedPois).filter(([id, value]) => {
        // PHÂN QUYỀN CHO BẢN DỊCH
        if (userRole === 'owner') {
            if (!ownedPois[id] && value.ownerUid !== currentUser.uid) return false;
        }
        const nameNormalized = removeAccents((value.Ten || "").toLowerCase());
        return nameNormalized.includes(searchTransNormalized);
    }).sort((a, b) => (a[1].Ten || "").localeCompare(b[1].Ten || "", 'vi'));
    
    renderTranslationTable(Object.fromEntries(transEntries));
}


function renderAudioTable(data) {
    tableAudioBody.innerHTML = "";
    if (Object.keys(data).length === 0) {
        tableAudioBody.innerHTML = `<tr><td colspan="3" style="text-align: center; color: var(--text-secondary); padding: 40px;">Không tìm thấy quán nào...</td></tr>`;
        return;
    }

    const langConfigs = [
        { code: "vi", color: "#ef4444" }, { code: "en", color: "#3b82f6" }, { code: "de", color: "#f59e0b" },
        { code: "fr", color: "#8b5cf6" }, { code: "ru", color: "#10b981" }, { code: "es", color: "#f43f5e" },
        { code: "zh", color: "#eab308" }, { code: "ja", color: "#ec4899" }, { code: "ko", color: "#6366f1" }
    ];

    for (const [id, value] of Object.entries(data)) {
        let statusHtml = '';
        const audioUrls = value.AudioUrls || {};
        
        statusHtml = `<div style="display:flex; gap:6px; flex-wrap:wrap; align-items:center;">`;
        let hasAny = false;
        langConfigs.forEach(l => {
            if (audioUrls[l.code]) {
                hasAny = true;
                statusHtml += `<span style="display:inline-flex; align-items:center; gap:4px; color: #fff; background: ${l.color}; padding: 4px 8px; border-radius: 6px; font-size: 11px; font-weight: 800; text-transform:uppercase; box-shadow: 0 2px 4px rgba(0,0,0,0.2);">${l.code}</span>`;
            }
        });
        
        if (!hasAny) {
            statusHtml += `<div style="color: var(--text-secondary); font-size: 12px; opacity: 0.6; display: flex; align-items: center; gap: 8px;">
                            <i class="fa-solid fa-robot" style="font-size: 16px;"></i> 
                            <span>Dùng AI Voice mặc định</span>
                        </div>`;
        }
        statusHtml += `</div>`;

        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>
                <div style="font-weight: 700; font-size: 16px; color: white; margin-bottom: 4px;">${value.Ten || 'N/A'}</div>
                <div style="font-size: 12px; color: var(--text-secondary); opacity: 0.7; display: flex; align-items: center; gap: 6px;">
                    <i class="fa-solid fa-location-dot" style="color: var(--accent); font-size: 10px;"></i>
                    ${value.DiaChi || 'Vĩnh Khánh, Quận 4'}
                </div>
            </td>
            <td>
                ${statusHtml}
            </td>
            <td>
                <div class="action-btns">
                    <button class="btn-audio-cta" onclick="openAudioModal('${id}')">
                        <i class="fa-solid fa-sliders"></i>
                        CÀI ĐẶT AUDIO
                    </button>
                </div>
            </td>
        `;
        tableAudioBody.appendChild(tr);
    }
}
// -----------------------------

// Handle Form Submit
poiForm.addEventListener("submit", async function(e) {
    e.preventDefault();
    if(!isConfigured) { 
        alert("Bạn chưa điền thông tin kết nối Firebase ở app.js!");
        return; 
    }

    const btnSave = document.getElementById("btn-save");
    const originalText = btnSave.innerText;
    btnSave.innerText = "Đang lưu và Upload ảnh (vui lòng đợi)...";
    btnSave.disabled = true;

    try {
        const id = document.getElementById("input-id").value;
        const lat = parseFloat(document.getElementById("input-lat").value);
        const lng = parseFloat(document.getElementById("input-lng").value);

        // 1. KIỂM TRA TRÙNG TỌA ĐỘ (Chỉ cho quán mới)
        if (!id && window.cachedPois) {
            const isDuplicate = Object.values(window.cachedPois).some(p => 
                Math.abs(p.Lat - lat) < 0.00001 && Math.abs(p.Lng - lng) < 0.00001
            );
            if (isDuplicate) {
                alert("LỖI: Tọa độ này đã tồn tại trong hệ thống. Vui lòng kiểm tra lại để tránh nộp trùng quán!");
                return;
            }
        }

        // Tự động up ảnh lẻ (nếu có chọn)
        let img1 = await uploadImageInput("file-imageurl");
        if(img1) document.getElementById("input-imageurl").value = img1;

        let img2 = await uploadImageInput("file-imageurl2");
        if(img2) document.getElementById("input-imageurl2").value = img2;

        let img3 = await uploadImageInput("file-imageurl3");
        if(img3) document.getElementById("input-imageurl3").value = img3;

        // Xử lý Menu
        const menuFileInput = document.getElementById("file-menuitems");
        let uploadedMenuUrls = [];
        if(menuFileInput && menuFileInput.files.length > 0) {
            for(let i = 0; i < menuFileInput.files.length; i++) {
                let mUrl = await uploadFile(menuFileInput.files[i], "quanan-menu");
                uploadedMenuUrls.push(mUrl);
            }
        }

        const menuStr = document.getElementById("input-menuitems").value;
        let menuArray = menuStr ? menuStr.split(",").map(item => item.trim()).filter(i => i) : [];
        menuArray = menuArray.concat(uploadedMenuUrls);

        const openTime = document.getElementById("input-open-time").value || "08:00";
        const closeTime = document.getElementById("input-close-time").value || "22:00";

        const data = {
            Ten: document.getElementById("input-ten").value,
            Loai: document.getElementById("input-loai").value,
            Lat: lat,
            Lng: lng,
            DiaChi: document.getElementById("input-diachi").value,
            SoDienThoai: document.getElementById("input-sodienthoai").value,
            Email: document.getElementById("input-email").value,
            Rating: parseFloat(document.getElementById("input-rating").value) || 0,
            OpeningHours: `${openTime} - ${closeTime}`,
            PriceRange: document.getElementById("input-pricerange").value,
            ImageUrl: document.getElementById("input-imageurl").value,
            ImageUrl2: document.getElementById("input-imageurl2").value,
            ImageUrl3: document.getElementById("input-imageurl3").value,
            MenuItems: menuArray,
            MoTa: document.getElementById("input-mota").value,
            ownerUid: id ? (window.cachedPois[id].ownerUid || currentUser.uid) : currentUser.uid,
            status: id ? (window.cachedPois[id].status || "approved") : "pending",
            createdAt: id ? (window.cachedPois[id].createdAt || Date.now()) : Date.now(),
            expiryDate: id ? (window.cachedPois[id].expiryDate || null) : null // Mặc định chưa có hạn khi chưa được duyệt
        };

        if(id) {
            await firebase.database().ref('quanan/' + id).update(data);
        } else {
            var newRef = firebase.database().ref('quanan').push();
            await newRef.set(data);
        }

        // Thành công: Reset form và đóng modal
        try {
            poiForm.reset();
            document.getElementById("input-id").value = "";
            document.getElementById("input-loai").value = "Oc";
        } catch(err) {}

        closeModal();
        alert("Thông tin quán đã được gửi! Vui lòng chờ Admin phê duyệt.");

    } catch(err) {
        console.error(err);
        alert("Đã có lỗi xảy ra: " + err.message);
    } finally {
        btnSave.innerText = originalText;
        btnSave.disabled = false;
    }
});

// Load Data
if(isConfigured) {
    firebase.database().ref('quanan').on('value', (snapshot) => {
        const data = snapshot.val();
        window.cachedPois = data; // Cache for edit lookup
        applyFilters(); 
        
        // Đảm bảo các bảng xếp hạng được vẽ lại ngay khi có dữ liệu quán
        if(typeof renderStatsView === 'function') renderStatsView(); 
        if(typeof lastAudioData !== 'undefined' && lastAudioData) renderAudioRankings(lastAudioData);
    });
}

// REAL-TIME DASHBOARD (SCORES & PRESENCE)
if (typeof firebase !== 'undefined') {
    // Helper để lấy ngày local YYYY-MM-DD
    const getTodayStr = () => new Date().toLocaleDateString('en-CA');
    let currentToday = getTodayStr();

    // Hàm lắng nghe lượt quét trong ngày
    function listenToTodayScans(day) {
        firebase.database().ref('analytics/' + day).on('value', (snap) => {
            const data = snap.val() || { scans: 0, revenue: 0, payments: 0, pois: {} };
            
            let displayScans = 0;
            if (userRole === 'owner') {
                // Lọc lượt quét cho các quán của chủ sở hữu
                const todayPois = data.pois || {};
                Object.entries(todayPois).forEach(([id, stats]) => {
                    if (window.ownedPois && window.ownedPois[id]) {
                        displayScans += (stats.scans || 0);
                    }
                });
                const val1 = document.getElementById("stat-value-1");
                if(val1) val1.innerText = displayScans;
            } else {
                displayScans = data.scans || 0;
                const val3 = document.getElementById("stat-value-3");
                if(val3) val3.innerText = displayScans;
            }
            
            const revenueEl = document.getElementById("stat-revenue-today");
            const paymentsEl = document.getElementById("stat-payments-today");
            if(revenueEl) revenueEl.innerText = "$" + (data.revenue || 0);
            if(paymentsEl) paymentsEl.innerText = data.payments || 0;
        });
    }

    listenToTodayScans(currentToday);

    // Tự động chuyển ngày nếu tab mở qua đêm
    setInterval(() => {
        const newToday = getTodayStr();
        if (newToday !== currentToday) {
            firebase.database().ref('analytics/' + currentToday).off(); // Ngắt cái cũ
            currentToday = newToday;
            listenToTodayScans(currentToday); // Nghe cái mới
        }
    }, 60000);

    // 3. Lắng nghe Xếp hạng Audio
    let lastAudioData = null;
    firebase.database().ref('analytics/audio_rank').on('value', (snapshot) => {
        lastAudioData = snapshot.val();
        renderAudioRankings(lastAudioData);
    });

    // 2. Lắng nghe trạng thái trực tuyến (Live)
    firebase.database().ref('presence').on('value', (snapshot) => {
        const presenceData = snapshot.val() || {};
        const now = Date.now();
        const uniqueDevices = new Set();

        for (const key in presenceData) {
            const session = presenceData[key];
            const lastSeen = session.last_seen || 0;
            if (Math.abs(now - lastSeen) < 45000) {
                const dId = session.deviceId || key;
                uniqueDevices.add(dId);
            }
        }
        
        const onlineCount = uniqueDevices.size;
        const liveElements = [
            document.getElementById("stat-value-4"), 
            document.getElementById("stat-live-count")
        ];
        
        liveElements.forEach(elem => {
            if (elem) {
                const prev = parseInt(elem.innerText) || 0;
                elem.innerText = onlineCount;
                if (prev !== onlineCount) {
                    elem.parentElement.classList.add("pulse-highlight");
                    setTimeout(() => elem.parentElement.classList.remove("pulse-highlight"), 1000);
                }
            }
        });

        // Nếu đang ở trang Lịch sử sử dụng, cập nhật bảng luôn
        if (typeof viewPresence !== 'undefined' && viewPresence && !viewPresence.classList.contains("hidden")) {
            loadPresenceTable();
        }
    });

    // 4. Lắng nghe Lịch sử thiết bị mới
    firebase.database().ref('device_history').on('child_added', (snapshot) => {
        if (typeof viewPresence !== 'undefined' && viewPresence && !viewPresence.classList.contains("hidden")) {
            loadPresenceTable();
        }
    });

    // Thêm listener cho nút xóa lịch sử
    if (btnClearPresenceHistory) {
        btnClearPresenceHistory.addEventListener("click", async () => {
            if(!confirm("Bạn có chắc chắn muốn xóa TOÀN BỘ lịch sử truy cập thiết bị? (Dữ liệu đang Online vẫn sẽ được giữ lại)")) return;
            try {
                await firebase.database().ref('device_history').remove();
                alert("Đã xóa lịch sử thành công!");
                loadPresenceTable();
            } catch(err) {
                alert("Lỗi khi xóa: " + err.message);
            }
        });
    }
}

function updateDashboardStats(data) {
    if (!data) return;

    let items = Object.values(data);
    
    // PHÂN QUYỀN THỐNG KÊ DASHBOARD
    if (userRole === 'owner') {
        items = items.filter(item => item.ownerUid === currentUser.uid);
    }

    const total = items.length;
    
    // 1. Tính số quán Đang mở cửa
    const now = new Date();
    const currentTime = now.getHours() * 60 + now.getMinutes();
    let openCount = 0;
    
    items.forEach(item => {
        try {
            const hours = (item.OpeningHours || "08:00 - 22:00").split(" - ");
            const openStr = hours[0].split(":");
            const closeStr = hours[1].split(":");
            const openTime = parseInt(openStr[0]) * 60 + parseInt(openStr[1]);
            const closeTime = parseInt(closeStr[0]) * 60 + parseInt(closeStr[1]);
            
            if (currentTime >= openTime && currentTime < closeTime) {
                openCount++;
            }
        } catch(e) {}
    });
    
    if (userRole === 'admin') {
        const val1 = document.getElementById("stat-value-1");
        const val2 = document.getElementById("stat-value-2");
        if(val1) val1.innerText = total;
        if(val2) val2.innerText = openCount;
    } else if (userRole === 'owner') {
        // ADMIN THỐNG KÊ (Hôm nay/Tháng) được xử lý trong renderStatsView
    }
}

function renderStatsView(aggregatedPoiStats = null) {
    const rankBody = document.getElementById("table-rank-body");
    if (!rankBody) return;
    
    // Safety check: Nếu danh sách quán chưa load từ Firebase thì thoát sớm tránh lỗi crash script
    if (!window.cachedPois) {
        rankBody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:20px; opacity:0.5;">Đang tải danh sách địa điểm...</td></tr>`;
        return;
    }

    let poisData = [];
    let ids = Object.keys(window.cachedPois);

    // PHÂN QUYỀN THỐNG KÊ
    if (userRole === 'owner') {
        ids = ids.filter(id => window.cachedPois[id].ownerUid === currentUser.uid);
    }

    poisData = ids.map(id => {
        const val = window.cachedPois[id];
        const stats = (aggregatedPoiStats && aggregatedPoiStats[id]) ? aggregatedPoiStats[id] : { scans: 0, revenue: 0 };
        return {
            id,
            Ten: val.Ten || "Chưa đặt tên",
            DiaChi: val.DiaChi || "N/A",
            newScans: stats.scans,
            newRevenue: stats.revenue,
            totalScans: parseInt(val.scanCount || 0),
            totalRevenue: parseInt(val.revenue || 0)
        };
    });

    // Sắp xếp: Ưu tiên lượt quét mới trước, sau đó mới đến tổng lượt quét
    poisData.sort((a, b) => b.newScans - a.newScans || b.totalScans - a.totalScans);

    // Cập nhật thẻ thống kê cho Chủ quán nếu đang ở view thống kê
    if (userRole === 'owner') {
        const totalOwnerScans = poisData.reduce((acc, poi) => acc + poi.newScans, 0);
        const val1 = document.getElementById("stat-value-1");
        if(val1) val1.innerText = totalOwnerScans;
    }

    // Render bảng xếp hạng 10 quán đầu tiên
    rankBody.innerHTML = "";
    poisData.slice(0, 10).forEach((poi, index) => {
        const tr = document.createElement("tr");
        const rankClass = index === 0 ? "rank-1" : (index === 1 ? "rank-2" : (index === 2 ? "rank-3" : ""));
        tr.innerHTML = `
            <td><span class="rank-badge ${rankClass}">${index + 1}</span></td>
            <td style="font-weight:600; color:white;">${poi.Ten}</td>
            <td style="color:var(--accent); font-weight:bold;">${poi.newScans} lần</td>
            <td style="color:#10b981; font-weight:bold;">$${poi.newRevenue}</td>
        `;
        rankBody.appendChild(tr);
    });
}


function renderFilteredTable(data) {
    if (!tableBody) return;
    tableBody.innerHTML = "";
    
    if(!data || Object.keys(data).length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" style="text-align: center; padding: 40px; opacity: 0.5;">Không tìm thấy quán nào...</td></tr>`;
        return;
    }

    for (const [id, value] of Object.entries(data)) {
        const isPending = value.status === "pending";
        const statusBadge = isPending 
            ? `<span style="background: rgba(239, 68, 68, 0.1); color: #ef4444; border: 1px solid #ef4444; padding: 4px 8px; border-radius: 6px; font-size: 11px; font-weight: 700;">CHỜ DUYỆT</span>`
            : `<span style="background: rgba(16, 185, 129, 0.1); color: #10b981; border: 1px solid #10b981; padding: 4px 8px; border-radius: 6px; font-size: 11px; font-weight: 700;">ĐÃ DUYỆT</span>`;

        // Logic Gia Hạn (Dành cho Chủ quán)
        let giaHanHtml = `<span style="color: var(--text-secondary); opacity: 0.5;">---</span>`;
        let diffDays = 0; 
        
        if (!isPending) {
            if (value.expiryDate) {
                const expiry = new Date(value.expiryDate);
                const now = new Date();
                const diffTime = expiry - now;
                diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                
                if (diffDays < 0) {
                    giaHanHtml = `<span style="color: #ef4444; font-weight: 800; font-size: 13px;"><i class="fa-solid fa-circle-exclamation"></i> HẾT HẠN</span>`;
                } else {
                    giaHanHtml = `<span style="color: #ef4444; font-weight: 800; font-size: 13px;"><i class="fa-solid fa-clock"></i> CÒN ${diffDays} NGÀY</span>`;
                }
            } else {
                // Trường hợp đã duyệt nhưng chưa có expiryDate trong DB (dữ liệu cũ). 
                // Không để mặc định 30để tránh hiểu lầm khi cộng dồn.
                giaHanHtml = `<span style="color: #ef4444; font-weight: 800; font-size: 11px;"><i class="fa-solid fa-triangle-exclamation"></i> CHƯA CÓ HẠN</span>`;
            }
        }

        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>
                <div style="font-weight: 600; color: white;">${value.Ten || 'N/A'}</div>
                <span style="font-size: 11px; color: var(--accent);"><i class="fa-solid fa-tag"></i> ${value.Loai || 'Khác'}</span>
            </td>
            <td>
                <div style="color: var(--text-secondary); font-size: 13px;">${value.DiaChi || 'Chưa cập nhật'}</div>
                <div style="font-size: 11px; color: var(--text-secondary); opacity: 0.7;"><i class="fa-solid fa-phone"></i> ${value.SoDienThoai || 'N/A'}</div>
            </td>
            <td style="font-size: 12px; color: var(--text-secondary);">${value.Lat}, ${value.Lng}</td>
            <td>${statusBadge}</td>
            <td>
                <div style="display: flex; flex-direction: column; gap: 5px;">
                    ${giaHanHtml}
                    ${(!isPending && userRole === 'owner') ? `<button class="btn-renew-inline" onclick="openRenewModal('${id}')" style="font-size: 10px; padding: 4px 8px;">GIA HẠN</button>` : ''}
                </div>
            </td>
            <td>
                <div class="action-btns">
                    ${(userRole === 'admin' && isPending) ? `
                        <button class="btn-primary btn-sm" onclick="approvePoi('${id}')" title="Phê duyệt" style="background:#10b981; border-color:#10b981;">
                            <i class="fa-solid fa-check-circle"></i> DUYỆT
                        </button>
                    ` : ''}
                    <button class="btn-primary btn-sm btn-view" onclick="viewPoi('${id}')" title="Xem chi tiết"><i class="fa-solid fa-eye"></i></button>
                    <button class="btn-primary btn-sm" onclick="openQrModal('${id}')" title="Mã QR" style="background:#5b21b6; border-color:#5b21b6;">
                        <i class="fa-solid fa-qrcode"></i>
                    </button>
                    <button class="btn-primary btn-sm btn-edit" 
                        ${(userRole === 'owner' && !isPending && typeof diffDays !== 'undefined' && diffDays < 0) ? 'disabled style="opacity:0.3; cursor:not-allowed;" title="Vui lòng gia hạn để chỉnh sửa"' : ''} 
                        onclick="editPoi('${id}')" title="Sửa">
                        <i class="fa-solid fa-pen-to-square"></i>
                    </button>
                    ${userRole === 'admin' ? `<button class="btn-primary btn-sm btn-trash" onclick="deletePoi('${id}')" title="Xóa"><i class="fa-solid fa-trash-can"></i></button>` : ''}
                </div>
            </td>
        `;
        tableBody.appendChild(tr);
    }
}

// Logic Duyệt quán (Admin Only)
window.approvePoi = async function(id) {
    if(!confirm("Xác nhận phê duyệt quán ăn này đưa vào hoạt động chính thức?")) return;
    try {
        const poi = window.cachedPois[id];
        const ownerUid = poi.ownerUid;
        
        // Cập nhật status và khởi tạo ngày hết hạn (30 ngày từ lúc duyệt) nếu chưa có
        const updates = { status: "approved" };
        if (!poi.expiryDate) {
            updates.expiryDate = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
        }

        await firebase.database().ref('quanan/' + id).update(updates);
        
        // 2. Gán vào ownedPois của User nếu có ownerUid
        if (ownerUid) {
            await firebase.database().ref(`users/${ownerUid}/ownedPois/${id}`).set(true);
        }
        
        alert("Đã phê duyệt thành công! Quán đã bắt đầu chu kỳ 30 ngày sử dụng.");
    } catch(err) {
        alert("Lỗi khi duyệt: " + err.message);
    }
};

// ==========================================
// THUYẾT MINH & AUDIO LOGIC (HOÀN THIỆN)
// ==========================================

const availableLangs = [
    { code: "vi", text: "Tiếng Việt", flag: "vn" },
    { code: "en", text: "English", flag: "us" },
    { code: "de", text: "Deutsch", flag: "de" },
    { code: "fr", text: "Français", flag: "fr" },
    { code: "ru", text: "Русский", flag: "ru" },
    { code: "es", text: "Español", flag: "es" },
    { code: "zh", text: "中文", flag: "cn" },
    { code: "ja", text: "日本語", flag: "jp" },
    { code: "ko", text: "한국어", flag: "kr" }
];

let globalAudioPlayer = new Audio();
let currentModalPoiId = null;
let pendingUploadLang = null;

// Gán sự kiện cho modal audio
document.getElementById("btn-close-audio-modal").addEventListener("click", () => {
    modalAudio.classList.add("hidden");
    globalAudioPlayer.pause();
    document.body.style.overflow = "auto";
});
document.getElementById("btn-cancel-audio").addEventListener("click", () => {
    modalAudio.classList.add("hidden");
    globalAudioPlayer.pause();
    document.body.style.overflow = "auto";
});

window.openAudioModal = function(poiId) {
    if(!window.cachedPois || !window.cachedPois[poiId]) return;
    const data = window.cachedPois[poiId];
    currentModalPoiId = poiId;
    
    document.getElementById("audio-poi-title").innerText = data.Ten || "Quán ăn";
    document.getElementById("input-audio-id").value = poiId;
    
    renderAudioRows(data.AudioUrls || {});
    
    modalAudio.classList.remove("hidden");
    document.body.style.overflow = "hidden";
};

function renderAudioRows(audioUrlsDict) {
    const listContainer = document.getElementById("audio-lang-list");
    if (!listContainer) return;
    listContainer.innerHTML = "";
    
    availableLangs.forEach(lang => {
        const audioUrl = audioUrlsDict[lang.code];
        const hasAudio = !!audioUrl;
        
        const row = document.createElement("div");
        row.className = "audio-lang-row";
        row.innerHTML = `
            <div class="lang-info">
                <img src="https://flagcdn.com/w40/${lang.flag}.png" alt="${lang.text}">
                <div class="lang-label">${lang.text}</div>
                <div class="lang-status ${hasAudio ? 'active' : ''}">
                    ${hasAudio ? '<i class="fa-solid fa-circle-check"></i> Đã có audio' : '<i class="fa-solid fa-robot"></i> Dùng giọng AI'}
                </div>
            </div>
            <div class="row-actions">
                ${hasAudio ? `
                    <button class="btn-audio-action play" onclick="playPreview('${audioUrl}', this)" title="Nghe thử">
                        <i class="fa-solid fa-play"></i> Nghe thử
                    </button>
                    <button class="btn-audio-action upload" onclick="triggerAudioUpload('${lang.code}')" title="Thay thế file khác">
                        <i class="fa-solid fa-rotate"></i> Thay thế
                    </button>
                    <button class="btn-audio-action delete" onclick="deleteAudioLang('${lang.code}')" title="Xóa audio này">
                        <i class="fa-solid fa-trash"></i> Xóa
                    </button>
                ` : `
                    <button class="btn-audio-action upload" onclick="triggerAudioUpload('${lang.code}')" title="Tải file lên">
                        <i class="fa-solid fa-cloud-arrow-up"></i> Tải lên
                    </button>
                `}
            </div>
        `;
        listContainer.appendChild(row);
    });
}

// Preview Audio
window.playPreview = function(url, btn) {
    if (globalAudioPlayer.src === url && !globalAudioPlayer.paused) {
        globalAudioPlayer.pause();
        btn.innerHTML = '<i class="fa-solid fa-play"></i>';
        return;
    }
    
    // Reset all buttons
    document.querySelectorAll('.btn-audio-action.play').forEach(b => b.innerHTML = '<i class="fa-solid fa-play"></i>');
    
    globalAudioPlayer.src = url;
    globalAudioPlayer.play();
    btn.innerHTML = `
        <div class="wave-icon">
            <div class="wave-bar"></div>
            <div class="wave-bar"></div>
            <div class="wave-bar"></div>
        </div>
    `;
    
    globalAudioPlayer.onended = () => {
        btn.innerHTML = '<i class="fa-solid fa-play"></i>';
    };
    
    globalAudioPlayer.onerror = () => {
        alert("Không thể phát file Audio này. Link có thể đã lỗi.");
        btn.innerHTML = '<i class="fa-solid fa-play"></i>';
    };
};

// Kiểm tra kết nối Storage
window.testStorageConnection = async function() {
    const btn = document.getElementById("btn-test-storage");
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang kiểm tra...';
    btn.disabled = true;
    
    try {
        // Chờ Auth sẵn sàng
        const user = firebase.auth().currentUser;
        if (!user) {
            await firebase.auth().signInAnonymously();
        }
        
        // Thử ghi 1 file test nhỏ
        const testRef = firebase.storage().ref().child('_test_connection/test.txt');
        const blob = new Blob(['test'], { type: 'text/plain' });
        await testRef.put(blob);
        
        // Thử đọc URL
        const url = await testRef.getDownloadURL();
        
        // Xóa file test
        await testRef.delete();
        
        btn.innerHTML = '<i class="fa-solid fa-circle-check" style="color: #22c55e;"></i> Kết nối OK!';
        btn.style.borderColor = '#22c55e';
        btn.style.color = '#22c55e';
        
        setTimeout(() => {
            btn.innerHTML = originalText;
            btn.style.borderColor = '';
            btn.style.color = '';
            btn.disabled = false;
        }, 3000);
        
    } catch(err) {
        console.error("Storage Test Failed:", err);
        let errorMsg = err.code || err.message;
        
        if (err.code === 'storage/unauthorized') {
            errorMsg = 'CHƯA CÓ QUYỀN! Cần sửa Storage Rules trong Firebase Console.';
        } else if (err.code === 'auth/operation-not-allowed') {
            errorMsg = 'Chưa bật Anonymous Auth! Vào Firebase Console → Authentication → Bật Anonymous.';
        }
        
        btn.innerHTML = `<i class="fa-solid fa-triangle-exclamation" style="color: #ef4444;"></i> ${errorMsg}`;
        btn.style.borderColor = '#ef4444';
        btn.style.color = '#ef4444';
        btn.disabled = false;
        
        setTimeout(() => {
            btn.innerHTML = originalText;
            btn.style.borderColor = '';
            btn.style.color = '';
        }, 8000);
    }
};

window.triggerAudioUpload = function(langCode) {
    pendingUploadLang = langCode;
    const fileInput = document.getElementById("shared-audio-file");
    if(fileInput) fileInput.click();
};

const sharedAudioFileInput = document.getElementById("shared-audio-file");
if(sharedAudioFileInput) {
    sharedAudioFileInput.addEventListener("change", async function(e) {
        const file = e.target.files[0];
        if (!file || !pendingUploadLang || !currentModalPoiId) return;
        
        const currentId = currentModalPoiId;
        const currentLang = pendingUploadLang;
        const currentLangName = availableLangs.find(l => l.code === currentLang)?.text || currentLang;
        
        // UI UI Setup
        const statusGlobal = document.getElementById("audio-upload-status-global");
        const progressText = document.getElementById("audio-progress-text");
        const progressBar = document.getElementById("audio-progress-bar");
        const progressPercent = document.getElementById("audio-progress-percent");

        if(statusGlobal) {
            statusGlobal.style.display = "block";
            progressText.innerText = `Đang tải: ${currentLangName}`;
            progressBar.style.width = "0%";
            progressPercent.innerText = "0%";
        }

        try {
            // 1. Kiểm tra File
            if(file.size > 25 * 1024 * 1024) throw new Error("File quá lớn (tối đa 25MB)");
            
            // 2. Upload Storage với Progress
            const downloadUrl = await uploadFile(file, `audio-guides/${currentId}`, (progress) => {
                const p = Math.round(progress);
                if(progressBar) progressBar.style.width = p + "%";
                if(progressPercent) progressPercent.innerText = p + "%";
            });

            if(!downloadUrl) throw new Error("Không lấy được link file sau khi tải.");
            
            progressText.innerText = "Đang lưu vào Database...";
            console.log("Storage Done -> Link:", downloadUrl);

            // 3. Database Update
            await firebase.database().ref(`quanan/${currentId}/AudioUrls/${currentLang}`).set(downloadUrl);
            
            // 4. Update UI & Local Cache
            if (window.cachedPois && window.cachedPois[currentId]) {
                if(!window.cachedPois[currentId].AudioUrls) window.cachedPois[currentId].AudioUrls = {};
                window.cachedPois[currentId].AudioUrls[currentLang] = downloadUrl;
                
                renderAudioRows(window.cachedPois[currentId].AudioUrls);
                if(typeof applyFilters === 'function') applyFilters(); // Làm mới bảng chính
            }
            
            progressText.innerText = "Tải lên thành công!";
            setTimeout(() => {
                if(statusGlobal) statusGlobal.style.display = "none";
            }, 3000);

        } catch (err) {
            console.error("Critical Upload Error:", err);
            alert("LỖI KHÔNG THỂ LƯU: " + err.message);
            if(statusGlobal) statusGlobal.style.display = "none";
        } finally {
            e.target.value = "";
            pendingUploadLang = null;
        }
    });
}

window.deleteAudioLang = async function(langCode) {
    if (!confirm(`Xóa file audio thu âm của ngôn ngữ này? Hệ thống sẽ quay về giọng đọc AI.`)) return;
    
    try {
        await firebase.database().ref(`quanan/${currentModalPoiId}/AudioUrls/${langCode}`).remove();
        if (window.cachedPois && window.cachedPois[currentModalPoiId] && window.cachedPois[currentModalPoiId].AudioUrls) {
            delete window.cachedPois[currentModalPoiId].AudioUrls[langCode];
            renderAudioRows(window.cachedPois[currentModalPoiId].AudioUrls);
        }
    } catch (err) {
        alert("Lỗi khi xóa: " + err.message);
    }
};

// ==========================================
// QUẢN LÝ QR CODE
// ==========================================
window.openQrModal = function(id) {
    if(!isConfigured) { alert("Vui lòng cấu hình Firebase trước!"); return; }
    if(window.cachedPois && window.cachedPois[id]) {
        document.getElementById("qr-poi-id").value = id;
        const qrContainer = document.getElementById("qrcode-container");
        qrContainer.innerHTML = "";
        
        let currentHref = window.location.href.split("?")[0].split("#")[0];
        let targetUrl = "";
        
        if(currentHref.startsWith("file://")) {
            // Sử dụng domain chính thức của bạn trên Firebase để QR luôn hoạt động đúng
            targetUrl = `https://vinhkhanhtrip.web.app/scan.html#poi=${id}`;
        } else {
            let pathStr = currentHref.replace("index.html", "");
            if(!pathStr.endsWith("/")) pathStr += "/";
            targetUrl = pathStr + `scan.html#poi=${id}`;
        }
        
        const directLinkInput = document.getElementById("qr-direct-link");
        if(directLinkInput) directLinkInput.value = targetUrl;
        
        currentQrCode = new QRCode(qrContainer, {
            text: targetUrl, width: 256, height: 256,
            colorDark : "#000000", colorLight : "#ffffff",
            correctLevel : QRCode.CorrectLevel.L
        });

        document.getElementById("modal-qr").classList.remove("hidden");
        document.body.style.overflow = "hidden";
    }
}

window.copyQrLink = function() {
    const input = document.getElementById("qr-direct-link");
    if(input && input.value) {
        input.select();
        navigator.clipboard.writeText(input.value).then(() => {
            alert("Đã sao chép link thành công!");
        });
    }
}

window.downloadQR = function() {
    const canvas = document.querySelector("#qrcode-container canvas");
    if(canvas) {
        const link = document.createElement('a');
        link.download = `QR_VKTrip_${document.getElementById("qr-poi-id").value}.png`;
        link.href = canvas.toDataURL("image/png");
        link.click();
    }
}

// ==========================================
// QUẢN LÝ BẢN DỊCH (TRANSLATIONS) - REDESIGN
// ==========================================
const translationLangs = [
    { code: "vi", name: "Tiếng Việt", flag: "vn" },
    { code: "en", name: "English", flag: "us" },
    { code: "de", name: "Deutsch", flag: "de" },
    { code: "fr", name: "Français", flag: "fr" },
    { code: "ru", name: "Русский", flag: "ru" },
    { code: "es", name: "Español", flag: "es" },
    { code: "zh", name: "中文", flag: "cn" },
    { code: "ja", name: "日本語", flag: "jp" },
    { code: "ko", name: "한국어", flag: "kr" }
];

const searchTransInput = document.getElementById("search-translation-input");
if(searchTransInput) {
    searchTransInput.addEventListener("input", (e) => {
        currentSearchTrans = e.target.value;
        applyFilters();
    });
}

const modalTranslation = document.getElementById("modal-translation");
document.getElementById("btn-close-trans-modal").addEventListener("click", () => {
    modalTranslation.classList.add("hidden");
    document.body.style.overflow = "auto";
});
document.getElementById("btn-cancel-trans").addEventListener("click", () => {
    modalTranslation.classList.add("hidden");
    document.body.style.overflow = "auto";
});

function renderTranslationTable(data) {
    const tableTransBody = document.getElementById("table-translation-body");
    if(!tableTransBody) return;
    tableTransBody.innerHTML = "";
    
    if (!data || Object.keys(data).length === 0) {
        tableTransBody.innerHTML = `<tr><td colspan="3" style="text-align: center; color: var(--text-secondary); padding: 40px;">Không tìm thấy quán nào...</td></tr>`;
        return;
    }

    for (const [id, value] of Object.entries(data)) {
        const translations = value.Translations || {};
        const verified = value.VerifiedTranslations || {};
        
        // Tính % tiến độ
        const count = translationLangs.filter(l => !!translations[l.code]).length;
        const percent = Math.round((count / translationLangs.length) * 100);
        

        let progressHtml = `
            <div style="width: 100%; max-width: 200px;">
                <div style="display:flex; justify-content:space-between; font-size:11px; margin-bottom:4px;">
                    <span>${count}/${translationLangs.length} ngôn ngữ</span>
                    <span style="font-weight:bold; color:var(--accent);">${percent}%</span>
                </div>
                <div style="width:100%; height:6px; background:rgba(255,255,255,0.05); border-radius:3px; overflow:hidden;">
                    <div style="width:${percent}%; height:100%; background:linear-gradient(to right, #3b82f6, #10b981); border-radius:3px; transition:width 0.5s ease;"></div>
                </div>
            </div>
        `;

        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>
                <div style="font-weight: 600; color: white;">${value.Ten || 'N/A'}</div>
                <div style="font-size: 11px; color: var(--text-secondary);">${value.DiaChi || 'Vĩnh Khánh'}</div>
            </td>
            <td>${progressHtml}</td>
            <td>
                <div class="action-btns">
                    <button class="btn-primary btn-sm btn-edit" onclick="openTranslationModal('${id}')" title="Sửa bản dịch"><i class="fa-solid fa-pen-to-square"></i></button>
                    <button class="btn-primary btn-sm btn-trash" onclick="deleteTranslations('${id}')" title="Xóa toàn bộ bản dịch"><i class="fa-solid fa-trash-can"></i></button>
                </div>
            </td>
        `;
        tableTransBody.appendChild(tr);
    }
}

window.speakPreview = function(langCode, btn) {
    const text = document.getElementById(`modal-trans-${langCode}`).value;
    if(!text) return;
    
    // Stop any current speech
    window.speechSynthesis.cancel();
    
    const utterance = new SpeechSynthesisUtterance(text);
    
    // Map code to voices if possible
    const voices = window.speechSynthesis.getVoices();
    // Simplified matching
    const voice = voices.find(v => v.lang.startsWith(langCode));
    if(voice) utterance.voice = voice;
    
    utterance.onstart = () => btn.classList.add("speaking");
    utterance.onend = () => btn.classList.remove("speaking");
    
    window.speechSynthesis.speak(utterance);
};

window.openTranslationModal = function(id) {
    if(!window.cachedPois || !window.cachedPois[id]) return;
    const data = window.cachedPois[id];
    const translations = data.Translations || {};
    
    document.getElementById("trans-poi-title").innerText = data.Ten || "Quán ăn";
    document.getElementById("input-trans-id").value = id;
    
    const grid = document.getElementById("modal-translation-grid");
    grid.innerHTML = "";
    
    translationLangs.forEach(l => {
        let val = translations[l.code] || "";
        if(l.code === "vi" && !val) val = data.MoTa || ""; 

        const item = document.createElement("div");
        item.className = "translation-item";
        item.innerHTML = `
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">
                <label style="margin:0;"><img src="https://flagcdn.com/w40/${l.flag}.png"> ${l.name}</label>
                <div style="display:flex; gap:10px; align-items:center;">
                    <span id="char-count-${l.code}" style="font-size:10px; color:var(--text-secondary);">${val.length} kí tự</span>
                    <button onclick="speakPreview('${l.code}', this)" class="btn-icon" style="font-size:14px; color:var(--accent);" title="Nghe thử"><i class="fa-solid fa-volume-high"></i></button>
                </div>
            </div>
            <textarea id="modal-trans-${l.code}" oninput="document.getElementById('char-count-${l.code}').innerText = this.value.length + ' kí tự'" placeholder="Nhập bản dịch ${l.name}...">${val}</textarea>
        `;
        grid.appendChild(item);
    });
    
    modalTranslation.classList.remove("hidden");
    document.body.style.overflow = "hidden";
};

document.getElementById("btn-modal-save-trans").addEventListener("click", async () => {
    const id = document.getElementById("input-trans-id").value;
    if(!id) return;
    
    const btn = document.getElementById("btn-modal-save-trans");
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang lưu...';
    
    const translations = {};
    translationLangs.forEach(l => {
        translations[l.code] = document.getElementById(`modal-trans-${l.code}`).value;
    });
    
    try {
        await firebase.database().ref('quanan/' + id + '/Translations').set(translations);
        if(window.cachedPois[id]) window.cachedPois[id].Translations = translations;
        
        modalTranslation.classList.add("hidden");
        document.body.style.overflow = "auto";
        applyFilters(); 
        alert("Đã lưu bản dịch thành công!");
    } catch(err) {
        alert("Lỗi khi lưu: " + err.message);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> LƯU THAY ĐỔI';
    }
});

async function translateApi(text, targetIso) {
    if(!text || targetIso === "vi") return text;
    try {
        const url = `https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl=${targetIso}&dt=t&q=${encodeURIComponent(text)}`;
        const response = await fetch(url);
        const data = await response.json();
        let translated = "";
        data[0].forEach(item => { translated += item[0]; });
        return translated;
    } catch (error) {
        console.error("Lỗi Google Translate API:", error);
        return "";
    }
}

document.getElementById("btn-modal-auto-translate").addEventListener("click", async () => {
    const viText = document.getElementById("modal-trans-vi").value;
    if(!viText) { alert("Vui lòng nhập nội dung Tiếng Việt trước!"); return; }
    
    const btn = document.getElementById("btn-modal-auto-translate");
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang dịch...';
    
    for(const l of translationLangs) {
        if(l.code === "vi") continue;
        const result = await translateApi(viText, l.code);
        document.getElementById(`modal-trans-${l.code}`).value = result;
    }
    
    btn.disabled = false;
    btn.innerHTML = '<i class="fa-solid fa-wand-sparkles"></i> TỰ ĐỘNG DỊCH';
});

window.deleteTranslations = async function(id) {
    if(!confirm("Bạn có chắc muốn xóa TOÀN BỘ bản dịch của quán này? (Sẽ quay về dùng dịch tự động on-the-fly)")) return;
    try {
        await firebase.database().ref('quanan/' + id + '/Translations').remove();
        if(window.cachedPois[id]) delete window.cachedPois[id].Translations;
        applyFilters();
        alert("Đã xóa bản dịch thành công!");
    } catch(err) {
        alert("Lỗi khi xóa: " + err.message);
    }
};

/* === CUSTOM CALENDAR & CHART LOGIC === */
window.setFilterMode = function(mode, el) {
    filterMode = mode;
    document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    
    if (mode === 'today') {
        startDate = endDate = null;
        document.getElementById('selected-date-text').innerText = 'Hôm nay';
        initCharts();
    } else {
        renderCalendar();
        document.getElementById('calendar-popup').classList.remove('hidden');
    }
};

window.toggleCalendar = () => document.getElementById('calendar-popup').classList.toggle('hidden');

function renderCalendar() {
    const popup = document.getElementById('calendar-popup');
    if (filterMode === 'month') {
        const months = ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'];
        popup.innerHTML = `
            <div class="calendar-header">
                <button class="calendar-btn" onclick="changeYear(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                <span>Năm ${selectedYear}</span>
                <button class="calendar-btn" onclick="changeYear(1)"><i class="fa-solid fa-chevron-right"></i></button>
            </div>
            <div class="month-grid">
                ${months.map((m, i) => `<div class="month-pill ${i===selectedMonth ? 'active' : ''}" onclick="selectMonth(${i})">${m}</div>`).join('')}
            </div>
        `;
    } else {
        // Date Range Picker (Simplistic beautiful grid)
        const d = new Date(selectedYear, selectedMonth, 1);
        const daysInMonth = new Date(selectedYear, selectedMonth + 1, 0).getDate();
        const startDay = d.getDay();
        
        popup.innerHTML = `
            <div class="calendar-header">
                <button class="calendar-btn" onclick="changeMonth(-1)"><i class="fa-solid fa-chevron-left"></i></button>
                <span>Tháng ${selectedMonth + 1}, ${selectedYear}</span>
                <button class="calendar-btn" onclick="changeMonth(1)"><i class="fa-solid fa-chevron-right"></i></button>
            </div>
            <div class="calendar-days">
                ${['CN','T2','T3','T4','T5','T6','T7'].map(d => `<div style="font-size:10px; opacity:0.5; text-align:center;">${d}</div>`).join('')}
                ${Array(startDay).fill('<div class="day-cell muted"></div>').join('')}
                ${Array.from({length: daysInMonth}, (_, i) => {
                    const day = i + 1;
                    const dateStr = `${selectedYear}-${String(selectedMonth+1).padStart(2,'0')}-${String(day).padStart(2,'0')}`;
                    const isSelected = dateStr === startDate || dateStr === endDate;
                    const inRange = startDate && endDate && dateStr > startDate && dateStr < endDate;
                    return `<div class="day-cell ${isSelected ? 'selected' : ''} ${inRange ? 'range' : ''}" onclick="selectDate('${dateStr}')">${day}</div>`;
                }).join('')}
            </div>
            <div style="margin-top:15px; font-size:11px; opacity:0.7; text-align:center;">Chọn ngày bắt đầu và kết thúc (tối đa 30 ngày)</div>
        `;
    }
}

window.changeYear = (v) => { selectedYear += v; renderCalendar(); };
window.changeMonth = (v) => { 
    selectedMonth += v; 
    if(selectedMonth < 0) { selectedMonth = 11; selectedYear--; }
    if(selectedMonth > 11) { selectedMonth = 0; selectedYear++; }
    renderCalendar(); 
};

window.selectMonth = (m) => {
    selectedMonth = m;
    filterMode = 'month';
    document.getElementById('selected-date-text').innerText = `Tháng ${m+1}/${selectedYear}`;
    document.getElementById('calendar-popup').classList.add('hidden');
    initCharts();
};

window.selectDate = (d) => {
    if (!startDate || (startDate && endDate)) {
        startDate = d;
        endDate = null;
    } else {
        const start = new Date(startDate);
        const end = new Date(d);
        const diff = Math.ceil(Math.abs(end - start) / (1000 * 60 * 60 * 24));
        if (diff > 30) {
            alert("Vui lòng chọn khoảng cách tối đa 30 ngày.");
            return;
        }
        if (end < start) {
            endDate = startDate;
            startDate = d;
        } else {
            endDate = d;
        }
        document.getElementById('selected-date-text').innerText = `${startDate} -> ${endDate}`;
        document.getElementById('calendar-popup').classList.add('hidden');
        initCharts();
    }
    renderCalendar();
};

async function initHourlyChart() {
    const ctx = document.getElementById('hourlyChart').getContext('2d');
    if (hourlyChart) hourlyChart.destroy();
    
    const today = new Date().toISOString().split('T')[0];
    const snap = await firebase.database().ref(`analytics/hourly/${today}`).once('value');
    const data = snap.val() || {};
    
    const labels = Array.from({length: 24}, (_, i) => `${i}h`);
    const values = Array.from({length: 24}, (_, i) => data[i] || 0);
    
    // Tìm giờ cao điểm
    let max = -1, peak = 0;
    values.forEach((v, i) => { if(v > max) { max = v; peak = i; } });
    const peakStr = `${peak}:00`;
    
    if (userRole === 'owner') {
        const val2 = document.getElementById('stat-value-2');
        if(val2) val2.innerText = peakStr;
    }
    
    // Vẫn cập nhật ID cũ nếu có ở chỗ khác
    const oldPeak = document.getElementById('stat-peak-hour');
    if(oldPeak) oldPeak.innerText = peakStr;

    hourlyChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Lượt truy cập',
                data: values,
                borderColor: '#ef4444',
                backgroundColor: 'rgba(239, 68, 68, 0.1)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, grid: { color: 'rgba(255,255,255,0.05)' } },
                x: { grid: { display: false } }
            }
        }
    });
}

function renderAudioRankings(data) {
    if (!data) return;
    const body = document.getElementById('table-audio-rank-body');
    if(!body) return;
    
    if (!window.cachedPois) {
        // Nếu chưa có danh sách quán, thử lại sau một chút
        setTimeout(() => renderAudioRankings(data), 500);
        return;
    }
    
    const pois = Object.entries(window.cachedPois).map(([id, poi]) => {
        const stats = data[id] || { plays: 0, total_seconds: 0 };
        return { id, name: poi.Ten, ...stats };
    }).sort((a,b) => b.plays - a.plays).slice(0, 10);
    
    let totalTime = 0, totalPlays = 0;
    
    pois.forEach((poi, i) => {
        totalTime += poi.total_seconds;
        totalPlays += poi.plays;
        const avg = poi.plays > 0 ? Math.round(poi.total_seconds / poi.plays) : 0;
        
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${i+1}</td>
            <td style="font-weight:600;">${poi.name}</td>
            <td style="color:var(--accent); font-weight:bold;">${poi.plays}</td>
            <td>
                <div style="font-size:11px; opacity:0.7;">Avg: ${avg}s</div>
                <div class="progress-inline"><div class="progress-bar-fill" style="width:${Math.min(100, avg*2)}%"></div></div>
            </td>
        `;
        body.appendChild(tr);
    });
    
    const overallAvg = totalPlays > 0 ? Math.round(totalTime / totalPlays) : 0;
    
    if (userRole === 'owner') {
        const val3 = document.getElementById('stat-value-3');
        if(val3) val3.innerText = `${overallAvg}s`;
    }
    
    const oldAvg = document.getElementById('stat-avg-listen-time');
    if(oldAvg) oldAvg.innerText = `${overallAvg}s`;
}
// ==========================================
// PROFILE & ACCOUNT MANAGEMENT
// ==========================================

// Quản lý tài khoản (Admin Only)
async function loadAccountsTable() {
    // Luôn tải thông báo thanh toán kèm theo vì đã gộp trang
    if (typeof loadPaymentNotifications === 'function') {
        loadPaymentNotifications();
    }

    const tableBody = document.getElementById("table-accounts-body");
    if (!tableBody) return;
    
    try {
        const snapshot = await firebase.database().ref('users').once('value');
        const users = snapshot.val();
        tableBody.innerHTML = "";
        
        if (!users) return;

        for (const [uid, user] of Object.entries(users)) {
            const roleText = user.role === 'admin' ? 'Quản trị viên' : 'Chủ quán VIP';
            const poiCount = user.ownedPois ? Object.keys(user.ownedPois).length : 0;
            const isBlocked = user.status === 'blocked';
            const statusLabel = isBlocked 
                ? '<span class="status-badge status-blocked">Bị khóa</span>' 
                : '<span class="status-badge status-active">Hoạt động</span>';

            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>
                    <div style="display:flex; align-items:center; gap:10px;">
                        <div style="width:36px; height:36px; border-radius:50%; background:var(--gold-gradient); display:flex; align-items:center; justify-content:center; font-weight:bold; color:black;">
                            ${(user.name || 'U').charAt(0).toUpperCase()}
                        </div>
                        <div>
                            <div style="font-weight:600; color:white;">${user.name || 'Người dùng'}</div>
                            <div style="font-size:11px; opacity:0.6;">${user.email || ''} <span class="poi-tag" style="padding:1px 6px; font-size:9px; vertical-align:middle;">${roleText}</span></div>
                        </div>
                    </div>
                </td>
                <td style="text-align:center;"><b style="color:var(--accent); font-size:16px;">${poiCount}</b> quán</td>
                <td style="text-align:center;">${statusLabel}</td>
                <td>
                    <div class="action-btns" style="justify-content: center;">
                        <button class="btn-primary btn-sm btn-view" onclick="showUserDetail('${uid}')" title="Xem chi tiết">
                            <i class="fa-solid fa-eye"></i>
                        </button>
                        ${user.role !== 'admin' ? `
                            <button class="btn-primary btn-sm ${isBlocked ? 'btn-unlock' : 'btn-lock'}" onclick="toggleBlockUser('${uid}', '${user.status || 'active'}')" title="${isBlocked ? 'Mở khóa' : 'Khóa tài khoản'}">
                                <i class="fa-solid ${isBlocked ? 'fa-lock-open' : 'fa-lock'}"></i>
                            </button>
                            <button class="btn-primary btn-sm btn-trash" onclick="deleteUserAccount('${uid}')" title="Xóa tài khoản">
                                <i class="fa-solid fa-user-slash"></i>
                            </button>
                        ` : ''}
                    </div>
                </td>
            `;
            tableBody.appendChild(tr);
        }
    } catch(err) {
        console.error(err);
    }
}

// Hàm hiển thị chi tiết đối tác toàn diện
window.showUserDetail = async function(uid) {
    try {
        const snap = await firebase.database().ref(`users/${uid}`).once('value');
        const user = snap.val();
        if(!user) return;

        // 1. Thông tin cơ bản
        document.getElementById("detail-user-head").innerHTML = `
            <div style="font-size:20px; font-weight:800; color:var(--accent);">${user.name || 'N/A'}</div>
            <div style="opacity:0.6; font-size:13px;">${user.email} | Role: ${user.role === 'admin' ? 'Admin' : 'Partner'}</div>
        `;

        // 2. Danh sách quán
        const listContainer = document.getElementById("detail-poi-list");
        listContainer.innerHTML = "";
        
        const ownedPoisList = Object.entries(window.cachedPois || {}).filter(([id, p]) => p.ownerUid === uid);
        
        if(ownedPoisList.length === 0) {
            listContainer.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:30px; opacity:0.3; font-style:italic;">Chưa có quán ăn nào được liên kết với tài khoản này.</td></tr>';
        } else {
            ownedPoisList.forEach(([id, p], index) => {
                const isPending = p.status === "pending";
                
                const startTs = p.createdAt || Date.now();
                const expiryTs = p.expiryDate || (startTs + 30 * 24 * 60 * 60 * 1000);
                
                const startStr = new Date(startTs).toLocaleDateString('vi-VN');
                const expiryStr = new Date(expiryTs).toLocaleDateString('vi-VN');
                
                const diffTime = expiryTs - Date.now();
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                
                let countdownHtml = "";
                if(diffDays < 0) {
                    countdownHtml = `<span style="color:#ef4444; font-weight:bold;">Hết hạn</span>`;
                } else if(diffDays <= 7) {
                    countdownHtml = `<span style="color:#f59e0b; font-weight:bold;">${diffDays} ngày</span>`;
                } else {
                    countdownHtml = `<span style="color:#10b981;">${diffDays} ngày</span>`;
                }

                const statusBadge = isPending 
                    ? `<span style="color:#ef4444; background:rgba(239,68,68,0.1); padding:2px 8px; border-radius:6px; font-size:10px; border:1px solid rgba(239,68,68,0.2); font-weight:700;">CHỜ DUYỆT</span>`
                    : `<span style="color:#10b981; background:rgba(16,185,129,0.1); padding:2px 8px; border-radius:6px; font-size:10px; border:1px solid rgba(16,185,129,0.2); font-weight:700;">ĐÃ DUYỆT</span>`;

                const tr = document.createElement("tr");
                tr.style.borderBottom = "1px solid rgba(255,255,255,0.02)";
                tr.innerHTML = `
                    <td style="opacity:0.4; text-align:center; font-family: monospace; font-size:11px;">${String(index + 1).padStart(2, '0')}</td>
                    <td>
                        <div style="font-weight:700; color:#fff; font-size:12px;">${p.Ten}</div>
                        <div style="font-size:9px; opacity:0.4; text-transform:uppercase;">${p.Loai || 'Khác'}</div>
                    </td>
                    <td style="text-align:center; opacity:0.7; font-size:11px;">${startStr}</td>
                    <td style="text-align:center; opacity:0.7; font-size:11px; color:var(--accent);">${expiryStr}</td>
                    <td style="text-align:center; font-size:11px;">${countdownHtml}</td>
                    <td style="text-align:center;">${statusBadge}</td>
                `;
                listContainer.appendChild(tr);
            });
        }


        document.getElementById("modal-user-detail").classList.remove("hidden");
    } catch(err) {
        console.error(err);
        alert("Không thể tải chi tiết đối tác.");
    }
}

// --- PROFILE MANAGEMENT ---
document.getElementById("user-profile").addEventListener("click", openMyProfile);

async function openMyProfile() {
    if (!currentUser) return;
    
    // Switch UI
    switchView(null, viewProfile);
    
    try {
        const snap = await firebase.database().ref('users/' + currentUser.uid).once('value');
        const data = snap.val() || {};
        
        document.getElementById("prof-name").value = data.name || "";
        document.getElementById("prof-email").value = currentUser.email || "";
        document.getElementById("prof-phone").value = data.phone || "";
        document.getElementById("prof-password").value = ""; // Luôn để trống
        
        document.getElementById("profile-name-display").innerText = data.name || "Chủ quán";
        document.getElementById("profile-avatar-large").innerText = (data.name || "A").charAt(0).toUpperCase();
        document.getElementById("profile-role-display").innerText = userRole === 'admin' ? "Quản trị viên" : "Đối tác VIP";
        
    } catch(err) {
        console.error(err);
    }
}

document.getElementById("btn-save-profile").addEventListener("click", async () => {
    if (!currentUser) return;
    
    const newName = document.getElementById("prof-name").value;
    const newEmail = document.getElementById("prof-email").value;
    const newPhone = document.getElementById("prof-phone").value;
    const newPass = document.getElementById("prof-password").value;
    
    const btn = document.getElementById("btn-save-profile");
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> ĐANG LƯU...';
    
    try {
        // 1. Cập nhật Database
        await firebase.database().ref('users/' + currentUser.uid).update({
            name: newName,
            phone: newPhone
        });
        
        // 2. Cập nhật Auth Email (nếu đổi)
        if (newEmail !== currentUser.email) {
            await currentUser.updateEmail(newEmail);
        }
        
        // 3. Cập nhật Auth Password (nếu nhập)
        if (newPass) {
            await currentUser.updatePassword(newPass);
        }
        
        alert("Cập nhật hồ sơ thành công!");
        
        // Refresh giao diện Sidebar
        document.getElementById("display-user-name").innerText = newName;
        document.getElementById("user-avatar-initial").innerText = newName.charAt(0).toUpperCase();
        
        openMyProfile(); // Reload form
        
    } catch (err) {
        console.error(err);
        if (err.code === 'auth/requires-recent-login') {
            alert("Vì lý do bảo mật, bạn cần đăng xuất và đăng nhập lại để thay đổi Email/Mật khẩu.");
            firebase.auth().signOut();
        } else {
            alert("Lỗi: " + err.message);
        }
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalText;
    }
});

// ==========================================
// HỆ THỐNG GIA HẠN & THANH TOÁN
// ==========================================

window.checkOwnerContracts = function() {
    const banner = document.getElementById("expiry-banner");
    if (!window.cachedPois) return;

    let hasWarning = false;
    const now = new Date();
    
    Object.values(window.cachedPois).forEach(poi => {
        if (poi.ownerUid === currentUser.uid && poi.expiryDate) {
            const expiry = new Date(poi.expiryDate);
            const diffDays = Math.ceil((expiry - now) / (1000 * 60 * 60 * 24));
            
            if (diffDays <= 7) {
                hasWarning = true;
            }
        }
    });

    // Chỉ hiển thị Banner cảnh báo, không khóa toàn trang
    if (hasWarning && banner) {
        banner.classList.remove("hidden");
    } else if (banner) {
        banner.classList.add("hidden");
    }
};

let currentRenewPoiId = null;
let currentPackagePrice = 500000;
let currentPackageMonths = 1;

window.openRenewModal = function(id) {
    const poi = window.cachedPois[id];
    if (!poi) return;
    
    currentRenewPoiId = id;
    document.getElementById("renew-poi-name").innerText = poi.Ten;
    
    // Tính toán số ngày còn lại để hiển thị màu đỏ
    let remainingText = "";
    if (poi.expiryDate) {
        const expiry = new Date(poi.expiryDate);
        const now = new Date();
        const diffDays = Math.ceil((expiry - now) / (1000 * 60 * 60 * 24));
        remainingText = `<span style="color: #ef4444; font-weight: 700;">(Còn ${diffDays < 0 ? 'đã hết hạn' : diffDays + ' ngày'})</span>`;
    }

    document.getElementById("renew-poi-expiry").innerHTML = `Hạn dùng hiện tại: ${poi.expiryDate || 'Chưa thiết lập'} ${remainingText}`;
    
    // Mặc định chọn gói 1 tháng
    selectPackage(1, 500000);
    
    document.getElementById("modal-renew").classList.remove("hidden");
};

window.selectPackage = function(months, price) {
    currentPackageMonths = months;
    currentPackagePrice = price;
    
    // UI Active
    document.querySelectorAll(".package-card").forEach((card, idx) => {
        if ((idx === 0 && months === 1) || (idx === 1 && months === 6) || (idx === 2 && months === 12)) {
            card.classList.add("active");
        } else {
            card.classList.remove("active");
        }
    });
    
    // Update QR & Content
    const content = `VKT ${currentRenewPoiId.substring(0,6)} GH${months}`;
    document.getElementById("payment-content").innerText = content;
    
    // QR generator: ngân hàng 970422 (MB), stk 0938634868, amount, addInfo
    const qrUrl = `https://img.vietqr.io/image/970422-0938634868-compact.jpg?amount=${price}&addInfo=${encodeURIComponent(content)}`;
    document.getElementById("qr-payment-img").src = qrUrl;
};

document.getElementById("btn-confirm-payment").addEventListener("click", async () => {
    if (!currentRenewPoiId) return;
    
    const btn = document.getElementById("btn-confirm-payment");
    const originalText = btn.innerText;
    btn.disabled = true;
    btn.innerText = "ĐANG GỬI THÔNG BÁO...";
    
    try {
        await firebase.database().ref('payment_notifications').push({
            poiId: currentRenewPoiId,
            poiName: window.cachedPois[currentRenewPoiId].Ten,
            ownerUid: currentUser.uid,
            ownerEmail: currentUser.email,
            packageMonths: currentPackageMonths,
            amount: currentPackagePrice,
            createdAt: Date.now(),
            status: 'pending'
        });
        
        alert("Thông báo thanh toán đã được gửi tới Quản trị viên. Chúng tôi sẽ phê duyệt yêu cầu của bạn chậm nhất trong 24h.");
        closeModal('modal-renew');
    } catch (err) {
        alert("Lỗi: " + err.message);
    } finally {
        btn.disabled = false;
        btn.innerText = "TÔI ĐÃ CHUYỂN KHOẢN";
    }
});

// LOGIC PHÊ DUYỆT THANH TOÁN (Admin Only)
window.loadPaymentNotifications = function() {
    const tableBody = document.getElementById("table-payments-body");
    if (!tableBody) return;
    
    tableBody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:40px; opacity:0.5;">Đang tải yêu cầu...</td></tr>`;
    
    firebase.database().ref('payment_notifications').on('value', (snapshot) => {
        const data = snapshot.val();
        tableBody.innerHTML = "";
        
        if (!data) {
            tableBody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:40px; opacity:0.5;">Không có yêu cầu thanh toán nào cần xử lý.</td></tr>`;
            return;
        }
        
        Object.entries(data).forEach(([key, val]) => {
            if (val.status !== 'pending') return; // Chỉ hiện các yêu cầu chưa duyệt
            
            const tr = document.createElement("tr");
            const ts = val.createdAt || val.timestamp || Date.now();
            const dateStr = new Date(ts).toLocaleString('vi-VN');
            
            tr.innerHTML = `
                <td>
                    <div style="font-weight: 700; color: var(--accent);">${val.poiName}</div>
                    <div style="font-size: 11px; opacity: 0.6;">D/S: ${dateStr}</div>
                </td>
                <td>
                    <div style="font-weight: 600;">${val.ownerEmail}</div>
                    <div style="font-size: 10px; opacity: 0.5;">UID: ${val.ownerUid.substring(0,8)}...</div>
                </td>
                <td>
                    <div style="color: #10b981; font-weight: 700;">${val.packageMonths} THÁNG</div>
                    <div style="font-size: 11px;">${(val.amount/1000).toLocaleString()}k VND</div>
                </td>
                <td>
                    <code style="background: rgba(255,255,255,0.1); padding: 4px 8px; border-radius: 4px; color: var(--accent);">VKT ${val.poiId.substring(0,6)} GH${val.packageMonths}</code>
                </td>
                <td>
                    <div class="action-btns">
                        <button class="btn-primary btn-sm" onclick="approveRenewal('${key}', '${val.poiId}', ${val.packageMonths})" style="background: #10b981; border:none;" title="Duyệt và cộng hạn">
                            <i class="fa-solid fa-check-circle"></i> DUYỆT
                        </button>
                        <button class="btn-primary btn-sm btn-trash" onclick="deleteNotification('${key}')" title="Xóa bỏ yêu cầu">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </div>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    });
};

window.approveRenewal = async function(notifId, poiId, months) {
    if (!confirm(`Xác nhận đã nhận tiền và cộng thêm ${months} tháng sử dụng cho quán này?`)) return;
    
    try {
        const poi = window.cachedPois[poiId];
        if (!poi) throw new Error("Không tìm thấy thông tin quán.");
        
        let currentExpiry = null;
        if (poi.expiryDate) {
            currentExpiry = new Date(poi.expiryDate);
        }
        
        const now = new Date();
        let baseDate = now;
        
        // LOGIC CỘNG DỒN NÂNG CAO: 
        // 1. Nếu quán vẫn còn hạn (currentExpiry > now): Lấy ngày hết hạn hiện tại làm gốc để cộng tiếp.
        // 2. Nếu quán đã hết hạn hoặc chưa có hạn: Lấy ngày hôm nay làm gốc để bắt đầu chu kỳ mới.
        if (currentExpiry && currentExpiry > now) {
            baseDate = currentExpiry;
        }
        
        // Thực hiện cộng thêm số tháng (Dùng setMonth để xử lý các tháng có độ dài khác nhau 28-31 ngày)
        const newExpiry = new Date(baseDate);
        newExpiry.setMonth(newExpiry.getMonth() + months);
        
        const finalDateStr = newExpiry.toISOString().split('T')[0];
        
        // 1. Cập nhật ngày hết hạn cho quán
        await firebase.database().ref(`quanan/${poiId}`).update({
            expiryDate: finalDateStr,
            status: "approved" // Luôn active lại quán sau khi nạp tiền
        });
        
        // 2. Đánh dấu thông báo là đã duyệt (hoặc xóa đi)
        await firebase.database().ref(`payment_notifications/${notifId}`).remove();
        
        alert(`Gia hạn thành công! Hạn dùng mới: ${finalDateStr} (Cộng nối tiếp từ hạn cũ).`);
    } catch(err) {
        alert("Lỗi khi duyệt: " + err.message);
    }
};

window.deleteNotification = async function(id) {
    if(!confirm("Xác nhận xóa bỏ yêu cầu này?")) return;
    await firebase.database().ref(`payment_notifications/${id}`).remove();
};

// LOGIC KHÓA/XÓA TÀI KHOẢN
window.toggleBlockUser = async function(uid, currentStatus) {
    const newStatus = currentStatus === 'blocked' ? 'active' : 'blocked';
    const actionText = newStatus === 'blocked' ? "KHÓA" : "MỞ KHÓA";
    
    if(!confirm(`Bạn có chắc chắn muốn ${actionText} tài khoản này không?`)) return;
    
    try {
        await firebase.database().ref(`users/${uid}`).update({
            status: newStatus
        });
        alert(`Đã ${actionText} tài khoản thành công!`);
        loadAccountsTable();
    } catch(err) {
        alert("Lỗi: " + err.message);
    }
};

window.deleteUserAccount = async function(uid) {
    if(!confirm("CẢNH BÁO: Bạn đang thực hiện XÓA vĩnh viễn tài khoản đối tác này khỏi Database. Các quán ăn của họ sẽ không bị xóa nhưng sẽ mất liên kết. Tiếp tục?")) return;
    
    try {
        await firebase.database().ref(`users/${uid}`).remove();
        alert("Đã xóa tài khoản đối tác thành công!");
        loadAccountsTable();
    } catch(err) {
        alert("Lỗi khi xóa: " + err.message);
    }
};

// === GIÁM SÁT THIẾT BỊ (PRESENCE) ===
async function loadPresenceTable() {
    const tableBody = document.getElementById("table-presence-body");
    const totalEl = document.getElementById("presence-total-count");
    const onlineEl = document.getElementById("presence-online-count");
    
    if (!tableBody) return;

    try {
        // 1. Lấy dữ liệu Real-time (Presence) để biết ai đang Online
        const presenceSnap = await firebase.database().ref('presence').once('value');
        const presenceData = presenceSnap.val() || {};
        const now = Date.now();

        // 2. Lấy dữ liệu Lịch sử (Persistent History)
        const historySnap = await firebase.database().ref('device_history').once('value');
        const historyData = historySnap.val() || {};
        
        tableBody.innerHTML = "";
        let total = 0;
        let online = 0;

        // Sắp xếp lịch sử theo thời gian vào (mới nhất lên đầu)
        const historyEntries = Object.entries(historyData)
            .filter(([id, data]) => data && typeof data === 'object') // Bảo vệ nếu data lỗi
            .sort((a, b) => (b[1].entry_time || 0) - (a[1].entry_time || 0));

        if (historyEntries.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:40px; opacity:0.5;">Chưa ghi nhận lịch sử thiết bị nào.</td></tr>`;
            if(totalEl) totalEl.innerText = "0";
            onlineEl && (onlineEl.innerText = "0");
            return;
        }

        historyEntries.forEach(([sid, hSession]) => {
            total++;
            
            // Tìm trong Presence xem Session này có đang Online không
            const pSession = presenceData[sid];
            let isOnline = false;
            if (pSession) {
                const lastSeen = pSession.last_seen || 0;
                isOnline = Math.abs(now - lastSeen) < 45000;
            }

            if (isOnline) online++;

            const entryTime = hSession.entry_time || 0;
            const date = new Date(entryTime);
            const dateStr = date.toLocaleDateString('vi-VN');
            const timeStr = date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

            const statusBadge = isOnline 
                ? '<span class="status-online-badge"><i class="fa-solid fa-circle-dot"></i> Online</span>' 
                : '<span class="status-offline-badge"><i class="fa-solid fa-moon"></i> Offline</span>';

            // Phân loại App/WebApp
            const source = hSession.source || (hSession.platform === 'web' ? 'webapp' : 'app');
            const sourceLabel = source === 'webapp' 
                ? '<span class="poi-tag" style="background:rgba(59, 130, 246, 0.1); color:#3b82f6; border-color:rgba(59, 130, 246, 0.2);">WebApp</span>'
                : '<span class="poi-tag" style="background:rgba(16, 185, 129, 0.1); color:#10b981; border-color:rgba(16, 185, 129, 0.2);">App</span>';

            const platformDisplay = (hSession.platform || 'unknown').toUpperCase();

            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>
                    <div style="font-weight:600; color:white;">${hSession.device || 'Thiết bị lạ'}</div>
                    <div style="font-size:10px; opacity:0.4; font-family:monospace;">ID: ${hSession.deviceId ? hSession.deviceId.substring(0, 12) : sid.substring(0, 10)}...</div>
                </td>
                <td>${sourceLabel}</td>
                <td>
                    <span style="font-size:12px; opacity:0.8;">${platformDisplay}</span>
                </td>
                <td>
                    <div style="font-weight:550; color:var(--accent);">${timeStr}</div>
                    <div style="font-size:11px; opacity:0.5;">${dateStr}</div>
                </td>
                <td>${statusBadge}</td>
            `;
            tableBody.appendChild(tr);
        });

        if(totalEl) totalEl.innerText = total;
        if(onlineEl) onlineEl.innerText = online;

    } catch (err) {
        console.error("Lỗi tải lịch sử thiết bị:", err);
    }
}
