CREATE DATABASE Cinestar;
USE Cinestar;

-- MÔN WEB

/* ========== 1. EMPLOYEE GROUP ========== */

CREATE TABLE CinemaBranch (
    BranchID VARCHAR(10) PRIMARY KEY,
    BranchName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(255),
    City NVARCHAR(100),
    District NVARCHAR(100),
    Phone VARCHAR(20),
    Email NVARCHAR(100),
    OpenHour TIME NOT NULL DEFAULT '08:00',
    CloseHour TIME NOT NULL DEFAULT '23:00',
    MapUrl NVARCHAR(255),
    ImageUrl NVARCHAR(255),
    Description NVARCHAR(MAX),
    IsDeleted BIT DEFAULT 0 NOT NULL
);

GO
CREATE TRIGGER trg_CinemaBranch_Insert
ON CinemaBranch
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO CinemaBranch (BranchID, BranchName, Address, City, District, Phone, Email, OpenHour, CloseHour, MapUrl, ImageUrl, Description, IsDeleted)
    SELECT
        'BRH-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,5)),  -- BRH-XXXXX
        BranchName, Address, City, District, Phone, Email, OpenHour, CloseHour, MapUrl, ImageUrl, Description, IsDeleted
    FROM inserted;
END;
GO
-- === Danh sách chi nhánh Cinestar (có MapUrl) ===
INSERT INTO CinemaBranch (BranchName, Address, City, District, Phone, Email, OpenHour, CloseHour, MapUrl, ImageUrl, Description)
VALUES
-- TP.HCM
(N'Cinestar Quốc Thanh (TP.HCM)', N'271 Nguyễn Trãi, Phường Nguyễn Cư Trinh, Quận 1', N'TP.HCM', N'Quận 1',NULL, NULL, N'08:00 - 23:00', N'https://www.google.com/maps/place/Cinestar+Qu%E1%BB%91c+Thanh/@10.7621425,106.6884932,17z/',NULL, N'Rạp chiếu phim hiện đại nằm tại trung tâm Quận 1, TP.HCM'),
(N'Cinestar Hai Bà Trưng (TP.HCM)', N'135 Hai Bà Trưng, Phường Bến Nghé, Quận 1', N'TP.HCM', N'Quận 1',NULL, NULL, N'08:00 - 23:00', N'https://www.google.com/maps/place/Cinestar+Hai+B%C3%A0+Tr%C6%B0ng/@10.7779282,106.6987733,17z/',NULL, N'Rạp chiếu phim hiện đại phục vụ khu vực trung tâm thành phố'),
(N'Cinestar Sinh Viên (TP.HCM)', N'19 Cao Thắng, Phường 2, Quận 3', N'TP.HCM', N'Quận 3',NULL, NULL, N'08:00 - 23:00', N'https://www.google.com/maps/place/Cinestar+Sinh+Vi%C3%AAn/@10.7736403,106.6820189,17z/',NULL, N'Rạp hướng đến đối tượng học sinh, sinh viên với giá vé ưu đãi'),
(N'Cinestar Satra Quận 6 (TP.HCM)', N'Tầng 4, TTTM Satra, 79 Kinh Dương Vương, P.12, Quận 6', N'TP.HCM', N'Quận 6', NULL, NULL, N'08:00 - 23:00',  N'https://www.google.com/maps/place/Cinestar+Satra+Qu%E1%BA%ADn+6/@10.7459881,106.6274459,17z/', NULL,  N'Rạp chiếu phim hiện đại phục vụ khu vực Tây TP.HCM'),
-- Miền Trung
(N'Cinestar Huế (TP. Huế)', N'25 Hai Bà Trưng, TP. Huế', N'Thừa Thiên Huế', N'TP. Huế', NULL, NULL, N'08:00 - 23:00', N'https://www.google.com/maps/place/Cinestar+Hu%E1%BA%BF/@16.4654929,107.5959275,17z/', NULL,  N'Rạp Cinestar tại trung tâm thành phố Huế'),
-- Tây Nguyên
(N'Cinestar Đà Lạt (Lâm Đồng)',  N'Quảng trường Lâm Viên, TP. Đà Lạt', N'Lâm Đồng', N'TP. Đà Lạt', NULL, NULL, N'08:00 - 23:00',  N'https://www.google.com/maps/place/Cinestar+Dalat/@11.9410438,108.4447163,17z/', NULL,  N'Rạp Cinestar với không gian lãng mạn tại Đà Lạt'),
(N'Cinestar Lâm Đồng (Đức Trọng)',  N'QL 20, Thị trấn Liên Nghĩa, Huyện Đức Trọng', N'Lâm Đồng', N'Đức Trọng', NULL, NULL, N'08:00 - 23:00',  N'https://www.google.com/maps/place/Cinestar+%C4%90%E1%BB%A9c+Tr%E1%BB%8Dng/@11.741356,108.373574,17z/', NULL,  N'Rạp chiếu phim phục vụ khu vực Đức Trọng - Lâm Đồng'),
-- Miền Tây
(N'Cinestar Mỹ Tho (Tiền Giang)',  N'01 Ấp Bắc, Phường 5, TP. Mỹ Tho', N'Tiền Giang', N'TP. Mỹ Tho', NULL, NULL, N'08:00 - 23:00',  N'https://www.google.com/maps/place/Cinestar+M%E1%BB%B9+Tho/@10.3554113,106.3587659,17z/', NULL,  N'Rạp Cinestar tại trung tâm thành phố Mỹ Tho'),
(N'Cinestar Kiên Giang (An Giang)',  N'Nguyễn Trung Trực, TP. Rạch Giá', N'Kiên Giang', N'Rạch Giá', NULL, NULL, N'08:00 - 23:00',  N'https://www.google.com/maps/place/Cinestar+Ki%C3%AAn+Giang/@10.0161215,105.0760212,17z/', NULL,  N'Rạp Cinestar phục vụ khu vực đồng bằng sông Cửu Long');

