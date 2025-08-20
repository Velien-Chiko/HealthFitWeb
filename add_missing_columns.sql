-- Script để thêm các cột còn thiếu vào bảng Orders
-- Chạy từng lệnh một hoặc chạy tất cả cùng lúc

-- 1. Thêm các cột thông tin thanh toán (Payment)
ALTER TABLE Orders ADD PaymentStatus nvarchar(max) NULL;
ALTER TABLE Orders ADD PaymentDate datetime2 NOT NULL DEFAULT GETDATE();
ALTER TABLE Orders ADD PaymentDueDate date NOT NULL DEFAULT GETDATE();
ALTER TABLE Orders ADD SessionId nvarchar(max) NULL;
ALTER TABLE Orders ADD PaymentIntentId nvarchar(max) NULL;

-- 2. Thêm các cột thông tin giao hàng (Shipping)
ALTER TABLE Orders ADD PhoneNumber nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE Orders ADD Address nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE Orders ADD City nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE Orders ADD Country nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE Orders ADD FullName nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE Orders ADD Email nvarchar(max) NOT NULL DEFAULT '';

-- 3. Kiểm tra xem các cột đã được thêm thành công chưa
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' 
ORDER BY ORDINAL_POSITION; 