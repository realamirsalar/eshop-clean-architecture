namespace E_Shop.Api.Contracts.Category
{
    public class UpdateCategoryRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