CREATE TABLE Employee (
    EmployeeID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BranchID VARCHAR(10) NULL FOREIGN KEY REFERENCES CinemaBranch(BranchID) ON DELETE SET NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) UNIQUE,
    Email NVARCHAR(100) UNIQUE,
    Address NVARCHAR(255),
    BirthDate DATE,
    HourWage INT,
    CCCD NVARCHAR(20) UNIQUE,
    Gender NVARCHAR(10),
    Role NVARCHAR(20),
    Username VARCHAR(50),
    PasswordHash VARCHAR(255),
    ImageUrl NVARCHAR(255),
    RegisterDate DATE DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0 NOT NULL
);

-- Insert 8 nhân viên mẫu
INSERT INTO Employee (BranchID, FullName, Phone, Email, Address, BirthDate, HourWage, CCCD, Gender, Role, Username, PasswordHash, ImageUrl, RegisterDate, IsDeleted)
VALUES
-- Quản lý
('BRH-54E61', N'Nguyễn Văn An', '0901234567', 'nguyenvanan@cinestar.vn', N'123 Nguyễn Trãi, Q.1, TP.HCM', '1985-05-15', 80000, '001085012345', N'Nam', N'Quản lý', 'vanan', 'hashed_password_1', '/image/employees/manager1.jpg', '2020-01-10', 0),
('BRH-76E9D', N'Trần Thị Bình', '0902345678', 'tranthibinh@cinestar.vn', N'456 Lê Lợi, Q.3, TP.HCM', '1990-08-20', 75000, '001090023456', N'Nữ', N'Quản lý', 'thibinh', 'hashed_password_2', '/image/employees/manager2.jpg', '2020-03-15', 0),
-- Thu ngân
('BRH-54E61', N'Lê Minh Châu', '0903456789', 'leminhchau@cinestar.vn', N'789 Võ Văn Tần, Q.3, TP.HCM', '1995-03-10', 50000, '001095034567', N'Nữ', N'Thu ngân', 'minhchau', 'hashed_password_3', '/image/employees/cashier1.jpg', '2021-06-20', 0),
('BRH-54E61', N'Phạm Hoàng Dũng', '0904567890', 'phamhoangdung@cinestar.vn', N'321 Hai Bà Trưng, Q.1, TP.HCM', '1998-11-25', 50000, '001098045678', N'Nam', N'Thu ngân', 'hoangdung', 'hashed_password_4', '/image/employees/cashier2.jpg', '2021-08-10', 0),
-- Nhân viên bán vé
('BRH-76E9D', N'Võ Thị Lan', '0905678901', 'vothilan@cinestar.vn', N'654 Cách Mạng Tháng 8, Q.10, TP.HCM', '1999-07-05', 45000, '001099056789', N'Nữ', N'Nhân viên bán vé', 'thilan', 'hashed_password_5', '/image/employees/ticket1.jpg', '2022-01-15', 0),
('BRH-76E9D', N'Hoàng Văn Hùng', '0906789012', 'hoangvanhung@cinestar.vn', N'987 Nguyễn Văn Cừ, Q.5, TP.HCM', '1997-12-30', 45000, '001097067890', N'Nam', N'Nhân viên bán vé', 'vanhung', 'hashed_password_6', '/image/employees/ticket2.jpg', '2022-03-20', 0),
-- Nhân viên kỹ thuật
('BRH-54E61', N'Đặng Thị Mai', '0907890123', 'dangthimai@cinestar.vn', N'147 Lý Thường Kiệt, Q.Tân Bình, TP.HCM', '1993-04-18', 60000, '001093078901', N'Nữ', N'Nhân viên kỹ thuật', 'thimai', 'hashed_password_7', '/image/employees/tech1.jpg', '2021-11-05', 0),
-- Bảo vệ
('BRH-76E9D', N'Trương Văn Sơn', '0908901234', 'truongvanson@cinestar.vn', N'258 Phan Đăng Lưu, Q.Phú Nhuận, TP.HCM', '1988-09-12', 40000, '001088089012', N'Nam', N'Bảo vệ', 'vanson', 'hashed_password_8', '/image/employees/security1.jpg', '2020-07-01', 0);
-- Kiểm tra kết quả


