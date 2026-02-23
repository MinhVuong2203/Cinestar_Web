Scaffold-DbContext "Server=edumallserver.database.windows.net;Database=Cinestar;User ID=edumall;Password=***;TrustServerCertificate=True;Encrypt=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context CineStarContext -UseDatabaseNames -NoOnConfiguring -DataAnnotations -Force

Dùng Visual Studio 2022 (VS tím)

- Hệ thống web rạp chiếu phim Cinestar được xây dựng nhằm hỗ trợ việc vận hành và quản lý toàn bộ hoạt động của một rạp chiếu phim hiện đại. Ứng dụng này hướng đến việc số hóa quy trình làm việc của nhân viên và trải nghiệm của khách hàng thông qua cơ sở dữ liệu tập trung, giúp theo dõi phim, phòng, ghế, suất chiếu, vé, hóa đơn, thanh toán, sản phẩm bán kèm và lịch làm việc.

- Phạm vi của hệ thống bao gồm hai mảng chính:
  + Về phía admin(dành cho quản trị viên) và mảng nhân viên (dành cho nhân viên tương ứng). Mảng nhân viên gồm các nhân viên có thể trực tiếp thao tác như nhiên viên bán vé, nhân viên trị phim, nhân viên quản trị phòng. Trong mảng quản trị viên, người đó có thể thao tác trên tất cả các chức năng của hệ thống bao gồm thêm phim mới, thiết lập lịch chiếu, theo dõi doanh thu, phân ca làm việc, xử lý hóa đơn, thống kê hiệu quả hoạt động của từng nhân viên, thiết lập phòng chiếu, sắp xếp vị trí các ghế trong phòng. Trong mảng nhân viên, tùy thuộc vào chức vụ của nhân viên mà có các chức năng tương ứng. Nhân viên bán vé có chức năng bán vé, bán bắp nước và xem doanh thu. Nhân viên phim có các chức năng thêm mới phim, thêm mới xuất chiếu. Nhân viên phòng chiếu có các chức năng thêm mới phòng chiếu.
  + Về phía khách hàng, khách hàng có thể truy cập vào hệ thống website để tìm kiếm các bộ phim theo nhu cầu, tham khảo giá vé của từng ghế, xem được ưu đãi của tài khoản của họ. Hệ thống được thiết kế đặt vé realtime sử dụng công nghệ SignalR giúp khách hàng nhận biết ghế nào đã được chọn, ghế nào còn trống. Hệ thống sử dụng thanh toán online thông qua thanh toán mã QR sử dụng cổng giao dịch PayOS.

- Tất cả dữ liệu đều được lưu trữ trong cơ sở dữ liệu SQL Server được thiết kế để đảm bảo tính toàn vẹn, dễ mở rộng và đồng bộ giữa nhiều người dùng.
