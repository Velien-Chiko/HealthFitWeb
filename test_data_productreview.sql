-- Script thêm dữ liệu test cho bảng ProductReview
-- Copy và paste vào SQL Server Management Studio

-- Kiểm tra dữ liệu hiện có
SELECT TOP (1000) [ReviewID]
      ,[UserID]
      ,[ProductID]
      ,[Rating]
      ,[Comment]
      ,[CreatedDate]
      ,[Response]
      ,[SellerReply]
      ,[ReplyAt]
FROM [HealthyShop].[dbo].[ProductReview]

-- Thêm dữ liệu test cho ProductReview
-- Lưu ý: Thay đổi UserID và ProductID theo dữ liệu thực tế trong database của bạn

INSERT INTO [HealthyShop].[dbo].[ProductReview] 
([UserID], [ProductID], [Rating], [Comment], [CreatedDate], [Response], [SellerReply], [ReplyAt])
VALUES
-- Đánh giá 1: Chưa phản hồi
(2, 1, 5, N'Sản phẩm rất tốt, chất lượng cao, giao hàng nhanh. Rất hài lòng với dịch vụ!', '2024-01-15 10:30:00', NULL, NULL, NULL),

-- Đánh giá 2: Đã phản hồi
(3, 1, 4, N'Sản phẩm ngon, giá cả hợp lý. Nhưng có thể cải thiện thêm về bao bì.', '2024-01-16 14:20:00', NULL, N'Cảm ơn bạn đã đánh giá! Chúng tôi sẽ cải thiện bao bì trong thời gian tới.', '2024-01-17 09:15:00'),

-- Đánh giá 3: Chưa phản hồi
(4, 2, 3, N'Sản phẩm tạm được, nhưng hơi đắt so với chất lượng.', '2024-01-18 16:45:00', NULL, NULL, NULL),

-- Đánh giá 4: Đã phản hồi
(5, 2, 5, N'Tuyệt vời! Sản phẩm đúng như mô tả, rất hài lòng.', '2024-01-19 11:10:00', NULL, N'Cảm ơn bạn đã tin tưởng chúng tôi! Chúc bạn sức khỏe tốt.', '2024-01-20 08:30:00'),

-- Đánh giá 5: Chưa phản hồi
(6, 3, 4, N'Chất lượng tốt, nhưng cần thêm nhiều lựa chọn hơn.', '2024-01-21 13:25:00', NULL, NULL, NULL),

-- Đánh giá 6: Đã phản hồi
(7, 3, 5, N'Sản phẩm rất bổ dưỡng, phù hợp cho người ăn kiêng.', '2024-01-22 15:40:00', NULL, N'Cảm ơn bạn! Chúng tôi luôn cố gắng mang đến sản phẩm chất lượng nhất.', '2024-01-23 10:20:00'),

-- Đánh giá 7: Chưa phản hồi
(8, 4, 2, N'Không như mong đợi, hơi thất vọng về chất lượng.', '2024-01-24 09:15:00', NULL, NULL, NULL),

-- Đánh giá 8: Đã phản hồi
(9, 4, 4, N'Giao hàng đúng hẹn, sản phẩm tươi ngon.', '2024-01-25 17:30:00', NULL, N'Cảm ơn bạn đã đánh giá tích cực! Chúng tôi sẽ tiếp tục cải thiện.', '2024-01-26 14:45:00'),

-- Đánh giá 9: Chưa phản hồi
(10, 5, 5, N'Rất hài lòng! Sẽ mua lại nhiều lần.', '2024-01-27 12:00:00', NULL, NULL, NULL),

-- Đánh giá 10: Đã phản hồi
(11, 5, 3, N'Sản phẩm ổn, nhưng giá hơi cao.', '2024-01-28 10:45:00', NULL, N'Cảm ơn phản hồi của bạn! Chúng tôi sẽ xem xét điều chỉnh giá.', '2024-01-29 16:20:00');

-- Kiểm tra dữ liệu sau khi thêm
SELECT TOP (1000) [ReviewID]
      ,[UserID]
      ,[ProductID]
      ,[Rating]
      ,[Comment]
      ,[CreatedDate]
      ,[Response]
      ,[SellerReply]
      ,[ReplyAt]
FROM [HealthyShop].[dbo].[ProductReview]
ORDER BY [ReviewID] DESC;

-- Script để xóa dữ liệu test (nếu cần)
-- DELETE FROM [HealthyShop].[dbo].[ProductReview] WHERE [ReviewID] > [số_id_cuối_cùng_trước_khi_test]; 