CREATE TABLE Customer (
    CustomerID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100),
    Phone VARCHAR(20) UNIQUE,
    Email NVARCHAR(100) UNIQUE,
    BirthDate DATE,
    Gender NVARCHAR(10),
    Username VARCHAR(50),
    PasswordHash VARCHAR(255),
    RegisterDate DATE DEFAULT GETDATE(),
    Point DECIMAL(18,2) DEFAULT 0 CHECK (Point >= 0),
    VipLevel INT DEFAULT 0 CHECK (VipLevel >= 0),
    IsDeleted BIT DEFAULT 0 NOT NULL
);

CREATE TABLE EmployeeChange (
    ChangeID VARCHAR(10) PRIMARY KEY,
    EmployeeID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Employee(EmployeeID) ON DELETE CASCADE,
    Phone VARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    BirthDate DATE,
    Username VARCHAR(50),
    PasswordHash VARCHAR(255),
    ImageUrl NVARCHAR(255),
    Status NVARCHAR(30),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ApprovedDate DATETIME
);
GO

CREATE TRIGGER trg_EmployeeChange_Insert
ON EmployeeChange
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO EmployeeChange(ChangeID, EmployeeID, Phone, Email, Address, BirthDate, Username, PasswordHash, ImageUrl, Status, CreatedDate, ApprovedDate)
    SELECT 
        'CHG-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        EmployeeID, Phone, Email, Address, BirthDate, Username, PasswordHash, ImageUrl, Status, CreatedDate, ApprovedDate
    FROM inserted;
END;
GO

-------------------
CREATE TABLE WorkShift (
    ShiftID VARCHAR(10) PRIMARY KEY,
    EmployeeID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Employee(EmployeeID) ON DELETE CASCADE,
    BranchID VARCHAR(10) NULL FOREIGN KEY REFERENCES CinemaBranch(BranchID) ON DELETE SET NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    WorkingHours FLOAT CHECK (WorkingHours >= 0),
    SalaryPerHour DECIMAL(18,2) CHECK (SalaryPerHour >= 0),
    Status NVARCHAR(30) -- nghỉ phép, Vắng, Hoàn thành, Đang làm, Sắp làm
);
GO


CREATE TRIGGER trg_WorkShift_Insert
ON WorkShift
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO WorkShift(ShiftID, EmployeeID, BranchID, StartTime, EndTime, WorkingHours, SalaryPerHour, Status)
    SELECT 
        'WS-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        EmployeeID, BranchID, StartTime, EndTime, WorkingHours, SalaryPerHour, Status
    FROM inserted;
END;
GO

CREATE OR ALTER TRIGGER trg_WorkShift_CalculateHours
ON WorkShift
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ws
    SET ws.WorkingHours = 
        DATEDIFF(MINUTE, i.StartTime, i.EndTime) / 60.0  -- đổi phút sang giờ (float)
    FROM WorkShift ws
    INNER JOIN inserted i ON ws.ShiftID = i.ShiftID;
END;
GO



/* ========== 2. MOVIE GROUP ========== */

