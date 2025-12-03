using OfficeOpenXml;

namespace MiniAppGIBA.Base.Helpers
{
    public static class ExportHandler
    {
        public static async Task<byte[]> ExportData(string sheetTitle, Dictionary<string, List<string>> data)
        {
            // Tạo ExcelPackage mới để làm việc với file Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Thêm một worksheet vào Excel file
                var worksheet = package.Workbook.Worksheets.Add(sheetTitle);

                // Thêm tiêu đề cột từ các key của dictionary
                int colIndex = 1;
                foreach (var key in data.Keys)
                {
                    worksheet.Cells[1, colIndex].Value = key; // Ghi key vào hàng đầu tiên (tiêu đề cột)
                    colIndex++;
                }

                // Lặp qua các hàng và điền dữ liệu vào Excel
                int maxRows = data.Values.Max(list => list.Count);

                // Dữ liệu bắt đầu từ hàng thứ 2, điền vào từng cột
                for (int row = 0; row < maxRows; row++)
                {
                    colIndex = 1; // Đặt lại colIndex cho mỗi hàng

                    foreach (var key in data.Keys)
                    {
                        // Kiểm tra nếu danh sách có dữ liệu cho hàng hiện tại
                        if (row < data[key].Count)
                        {
                            worksheet.Cells[row + 2, colIndex].Value = data[key][row];
                        }
                        else
                        {
                            worksheet.Cells[row + 2, colIndex].Value = string.Empty; // Nếu không có dữ liệu thì để trống
                        }
                        colIndex++;
                    }
                }

                // Lưu file vào bộ nhớ (MemoryStream)
                using (var stream = new MemoryStream())
                {
                    // Lưu Excel vào stream
                    await package.SaveAsAsync(stream);

                    // Đặt lại vị trí của stream về đầu để lấy byte[]
                    return stream.ToArray();
                }
            }
        }
    }
}
