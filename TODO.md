# TODO - Fix 2 lỗi nghiêm trọng

## Backend (Lỗi 1)
- [x] Xác định chính xác đường dẫn file .csproj đang tồn tại trong repo.
- [x] Cung cấp lệnh `dotnet build` trỏ đúng tới `.csproj` (không đổi cấu trúc project).
- [ ] (Nếu cần) Cung cấp thêm lệnh `dotnet build` chạy theo solution nếu bạn muốn.


## Frontend (Lỗi 2)
- [ ] Kiểm tra `MIS_Project_FE/src/pages/manager/ManagerDashboard.jsx` nơi xảy ra crash `reading 'map' of undefined`.
- [ ] Thêm state `isLoading` và `error` bằng useState.
- [ ] Bọc UI theo loading/error state: hiển thị “Loading...” và message lỗi.
- [ ] Đảm bảo data cho List/Table luôn là mảng (fallback = []) trước khi render.
- [ ] (Không dùng optional chaining đơn thuần) giữ cách xử lý rõ ràng để debug dễ.