CREATE TABLE Movie (
    MovieID VARCHAR(10) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    DurationMinutes INT CHECK (DurationMinutes > 0),
    Genre NVARCHAR(100),
    Language NVARCHAR(50),
    Sub NVARCHAR(50),
    Dub BIT,
    AgeLimit NVARCHAR(10),
    StartTime DATETIME,
    EndTime DATETIME,
    Description NVARCHAR(MAX),
    ImageUrl NVARCHAR(255),
    LinkTrailer NVARCHAR(200),
    IsDeleted BIT DEFAULT 0 NOT NULL
);
GO
CREATE OR ALTER PROCEDURE sp_GetMoviesPaged
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchKeyword NVARCHAR(200) = NULL,
    @IsCurrentlyShowing INT = NULL  -- NULL: all, 1: showing, 0: upcoming, -1: ended
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @TotalRecords INT;
    DECLARE @Now DATETIME = GETDATE();
    
    -- Đếm tổng số bản ghi
    SELECT @TotalRecords = COUNT(*)
    FROM Movie
    WHERE IsDeleted = 0
        AND (@SearchKeyword IS NULL OR Title LIKE N'%' + @SearchKeyword + '%')
        AND (
            @IsCurrentlyShowing IS NULL 
            OR (@IsCurrentlyShowing = 1 AND StartTime <= @Now AND (EndTime IS NULL OR EndTime >= @Now))
            OR (@IsCurrentlyShowing = 0 AND StartTime > @Now)
            OR (@IsCurrentlyShowing = -1 AND EndTime < @Now)
        );
    
    -- Lấy dữ liệu phân trang - ✅ THÊM Sub và Dub
    SELECT 
        MovieID,
        Title,
        DurationMinutes,
        Genre,
        Language,
        Sub,              -- ✅ THÊM CỘT NÀY
        Dub,              -- ✅ THÊM CỘT NÀY
        AgeLimit,
        StartTime,
        EndTime,
        Description,
        ImageUrl,
        LinkTrailer,
        IsDeleted,
        @TotalRecords AS TotalRecords,
        CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS TotalPages
    FROM Movie
    WHERE IsDeleted = 0
        AND (@SearchKeyword IS NULL OR Title LIKE N'%' + @SearchKeyword + '%')
        AND (
            @IsCurrentlyShowing IS NULL 
            OR (@IsCurrentlyShowing = 1 AND StartTime <= @Now AND (EndTime IS NULL OR EndTime >= @Now))
            OR (@IsCurrentlyShowing = 0 AND StartTime > @Now)
            OR (@IsCurrentlyShowing = -1 AND EndTime < @Now)
        )
    ORDER BY 
        CASE 
            WHEN @IsCurrentlyShowing = 0 THEN StartTime
        END ASC,
        CASE 
            WHEN @IsCurrentlyShowing IS NULL OR @IsCurrentlyShowing IN (1, -1) THEN StartTime
        END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

DROP PROC sp_GetMoviesPaged
EXEC sp_GetMoviesPaged @PageNumber = 1, @PageSize = 10, @SearchKeyword = NULL, @IsCurrentlyShowing = NULL;

CREATE TRIGGER trg_Movie_Insert
ON Movie
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Movie(MovieID, Title, DurationMinutes, Genre, Language, Sub, Dub, AgeLimit, StartTime, EndTime, Description, ImageUrl, LinkTrailer, IsDeleted)
    SELECT 
        'MOV-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,5)),
        Title, DurationMinutes, Genre, Language, Sub, Dub, AgeLimit, StartTime, EndTime, Description, ImageUrl, LinkTrailer, IsDeleted
    FROM inserted;
END;
GO


CREATE TABLE Room (
    RoomID VARCHAR(10) PRIMARY KEY,
    RoomName NVARCHAR(100) NOT NULL,
    SeatCount INT CHECK (SeatCount >= 0),
    Description NVARCHAR(MAX),
    RoomType NVARCHAR(50),
    ImageUrl NVARCHAR(255),
    BranchID VARCHAR(10) NULL FOREIGN KEY REFERENCES CinemaBranch(BranchID) ON DELETE SET NULL,
    IsDeleted BIT DEFAULT 0 NOT NULL
);
GO

CREATE TRIGGER trg_Room_Insert
ON Room
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Room(RoomID, RoomName, SeatCount, Description, RoomType, ImageUrl, BranchID, IsDeleted)
    SELECT 
        'ROM-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,5)),
        RoomName, SeatCount, Description, RoomType, ImageUrl, BranchID, IsDeleted
    FROM inserted;
