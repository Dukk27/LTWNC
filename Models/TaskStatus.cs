namespace WNC_G13.Models
{
    public enum TaskStatus
    {
        NotReady = 0,    // Chưa sẵn sàng
        ToDo = 1,        // Cần làm
        InProgress = 2,  // Đang tiến hành
        Completed = 3,   // Đã hoàn thành
        Delayed = 4     // Đã lưu trữ (hoặc trạng thái khác bạn muốn)
    }

}
