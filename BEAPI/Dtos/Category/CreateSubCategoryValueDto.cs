namespace BEAPI.Dtos.Category
{
    public class CreateSubCategoryValueDto
    {
        public Guid Id { get; set; }
        public List<CreateCategoryValueDto> CreateCategoryValueDtos { get; set; }
    }
}