END;
GO

CREATE TABLE Seat (
    SeatID VARCHAR(10) PRIMARY KEY,
    SeatName NVARCHAR(50) NOT NULL,
    SeatType NVARCHAR(50),
    RoomID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Room(RoomID) ON DELETE CASCADE,
    IsDeleted BIT DEFAULT 0 NOT NULL,
    CONSTRAINT UQ_Seat UNIQUE (SeatName, RoomID)
);
GO

CREATE OR ALTER TRIGGER trg_Seat_Insert
ON Seat
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Seat(SeatID, SeatName, SeatType, RoomID, IsDeleted)
    SELECT 
        'ST-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,7)),
        SeatName, SeatType, RoomID, IsDeleted
    FROM inserted;
END;
GO

CREATE TABLE ShowTime (
    ShowTimeID VARCHAR(10) PRIMARY KEY,
    StartTime DATETIME NOT NULL,
    Price DECIMAL(18,2) CHECK (Price > 0),
    MovieID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Movie(MovieID) ON DELETE CASCADE,
    RoomID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Room(RoomID) ON DELETE CASCADE,
    IsDeleted BIT DEFAULT 0 NOT NULL
);
GO

DROP TRIGGER trg_ShowTime_Insert

CREATE OR ALTER PROCEDURE sp_GetShowTimesPaged
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @BranchID VARCHAR(10) = NULL,
    @MovieID VARCHAR(10) = NULL,
    @RoomID VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @TotalRecords INT;
    
    -- Đếm tổng số record
    SELECT @TotalRecords = COUNT(*)
    FROM ShowTime st
    INNER JOIN Room r ON st.RoomID = r.RoomID
    WHERE st.IsDeleted = 0
        AND (@BranchID IS NULL OR r.BranchID = @BranchID)
        AND (@MovieID IS NULL OR st.MovieID = @MovieID)
        AND (@RoomID IS NULL OR st.RoomID = @RoomID);
    
    SELECT 
        st.ShowTimeID,                             
        st.StartTime,                              
        ISNULL(st.Price, 0) AS Price,            
        st.MovieID,                                
        m.Title AS MovieTitle,
        ISNULL(m.DurationMinutes, 0) AS DurationMinutes,
        st.RoomID,                                  
        r.RoomName,                                 
        r.BranchID,                                
        cb.BranchName,                              
        st.IsDeleted,                              
        @TotalRecords AS TotalRecords,              
        CAST(CEILING(CAST(@TotalRecords AS FLOAT) / @PageSize) AS INT) AS TotalPages  
    FROM ShowTime st
    INNER JOIN Movie m ON st.MovieID = m.MovieID
    INNER JOIN Room r ON st.RoomID = r.RoomID
    INNER JOIN CinemaBranch cb ON r.BranchID = cb.BranchID
    WHERE st.IsDeleted = 0
        AND (@BranchID IS NULL OR r.BranchID = @BranchID)
        AND (@MovieID IS NULL OR st.MovieID = @MovieID)
        AND (@RoomID IS NULL OR st.RoomID = @RoomID)
    ORDER BY st.StartTime DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO


DROP PROC sp_GetShowTimesPaged

CREATE TABLE Ticket (
    TicketID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ShowTimeID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES ShowTime(ShowTimeID) ON DELETE CASCADE,
    SeatID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Seat(SeatID),
    TicketType NVARCHAR(50),
    Price DECIMAL(18,2) CHECK (Price >= 0),
    Status NVARCHAR(20),
	LockedBy UNIQUEIDENTIFIER NULL,  -- CustomerID
    LockedAt DATETIME NULL,
    IsDeleted BIT DEFAULT 0 NOT NULL,
    CONSTRAINT UQ_Ticket UNIQUE (ShowTimeID, SeatID)
);


/* ========== 3. PRODUCT GROUP ========== */

CREATE TABLE Product (
    ProductID VARCHAR(10) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    ProductType NVARCHAR(50),
    Price DECIMAL(18,2) CHECK (Price >= 0),
    ImageUrl NVARCHAR(255),
    IsDeleted BIT DEFAULT 0 NOT NULL
);
GO

CREATE TRIGGER trg_Product_Insert
ON Product
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Product(ProductID, ProductName, ProductType, Price, ImageUrl, IsDeleted)
    SELECT 
        'PRD-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        ProductName, ProductType, Price, ImageUrl, IsDeleted
    FROM inserted;
