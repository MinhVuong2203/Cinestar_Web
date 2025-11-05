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


    // Tìm kiếm nhân viên
    $('#searchEmployee').on('keyup', function () {
        const searchText = $(this).val().toLowerCase();
        let visibleCount = 0;

        $('.employee-row').each(function () {
            const employeeName = $(this).data('employee-name');
            if (employeeName.includes(searchText)) {
                $(this).show();
                visibleCount++;
            } else {
                $(this).hide();
            }
        });

        $('#employeeCount').text(visibleCount);
    });

    // Xuất file excel
    $('#exportExcel').on('click', function () {
        const branchId = $('select[name="branchId"]').val();
        const role = $('#roleFilter').val();
        const fromDate = $('input[name="fromDate"]').val();
        const toDate = $('input[name="toDate"]').val();

        if (!branchId) {
            alert('Vui lòng chọn chi nhánh!');
            return;
        }

        // Tạo URL với query parameters
        let url = `/Admin/WorkShift/ExportExcel?branchId=${branchId}&fromDate=${fromDate}&toDate=${toDate}`;
        if (role) {
            url += `&role=${encodeURIComponent(role)}`;
        }
        // Mở link download
        window.location.href = url;
    });

});
