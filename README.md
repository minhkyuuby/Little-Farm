# Game Developer Technical Test

Repository này chứa bài làm của tôi cho bài test vị trí **Game Developer**.

---

Android Download: https://drive.google.com/drive/folders/1ZK-rDuNDNBjX0TdJBcW5zvzsgDCSJm34?usp=drive_link

# Thời gian phát triển

Do sắp xếp thời gian cá nhân, tôi chỉ thực sự bắt đầu triển khai project từ **sáng Chủ Nhật**.  
Vì vậy phiên bản hiện tại của project tương đương khoảng **2 ngày làm việc**.

Trong khoảng thời gian này, tôi ưu tiên hoàn thành:

- **Core gameplay loop**
- **Kiến trúc hệ thống**
- **Khả năng mở rộng của gameplay systems**

thay vì tập trung vào polish hoặc tối ưu sâu.

---

# Các tính năng đã triển khai

## Core Gameplay Loop

Core gameplay loop của game đã được triển khai, bao gồm sự tương tác giữa:

- **Customer**
- **Delivery**
- **Plant / Fruit production**

---

## Hành vi Customer và Delivery

Hành vi của **Customer** và **Delivery** được xây dựng bằng **Behavior Graph**, giúp:

- Dễ mở rộng logic hành vi
- Dễ chỉnh sửa và cấu hình
- Tách biệt logic AI khỏi gameplay code

Một số hành vi chính:

- Customer tạo yêu cầu sản phẩm
- Delivery nhận nhiệm vụ lấy fruit
- Delivery giao fruit cho customer

---

## Hệ thống Plant và Fruit

Hệ thống **plant growth và fruit production** đã được triển khai.

Đặc điểm:

- Mỗi plant có thể tạo ra **nhiều loại fruit khác nhau**
- Fruit có thể được thu hoạch để phục vụ hệ thống delivery
- Các cấu hình plant và fruit được quản lý bằng **ScriptableObject**

Cách tiếp cận này giúp:

- Dễ thêm plant mới
- Dễ thêm fruit mới
- Không cần chỉnh sửa gameplay code

---

## Hệ thống Economy

Một hệ thống **economy cơ bản** đã được triển khai.

Hiện tại bao gồm:

- Currency: **Coin**
- Flow cơ bản: gameplay → reward → economy

Hệ thống được thiết kế để **dễ mở rộng thêm nhiều loại currency khác** trong tương lai.

---

# Tối ưu cơ bản

## Object Pooling

Một số đối tượng được spawn thường xuyên đã được **pooling** để giảm allocation và tăng hiệu năng:

- Customer
- Delivery
- Fruit

Điều này giúp giảm chi phí tạo/destroy object liên tục trong gameplay.

---

# Điểm nổi bật về kiến trúc

## EventBus System

Project sử dụng một **EventBus System** để quản lý giao tiếp giữa các hệ thống.

Đặc điểm:

- Sử dụng **struct subjects**
- Hỗ trợ communication giữa:
  - Gameplay systems
  - UI
  - State updates
- Giảm coupling giữa các component

Cách tiếp cận này có thể xem như một dạng triển khai của **Observer Pattern**.

Ví dụ các sự kiện sử dụng EventBus:

- Gameplay state update
- UI update
- Fruit harvested
- Delivery completed

---

# Known Issues / Bugs

## Customer UI Image

UI của **Delivery Capacity View** hiện tại chưa cập nhật đúng hình ảnh thumbnail.

Hiện vẫn đang sử dụng **placeholder image (gem icon)**.

---

## Delivery bị kẹt khi di chuyển

Trong một số trường hợp hiếm, **delivery agent có thể bị kẹt khi di chuyển**.

Nguyên nhân dự đoán:

- Cấu hình **NavMeshAgent** chưa tối ưu
- **Built-in Navigation Action** của Behavior Graph sử dụng NavMeshAgent có thể chưa xử lý tốt một số edge cases

Cần thêm logic xử lý path recovery hoặc điều chỉnh cấu hình navigation.

---

# Các cải tiến có thể thực hiện thêm

Nếu có thêm thời gian phát triển, tôi sẽ triển khai thêm:

- Cải thiện độ ổn định của navigation system
- Mở rộng hệ thống economy (multiple currencies, upgrades)
- Cải thiện UI feedback
- Gameplay balancing
- Save / Load system
- Debug tools cho EventBus

---

# Unity Version

Project được phát triển bằng: 6000.3.10f1