END;
GO

CREATE TABLE MovieProduct (
    MovieProductID VARCHAR(10) PRIMARY KEY,
    MovieID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Movie(MovieID) ON DELETE CASCADE,
    ProductID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Product(ProductID) ON DELETE CASCADE,
    OfferType NVARCHAR(20),
    Quantity INT CHECK (Quantity >= 0),
    Note NVARCHAR(255),
    CONSTRAINT UQ_MovieProduct UNIQUE (MovieID, ProductID)
);
GO

CREATE TRIGGER trg_MovieProduct_Insert
ON MovieProduct
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO MovieProduct(MovieProductID, MovieID, ProductID, OfferType, Quantity, Note)
    SELECT 
        'MP-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        MovieID, ProductID, OfferType, Quantity, Note
    FROM inserted;
END;
GO


/* ========== 4. INVOICE & PAYMENT GROUP ========== */

CREATE TABLE Invoice (
    InvoiceID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EmployeeID UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Employee(EmployeeID) ON DELETE SET NULL,
    CustomerID UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Customer(CustomerID) ON DELETE SET NULL,
    BranchID VARCHAR(10) NULL FOREIGN KEY REFERENCES CinemaBranch(BranchID) ON DELETE SET NULL,
    IssueDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) CHECK (TotalAmount >= 0),
    Discount DECIMAL(18,2) DEFAULT 0 CHECK (Discount >= 0),
    Status NVARCHAR(30),
    IsDeleted BIT DEFAULT 0 NOT NULL
);

CREATE TABLE InvoiceTicket (
    InvoiceTicketID VARCHAR(10) PRIMARY KEY,
    InvoiceID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Invoice(InvoiceID) ON DELETE CASCADE,
    TicketID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Ticket(TicketID) ON DELETE CASCADE,
    Quantity INT DEFAULT 1 CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) CHECK (UnitPrice >= 0),
    CONSTRAINT UQ_Invoice_Ticket UNIQUE (InvoiceID, TicketID)
);
GO



CREATE TRIGGER trg_InvoiceTicket_Insert
ON InvoiceTicket
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO InvoiceTicket(InvoiceTicketID, InvoiceID, TicketID, Quantity, UnitPrice)
    SELECT 
        'IT-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        InvoiceID, TicketID, Quantity, UnitPrice
    FROM inserted;
END;
GO

CREATE TABLE InvoiceProduct (
    InvoiceProductID VARCHAR(10) PRIMARY KEY,
    InvoiceID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Invoice(InvoiceID) ON DELETE CASCADE,
    ProductID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Product(ProductID) ON DELETE CASCADE,
    Quantity INT DEFAULT 1 CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) CHECK (UnitPrice >= 0),
    CONSTRAINT UQ_Invoice_Product UNIQUE (InvoiceID, ProductID)
);
GO

CREATE TRIGGER trg_InvoiceProduct_Insert
ON InvoiceProduct
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO InvoiceProduct(InvoiceProductID, InvoiceID, ProductID, Quantity, UnitPrice)
    SELECT 
        'IP-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
        InvoiceID, ProductID, Quantity, UnitPrice
    FROM inserted;
END;
GO

CREATE TABLE Payment (
    PaymentID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    InvoiceID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Invoice(InvoiceID) ON DELETE CASCADE,
    Method NVARCHAR(50),
    Amount DECIMAL(18,2) CHECK (Amount >= 0),
    PaymentTime DATETIME DEFAULT GETDATE()
);

-- TABLE LANGUAGE
CREATE TABLE Language (
    LanguageCode VARCHAR(5) PRIMARY KEY, -- 'vi', 'en'
    LanguageName NVARCHAR(50) NOT NULL
);
INSERT INTO Language VALUES ('vi', N'Tiếng Việt'), ('en', N'English');

CREATE TABLE TextTranslation (
    TextKey VARCHAR(100) NOT NULL,        -- key duy nhất (ví dụ: ROLE_EMPLOYEE, STATUS_PAID)
    LanguageCode VARCHAR(5) NOT NULL,     -- 'vi', 'en'
    DisplayText NVARCHAR(255) NOT NULL,   -- chuỗi hiển thị
    PRIMARY KEY (TextKey, LanguageCode),
    FOREIGN KEY (LanguageCode) REFERENCES Language(LanguageCode)
);

