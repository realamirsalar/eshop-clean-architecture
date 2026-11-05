namespace E_Shop.Api.Contracts.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
