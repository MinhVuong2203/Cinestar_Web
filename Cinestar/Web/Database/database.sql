CREATE DATABASE Cinestar;
USE Cinestar;

/* ========== 1. EMPLOYEE GROUP ========== */

CREATE TABLE CinemaBranch (
    BranchID VARCHAR(10) PRIMARY KEY,
    BranchName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(255),
    City NVARCHAR(100),
    District NVARCHAR(100),
    Phone VARCHAR(20),
    Email NVARCHAR(100),
    OpenHours NVARCHAR(100),
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
    INSERT INTO CinemaBranch (BranchID, BranchName, Address, City, District, Phone, Email, OpenHours, MapUrl, ImageUrl, Description, IsDeleted)
    SELECT
        'BRH-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,5)),  -- BRH-XXXXX
        BranchName, Address, City, District, Phone, Email, OpenHours, MapUrl, ImageUrl, Description, IsDeleted
    FROM inserted;
END;
GO

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

CREATE TABLE WorkShift (
    ShiftID VARCHAR(10) PRIMARY KEY,
    EmployeeID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Employee(EmployeeID) ON DELETE CASCADE,
    BranchID VARCHAR(10) NULL FOREIGN KEY REFERENCES CinemaBranch(BranchID) ON DELETE SET NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    WorkingHours FLOAT CHECK (WorkingHours >= 0),
    SalaryPerHour DECIMAL(18,2) CHECK (SalaryPerHour >= 0),
    Status NVARCHAR(30)
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
    RoomName NVARCHAR(100) NOT NULL UNIQUE,
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

CREATE TRIGGER trg_Seat_Insert
ON Seat
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Seat(SeatID, SeatName, SeatType, RoomID, IsDeleted)
    SELECT 
        'ST-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,6)),
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

CREATE TRIGGER trg_ShowTime_Insert
ON ShowTime
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO ShowTime(ShowTimeID, StartTime, Price, MovieID, RoomID, IsDeleted)
    SELECT 
        'STIME-' + UPPER(SUBSTRING(CONVERT(VARCHAR(40), NEWID()),1,5)),
        StartTime, Price, MovieID, RoomID, IsDeleted
    FROM inserted;
END;
GO

CREATE TABLE Ticket (
    TicketID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ShowTimeID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES ShowTime(ShowTimeID),
    SeatID VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Seat(SeatID),
    TicketType NVARCHAR(50),
    Price DECIMAL(18,2) CHECK (Price >= 0),
    Status NVARCHAR(20),
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
 
 INSERT INTO Employee 
(FullName, Phone, Email, Address, BirthDate, HourWage, CCCD, Gender, Role, Username, PasswordHash, ImageUrl, RegisterDate, IsDeleted)
VALUES
-- Quản lý
(N'Nguyễn Văn A', '0912345678', 'nguyenvana@company.com', N'123 Trần Hưng Đạo, Hà Nội', '1985-05-20', 30000, '012345678901', N'Nam', 'Admin', 'adminA', '123456', N'/images/adminA.jpg', GETDATE(), 0),

(N'Trần Thị B', '0923456789', 'tranthib@company.com', N'45 Lê Lợi, TP.HCM', '1990-08-15', 28000, '012345678902', N'Nữ', 'Admin', 'adminB', '123456', N'/images/adminB.jpg', GETDATE(), 0),

-- Nhân viên
(N'Lê Văn C', '0934567890', 'levanc@company.com', N'78 Hai Bà Trưng, Hà Nội', '1995-03-10', 20000, '012345678903', N'Nam', 'Employee', 'staffC', '123456', N'/images/staffC.jpg', GETDATE(), 0),

(N'Phạm Thị D', '0945678901', 'phamthid@company.com', N'56 Nguyễn Huệ, TP.HCM', '1998-11-25', 20000, '012345678904', N'Nữ', 'Employee', 'staffD', '123456', N'/images/staffD.jpg', GETDATE(), 0),

(N'Hoàng Văn E', '0956789012', 'hoange@company.com', N'12 Võ Thị Sáu, Đà Nẵng', '1997-07-07', 20000, '012345678905', N'Nam', 'Employee', 'staffE', '123456', N'/images/staffE.jpg', GETDATE(), 0),

(N'Ngô Thị F', '0967890123', 'ngothif@company.com', N'90 Lý Thường Kiệt, Huế', '2000-01-12', 20000, '012345678906', N'Nữ', 'Employee', 'staffF', '123456', N'/images/staffF.jpg', GETDATE(), 0);







