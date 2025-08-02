namespace BEAPI.Helper
{
    public static class GuidHelper
    {
        /// <summary>
        /// Kiểm tra chuỗi có phải là Guid hợp lệ không, nếu không thì throw Exception
        /// </summary>
        public static Guid ParseOrThrow(string input, string fieldName = "Id")
        {
            if (!Guid.TryParse(input, out var guid))
                throw new Exception($"Invalid {fieldName} format");

            return guid;
        }
    }

}