-- Gender
INSERT INTO TextTranslation VALUES 
('MALE','vi',N'Nam'), ('MALE','en','Male'),
('FEMALE','vi',N'Nữ'), ('FEMALE','en','Female'),
('OTHER','vi',N'Khác'), ('OTHER','en','Other');

-- Role
INSERT INTO TextTranslation VALUES 
('EMPLOYEE','vi',N'Nhân viên'), ('EMPLOYEE','en','Employee'),
('ADMIN','vi',N'Quản trị'), ('ADMIN','en','Admin');

-- EmployeeChange Status
INSERT INTO TextTranslation VALUES
('PENDING','vi',N'Đang chờ'), ('PENDING','en','Pending'),
('APPROVED','vi',N'Đã duyệt'), ('APPROVED','en','Approved'),
('REJECTED','vi',N'Đã từ chối'), ('REJECTED','en','Rejected');

-- WorkShift Status
INSERT INTO TextTranslation VALUES
('SHIFT_PENDING','vi',N'Chờ duyệt ca'), ('SHIFT_PENDING','en','Pending approval'),
('SHIFT_APPROVED','vi',N'Đã duyệt ca'), ('SHIFT_APPROVED','en','Approved'),
('ABSENT','vi',N'Vắng'), ('ABSENT','en','Absent'),
('SHIFT_CANCELED','vi',N'Đã hủy'), ('SHIFT_CANCELED','en','Canceled'),
('SHIFT_REJECTED','vi',N'Không duyệt'), ('SHIFT_REJECTED','en','Rejected');

-- Movie AgeLimit
INSERT INTO TextTranslation VALUES
('AGE_P','vi',N'P (mọi lứa tuổi)'), ('AGE_P','en','P (all ages)'),
('AGE_13','vi',N'13+'), ('AGE_13','en','13+'),
('AGE_16','vi',N'16+'), ('AGE_16','en','16+'),
('AGE_18','vi',N'18+'), ('AGE_18','en','18+');

-- Movie Genre (bạn có thể mở rộng thêm nếu cần)
INSERT INTO TextTranslation VALUES
('GENRE_ACTION','vi',N'Hành động'), ('GENRE_ACTION','en','Action'),
('GENRE_COMEDY','vi',N'Hài'), ('GENRE_COMEDY','en','Comedy'),
('GENRE_DRAMA','vi',N'Tâm lý'), ('GENRE_DRAMA','en','Drama'),
('GENRE_HORROR','vi',N'Kinh dị'), ('GENRE_HORROR','en','Horror'),
('GENRE_ROMANCE','vi',N'Tình cảm'), ('GENRE_ROMANCE','en','Romance'),
('GENRE_SCIFI','vi',N'Khoa học viễn tưởng'), ('GENRE_SCIFI','en','Sci-Fi'),
('GENRE_ANIMATION','vi',N'Hoạt hình'), ('GENRE_ANIMATION','en','Animation'),
('GENRE_DOCU','vi',N'Tài liệu'), ('GENRE_DOCU','en','Documentary');

-- Movie Language (ví dụ)
INSERT INTO TextTranslation VALUES
('LANG_VI','vi',N'Tiếng Việt'), ('LANG_VI','en','Vietnamese'),
('LANG_EN','vi',N'Tiếng Anh'), ('LANG_EN','en','English'),
('LANG_JP','vi',N'Tiếng Nhật'), ('LANG_JP','en','Japanese'),
('LANG_KR','vi',N'Tiếng Hàn'), ('LANG_KR','en','Korean');

-- RoomType
INSERT INTO TextTranslation VALUES
('ROOM_2D','vi',N'2D'), ('ROOM_2D','en','2D'),
('ROOM_3D','vi',N'3D'), ('ROOM_3D','en','3D'),
('ROOM_IMAX','vi',N'IMAX'), ('ROOM_IMAX','en','IMAX'),
('ROOM_4DX','vi',N'4DX'), ('ROOM_4DX','en','4DX');

-- SeatType
INSERT INTO TextTranslation VALUES
('SEAT_NORMAL','vi',N'Thường'), ('SEAT_NORMAL','en','Normal'),
('SEAT_VIP','vi',N'VIP'), ('SEAT_VIP','en','VIP'),
('SEAT_COUPLE','vi',N'Đôi'), ('SEAT_COUPLE','en','Couple');

