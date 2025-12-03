const bookingStatusMapping = {
	0: `<div class="badge badge-warning">Chờ xác nhận</div>`, // Pending
	1: `<div class="badge badge-primary">Đã xác nhận</div>`, // Confirmed
	2: `<div class="badge badge-success">Đã check-in</div>`, // CheckIn
	3: `<div class="badge badge-info">Đang diễn ra</div>`, // InProgress
	4: `<div class="badge badge-success">Đã hoàn thành</div>`, // Completed
	5: `<div class="badge badge-danger">Đã hủy</div>`, // Canceled
	6: `<div class="badge badge-danger">Từ chối</div>`, // Rejected
};

const paymentStatusMapping = {
	0: `<div class="badge badge-warning">Chưa thanh toán</div>`, // Unpaid
	1: `<div class="badge badge-success">Đã thanh toán</div>`, // Paid
	2: `<div class="badge badge-danger">Thanh toán thất bại</div>`, // Failed
	3: `<div class="badge badge-info">Đã hoàn tiền</div>`, // Refunded
};

const deliveryStatusMapping = {
	0: `<div class="badge badge-warning">Đang chờ</div>`,
	1: `<div class="badge badge-warning">Đang chuẩn bị</div>`,
	2: `<div class="badge badge-success">Đã xác nhận</div>`,
	3: `<div class="badge badge-primary">Đã vận chuyển</div>`,
	4: `<div class="badge badge-danger">Đã hủy</div>`,
};

const orderStatusMapping = {
	0: `<div class="badge badge-warning">Chờ xác nhận</div>`, // Pending
	1: `<div class="badge badge-primary">Đã xác nhận</div>`, // Confirmed
	2: `<div class="badge badge-success">Đã hoàn thành</div>`, // Completed
	3: `<div class="badge badge-danger">Đã hủy</div>`, // Cancelled
};

const requestStatusMapping = {
	1: `<div class="badge badge-warning">Đang chờ duyệt</div>`,
	2: `<div class="badge badge-danger">Đã từ chối</div>`,
	3: `<div class="badge badge-success">Đã xác nhận</div>`,
	4: `<div class="badge badge-danger">Đã hủy</div>`,
	5: `<div class="badge badge-success">Đã hoàn thành</div>`,
};

const appointmentStatusMapping = {
	1: `<div class="badge badge-primary">Đã gửi</div>`,
	2: `<div class="badge badge-success">Đã xác nhận</div>`,
	3: `<div class="badge badge-danger">Đã hủy</div>`,
};
