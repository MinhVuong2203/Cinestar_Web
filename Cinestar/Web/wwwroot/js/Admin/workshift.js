// WorkShift Management JavaScript

$(document).ready(function () {

    // Xếp ca
    $('.assign-btn').on('click', function () {
        const empId = $(this).data('emp-id');
        const date = $(this).data('date');
        const slot = $(this).data('slot');
        const branchId = $('select[name="branchId"]').val();
        if (!branchId) return; 
        $.post('/Admin/WorkShift/AssignShift', { employeeId: empId, branchId, date, slot }, function (res) {
            if (res.success) {
                location.reload();
            } else {
                alert(res.message);
            }
        });
    });

    // Xóa ca
    $('.remove-btn').on('click', function () {
        const id = $(this).data('id');
        if (!confirm('Xóa ca này?')) return;
        $.post('/Admin/WorkShift/RemoveShift', { shiftId: id }, function (res) {
            if (res.success) location.reload();
            else alert('Không xóa được!');
        });
    });

    $('.absent').on('click', function () {
        const id = $(this).data('id');
    });

    // Cập nhật trạng thái
    $('.btn-status-absent, .btn-status-leave, .btn-status-complete').on('click', function () {
        const id = $(this).data('id');
        const st = $(this).data('status');
        $.post('/Admin/WorkShift/UpdateStatusShift', { shiftId: id, status: st }, function (res) {
            if (res.success) location.reload();
            else alert('Thao tác xảy ra lỗi!');
        });
    });



});