-- TicketType
INSERT INTO TextTranslation VALUES
('TICKET_STANDARD','vi',N'Tiêu chuẩn'), ('TICKET_STANDARD','en','Standard'),
('TICKET_STUDENT','vi',N'Sinh viên'), ('TICKET_STUDENT','en','Student'),
('TICKET_CHILD','vi',N'Trẻ em'), ('TICKET_CHILD','en','Child'),
('TICKET_COMBO','vi',N'Combo'), ('TICKET_COMBO','en','Combo');

-- Ticket Status
INSERT INTO TextTranslation VALUES
('AVAILABLE','vi',N'Còn trống'), ('AVAILABLE','en','Available'),
('SOLD','vi',N'Đã bán'), ('SOLD','en','Sold');

-- ProductType
INSERT INTO TextTranslation VALUES
('FOOD','vi',N'Đồ ăn'), ('FOOD','en','Food'),
('DRINK','vi',N'Đồ uống'), ('DRINK','en','Drink'),
('SOUVENIR','vi',N'Quà lưu niệm'), ('SOUVENIR','en','Souvenir'),
('COMBO','vi',N'Combo'), ('COMBO','en','Combo');

-- MovieProduct OfferType
INSERT INTO TextTranslation VALUES
('FREE','vi',N'Miễn phí'), ('FREE','en','Free'),
('SEPARATE','vi',N'Riêng biệt'), ('SEPARATE','en','Separate');

-- Invoice Status
INSERT INTO TextTranslation VALUES
('INVOICE_PENDING','vi',N'Đang chờ xử lí'), ('INVOICE_PENDING','en','Pending'),
('PAID','vi',N'Đã thanh toán'), ('PAID','en','Paid'),
('UNPAID','vi',N'Chưa thanh toán'), ('UNPAID','en','Unpaid');

-- Payment Method
INSERT INTO TextTranslation VALUES
('CASH','vi',N'Tiền mặt'), ('CASH','en','Cash'),
('BANK','vi',N'Chuyển khoản'), ('BANK','en','Bank Transfer');






DROP TABLE [dbo].[InvoiceTicket]
DROP TABLE [dbo].[InvoiceProduct]
DROP TABLE [dbo].[Payment]
DROP TABLE [dbo].[Invoice]
DROP TABLE [dbo].[Ticket]
DROP TABLE [dbo].[Seat]
DROP TABLE [dbo].[ShowTime]
DROP TABLE [dbo].[Room]
DROP TABLE [dbo].[MovieProduct]
DROP TABLE [dbo].[Product]
DROP TABLE [dbo].[Movie]
DROP TABLE [dbo].[Customer]
DROP TABLE [dbo].[EmployeeChange]
DROP TABLE [dbo].[WorkShift]
DROP TABLE [dbo].[Employee]
DROP TABLE [dbo].[CinemaBranch]
 

 -- Trigger tự động tạo ticket cho TẤT CẢ ghế trong 
CREATE OR ALTER TRIGGER trg_AutoCreateTickets
ON ShowTime
AFTER INSERT
AS
BEGIN
    INSERT INTO Ticket (ShowTimeID, SeatID, TicketType, Price, Status)
    SELECT 
        i.ShowTimeID,
        s.SeatID,
        s.SeatType,  -- 'Standard', 'VIP', 'Couple'
		CASE 
            WHEN s.SeatType = N'Ghế VIP' THEN i.Price + 20000
            WHEN s.SeatType = N'Ghế Couple' THEN 2*i.Price + 20000
            ELSE i.Price
        END,
        N'Trống'  -- Trạng thái ban đầu
    FROM inserted i
    JOIN Seat s ON s.RoomID = i.RoomID
    WHERE s.IsDeleted = 0;
END;
GO

UPATE 

CREATE OR ALTER TRIGGER trg_UpdateTicketPrice
ON ShowTime
AFTER UPDATE
AS
BEGIN
    -- Chỉ update khi giá hoặc phòng chiếu thay đổi
    IF UPDATE(Price)
    BEGIN
        UPDATE t
        SET Price =
            CASE 
                WHEN s.SeatType = N'Ghế VIP' THEN i.Price + 20000
                WHEN s.SeatType = N'Ghế đôi' THEN 2*i.Price + 20000
                ELSE i.Price
            END
        FROM Ticket t
        JOIN inserted i ON t.ShowTimeID = i.ShowTimeID
        JOIN Seat s ON s.SeatID = t.SeatID;
    END
END;
GO






-- Employee
-- VanKien VanKien@123
-- Vandong Vandong@123


